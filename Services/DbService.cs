using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xk7.Helper;
using Xk7.Helper.Enums;
using Xk7.Helper.Exceptions;
using Xk7.Helper.Extensions;
using Xk7.Model;

namespace Xk7.Services
{
    public class DbService : IDbService
    {
        private readonly DbConnection _connection;
        public DbService(DbConnection connection, bool needOpenConnection = true)
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
        public bool ExistsUser(string login)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = $"SELECT `Login` FROM `User` WHERE `Login` = @Login";
                command.AddParameterWithValue("@Login", login);

                return command.ExecuteScalar() != null;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public bool IsBannedUser(string login)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = $"SELECT `IsBlocked` FROM `User` WHERE `Login` = @Login";
                command.AddParameterWithValue("@Login", login);

                var reader = command.ExecuteScalar();
                return reader == null || (bool)reader;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public string GetHashPassword(string login)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = $"SELECT `Password` FROM `User` WHERE `Login` = @Login";
                command.AddParameterWithValue("@Login", login);

                using var reader = command.ExecuteReader();
                return reader.Read() ? reader.GetString(0) : string.Empty;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        // Check without `ExistsUser`
        public AddUserResult AddUser(User user)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");

            if (ExistsUser(user.Login))
                return AddUserResult.UserExists;

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText =
                    $"INSERT INTO `User` VALUES(@IdUserRole, @Login, @Password, @FirstName, @SecondName, @DateBirthday, @IsBlocked)";
                command.AddParameterWithValue("@IdUserRole", (int)user.IdUserRole);
                command.AddParameterWithValue("@Login", user.Login);
                command.AddParameterWithValue("@Password", user.Password);
                command.AddParameterWithValue("@FirstName", user.FirstName);
                command.AddParameterWithValue("@SecondName", user.SecondName);
                command.AddParameterWithValue("@DateBirthday", user.DateBirthday.ToString("yyyy-MM-dd"));
                command.AddParameterWithValue("@IsBlocked", user.IsBanned);

                return command.ExecuteNonQuery() == 1 ? AddUserResult.Success : AddUserResult.Unknown;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public DataRow? GetDataUserByLogin(string login)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = $"SELECT * FROM `User` WHERE `Login` = @Login";
                command.AddParameterWithValue("@Login", login);
                using var reader = command.ExecuteReader();

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
        public CommonAddResult AddLog(string login, LoggingType loggingType)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");

            if (!ExistsUser(login))
                return CommonAddResult.NotExistsUser;

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = $"INSERT INTO `Logging`(`IdLoggingType`, `Login`, `UTCDateTime`) VALUES(@LoggingType, @Login, UTC_TIMESTAMP());";
                command.AddParameterWithValue("@LoggingType", loggingType);
                command.AddParameterWithValue("@Login", login);

                return command.ExecuteNonQuery() == 1 ? CommonAddResult.Success : CommonAddResult.Unknown;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }

        public DataTable? GetTable(string nameTable)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = $"SELECT * FROM `{nameTable}`";
                using var reader = command.ExecuteReader();
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

        CommonAddResult IDbService.UpdateUserTable(string OldLogin, DbUser NewUser)
        {
            if (_connection is not { State: ConnectionState.Open })
                throw new ConnectionException("Connection refused");

            if (!ExistsUser(OldLogin))
                return CommonAddResult.NotExistsUser;

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = $"UPDATE `User` SET `IdUserRole` = @IdUserRole,`Login` =  @Login, `Password` =  @Password, `FirstName` = @FirstName, `SecondName` =  @SecondName, `DateBirthday` = @DateBirthday, `IsBlocked` = @IsBlocked WHERE `Login` == @OldLogin ";
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
    }
}
