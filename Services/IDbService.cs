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
    [Obsolete("Soon all methods of this class will be moved to the `IDbAsyncService` class. This class will be deleted.")]
    public interface IDbService
    {
        bool ExistsUser(string login);
        bool IsBannedUser(string login);
        string GetHashPassword(string login);
        AddUserResult AddUser(User user);
        DataRow? GetDataUserByLogin(string login);
        CommonAddResult AddLogAsync(string login, LoggingType loggingType);
        DataTable? GetTable(string nameTable);
        internal CommonAddResult UpdateUserTable(string OldLogin, DbUser NewUser);
    }
}
