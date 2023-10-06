using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xk7.Helper.Enums;
using Xk7.Model;

namespace Xk7.Services
{
    internal interface IDbAsyncService
    {
        Task<bool> ExistsUserAsync(string login);
        Task<bool> IsBannedUserAsync(string login);
        Task<string> GetHashPasswordAsync(string login);
        Task<AddUserResult> AddUserAsync(User user);
        Task<UpdateUserResult> UpdateUserByLoginAsync(string oldLogin, DbUser newUser);
        Task<DataRow?> GetDataUserByLoginAsync(string login);
        Task<CommonAddResult> AddLogAsync(string login, LoggingType loggingType);
        Task<DataTable?> GetTableAsync(string nameTable);
        Task<UserRole> GetUserRoleAsync(string login);
        Task<bool> ExistsTestAsync(uint idTest);
        Task<AddImageResult> AddImageForTestAsync(IFileService fileService, uint idTest, string filepath);
        Task<AddTestResult> AddTestWithoutImageAsync(DbTest test);
        Task<AddTestImageResult> AddTestWithImageAsync(IFileService fileService, DbTest test, string filepath);
    }
}
