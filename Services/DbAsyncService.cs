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
using Org.BouncyCastle.Crypto;
using static System.Net.Mime.MediaTypeNames;
using System.Transactions;

namespace Xk7.Services
{
	internal class DbAsyncService : IDbAsyncService
	{
		private readonly DbConnection _connection;
		private readonly bool _needOpenClose;
		public DbAsyncService(DbConnection connection, bool needOpenCloceConnection = false)
		{
			_connection = connection;
			_needOpenClose = needOpenCloceConnection;
			if (needOpenCloceConnection)
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
		public DbConnection GetCurrentConnection() => _connection;
		private static async Task<bool> ExistsUserImplAsync(DbCommand command, string login)
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
		private static async Task<bool> IsBannedUserImplAsync(DbCommand command, string login)
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
		public async Task<AddUserResult> AddUserAsync(IDbUser user)
		{
			await OpenAsync();

			try
			{
				await using var command = _connection.CreateCommand();

				if (await ExistsUserImplAsync(command, user.Login))
					return AddUserResult.UserExists;

				command.CommandText =
					$"INSERT INTO `User` VALUES(@IdUserRole, @Login, @Password, @FirstName, @SecondName, @DateBirthday, @IsBlocked)";
				command.AddParameterWithValue("@IdUserRole", user.IdUserRole);
				command.AddParameterWithValue("@Password", user.HashPassword);
				command.AddParameterWithValue("@FirstName", user.FirstName);
				command.AddParameterWithValue("@SecondName", user.SecondName);
				command.AddParameterWithValue("@DateBirthday", user.DateBirthday?.ToString(IDbUser.DbDateFormat));
				command.AddParameterWithValue("@IsBlocked", user.IsBlocked);

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

		public async Task<UpdateUserResult> UpdateUserByLoginAsync(string oldLogin, IDbUser newUser)
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
				command.AddParameterWithValue("@IdUserRole", (int)newUser.IdUserRole);
				command.AddParameterWithValue("@Login", newUser.Login);
				command.AddParameterWithValue("@HashPassword", newUser.HashPassword);
				command.AddParameterWithValue("@FirstName", newUser.FirstName);
				command.AddParameterWithValue("@SecondName", newUser.SecondName);
				command.AddParameterWithValue("@DateBirthday", newUser.DateBirthday?.ToString(IDbUser.DbDateFormat));
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
				return (UserRole)(int)reader;
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
		private static async Task<bool> TestExistsImplAsync(DbCommand command, int id)
		{
			try
			{
				command.CommandText = $"SELECT `Id` FROM `{TestNameTables.Test}` WHERE `Id` = @Id";
				command.Parameters.Clear();
				command.AddParameterWithValue("@Id", id);

				var reader = await command.ExecuteScalarAsync();
				return reader != null && Convert.ToUInt32(reader) == id;
			}
			catch (Exception ex)
			{
				throw new ExecuteException(ex.Message);
			}
		}
		public async Task<bool> TestExistsAsync(int idTest)
		{
			await OpenAsync();

			var result = await TestExistsImplAsync(_connection.CreateCommand(), idTest);

			await CloseAsync();
			return result;
		}
		private static async Task<bool> QuestionExistsImplAsync(DbCommand command, int id)
		{
			try
			{
				command.CommandText = $"SELECT `Id` FROM `{TestNameTables.Question}` WHERE `Id` = @Id";
				command.Parameters.Clear();
				command.AddParameterWithValue("@Id", id);

				var reader = await command.ExecuteScalarAsync();
				return reader != null && Convert.ToUInt32(reader) == id;
			}
			catch (Exception ex)
			{
				throw new ExecuteException(ex.Message);
			}
		}
		public async Task<bool> QuestionExistsAsync(int idQuestion)
		{
			await OpenAsync();

			var result = await QuestionExistsImplAsync(_connection.CreateCommand(), idQuestion);

			await CloseAsync();
			return result;
		}
		private static async Task<bool> ImageTestExistsImplAsync(DbCommand command, int idTest)
		{
			try
			{
				command.CommandText = $"SELECT `IdTest` FROM `{TestNameTables.ImageTest}` WHERE `IdTest` = @IdTest";
				command.Parameters.Clear();
				command.AddParameterWithValue("@IdTest", idTest);

				var reader = await command.ExecuteScalarAsync();
				return reader != null && (int)reader == idTest;
			}
			catch (Exception ex)
			{
				throw new ExecuteException(ex.Message);
			}
		}
		public async Task<bool> ImageTestExistsAsync(int idTest)
		{
			await OpenAsync();

			var result = await ImageTestExistsImplAsync(_connection.CreateCommand(), idTest);

			await CloseAsync();
			return result;
		}
		private static async Task<bool> ImageQuestionExistsImplAsync(DbCommand command, int idQuestion)
		{
			try
			{
				command.CommandText = $"SELECT `IdQuestion` FROM `{TestNameTables.ImageQuestion}` WHERE `IdQuestion` = @IdQuestion";
				command.Parameters.Clear();
				command.AddParameterWithValue("@IdQuestion", idQuestion);

				var reader = await command.ExecuteScalarAsync();
				return reader != null && (int)reader == idQuestion;
			}
			catch (Exception ex)
			{
				throw new ExecuteException(ex.Message);
			}
		}
		public async Task<bool> ImageQuestionExistsAsync(int idQuestion)
		{
			await OpenAsync();

			var result = await ImageQuestionExistsImplAsync(_connection.CreateCommand(), idQuestion);

			await CloseAsync();
			return result;
		}
		private static async Task<int> AnswerExistsImplAsync(DbCommand command, string nameIdField, int valueIdField)
		{
			try
			{
				command.CommandText = $"SELECT COUNT(`{nameIdField}`) FROM `{TestNameTables.Answer}` WHERE `{nameIdField}` = @Id";
				command.Parameters.Clear();
				command.AddParameterWithValue("@Id", valueIdField);

				var reader = await command.ExecuteScalarAsync();
				return (reader != null) ? Convert.ToInt32(reader) : 0;
			}
			catch (Exception ex)
			{
				throw new ExecuteException(ex.Message);
			}
		}
		private static async Task<bool> AnswerExistsImplAsync(DbCommand command, int idAnswer)
		{
			return await AnswerExistsImplAsync(command, "Id", idAnswer) == 1;
		}
		public async Task<bool> AnswerExistsAsync(int idAnswer)
		{
			await OpenAsync();

			var result = await AnswerExistsImplAsync(_connection.CreateCommand(), idAnswer);

			await CloseAsync();
			return result;
		}
		public async Task<bool> AnswerQuestionExistsAsync(int idQuestion)
		{
			await OpenAsync();

			var result = await AnswerExistsImplAsync(_connection.CreateCommand(), "IdQuestion", idQuestion);

			await CloseAsync();
			return result > 0;
		}
		public async Task<DataTable?> GetAllTestsWithImage()
		{
			await OpenAsync();
			try
			{
				await using var command = _connection.CreateCommand();
				command.CommandText = $"SELECT `Id`, `Tittle`, `Description`, `UtcDateTimeCreated`, `WhoCreated`, `HasImage`, `Path` " +
					$"FROM `Test` left join `TestImage` ON `Test`.`Id` = `TestImage`.`IdTest`";

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
		public async Task<DataTable?> GetQuestionsWithImageByIdTest(int idTest)
		{
			await OpenAsync();

			try
			{
				await using var command = _connection.CreateCommand();
				command.CommandText = $"SELECT `Id`, `IdTest`, `IdType`, `OrdinalNumber`, `Text`, `HasImage`, `Path` " +
					$"FROM `{TestNameTables.Question}` LEFT JOIN `{TestNameTables.ImageQuestion}` " +
					$"ON `{TestNameTables.Question}`.`Id` = `{TestNameTables.ImageQuestion}`.`IdQuestion` " +
					$"WHERE `IdTest` = @IdTest ORDER BY `OrdinalNumber`";
				command.AddParameterWithValue("@IdTest", idTest);

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
		public async Task<DataTable?> GetAnswersByIdQuestion(int idQuestion)
		{
            await OpenAsync();

            try
            {
				uint newIdQuestion = (uint)idQuestion;
                await using var command = _connection.CreateCommand();
				command.CommandText = $"SELECT * FROM `SimpleAnswer` WHERE `IdQuestion` = @IdQuestion";
				command.AddParameterWithValue("@IdQuestion", newIdQuestion);

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
        public async Task<string?> GetPathImageTestAsync(int idTest)
		{
			await OpenAsync();

			try
			{
				await using var command = _connection.CreateCommand();
				command.CommandText = $"SELECT `Path` FROM `{TestNameTables.ImageTest}` WHERE `IdTest` = @IdTest";
				command.Parameters.Clear();
				command.AddParameterWithValue("@IdTest", idTest);

				var reader = await command.ExecuteScalarAsync();

				await CloseAsync();
				return reader as string ?? null;
			}
			catch (Exception ex)
			{
				throw new ExecuteException(ex.Message);
			}
		}
		public async Task<string?> GetPathImageQuestionAsync(int idQuestion)
		{
			await OpenAsync();

			try
			{
				await using var command = _connection.CreateCommand();
				command.CommandText = $"SELECT `Path` FROM `{TestNameTables.ImageQuestion}` WHERE `IdQuestion` = @IdQuestion";
				command.Parameters.Clear();
				command.AddParameterWithValue("@IdQuestion", idQuestion);

				var reader = await command.ExecuteScalarAsync();

				await CloseAsync();
				return reader as string ?? null;
			}
			catch (Exception ex)
			{
				throw new ExecuteException(ex.Message);
			}
		}
		public async Task<AddTestResult> AddTestImplAsync(DbCommand command, IDbTest test)
		{
			if (await TestExistsImplAsync(command, test.Id))
				return AddTestResult.TestExists;

			try
			{
				command.CommandText = $"INSERT INTO `{TestNameTables.Test}` " +
					$"VALUES(@Id, @Tittle, @Description, @UtcDateTimeCreated, @WhoCreated, @HasImage); SELECT @@IDENTITY";
				command.Parameters.Clear();
				command.AddParameterWithValue("@Id", test.Id);
				command.AddParameterWithValue("@Tittle", test.Tittle);
				command.AddParameterWithValue("@Description", test.Description);
				command.AddParameterWithValue("@UtcDateTimeCreated", test.UtcDateTimeCreated);
				command.AddParameterWithValue("@WhoCreated", test.WhoCreated);
				command.AddParameterWithValue("@HasImage", test.HasImage);

				var result = Convert.ToInt32(await command.ExecuteScalarAsync());
				
				if (result >= 0 && test.Id == 0)
					test.Id = (int)result;

				return (result == test.Id) ? AddTestResult.Success : AddTestResult.Unknown;
			}
			catch (Exception ex)
			{
				throw new ExecuteException(ex.Message);
			}
		}
		public async Task<AddTestResult> AddTestAsync(IDbTest test)
		{
			await OpenAsync();

			var result = await AddTestImplAsync(_connection.CreateCommand(), test);

			await CloseAsync();
			return result;
		}
		private static async Task<AddQuestionResult> AddQuestionImplAsync(DbCommand command, IDbQuestion dbQuestion)
		{
			if (!await TestExistsImplAsync(command, dbQuestion.IdTest))
				return AddQuestionResult.TestNotExists;

			if (await QuestionExistsImplAsync(command, dbQuestion.Id))
				return AddQuestionResult.QuestionExists;

			try
			{
				command.CommandText = $"INSERT INTO `{TestNameTables.Question}` " +
					$"VALUES (@Id, @IdTest, @IdType, @OrdinalNumber, @Text, @HasImage); SELECT @@IDENTITY";
				command.Parameters.Clear();
				command.AddParameterWithValue("@Id", dbQuestion.Id);
				command.AddParameterWithValue("@IdTest", dbQuestion.IdTest);
				command.AddParameterWithValue("@IdType", dbQuestion.IdType);
				command.AddParameterWithValue("@OrdinalNumber", dbQuestion.OrdinalNumber);
				command.AddParameterWithValue("@Text", dbQuestion.Text);
				command.AddParameterWithValue("@HasImage", dbQuestion.HasImage);

				var result = Convert.ToInt32(await command.ExecuteScalarAsync());
				if (result >= 0 && dbQuestion.Id == 0)
					dbQuestion.Id = (int)result;

				return (result == dbQuestion.Id) ? AddQuestionResult.Success : AddQuestionResult.Unknown;
			}
			catch (Exception ex)
			{
				throw new ExecuteException(ex.Message);
			}
		}
		public async Task<AddQuestionResult> AddQuestionAsync(IDbQuestion dbQuestion)
		{
			await OpenAsync();

			var result = await AddQuestionImplAsync(_connection.CreateCommand(), dbQuestion);

			await CloseAsync();
			return result;
		}
		private static async Task<AddImageResult> AddImageTestImplAsync(DbCommand command, int idTest, string remotePath)
		{
			if (!await TestExistsImplAsync(command, idTest))
				return AddImageResult.IdNotExists;

			if (await ImageTestExistsImplAsync(command, idTest))
				return AddImageResult.ImageExists;
			try
			{
				command.CommandText = $"INSERT INTO `{TestNameTables.ImageTest}` VALUES(@IdTest, @Path)";
				command.Parameters.Clear();
				command.AddParameterWithValue("@IdTest", idTest);
				command.AddParameterWithValue("@Path", remotePath);

				return await command.ExecuteNonQueryAsync() == 1 ? AddImageResult.Success : AddImageResult.Unknown;
			}
			catch (Exception ex)
			{
				throw new ExecuteException(ex.Message);
			}
		}
		public async Task<AddImageResult> AddImageTestAsync(int idTest, string remotePath, DbTransaction? dbTransaction = null)
		{
			await OpenAsync();

			await using var command = _connection.CreateCommand();
			command.Transaction = dbTransaction;
			var result = await AddImageTestImplAsync(command, idTest, remotePath);

			await CloseAsync();
			return result;
		}
		private static async Task<AddImageResult> AddImageQuestionImplAsync(DbCommand command, int idQuestion, string remotePath)
		{
			if (!await QuestionExistsImplAsync(command, idQuestion))
				return AddImageResult.IdNotExists;

			if (await ImageQuestionExistsImplAsync(command, idQuestion))
				return AddImageResult.ImageExists;
			try
			{
				command.CommandText = $"INSERT INTO `{TestNameTables.ImageQuestion}` VALUES(@IdQuestion, @Path)";
				command.Parameters.Clear();
				command.AddParameterWithValue("@IdQuestion", idQuestion);
				command.AddParameterWithValue("@Path", remotePath);

				return await command.ExecuteNonQueryAsync() == 1 ? AddImageResult.Success : AddImageResult.Unknown;
			}
			catch (Exception ex)
			{
				throw new ExecuteException(ex.Message);
			}
		}
		public async Task<AddImageResult> AddImageQuestionAsync(int idQuestion, string remotePath, DbTransaction? dbTransaction = null)
		{
			await OpenAsync();

			await using var command = _connection.CreateCommand();
			command.Transaction = dbTransaction;
			var result = await AddImageQuestionImplAsync(command, idQuestion, remotePath);

			await CloseAsync();
			return result;
		}
        private static async Task<AddAnswerResult> AddAnswerImplAsync(DbCommand command, DbSimpleAnswer dbSimpleAnswer)
        {
            if (await AnswerExistsImplAsync(command, dbSimpleAnswer.Id))
                return AddAnswerResult.AnswerExists;

            if (!await QuestionExistsImplAsync(command, dbSimpleAnswer.IdQuestion))
                return AddAnswerResult.QuestionNotExists;

            try
            {
                command.CommandText = $"INSERT INTO `{TestNameTables.Answer}` VALUES (@Id, @IdQuestion, @Text, @IsCorrect)";
                command.Parameters.Clear();
                command.AddParameterWithValue("@Id", dbSimpleAnswer.Id);
                command.AddParameterWithValue("@IdQuestion", dbSimpleAnswer.IdQuestion);
                command.AddParameterWithValue("@Text", dbSimpleAnswer.Text);
				command.AddParameterWithValue("@IsCorrect", dbSimpleAnswer.IsCorrect);

                return await command.ExecuteNonQueryAsync() == 1 ? AddAnswerResult.Success : AddAnswerResult.Unknown;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<AddAnswerResult> AddAnswerAsync(DbSimpleAnswer dbSimpleAnswer)
        {
            await OpenAsync();

            var result = await AddAnswerImplAsync(_connection.CreateCommand(), dbSimpleAnswer);

            await CloseAsync();
            return result;
        }
		private async static Task<RemoveResult> RemoveQuestionAsync(DbCommand command, int idQuestion)
		{
            var resultRemoveImage = await RemoveImageQuestionImplAsync(command, idQuestion);
			if (resultRemoveImage == RemoveImageResult.ImageNotExists)
				return RemoveResult.NotExists;
			else if (resultRemoveImage == RemoveImageResult.Unknown)
				return RemoveResult.Unknown;

			await RemoveAnswersImplAsync(command, "IdQuestion", idQuestion);

            try
			{
				command.CommandText = $"DELETE FROM `{TestNameTables.Question}` WHERE `Id` = @Id";
				command.Parameters.Clear();
				command.AddParameterWithValue("@Id", idQuestion);

				return (await command.ExecuteNonQueryAsync() == 1) ? RemoveResult.Success : RemoveResult.Unknown;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }

        public async Task<RemoveResult> RemoveQuestionAsync(int idQuastion, DbTransaction? dbTransaction = null)
		{
            await OpenAsync();
			await using var command = _connection.CreateCommand();
			command.Transaction = dbTransaction;
			var result = await RemoveQuestionAsync(command, idQuastion);

			await CloseAsync();
			return result;
        }
		//private static async Task<int> RemoveQuastionsByIdTestImplAsync(DbCommand command, int idTest)
		//{
		//	try
		//	{

		//	}
  //          catch (Exception ex)
  //          {
  //              throw new ExecuteException(ex.Message);
  //          }
  //      }

  //      public async Task<int> RemoveQuastionsByIdTestAsync(int idTest)
		//{

		//}
        private static async Task<RemoveImageResult> RemoveImageTestImplAsync(DbCommand command, int idImage)
		{
			if (!await ImageTestExistsImplAsync(command, idImage))
				return RemoveImageResult.ImageNotExists;

			try
			{
				command.CommandText = $"DELETE FROM `{TestNameTables.ImageTest}` WHERE `IdTest` = @IdImage";
				command.Parameters.Clear();
				command.AddParameterWithValue("@IdImage", idImage);

				return (await command.ExecuteNonQueryAsync() == 1) ? RemoveImageResult.Success : RemoveImageResult.Unknown;
			}
			catch (Exception ex)
			{
				throw new ExecuteException(ex.Message);
			}
		}
		public async Task<RemoveImageResult> RemoveImageTestAsync(int idTest, DbTransaction? dbTransaction = null)
		{
			await OpenAsync();

			await using var command = _connection.CreateCommand();
			command.Transaction = dbTransaction;
			var result = await RemoveImageTestImplAsync(command, idTest);

			await CloseAsync();
			return result;
		}
		private static async Task<RemoveImageResult> RemoveImageQuestionImplAsync(DbCommand command, int idImage)
		{
			if (!await ImageQuestionExistsImplAsync(command, idImage))
				return RemoveImageResult.ImageNotExists;

			try
			{
				command.CommandText = $"DELETE FROM `{TestNameTables.ImageQuestion}` WHERE `IdQuestion` = @IdImage";
				command.Parameters.Clear();
				command.AddParameterWithValue("@IdImage", idImage);

				return (await command.ExecuteNonQueryAsync() == 1) ? RemoveImageResult.Success : RemoveImageResult.Unknown;
			}
			catch (Exception ex)
			{
				throw new ExecuteException(ex.Message);
			}
		}
		public async Task<RemoveImageResult> RemoveImageQuestionAsync(int idQuestion, DbTransaction? dbTransaction = null)
		{
			await OpenAsync();

			await using var command = _connection.CreateCommand();
			command.Transaction = dbTransaction;
			var result = await RemoveImageQuestionImplAsync(command, idQuestion);

			await CloseAsync();
			return result;
		}
		//RemoveAllAnwerByIdQuestion
		private static async Task<int> RemoveAnswersImplAsync(DbCommand command, string nameIdField, int valueIdField)
		{
            try
            {
                command.CommandText = $"DELETE FROM `{TestNameTables.Answer}` WHERE `{nameIdField}` = @Id";
                command.Parameters.Clear();
                command.AddParameterWithValue("@Id", valueIdField);

                return Convert.ToInt32(await command.ExecuteNonQueryAsync());
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<RemoveResult> RemoveAnswerAsync(int idAnswer)
		{
			await OpenAsync();

            await using var command = _connection.CreateCommand();
            if (!await AnswerExistsImplAsync(command, idAnswer))
                return RemoveResult.NotExists;

            var result = await RemoveAnswersImplAsync(command, "Id", idAnswer);

			await CloseAsync();
			return (result == 1) ? RemoveResult.Success : RemoveResult.Unknown;
		}
        public async Task<int> RemoveAnswersByIdQuestionAsync(int idQuestion)
		{
			await OpenAsync();
            var result = await RemoveAnswersImplAsync(_connection.CreateCommand(), "IdQuestion", idQuestion);

            await CloseAsync();
			return result;
        }
        

		// ============================

		//public async Task<AddTestResult> AddTestWithImageAsync(IDbTestWithImage test)
		//{
		//    await OpenAsync();
		//    await using var transaction = await _connection.BeginTransactionAsync(IsolationLevel.RepeatableRead);

		//    try
		//    {
		//        await using var command = _connection.CreateCommand();
		//        command.Transaction = transaction;

		//        var resultAddTest = await AddTestImplAsync(command, test);
		//        if (resultAddTest != AddTestResult.Success)
		//        {
		//            await transaction.RollbackAsync();
		//            await CloseAsync();
		//            return resultAddTest;
		//        }

		//        var resultAddImageTest = await AddImageImplAsync(command, "TestImage", test.Id, test.Path);
		//        if (resultAddImageTest != AddImageResult.Success)
		//        {
		//            await transaction.RollbackAsync();
		//            await CloseAsync();
		//            return AddTestResult.Unknown;
		//        }

		//        await transaction.CommitAsync();
		//        await CloseAsync();
		//        return AddTestResult.Success;
		//    }
		//    catch (Exception ex)
		//    {
		//        await transaction.RollbackAsync();
		//        await CloseAsync();
		//        throw new ExecuteException(ex.Message);
		//    }
		//}
	}
}