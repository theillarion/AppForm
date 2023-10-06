using MySql.Data.MySqlClient;

using MySqlX.XDevAPI.Common;
using MySqlX.XDevAPI.Relational;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
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

using Xk7.Helper;
using Xk7.Helper.Enums;
using Xk7.Helper.Exceptions;
using Xk7.Model;
using Xk7.pages;
using Xk7.Services;

namespace Xk7.Views
{
    /// <summary>
    /// Логика взаимодействия для AdminPanel.xaml
    /// </summary>
    /// 
    
    public partial class AdminPanel : Page
    {
        private readonly IDbAsyncService _dbService;
        private readonly IFileService _fileService;
        private const string TitlePage = "AdminPanel";
        public AdminPanel()
        {
            InitializeComponent();
            var dbService = App.ConfigureDefaultDbService(App.FatalError);
            var fileService = App.ConfigureDefaultFileService(App.FatalError);
            if (dbService == null || fileService == null)
                App.FatalError(null);
            else
            {
                _dbService = dbService;
                _fileService = fileService;
            }
        }
        public static async Task<AdminPanel> CreateAsync()
        {
            var adminPanel = new AdminPanel();

            //TODO: DELETE!!! (FOR DEBUG)
            try
            {
                var result = await adminPanel._dbService.AddTestWithImageAsync(adminPanel._fileService, new DbTest(1, "Test", "Bla bla bla", DateTime.UtcNow, "admin", true), "test_picture.jpg");
                MessageBox.Show(Enum.GetName(typeof(AddTestImageResult), result), "Result", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            //END BLOCK

            var tableUser = await adminPanel._dbService.GetTableAsync("User");
            var collectionUsers = new ObservableCollection<DbUser>();

            if (tableUser != null)
                foreach (DataRow row in tableUser.Rows)
                {
                    var rowUser = Factory.FromDataRow<DbUser>(row);
                    if (rowUser != null)
                        collectionUsers.Add(rowUser);
                }
            adminPanel.dbTable.ItemsSource = collectionUsers;

            return adminPanel;
        }

        private void ChangeLanguageClick(object sender, RoutedEventArgs e)
        {
            if (App.language.Equals("ru"))
            {
                App.language = "en";
                UICultureService.SetCulture(new CultureInfo(App.language));
            }
            else
            {
                App.language = "ru";
                UICultureService.SetCulture(new CultureInfo(App.language));
            }
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            App.MainFrame.Navigate(new Auth(_dbService, _fileService));
        }

        private void AdminPanelEditButton(object sender, RoutedEventArgs e)
        {
            var user = dbTable.SelectedItem as DbUser;         
            if (user is null) 
                return;
            else
            {
                App.MainFrame.Navigate(new EditUser(_dbService, user));
            }    
        }



    }
}
