using MySql.Data.MySqlClient;
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
        private const string TitlePage = "AdminPanel";
        public AdminPanel()
        {
            InitializeComponent();
            var dbService = App.ConfigureDefaultDbService(App.FatalError);
            if (dbService == null)
                App.FatalError(null);
            else
                _dbService = dbService;
        }
        public static async Task<AdminPanel> CreateAsync()
        {
            var Result = new AdminPanel();
            var Table = await Result._dbService.GetTable("User");
            var test = new ObservableCollection<DbUser>();
            foreach (DataRow row in Table.Rows)
            {
                //Factory.FromDataRow<DbUser>
                var obj = new DbUser()
                {
                    IdUserRole   = (uint) row.ItemArray[0],
                    Login        = (string) row.ItemArray[1],
                    HashPassword = (byte[]) row.ItemArray[2],
                    FirstName    = (string) row.ItemArray[3],
                    SecondName   = (string) row.ItemArray[4],
                    DateBirthday = (DateTime) row.ItemArray[5],
                    IsBlocked    = (bool) row.ItemArray[6]
                };
                test.Add(obj);
            }
            Result.dbTable.ItemsSource = test;

            return Result;
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
            App.MainFrame.Navigate(new Auth(_dbService));
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
