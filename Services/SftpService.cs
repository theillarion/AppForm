using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Xk7.Helper.Enums;
using Xk7.Helper.Exceptions;
using Renci.SshNet;

namespace Xk7.Services
{
    internal class SftpService : IFileService
    {
        private readonly SftpClient _client;
        private readonly bool _needConnect;
        public SftpService(SftpClient client, bool needFirstConnect = true, bool needConnect = false)
        {
            _client = client;
            if (!needFirstConnect)
                return;

            try
            {
                _client.Connect();
            }
            catch (Exception)
            {
                throw new SftpConnectionException("Connection refused");
            }
        }
        private void ConnectImpl()
        {
            if (_needConnect)
                _client.Connect();

            if (!_client.IsConnected)
                throw new SftpConnectionException("Connection refused");
        }
        private async Task ConnectImplAsync()
        {
            if (_needConnect)
                await Task.Run(() => _client.Connect());

            if (!_client.IsConnected)
                throw new SftpConnectionException("Connection refused");
        }
        private void DisconnecttImpl()
        {
            if (_needConnect)
                _client.Disconnect();
        }
        private async Task DisconnecttImplAsync()
        {
            if (_needConnect)
                await Task.Run(() => _client.Disconnect());
        }
        public bool CreateDirectory(string path)
        {
            ConnectImpl();

            try
            {
                if (ExistsPath(path))
                    return false;

                _client.CreateDirectory(path);

                DisconnecttImpl();
                return true;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }

        }
        public async Task<bool> CreateDirectoryAsync(string path)
        {
            await ConnectImplAsync();

            try
            {
                if (await ExistsPathAsync(path))
                    return false;

                await Task.Run(() => _client.CreateDirectory(path));

                await DisconnecttImplAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }

        }
        public async Task<LoadFileResult> DownloadFileAsync(string srcFilePath, string destFilePath, bool canOverride = true)
        {
            await ConnectImplAsync();

            if (!canOverride && File.Exists(destFilePath))
                return LoadFileResult.DestinationFileExists;
            try
            {
                await using var fileStream = File.OpenWrite(destFilePath);

                if (!await ExistsPathAsync(srcFilePath))
                    return LoadFileResult.SourceFileNotExists;

                await Task.Run(() => _client.DownloadFile(srcFilePath, fileStream));

                await DisconnecttImplAsync();
                return LoadFileResult.Success;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public bool ExistsPath(string path)
        {
            ConnectImpl();

            try
            {
                var result = _client.Exists(path);

                DisconnecttImpl();
                return result;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<bool> ExistsPathAsync(string path)
        {
            await ConnectImplAsync();

            try
            {
                var result = await Task.Run(() => _client.Exists(path));

                await DisconnecttImplAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<RemoveFileResult> RemoveDirectoryAsync(string path)
        {
            await ConnectImplAsync();

            try
            {
                if (!await ExistsPathAsync(path))
                    return RemoveFileResult.NotExists;

                await Task.Run(() => _client.DeleteDirectory(path));

                await DisconnecttImplAsync();
                return RemoveFileResult.Success;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<RemoveFileResult> RemoveFileAsync(string filePath)
        {
            await ConnectImplAsync();

            try
            {
                if (!await ExistsPathAsync(filePath))
                    return RemoveFileResult.NotExists;

                await Task.Run(() => _client.DeleteFile(filePath));

                await DisconnecttImplAsync();
                return RemoveFileResult.Success;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<LoadFileResult> UploadFileAsync(string srcFilePath, string dstFilePath, bool canOverride = true)
        {
            await ConnectImplAsync();

            if (!canOverride && !File.Exists(srcFilePath))
                return LoadFileResult.SourceFileNotExists;
            try
            {
                await using var fileStream = File.OpenRead(srcFilePath);

                if (await ExistsPathAsync(dstFilePath))
                    return LoadFileResult.DestinationFileExists;

                await Task.Run(() => _client.UploadFile(fileStream, dstFilePath));

                await DisconnecttImplAsync();
                return LoadFileResult.Success;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
    }
}
