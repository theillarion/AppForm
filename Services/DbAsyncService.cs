using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using Xk7.Helper.Enums;
using Xk7.Helper.Converts;
using Xk7.Helper.Exceptions;
using Xk7.Helper.Extensions;
using Xk7.Model;

namespace Xk7.Services
{
    internal class DbAsyncService : IDbAsyncService
    {
        private readonly DbConnection _connection;
        private readonly bool _needOpenClose;
        public DbAsyncService(DbConnection connection, bool needFirstOpenConnection = true, bool needOpenCloceConnection = false)
        {
            _connection = connection;
            _needOpenClose = needOpenCloceConnection;
            if (!needFirstOpenConnection)
                return;
            try
            {
                _connection.Open();
            }
            catch (Exception)
            {
                throw new ConnectionException("Connection refused");
            }
        }
        private async Task OpenAsync()
        {
            if (_needOpenClose)
                await _connection.OpenAsync();

            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");
        }
        private async Task CloseAsync()
        {
            if (_needOpenClose)
                await _connection.CloseAsync();
        }
        public async Task<bool> ExistsUserImplAsync(DbCommand command, string login)
        {
            try
            {
                command.CommandText = $"SELECT `Login` FROM `User` WHERE `Login` = @Login";
                command.AddParameterWithValue("@Login", login);

                return await command.ExecuteScalarAsync() != null;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<bool> ExistsUserAsync(string login)
        {
            await OpenAsync();

            var result = await ExistsUserImplAsync(_connection.CreateCommand(), login);

            await CloseAsync();
            return result;
        }
        public static async Task<bool> IsBannedUserImplAsync(DbCommand command, string login)
        {
            try
            {
                command.CommandText = $"SELECT `IsBlocked` FROM `User` WHERE `Login` = @Login";
                command.AddParameterWithValue("@Login", login);

                var reader = await command.ExecuteScalarAsync();
                return reader == null || (bool)reader;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<bool> IsBannedUserAsync(string login)
        {
            await OpenAsync();

            var result = await IsBannedUserImplAsync(_connection.CreateCommand(), login);

            await CloseAsync();
            return result;
        }
        public async Task<string> GetHashPasswordAsync(string login)
        {
            await OpenAsync();

            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = $"SELECT `Password` FROM `User` WHERE `Login` = @Login";
                command.AddParameterWithValue("@Login", login);

                await using var reader = await command.ExecuteReaderAsync();
                var result = await reader.ReadAsync() ? reader.GetString(0) : string.Empty;

                await CloseAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        // Check without `ExistsUser`
        public async Task<AddUserResult> AddUserAsync(User user)
        {
            await OpenAsync();

            if (await ExistsUserImplAsync(_connection.CreateCommand(), user.Login))
                return AddUserResult.UserExists;

            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText =
                    $"INSERT INTO `User` VALUES(@IdUserRole, @Login, @Password, @FirstName, @SecondName, @DateBirthday, @IsBlocked)";
                command.AddParameterWithValue("@IdUserRole", (int)user.IdUserRole);
                command.AddParameterWithValue("@Login", user.Login);
                command.AddParameterWithValue("@Password", user.Password);
                command.AddParameterWithValue("@FirstName", user.FirstName);
                command.AddParameterWithValue("@SecondName", user.SecondName);
                command.AddParameterWithValue("@DateBirthday", user.DateBirthday.ToString("yyyy-MM-dd"));
                command.AddParameterWithValue("@IsBlocked", user.IsBanned);

                var result = await command.ExecuteNonQueryAsync() == 1 ? AddUserResult.Success : AddUserResult.Unknown;

                await CloseAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<DataRow?> GetDataUserByLoginAsync(string login)
        {
            await OpenAsync();

            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = $"SELECT * FROM `User` WHERE `Login` = @Login";
                command.AddParameterWithValue("@Login", login);
                await using var reader = await command.ExecuteReaderAsync();
                if (!reader.HasRows)
                    return null;
                using var table = new DataTable();
                table.Load(reader);

                await CloseAsync();
                return table.Rows[0];
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<CommonAddResult> AddLogAsync(string login, LoggingType loggingType)
        {
            await OpenAsync();

            if (!await ExistsUserImplAsync(_connection.CreateCommand(), login))
                return CommonAddResult.NotExistsUser;

            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = $"INSERT INTO `Logging`(`IdLoggingType`, `Login`, `UTCDateTime`) VALUES(@LoggingType, @Login, UTC_TIMESTAMP());";
                command.AddParameterWithValue("@LoggingType", loggingType);
                command.AddParameterWithValue("@Login", login);

                var result = await command.ExecuteNonQueryAsync() == 1 ? CommonAddResult.Success : CommonAddResult.Unknown;

                await CloseAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }

        public async Task<UpdateUserResult> UpdateUserByLoginAsync(string oldLogin, DbUser newUser)
        {
            await OpenAsync();

            if (!newUser.AllFieldsFilled())
                return UpdateUserResult.Unknown;

            if (!await ExistsUserImplAsync(_connection.CreateCommand(), oldLogin))
                return UpdateUserResult.UserNotExists;

            try
            {
                await using var command = _connection.CreateCommand();

                if (oldLogin != newUser.Login && await ExistsUserImplAsync(_connection.CreateCommand(), newUser.Login))
                    return UpdateUserResult.LoginIsBusy;

                command.CommandText = $"UPDATE `User` SET `IdUserRole` = @IdUserRole, `Login` = @Login," +
                    $" `HashPassword` = @HashPassword, `FirstName` = @FirstName, `SecondName` =  @SecondName," +
                    $" `DateBirthday` = @DateBirthday, `IsBlocked` = @IsBlocked WHERE `Login` = @OldLogin;";
                command.AddParameterWithValue("@IdUserRole", (uint)newUser.IdUserRole);
                command.AddParameterWithValue("@Login", newUser.Login);
                command.AddParameterWithValue("@HashPassword", newUser.HashPassword);
                command.AddParameterWithValue("@FirstName", newUser.FirstName);
                command.AddParameterWithValue("@SecondName", newUser.SecondName);
                command.AddParameterWithValue("@DateBirthday", newUser.DateBirthday.ToString("yyyy-MM-dd"));
                command.AddParameterWithValue("@IsBlocked", newUser.IsBlocked);
                command.AddParameterWithValue("@OldLogin", oldLogin);
                var result = await command.ExecuteNonQueryAsync() == 1 ? UpdateUserResult.Success : UpdateUserResult.Unknown;

                await CloseAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<DataTable?> GetTableAsync(string nameTable)
        {
            await OpenAsync();

            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = $"SELECT * FROM `{nameTable}`";
                await using var reader = await command.ExecuteReaderAsync();
                if (!reader.HasRows)
                    return null;
                using var result = new DataTable();
                result.Load(reader);

                await CloseAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public static async Task<UserRole> GetUserRoleByLoginImplAsync(DbCommand command, string login)
        {
            try
            {
                command.CommandText = $"SELECT IdUserRole FROM `User` WHERE `Login` = @Login";
                command.AddParameterWithValue("@Login", login);

                var reader = await command.ExecuteScalarAsync();
                if (reader == null)
                    throw new ExecuteException("User not exists");
                return (UserRole)(uint)reader;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<UserRole> GetUserRoleAsync(string login)
        {
            await OpenAsync();

            var result = await GetUserRoleByLoginImplAsync(_connection.CreateCommand(), login);

            await CloseAsync();
            return result;
        }
        public async Task<bool> ExistsTestImplAsync(DbCommand command, uint idTest)
        {
            try
            {
                command.CommandText = $"SELECT `Id` FROM `Test` WHERE `Id` = @Id";
                command.AddParameterWithValue("@Id", idTest);

                var reader = await command.ExecuteScalarAsync();
                return reader != null && (uint)reader == idTest;
            }
            catch(Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<bool> ExistsTestAsync(uint idTest)
        {
            await OpenAsync();

            var result = await ExistsTestImplAsync(_connection.CreateCommand(), idTest);

            await CloseAsync();
            return result;
        }
        public async Task<AddImageResult> AddImageForTestImplAsync(IFileService fileService, DbCommand command, uint idTest, string filePath)
        {
            if (!await ExistsTestImplAsync(command, idTest))
                return AddImageResult.TestNotExists;

            try
            {
                var basePath = await SftpSettingsService.GetServerWorkDirectoryTest(fileService);
                var remoteFilePath = basePath + idTest.ToString() + Path.GetExtension(filePath);
                var result_upload = Converts.ConvertEnum(await fileService.UploadFileAsync(filePath, remoteFilePath));

                if (result_upload != AddImageResult.Success)
                    return result_upload;

                command.CommandText = $"INSERT INTO `TestImage`(`Id`, `Path`) VALUES(@Id, @Path)";
                command.AddParameterWithValue("@Path", remoteFilePath);

                return await command.ExecuteNonQueryAsync() == 1 ? AddImageResult.Success : AddImageResult.Unknown;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<AddImageResult> AddImageForTestAsync(IFileService fileService, uint idTest, string filePath)
        {
            await OpenAsync();

            var result = await AddImageForTestImplAsync(fileService, _connection.CreateCommand(), idTest, filePath);

            await CloseAsync();
            return result;
        }
        public async Task<AddTestResult> AddTestWithoutImageImplAsync(DbCommand command, DbTest test)
        {
            try
            {
                if (await ExistsTestImplAsync(command, test.Id))
                    return AddTestResult.TestExists;

                command.CommandText = $"INSERT INTO `Test`(`Id`, `Tittle`, `Description`, `UtcDateTimeCreated`, `WhoCreated`, `HasImage`)" +
                    $" VALUES(@Id, @Tittle, @Description, @UtcDateTimeCreated, @WhoCreated, @HasImage)";
                command.AddParameterWithValue("@Tittle", test.Tittle);
                command.AddParameterWithValue("@Description", test.Description);
                command.AddParameterWithValue("@UtcDateTimeCreated", test.UtcDateTimeCreated.ToString("yyyy-MM-dd hh:mm:ss"));
                command.AddParameterWithValue("@WhoCreated", test.WhoCreated);
                command.AddParameterWithValue("@HasImage", test.HasImage);

                return await command.ExecuteNonQueryAsync() == 1 ? AddTestResult.Success : AddTestResult.Unknown;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<AddTestResult> AddTestWithoutImageAsync(DbTest test)
        {
            await OpenAsync();

            var result = await AddTestWithoutImageImplAsync(_connection.CreateCommand(), test);

            await CloseAsync();
            return result;
        }
        public async Task<AddTestImageResult> AddTestWithImageAsync(IFileService fileService, DbTest test, string filePath)
        {
            await OpenAsync();
            var result = (AddTestImageResult)Converts.ConvertEnum(await AddTestWithoutImageImplAsync(_connection.CreateCommand(), test));

            if (result != AddTestImageResult.Success)
            {
                await CloseAsync();
                return result;
            }

            var new_result = (AddTestImageResult)await AddImageForTestImplAsync(fileService, _connection.CreateCommand(), test.Id, filePath);

            await CloseAsync();
            return new_result;
        }
    }
}
