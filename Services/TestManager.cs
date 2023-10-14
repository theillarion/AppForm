using MySqlX.XDevAPI.Common;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

using Xk7.Helper;
using Xk7.Helper.Enums;
using Xk7.Helper.Exceptions;
using Xk7.Model;

namespace Xk7.Services
{
    internal class TestManager
    {
        private readonly IDbAsyncService _dbService;
        private readonly IFileService _fileService;
        private readonly ImageManager _imageManager;
        private SortedSet<IDbTestWithImage> _cacheTests;
        private SortedSet<IDbQuestionWithImage> _cacheQuestions;
        private SortedSet<IDbAnswer> _cacheAnswers;
        private static readonly string DirectoryForTest = Path.Combine(Directory.GetCurrentDirectory(), "tests");
        private static readonly string DirectoryForQuestion = Path.Combine(Directory.GetCurrentDirectory(), "questions");

        public TestManager(IDbAsyncService dbService, IFileService fileService)
        {
            _dbService = dbService;
            _fileService = fileService;
            _imageManager = new(dbService, fileService);
            _cacheTests = new();
            _cacheQuestions = new();
            _cacheAnswers = new();
        }
        private static string GetWorkDirectoryForTests()
        {
            if (!Directory.Exists(DirectoryForTest))
                Directory.CreateDirectory(DirectoryForTest);
            return DirectoryForTest;
        }
        private static string GetWorkDirectoryForQuestions()
        {
            if (!Directory.Exists(DirectoryForQuestion))
                Directory.CreateDirectory(DirectoryForQuestion);
            return DirectoryForQuestion;
        }
        private async Task<KeyValuePair<int, LoadImageResult>> WrapperDownloadTestAsync(int id, string localPath)
        {
            return new(id, await _imageManager.DownloadImageTestAsync(id, localPath));
        }
        private async Task<KeyValuePair<int, LoadImageResult>> WrapperDownloadQuestionAsync(int id, string localPath)
        {
            return new(id, await _imageManager.DownloadImageQuestionAsync(id, localPath));
        }
        private async Task InitialCacheTestsAsync()
        {
            try
            {
                _cacheTests = new();
                var tableTests = await _dbService.GetAllTestsWithImage();
                var listTasksDownload = new List<Task<KeyValuePair<int, LoadImageResult>>>();
                if (tableTests == null)
                    return;

                foreach (DataRow row in tableTests.Rows)
                {
                    var result = Factory.FromDataRow<DbTestWithImage>(row);
                    if (result != null)
                    {
                        if (result.HasImage && result.Path != string.Empty)
                        {
                            result.LocalPath = Path.Combine(GetWorkDirectoryForTests(), result.Id + Path.GetExtension(result.Path));
                            listTasksDownload.Add(WrapperDownloadTestAsync(result.Id, result.LocalPath));
                        }
                        _cacheTests.Add(result);
                    }
                }

                while (listTasksDownload.Count > 0)
                {
                    var task = await Task.WhenAny(listTasksDownload.ToArray());
                    listTasksDownload.Remove(task);
                    if (task.Result.Value == LoadImageResult.IdNotExists)
                        _cacheTests.RemoveWhere(x => x.Id == task.Result.Key);
                    else if (task.Result.Value == LoadImageResult.SourceImageNotExists || task.Result.Value == LoadImageResult.Unknown)
                    {
                        var resultTest = _cacheTests.First(x => x.Id == task.Result.Key);
                        if (resultTest != null)
                            resultTest.LocalPath = string.Empty;
                    }
                }
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
        public async Task<IReadOnlyCollection<IDbTestWithImage>> GetAllTestAsync()
        {
            if (_cacheTests == null || _cacheTests.Count == 0)
                await InitialCacheTestsAsync();
            return _cacheTests.ToList().AsReadOnly();
        }
        private async Task LoadQuestionsAsync(int idTest)
        {
            try
            {
                if (_cacheQuestions == null)
                    _cacheQuestions = new();
                var tableQuestion = await _dbService.GetQuestionsWithImageByIdTest(idTest);
                var listTasksDownload = new List<Task<KeyValuePair<int, LoadImageResult>>>();

                if (tableQuestion == null)
                    return;

                foreach (DataRow row in tableQuestion.Rows)
                {
                    var result = Factory.FromDataRow<DbQuestionWithImage>(row);
                    if (result != null)
                    {
                        if (result.HasImage && result.Path != string.Empty)
                        {
                            result.LocalPath = Path.Combine(GetWorkDirectoryForQuestions(), result.Id + Path.GetExtension(result.Path));
                            listTasksDownload.Add(WrapperDownloadQuestionAsync(result.Id, result.LocalPath));
                        }
                        _cacheQuestions.Add(result);
                    }
                }

                while (listTasksDownload.Count > 0)
                {
                    var task = await Task.WhenAny(listTasksDownload.ToArray());
                    listTasksDownload.Remove(task);
                    if (task.Result.Value == LoadImageResult.IdNotExists)
                        _cacheQuestions.RemoveWhere(x => x.Id == task.Result.Key);
                    else if (task.Result.Value == LoadImageResult.SourceImageNotExists || task.Result.Value == LoadImageResult.Unknown)
                    {
                        var resultQuestion = _cacheQuestions.First(x => x.Id == task.Result.Key);
                        if (resultQuestion != null)
                            resultQuestion.LocalPath = string.Empty;
                    }
                }
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
        public async Task<IReadOnlyCollection<IDbQuestionWithImage>> GetQuestionsByIdTestAsync(int idTest)
        {
            if (!_cacheQuestions.Any(x => x.IdTest == idTest))
                await LoadQuestionsAsync(idTest);
            return _cacheQuestions.Where(x => x.IdTest == idTest).ToList().AsReadOnly();
        }
        private async Task LoadAnswersAsync(int idQuestion)
        {
            try
            {
                if (_cacheQuestions == null)
                    _cacheQuestions = new();
                var tableTests = await _dbService.GetAnswersByIdQuestion(idQuestion);
                if (tableTests == null)
                    return;

                foreach (DataRow row in tableTests.Rows)
                {
                    var result = Factory.FromDataRow<DbSimpleAnswer>(row);
                    if (result != null && result.AllFieldsFilled())
                        _cacheAnswers.Add(result);
                }
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
        public async Task<IReadOnlyCollection<IDbAnswer>> GetAnswersByIdQuestionAsync(int idQuestion)
        {
            if (!_cacheAnswers.Any(x => x.IdQuestion == idQuestion))
                await LoadAnswersAsync(idQuestion);
            return _cacheAnswers.Where(x => x.IdQuestion == idQuestion).ToList().AsReadOnly();
        }
    }
}