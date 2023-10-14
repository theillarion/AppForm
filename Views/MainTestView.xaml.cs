using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Xk7.Services;
using Xk7.ViewModel;

namespace Xk7.Views
{
    /// <summary>
    /// Логика взаимодействия для MainTestView.xaml
    /// </summary>
    public partial class MainTestView : Page
    {
        internal MainTestView(IDbAsyncService dbService, IFileService fileService)
        {
            InitializeComponent();

            DataContext = new MainTestViewModel(dbService, fileService);
        }
        internal static async Task<MainTestView> CreateAsync(IDbAsyncService dbService, IFileService fileService)
        {
            return new MainTestView(dbService, fileService)
            {
                DataContext = await MainTestViewModel.CreateAsync(dbService, fileService)
            };
        }
    }
}