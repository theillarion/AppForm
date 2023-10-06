using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xk7.Helper;
using Xk7.Model;
using Renci.SshNet;
using Newtonsoft.Json;

namespace Xk7.Services
{
    public static class SftpSettingsService
    {
        public static string Filename = "SftpSettings.json";
        public static string FullPath = AppEnvironment.GetRootWorkDirectory() + Path.DirectorySeparatorChar + Filename;
        public static string ServerWorkDirectory = "/uploads/";
        public static string ServerWorkDirectoryTest = ServerWorkDirectory + "tests/";
        public static string ServerWorkDirectoryQuestion = ServerWorkDirectory + "questions/";
        internal static async Task<string> GetServerWorkDirectory(IFileService fileService)
        {
            if (!await fileService.ExistsPathAsync(ServerWorkDirectory))
                await fileService.CreateDirectoryAsync(ServerWorkDirectory);
            return ServerWorkDirectory;
        }
        internal static async Task<string> GetServerWorkDirectoryTest(IFileService fileService)
        {
            if (!await fileService.ExistsPathAsync(ServerWorkDirectoryTest))
                await fileService.CreateDirectoryAsync(ServerWorkDirectoryTest);
            return ServerWorkDirectoryTest;
        }
        internal static async Task<string> GetServerWorkDirectoryQuestion(IFileService fileService)
        {
            if (!await fileService.ExistsPathAsync(ServerWorkDirectoryQuestion))
                await fileService.CreateDirectoryAsync(ServerWorkDirectoryQuestion);
            return ServerWorkDirectoryQuestion;
        }
        public static SftpClient? LoadSettings()
        {
            var file = File.ReadAllText(FullPath);
            var result = JsonConvert.DeserializeObject<SftpSettings>(file);
            return result != null ? new SftpClient(result.Host, result.Port, result.Username, result.Password) : null;
        }
        public static bool FileExists() => File.Exists(FullPath);
        public static void Remove() => File.Delete(FullPath);
    }
}
