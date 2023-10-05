using System.Data;
using System.Data.Common;
using System.IO;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Xk7.Helper
{
    public static class AppEnvironment
    {
        public static string GetRootWorkDirectory()
        {
            var projectPath = Directory.GetCurrentDirectory();
            while (projectPath != null && !Directory.Exists(Path.Combine(projectPath, "bin")))
            {
                projectPath = Directory.GetParent(projectPath)?.FullName;
            }
            return projectPath ?? "";
        }
    }
}
