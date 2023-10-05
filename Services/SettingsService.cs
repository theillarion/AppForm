using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xk7.Helper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Xk7.Services
{
    public static class DbSettingsService
    {
        private const string FileSettings = "SettingsMySql.json";
        public static DbConnectionStringBuilder LoadDbSettings()
        {
            var file = File.ReadAllText(AppEnvironment.GetRootWorkDirectory() + Path.DirectorySeparatorChar + FileSettings);
            var result = JsonConvert.DeserializeObject<MySqlConnectionStringBuilder>(file);
            return result ?? new MySqlConnectionStringBuilder();
        }
        public static bool DbSettingsFileExists() =>
            File.Exists(AppEnvironment.GetRootWorkDirectory() + Path.DirectorySeparatorChar + FileSettings);
        public static void RemoveDbSettings() =>
            File.Delete(AppEnvironment.GetRootWorkDirectory() + Path.DirectorySeparatorChar + FileSettings);
    }
}
