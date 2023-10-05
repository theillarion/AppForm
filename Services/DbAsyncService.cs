using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Xml;
using Xk7.Helper.Enums;
using Xk7.Helper.Exceptions;
using Xk7.Helper.Extensions;
using Xk7.Model;

namespace Xk7.Services
{
    internal class DbAsyncService : IDbAsyncService
    {
        private readonly DbConnection? _connection;
        public DbAsyncService(DbConnection connection, bool needOpenConnection = true)
        {
            if (!needOpenConnection)
                return;
            _connection = connection;
            try
            {
                _connection.Open();
            }
            catch (Exception)
            {
                throw new ConnectionException("Connection refused");
            }
        }
        public async Task<bool> ExistsUserAsync(string login)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");

            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = $"SELECT `Login` FROM `User` WHERE `Login` = @Login";
                command.AddParameterWithValue("@Login", login);

                return await command.ExecuteScalarAsync() != null;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public static async Task<bool> IsBannedUserAsync(DbCommand command, string login)
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
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");

            return await IsBannedUserAsync(_connection.CreateCommand(), login);
        }
        public async Task<string> GetHashPasswordAsync(string login)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");

            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = $"SELECT `Password` FROM `User` WHERE `Login` = @Login";
                command.AddParameterWithValue("@Login", login);

                await using var reader = await command.ExecuteReaderAsync();
                return await reader.ReadAsync() ? reader.GetString(0) : string.Empty;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        // Check without `ExistsUser`
        public async Task<AddUserResult> AddUserAsync(User user)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");

            if (await ExistsUserAsync(user.Login))
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

                return await command.ExecuteNonQueryAsync() == 1 ? AddUserResult.Success : AddUserResult.Unknown;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<DataRow?> GetDataUserByLoginAsync(string login)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");
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
                return table.Rows[0];
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<CommonAddResult> AddLog(string login, LoggingType loggingType)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");

            if (!await ExistsUserAsync(login))
                return CommonAddResult.NotExistsUser;

            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = $"INSERT INTO `Logging`(`IdLoggingType`, `Login`, `UTCDateTime`) VALUES(@LoggingType, @Login, UTC_TIMESTAMP());";
                command.AddParameterWithValue("@LoggingType", loggingType);
                command.AddParameterWithValue("@Login", login);

                return await command.ExecuteNonQueryAsync() == 1 ? CommonAddResult.Success : CommonAddResult.Unknown;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }

        public async Task<CommonAddResult> UpdateUserTableAsync(string OldLogin, DbUser NewUser)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");

            if (!await ExistsUserAsync(OldLogin))
                return CommonAddResult.NotExistsUser;

            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = $"UPDATE `User` SET `IdUserRole` = @IdUserRole,`Login` =  @Login, `Password` =  @Password, `FirstName` = @FirstName, `SecondName` =  @SecondName, `DateBirthday` = @DateBirthday, `IsBlocked` = @IsBlocked WHERE `Login` == @OldLogin;";
                command.AddParameterWithValue("@IdUserRole", (int)NewUser.IdUserRole);
                command.AddParameterWithValue("@Login", NewUser.Login);
                command.AddParameterWithValue("@Password", NewUser.HashPassword);
                command.AddParameterWithValue("@FirstName", NewUser.FirstName);
                command.AddParameterWithValue("@SecondName", NewUser.SecondName);
                command.AddParameterWithValue("@DateBirthday", NewUser.DateBirthday.ToString("yyyy-MM-dd"));
                command.AddParameterWithValue("@IsBlocked", NewUser.IsBlocked);
                command.AddParameterWithValue("@OldLogin", OldLogin);
                return command.ExecuteNonQuery() == 1 ? CommonAddResult.Success : CommonAddResult.Unknown;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }

        }
        public async Task<DataTable?> GetTable(string nameTable)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");

            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = $"SELECT * FROM `{nameTable}`";
                await using var reader = await command.ExecuteReaderAsync();
                if (!reader.HasRows)
                    return null;
                using var result = new DataTable();
                result.Load(reader);
                return result;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public static async Task<UserRole> GetUserRoleByLogin(DbCommand command, string login)
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
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");

            return await GetUserRoleByLogin(_connection.CreateCommand(), login);
        }
        public static async Task<bool> IsBusySlotAsync(DbCommand command, uint idSlot, bool blockTable = false)
        {
            try
            {
                command.CommandText = $"SELECT IsBusy FROM Timetable WHERE Id = @IdSlot";
                if (blockTable)
                    command.CommandText += " FOR UPDATE";
                command.AddParameterWithValue("@IdSlot", idSlot);

                var reader = await command.ExecuteScalarAsync();
                return reader == null || (bool)reader;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<bool> IsBusySlotAsync(uint idSlot)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");

            return await IsBusySlotAsync(_connection.CreateCommand(), idSlot, false);
        }
        public async Task<AddSlotResult> AddSlotAsync(string employeeLogin, DateOnly slotDate, TimeOnly slotTime)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");

            if (!await ExistsUserAsync(employeeLogin))
                return AddSlotResult.NotExistsUser;

            if (await GetUserRoleAsync(employeeLogin) != UserRole.Psychologist)
                return AddSlotResult.WrongUserRole;

            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = "INSERT INTO `Timetable`(`EmployeeLogin`, `SlotDate`, `SlotTime`, `IsBusy`)" +
                                  "VALUES(@EmployeeLogin, @SlotDate, @SlotTime, @IsBusy)";
                command.AddParameterWithValue("@EmployeeLogin", employeeLogin);
                command.AddParameterWithValue("@SlotDate", slotDate.ToString("yyyy-MM-dd"));
                command.AddParameterWithValue("@SlotTime", slotTime.ToString("HH:mm"));
                command.AddParameterWithValue("@IsBusy", false);

                return await command.ExecuteNonQueryAsync() == 1 ? AddSlotResult.Success : AddSlotResult.Unknown;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<BlockSlotResult> BlockSlotAsync(uint idSlot, string userLogin)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");

            await using var transaction = await _connection.BeginTransactionAsync(IsolationLevel.RepeatableRead);
            try
            {
                await using var command = _connection.CreateCommand();
                command.Transaction = transaction;

                if (await IsBannedUserAsync(command, userLogin))
                {
                    await transaction.RollbackAsync();
                    return BlockSlotResult.UserNotExists;
                }

                if (await IsBusySlotAsync(command, idSlot, true))
                {
                    await transaction.RollbackAsync();
                    return BlockSlotResult.SlotNotExists;
                }

                command.CommandText = "INSERT INTO `UserTimetable`(`IdTimetable`, `UserLogin`) VALUES (@IdSlot, @Login)";

                if (await command.ExecuteNonQueryAsync() != 1)
                {
                    await transaction.RollbackAsync();
                    return BlockSlotResult.Unknown;
                }

                command.CommandText = "UPDATE `Timetable` SET `IsBusy` = @Value WHERE `Id` = @IdSlot";
                command.AddParameterWithValue("@Value", true);

                if (await command.ExecuteNonQueryAsync() != 1)
                {
                    await transaction.RollbackAsync();
                    return BlockSlotResult.Unknown;
                }

                await transaction.CommitAsync();
                return BlockSlotResult.Success;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new ExecuteException(ex.Message);
            }
        }

        public async Task<DataTable?> GetSlotsTableByLogin(string login)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");
            try
            {
                await using var command = _connection.CreateCommand();
                //command.CommandText = $"SELECT IdTimetable, EmployeeLogin, SlotDate, SlotTime FROM Timetable JOIN UserTimetable ON Timetable.Id =  UserTimetable.IdTimetable AND UserLogin = `test`;";
                command.CommandText = $"SELECT * FROM `Timetable`;";
                await using var reader = await command.ExecuteReaderAsync();
                if (!reader.HasRows)
                    return null;
                using var result = new DataTable();
                result.Load(reader);
                return result;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }

        public async Task<DataRowCollection?> GetSlotsRowsByLogin(string login)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");
            try
            {
                await using var command = _connection.CreateCommand();
                //command.CommandText = $"SELECT IdTimetable, EmployeeLogin, SlotDate, SlotTime FROM Timetable JOIN UserTimetable ON Timetable.Id =  UserTimetable.IdTimetable AND UserLogin = `test`;";
                command.AddParameterWithValue("@Login", login);

                command.CommandText = $"SELECT `IdTimetable`, `EmployeeLogin`, `SlotDate`, `SlotTime` " +
                                      $"FROM `Timetable` JOIN `UserTimetable` " +
                                      $"ON `Timetable`.`Id` =  `UserTimetable`.`IdTimetable` AND `UserLogin` = @Login";

                await using var reader = await command.ExecuteReaderAsync();
                if (!reader.HasRows)
                    return null;
                using var result = new DataTable();
                result.Load(reader);
                return result.Rows;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }

        public async Task<bool> DeleteSlot(uint id)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");
            try
            {
                await using var command = _connection.CreateCommand();

                command.AddParameterWithValue("@Id", id);

                command.CommandText = $"DELETE " +
                                        $"FROM `UserTimetable` " +
                                        $"WHERE `IdTimetable` =  @Id";

                await using var reader = await command.ExecuteReaderAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }

        public async Task<DataRowCollection?> GetFreeSlotsRows()
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");
            try
            {
                await using var command = _connection.CreateCommand();

                command.CommandText = $"SELECT `Login`, `SlotDate`, `SlotTime` " +
                                      $"FROM `Timetable` FULL JOIN `User` ON `EmployeeLogin` = `Login` WHERE `IsBusy` = 0";

                await using var reader = await command.ExecuteReaderAsync();
                if (!reader.HasRows)
                    return null;
                using var result = new DataTable();
                result.Load(reader);
                return result.Rows;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }

        }

        public async Task<DataRowCollection?> setBusyGetId(string? userLogin, string dateOnly, string timeOnly)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");
            try
            {
                await using var command = _connection.CreateCommand();

                command.AddParameterWithValue("@userLogin", userLogin);
                command.AddParameterWithValue("@dateOnly", dateOnly);
                command.AddParameterWithValue("@timeOnly", timeOnly);

                command.CommandText = $"UPDATE `Timetable`" +
                                        $"SET `IsBusy` = 1 " +
                                        $"WHERE `EmployeeLogin` =  @userLogin AND `SlotDate` =  @dateOnly AND `SlotTime` =  @timeOnly";

                await using var reader1 = await command.ExecuteReaderAsync();
                reader1.Close();
                command.CommandText = $"SELECT `Id` FROM `Timetable`" +
                                        $"WHERE `EmployeeLogin` =  @userLogin AND `SlotDate` =  @dateOnly AND `SlotTime` =  @timeOnly";

                await using var reader = await command.ExecuteReaderAsync();
                if (!reader.HasRows)
                    return null;
                using var result = new DataTable();
                result.Load(reader);
                return result.Rows;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }

        public async Task insertUserSlot(string? userLogin, string dateOnly, string timeOnly)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");
            try
            {
                await using var command = _connection.CreateCommand();

                command.AddParameterWithValue("@userLogin", userLogin);
                command.AddParameterWithValue("@dateOnly", dateOnly);
                command.AddParameterWithValue("@timeOnly", timeOnly);

                command.CommandText = $"UPDATE `Timetable`" +
                                        $"SET `IsBusy` = 1 " +
                                        $"WHERE `EmployeeLogin` =  @userLogin AND `SlotDate` =  @dateOnly AND `SlotOnly` =  @timeOnly";

                await using var reader = await command.ExecuteReaderAsync();
                return;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }

        public async Task InsertUserTimeTable(uint id, string? login)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");
            try
            {
                await using var command = _connection.CreateCommand();

                command.AddParameterWithValue("@Id", id);
                command.AddParameterWithValue("@login", login);

                command.CommandText = $"INSERT " +
                                        $"INTO `UserTimetable` " +
                                        $"VALUES (@Id, @login)";

                await using var reader = await command.ExecuteReaderAsync();
                return;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }

        public async Task<DataRowCollection?> GetAllNotifications(string login, DateTime dateTimeBefore)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");

            await using var transaction = await _connection.BeginTransactionAsync(IsolationLevel.RepeatableRead);

            try
            {
                await using var command = _connection.CreateCommand();
                command.Transaction = transaction;
                command.AddParameterWithValue("@Login", login);
                command.CommandText = $"SELECT `DateTimeMark` " +
                                        $"FROM `NotificationLastAccessMark` " +
                                        $"WHERE `UserLogin` = @Login FOR UPDATE";

                var reader = await command.ExecuteScalarAsync();
                var dateTimeNow = DateTime.UtcNow;
                command.AddParameterWithValue("@DateTimeBefore", dateTimeBefore.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                command.CommandText = $"SELECT `DateTimeCreated`, `Description`, `IsChecked`" +
                                        $"FROM `Notification`" +
                                        $"WHERE Notification.UserLogin = @Login AND DateTimeCreated > @DateTimeBefore";
                
                await using var newReader = await command.ExecuteReaderAsync();
                if (!newReader.HasRows)
                    return null;
                using var table = new DataTable();
                table.Load(newReader);
                var result_row = table.Rows;
                await newReader.CloseAsync();

                if (reader == null)
                {
                    command.CommandText = $"INSERT INTO `NotificationLastAccessMark`(`UserLogin`, `DateTimeMark`) " +
                                          $"VALUES (@Login, @DateTimeMarkNow)";
                    command.AddParameterWithValue("@DateTimeMarkNow", dateTimeNow.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    // Должно быть затронута ровно 1 строка, иначе это отмена транзакции
                    if (await command.ExecuteNonQueryAsync() != 1)
                    {
                        await transaction.RollbackAsync();
                        return null;
                    }
                    await transaction.CommitAsync();
                }
                return result_row;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<DataRowCollection?> GetNewNotifications(string login)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");

            await using var transaction = await _connection.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                await using var command = _connection.CreateCommand();
                command.Transaction = transaction;
                command.AddParameterWithValue("@Login", login);

                command.CommandText = $"SELECT `DateTimeMark` " +
                                        $"FROM `NotificationLastAccessMark` " +
                                        $"WHERE `UserLogin` = @Login FOR UPDATE";
                var reader = await command.ExecuteScalarAsync();


                // Необходимо создать метку
                if (reader == null)
                {
                    var dateTimeNow = DateTime.UtcNow;
                    // Получаем все уведомления пользователя
                    command.CommandText = $"SELECT `DateTimeCreated`, `Description`, `IsChecked` " +
                                            $"FROM `Notification` " +
                                            $"WHERE `UserLogin` = @Login FOR UPDATE";
                    await using var newReader = await command.ExecuteReaderAsync();
                    // Увдомлений нету, ничего не делаем и возвращаем null collection
                    if (newReader == null || !newReader.HasRows)
                    {
                        // TODO: нужно ли делать Commit  перед return null, если в транзакции при текущем ветвлении использовались только select'ы
                        return null;
                    }
                    // Уведомления есть, создаем метку и возвращаем collection
                    else
                    {
                        using var table = new DataTable();
                        table.Load(newReader);
                        var result_rows = table.Rows;
                        await newReader.CloseAsync();

                        command.CommandText = $"INSERT INTO `NotificationLastAccessMark`(`UserLogin`, `DateTimeMark`) " +
                                                $"VALUES (@Login, @DateTimeMarkNow)";
                        command.AddParameterWithValue("@DateTimeMarkNow", dateTimeNow.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        // Должно быть затронута ровно 1 строка, иначе это отмена транзакции
                        if (await command.ExecuteNonQueryAsync() != 1)
                        {
                            await transaction.RollbackAsync();
                            return null;
                        }
                        await transaction.CommitAsync();
                        return result_rows;
                    }
                }
                else
                {
                    var dateTimeNow = DateTime.UtcNow;
                    command.CommandText = $"SELECT `DateTimeCreated`, `Description`, `IsChecked` " +
                                            $"FROM `Notification` " +
                                            $"WHERE `UserLogin` = @Login AND `DateTimeCreated` > @DateTimeMark FOR UPDATE";
                    command.AddParameterWithValue("@DateTimeMark", ((DateTime)reader).ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    await using var newReader = await command.ExecuteReaderAsync();
                    // Увдомлений нету, ничего не делаем и возвращаем null collection
                    if (newReader == null || !newReader.HasRows)
                    {
                        // TODO: нужно ли делать Commit  перед return null, если в транзакции при текущем ветвлении использовались только select'ы
                        return null;
                    }
                    // Уведомления есть, обновляем метку и возвращаем collection
                    else
                    {
                        using var table = new DataTable();
                        table.Load(newReader);
                        var result_rows = table.Rows;
                        await newReader.CloseAsync();

                        command.CommandText = $"UPDATE `NotificationLastAccessMark` " +
                                                $"SET `DateTimeMark` = @DateTimeMarkNow " +
                                                $"WHERE `UserLogin` = @Login";
                        command.AddParameterWithValue("@DateTimeMarkNow", dateTimeNow.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        // Должно быть затронута ровно 1 строка, иначе это отмена транзакции
                        if (await command.ExecuteNonQueryAsync() != 1)
                        {
                            await transaction.RollbackAsync();
                            return null;
                        }
                        await transaction.CommitAsync();
                        return result_rows;
                    }
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new ExecuteException(ex.Message);
            }
        }
    }

}
