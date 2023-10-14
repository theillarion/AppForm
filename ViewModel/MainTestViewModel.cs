using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xk7.Model;
using Xk7.Services;

namespace Xk7.ViewModel
{
    internal class MainTestViewModel : ViewModelBase
    {
        private readonly TestManager _testManager;
        private readonly ObservableCollection<TestViewModel> _tests;
        public IEnumerable<TestViewModel> Tests => _tests;
        public MainTestViewModel(IDbAsyncService dbAsyncService, IFileService fileService)
        {
            _testManager = new(dbAsyncService, fileService);
            _tests = new ObservableCollection<TestViewModel>();
        }
        public static async Task<MainTestViewModel> CreateAsync(IDbAsyncService dbAsyncService, IFileService fileService)
        {
            MainTestViewModel viewResult = new(dbAsyncService, fileService);
            var result = await viewResult._testManager.GetAllTestAsync();
            foreach (var elem in result)
                viewResult._tests.Add(new TestViewModel(elem.Tittle, elem.Description, elem.LocalPath));

            return viewResult;
        }
    }
}