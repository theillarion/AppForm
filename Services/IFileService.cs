using System.Threading.Tasks;
using Xk7.Helper.Enums;

namespace Xk7.Services
{
    internal interface IFileService
    {
        Task<LoadFileResult> UploadFileAsync(string srcFilePath, string destFilePath, bool isOverride = false);
        Task<LoadFileResult> DownloadFileAsync(string srcFilePath, string destFilePath, bool isOverride = false);
        bool ExistsPath(string path);
        Task<bool> ExistsPathAsync(string path);
        Task<RemoveResult> RemoveFileAsync(string filePath);
        bool CreateDirectory(string path);
        Task<bool> CreateDirectoryAsync(string path);
        Task<RemoveResult> RemoveDirectoryAsync(string path);
    };
}
