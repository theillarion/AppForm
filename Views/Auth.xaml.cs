using Xk7.Helper.Converts;
using Xk7.Helper.Exceptions;
using Xk7.Helper;
using Xk7.Model;

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
using MySql.Data.MySqlClient;
using Xk7.Helper.Enums;
using Xk7.Views;
using System.Globalization;

namespace Xk7.pages
{
    public partial class Auth : Page
    {
        private readonly IDbAsyncService _dbAsyncService;
        private readonly IFileService _fileService;
        private const string TitlePage = "Authentication";
        internal Auth(IDbAsyncService dbAsyncService, IFileService fileService)
        {
            InitializeComponent();
            _dbAsyncService = dbAsyncService;
            AuthExceptionTextBox.Visibility = Visibility.Hidden;
            _fileService = fileService; 
        }
        private void SetError(string? message)
        {
            AuthExceptionTextBox.Text = message ?? "Unknown error";
            AuthExceptionTextBox.Visibility = Visibility.Visible;
        }
        private void loginTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (loginTextBox.Text.Equals(UICultureService.GetProperty("TextInputLogin")))
            {
                loginTextBox.Text = string.Empty;
                loginTextBox.SetCurrentValue(ForegroundProperty, Brushes.Black);
            }
            // проверка, если при переходе в поле ввода логина, поле пароля так и не было заполнено
            if (passTextBox.Text.Trim() == string.Empty)
            {
                passTextBox.Text = UICultureService.GetProperty("TextInputPassword");
                passTextBox.SetCurrentValue(ForegroundProperty, Brushes.Gray);
            }
        }
        private void passTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (passTextBox.Text.Equals(UICultureService.GetProperty("TextInputPassword")))
            {
                passTextBox.Text = string.Empty;
                passTextBox.SetCurrentValue(ForegroundProperty, Brushes.Black);
            }
            // проверка, если при переходе в поле ввода пароля, поле логина так и не было заполнено
            if (loginTextBox.Text.Trim() == string.Empty)
            {
                loginTextBox.Text = UICultureService.GetProperty("TextInputLogin");
                loginTextBox.SetCurrentValue(ForegroundProperty, Brushes.Gray);
            }
        }
        private async void loginClick(object sender, RoutedEventArgs e)
        {
            var login = loginTextBox.Text.Trim();
            var password = passTextBox.Text.Trim();
            try
            {
                var row = await _dbAsyncService.GetDataUserByLoginAsync(login);
                if (row == null)
                    SetError(UICultureService.GetProperty("ErrorUserNotExists"));
                else
                {
                    var user = UserFactory.FromDataRow<DbUser>(row);
                    if (user == null)
                        SetError(UICultureService.GetProperty("UnknownError"));
                    else if (user.IsBlocked)
                    {
                        SetError(UICultureService.GetProperty("ErrorUserBlocked"));
                        await _dbAsyncService.AddLogAsync(login, LoggingType.FailedAuthenticationUserBanned);
                    }
                        
                    else if (Converts.ConvertByteArrayToString(user.HashPassword)
                             != Converts.ConvertStringToHeshString(password))
                    {
                        SetError(UICultureService.GetProperty("ErrorWrongPassword"));
                        await _dbAsyncService.AddLogAsync(login, LoggingType.FailedAuthenticationWrongPassword);
                    }
                    else
                    {
                        if (user.IdUserRole == (uint)UserRole.SuperAdmin)
                            App.MainFrame.Navigate(await AdminPanel.CreateAsync());
                        else if (user.IdUserRole < (uint)UserRole.SuperAdmin)
                            App.MainFrame.Navigate(new UserProfile(_dbAsyncService, _fileService, (DbUser)user));
                        else
                            MessageBox.Show("User has been successfully authorized", "Authentication", MessageBoxButton.OK, MessageBoxImage.Information);
                        await _dbAsyncService.AddLogAsync(login, LoggingType.SuccessAuthentication);
                    }
                }
            }
            catch (ConnectionException)
            {
                SetError(UICultureService.GetProperty("ExceptionConnectionRefused"));
            }
            catch (Exception ex) when (ex is ExecuteException or FactoryException)
            {
                SetError(UICultureService.GetProperty("ExceptionExecute"));
            }
            catch (Exception)
            {
                SetError(UICultureService.GetProperty("UnknownError"));
            }    
        }
        private void regButton_Click(object sender, RoutedEventArgs e)
        {
            App.MainFrame.Navigate(new Registration(_dbAsyncService, _fileService));
        }
        private void employeeLoginButton_Click(object sender, RoutedEventArgs e)
        {
            loginClick(sender, e);
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
    }
}
