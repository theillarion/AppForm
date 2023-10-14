using System;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Xk7.Helper.Converts;
using Xk7.Helper.Enums;
using Xk7.Helper.Exceptions;

namespace Xk7.Services
{
	internal enum RemoveImageResult
	{
        Success,
        ImageNotExists,
        Unknown
    }
    internal class ImageManager
	{
		private readonly IDbAsyncService _dbService;
		private readonly IFileService _fileService;
		public ImageManager(IDbAsyncService dbService, IFileService fileService)
		{
			_dbService = dbService;
			_fileService = fileService;
		}
        public async Task<LoadImageResult> DownloadImageTestAsync(int idTest, string localPath)
        {
            try
            {
                if (!await _dbService.TestExistsAsync(idTest))
                    return LoadImageResult.IdNotExists;

                string? remotePath = await _dbService.GetPathImageTestAsync(idTest);
                if (remotePath == null)
                    return LoadImageResult.SourceImageNotExists;

                var resultDownloadImage = await _fileService.DownloadFileAsync(remotePath, localPath);

                if (resultDownloadImage != LoadFileResult.Success)
                {
                    if (resultDownloadImage != LoadFileResult.Unknown)
                        return (resultDownloadImage == LoadFileResult.SourceFileNotExists) ? LoadImageResult.SourceImageNotExists : LoadImageResult.DestinationImageExists;
                    return LoadImageResult.Unknown;
                }
                return LoadImageResult.Success;
            }
            catch (Exception ex) when (ex is ConnectionException or SftpConnectionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
        public async Task<LoadImageResult> DownloadImageQuestionAsync(int idQuestion, string localPath)
		{
            try
            {
                if (!await _dbService.QuestionExistsAsync(idQuestion))
                    return LoadImageResult.IdNotExists;

                string? remotePath = await _dbService.GetPathImageQuestionAsync(idQuestion);
                if (remotePath == null)
                    return LoadImageResult.SourceImageNotExists;

                var resultDownloadImage = await _fileService.DownloadFileAsync(remotePath, localPath);

                if (resultDownloadImage != LoadFileResult.Success)
                {
                    if (resultDownloadImage != LoadFileResult.Unknown)
                        return (resultDownloadImage == LoadFileResult.SourceFileNotExists) ? LoadImageResult.SourceImageNotExists : LoadImageResult.DestinationImageExists;
                    return LoadImageResult.Unknown;
                }
                return LoadImageResult.Success;
            }
            catch (Exception ex) when (ex is ConnectionException or SftpConnectionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ExecuteException(ex.Message);
            }
        }
		public async Task<RemoveImageResult> RemoveImageTestAsync(int idTest)
		{
			await using var transaction = await _dbService.GetCurrentConnection().BeginTransactionAsync(IsolationLevel.RepeatableRead);
			try
			{
				var remotePath = await _dbService.GetPathImageTestAsync(idTest);

                var resultRemoveFromDb = await _dbService.RemoveImageTestAsync(idTest, transaction);
				if (remotePath == null || resultRemoveFromDb != RemoveImageResult.Success)
				{
                    transaction.Rollback();
					return resultRemoveFromDb;
                }

				var resultRemoveFromFileServer = await _fileService.RemoveFileAsync(remotePath);
				if (resultRemoveFromFileServer == RemoveFileResult.Unknown)
				{
                    transaction.Rollback();
					return RemoveImageResult.Unknown;
                }

				transaction.Commit();
                return (resultRemoveFromFileServer == RemoveFileResult.Success) ? RemoveImageResult.Success : RemoveImageResult.ImageNotExists;
            }
            catch (Exception ex) when (ex is ConnectionException or SftpConnectionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new ExecuteException(ex.Message);
            }
        }
		public async Task<LoadImageResult> UploadImageQuestionAsync(int idQuestion, string localPath)
		{
            await using var transaction = await _dbService.GetCurrentConnection().BeginTransactionAsync(IsolationLevel.RepeatableRead);
            try
            {
                var basePath = await SftpSettingsService.GetServerWorkDirectoryQuestion(_fileService);
                var remoteFilePath = basePath + idQuestion.ToString() + Path.GetExtension(localPath);

                var resultAddImageTest = await _dbService.AddImageQuestionAsync(idQuestion, remoteFilePath, transaction);
                if (resultAddImageTest != AddImageResult.Success)
                {
                    transaction.Rollback();
                    if (resultAddImageTest != AddImageResult.Unknown)
                        return (resultAddImageTest == AddImageResult.IdNotExists) ? LoadImageResult.IdNotExists : LoadImageResult.DestinationImageExists;
                    return LoadImageResult.Unknown;
                }

                var resultUploadFile = await _fileService.UploadFileAsync(localPath, remoteFilePath);
                if (resultUploadFile != LoadFileResult.Success)
                {
                    transaction.Rollback();
                    if (resultUploadFile != LoadFileResult.Unknown)
                        return (resultUploadFile == LoadFileResult.SourceFileNotExists) ? LoadImageResult.SourceImageNotExists : LoadImageResult.DestinationImageExists;
                    return LoadImageResult.Unknown;
                }

                transaction.Commit();
                return LoadImageResult.Success;
            }
            catch (Exception ex) when (ex is ConnectionException or SftpConnectionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new ExecuteException(ex.Message);
            }
        }
		public async Task<LoadImageResult> UploadImageTestAsync(int idTest, string localPath)
		{
			await using var transaction = await _dbService.GetCurrentConnection().BeginTransactionAsync(IsolationLevel.RepeatableRead);
			try
			{
				var basePath = await SftpSettingsService.GetServerWorkDirectoryTest(_fileService);
				var remoteFilePath = basePath + idTest.ToString() + Path.GetExtension(localPath);
				
				var resultAddImageTest = await _dbService.AddImageTestAsync(idTest, remoteFilePath, transaction);
				if (resultAddImageTest != AddImageResult.Success)
				{
					transaction.Rollback();
					if (resultAddImageTest != AddImageResult.Unknown)
						return (resultAddImageTest == AddImageResult.IdNotExists) ? LoadImageResult.IdNotExists : LoadImageResult.DestinationImageExists;
					return LoadImageResult.Unknown;
				}

				var resultUploadFile = await _fileService.UploadFileAsync(localPath, remoteFilePath);
				if (resultUploadFile != LoadFileResult.Success)
				{
					transaction.Rollback();
					if (resultUploadFile != LoadFileResult.Unknown)
						return (resultUploadFile == LoadFileResult.SourceFileNotExists) ? LoadImageResult.SourceImageNotExists : LoadImageResult.DestinationImageExists;
					return LoadImageResult.Unknown;
				}

				transaction.Commit();
				return LoadImageResult.Success;
			}
			catch (Exception ex) when (ex is ConnectionException or SftpConnectionException)
			{
				throw;
			}
			catch (Exception ex)
			{
				transaction.Rollback();
				throw new ExecuteException(ex.Message);
			}
		}
	}
}