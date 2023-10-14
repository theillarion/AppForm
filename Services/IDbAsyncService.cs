using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xk7.Helper.Enums;
using Xk7.Model;

namespace Xk7.Services
{
    internal interface IDbAsyncService
    {
        DbConnection GetCurrentConnection();
        Task<bool> ExistsUserAsync(string login);
        Task<bool> IsBannedUserAsync(string login);
        Task<string> GetHashPasswordAsync(string login);
        Task<AddUserResult> AddUserAsync(IDbUser user);
        Task<UpdateUserResult> UpdateUserByLoginAsync(string oldLogin, IDbUser newUser);
        Task<DataRow?> GetDataUserByLoginAsync(string login);
        Task<CommonAddResult> AddLogAsync(string login, LoggingType loggingType);
        Task<DataTable?> GetTableAsync(string nameTable);
        Task<UserRole> GetUserRoleAsync(string login);
        Task<bool> TestExistsAsync(int idTest);
        Task<bool> QuestionExistsAsync(int idQuestion);
        Task<bool> ImageTestExistsAsync(int idTest);
        Task<bool> ImageQuestionExistsAsync(int idQuestion);
        Task<bool> AnswerExistsAsync(int idAnswer);
        Task<bool> AnswerQuestionExistsAsync(int idQuestion);
        Task<DataTable?> GetAllTestsWithImage();
        //Task<DataRow?> GetQuestionWithImage(int id);
        Task<DataTable?> GetQuestionsWithImageByIdTest(int idTest);
        Task<DataTable?> GetAnswersByIdQuestion(int idQuestion);
        Task<string?> GetPathImageTestAsync(int idTest);
        Task<string?> GetPathImageQuestionAsync(int idQuestion);
        Task<AddTestResult> AddTestAsync(IDbTest test);
        Task<AddQuestionResult> AddQuestionAsync(IDbQuestion dbQuestion);
        Task<AddImageResult> AddImageTestAsync(int idTest, string remotePath, DbTransaction? dbTransaction = null);
        Task<AddImageResult> AddImageQuestionAsync(int idQuestion, string remotePath, DbTransaction? dbTransaction = null);
        Task<AddAnswerResult> AddAnswerAsync(DbSimpleAnswer dbSimpleAnswer);
        Task<RemoveResult> RemoveQuestionAsync(int idQuastion, DbTransaction? dbTransaction = null);
        //Task<int> RemoveQuastionsByIdTestAsync(int idTest);
        Task<RemoveImageResult> RemoveImageTestAsync(int idImage, DbTransaction? dbTransaction = null);
        Task<RemoveImageResult> RemoveImageQuestionAsync(int idImage, DbTransaction? dbTransaction = null);
        Task<RemoveResult> RemoveAnswerAsync(int idAnswer);
        Task<int> RemoveAnswersByIdQuestionAsync(int idQuestion);
    }
}