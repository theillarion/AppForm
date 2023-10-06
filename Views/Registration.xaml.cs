using Xk7.Helper.Enums;
using Xk7.Helper.Exceptions;
using Xk7.Model;
using Xk7.Services;

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
using MySql.Data.MySqlClient;
using Xk7.Views;

namespace Xk7.pages
{
    public partial class Registration : Page
    {
        private readonly IDbAsyncService _dbAsyncService;
        private readonly IFileService _fileService;
        internal Registration(IDbAsyncService dbAsyncService, IFileService fileService)
        {
            InitializeComponent();
            _dbAsyncService = dbAsyncService;
            RegistrationExceptionTextBox.Visibility = Visibility.Hidden;
            _fileService = fileService; 
        }
        private void LoginTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBox regLoginTextBox = (TextBox) sender;
            if (regLoginTextBox.Text.Trim() != string.Empty)
            {
                regLoginTextBox.Text = string.Empty;
                regLoginTextBox.SetCurrentValue(ForegroundProperty, Brushes.Black);
            }
            RemoveEmptyFields("Login");
        }
        private void PassTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBox regPassTextBox = (TextBox)sender;
            if (regPassTextBox.Text.Trim() != string.Empty)
            {
                regPassTextBox.Text = string.Empty;
                regPassTextBox.SetCurrentValue(ForegroundProperty, Brushes.Black);
            }
            RemoveEmptyFields("Password");
        }
        private void NameTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBox regNameTextBox = (TextBox)sender;
            if (regNameTextBox.Text.Trim() != string.Empty)
            {
                regNameTextBox.Text = string.Empty;
                regNameTextBox.SetCurrentValue(ForegroundProperty, Brushes.Black);
            }
            RemoveEmptyFields("Name");
        }
        private void SecondNameTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBox regSecondNameTextBox = (TextBox)sender;
            if (regSecondNameTextBox.Text.Trim() != string.Empty)
            {
                regSecondNameTextBox.Text = string.Empty;
                regSecondNameTextBox.SetCurrentValue(ForegroundProperty, Brushes.Black);
            }
            RemoveEmptyFields("SecondName");
        }
        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            datePicker.SetCurrentValue(ForegroundProperty, Brushes.Black);
            RemoveEmptyFields("Date");
        }
        private void SetError(string? message)
        {
            RegistrationExceptionTextBox.Text = message ?? "Unknown error";
            RegistrationExceptionTextBox.Visibility = Visibility.Visible;
        }
        private async void regRegistrationClick(object sender, RoutedEventArgs e)
        {
            var login = LoginTextBox.Text.Trim();
            var password = PassTextBox.Text.Trim();
            var name = NameTextBox.Text.Trim();
            var secondName = SecondNameTextBox.Text.Trim();
            
            if (login == string.Empty || password == string.Empty || name == string.Empty || secondName == string.Empty || datePicker.SelectedDate == null)
            {
                SetError(UICultureService.GetProperty("RegistrationIncorrectData"));
                return;
            }
            var birth = DateOnly.FromDateTime((DateTime)datePicker.SelectedDate);

            try
            {
                var user = new User(UserRole.User, login, new HashedValue(password), name, secondName, birth, false);

                var result = await _dbAsyncService.AddUserAsync(user);
                switch (result)
                {
                    case AddUserResult.Success:
                        App.MainFrame.Navigate(new UserProfile(_dbAsyncService, _fileService, new DbUser(user)));
                        await _dbAsyncService.AddLogAsync(login, LoggingType.SuccessRegistration);
                        break;
                    case AddUserResult.UserExists:
                        SetError(UICultureService.GetProperty("ErrorUserExists"));
                        break;
                    default:
                        SetError(UICultureService.GetProperty("UnknownError"));
                        break;
                }
            }
            catch (ConnectionException ex)
            {
                SetError(UICultureService.GetProperty("ExceptionConnectionRefused"));
            }
            catch (ExecuteException)
            {
                SetError(UICultureService.GetProperty("ExceptionExecute"));
            }
            catch (Exception)
            {
                SetError(UICultureService.GetProperty("UnknownError"));
            }
        }
        private void regBackClick(object sender, RoutedEventArgs e)
        {
            App.MainFrame.Navigate(new Auth(_dbAsyncService, _fileService));
        }
        // TODO: Fix
        private void RemoveEmptyFields(string CurrentField)
        {
            switch (CurrentField)
            {
                case "Name":
                    if (SecondNameTextBox.Text.Trim() == string.Empty)
                    {
                        //SecondNameTextBox.Text = UICultureService.GetProperty("TextInputSecondName");
                        SecondNameTextBox.SetCurrentValue(ForegroundProperty, Brushes.Gray);
                    }
                    if (PassTextBox.Text.Trim() == string.Empty)
                    {
                        //PassTextBox.Text = UICultureService.GetProperty("TextInputPassword");
                        PassTextBox.SetCurrentValue(ForegroundProperty, Brushes.Gray);
                    }
                    if (LoginTextBox.Text.Trim() == string.Empty)
                    {
                        //LoginTextBox.Text = UICultureService.GetProperty("TextInputLogin");
                        LoginTextBox.SetCurrentValue(ForegroundProperty, Brushes.Gray);
                    }
                    break;

                case "SecondName":
                    if (NameTextBox.Text.Trim() == string.Empty)
                    {
                        //NameTextBox.Text = UICultureService.GetProperty("TextInputName");
                        NameTextBox.SetCurrentValue(ForegroundProperty, Brushes.Gray);
                    }
                    if (PassTextBox.Text.Trim() == string.Empty)
                    {
                        //PassTextBox.Text = UICultureService.GetProperty("TextInputPassword");
                        PassTextBox.SetCurrentValue(ForegroundProperty, Brushes.Gray);
                    }
                    if (LoginTextBox.Text.Trim() == string.Empty)
                    {
                        //LoginTextBox.Text = UICultureService.GetProperty("TextInputLogin");
                        LoginTextBox.SetCurrentValue(ForegroundProperty, Brushes.Gray);
                    }
                    break;
                case "Login":
                    if (NameTextBox.Text.Trim() == string.Empty)
                    {
                        //NameTextBox.Text = UICultureService.GetProperty("TextInputName");
                        NameTextBox.SetCurrentValue(ForegroundProperty, Brushes.Gray);
                    }

                    if (SecondNameTextBox.Text.Trim() == string.Empty)
                    {
                        //SecondNameTextBox.Text = UICultureService.GetProperty("TextInputSecondName");
                        SecondNameTextBox.SetCurrentValue(ForegroundProperty, Brushes.Gray);
                    }
                    if (PassTextBox.Text.Trim() == string.Empty)
                    {
                        //PassTextBox.Text = UICultureService.GetProperty("TextInputPassword");
                        PassTextBox.SetCurrentValue(ForegroundProperty, Brushes.Gray);
                    }
                    break;
                case "Password":
                    if (NameTextBox.Text.Trim() == string.Empty)
                    {
                        //NameTextBox.Text = UICultureService.GetProperty("TextInputName");
                        NameTextBox.SetCurrentValue(ForegroundProperty, Brushes.Gray);
                    }
                    if (SecondNameTextBox.Text.Trim() == string.Empty)
                    {
                        //SecondNameTextBox.Text = UICultureService.GetProperty("TextInputSecondName");
                        SecondNameTextBox.SetCurrentValue(ForegroundProperty, Brushes.Gray);
                    }
                    if (LoginTextBox.Text.Trim() == string.Empty)
                    {
                        //LoginTextBox.Text = UICultureService.GetProperty("TextInputLogin");
                        LoginTextBox.SetCurrentValue(ForegroundProperty, Brushes.Gray);
                    }
                    break;
                case "Date":
                    if (NameTextBox.Text.Trim() == string.Empty)
                    {
                        //NameTextBox.Text = UICultureService.GetProperty("TextInputName");
                        NameTextBox.SetCurrentValue(ForegroundProperty, Brushes.Gray);
                    }
                    if (SecondNameTextBox.Text.Trim() == string.Empty)
                    {
                        //SecondNameTextBox.Text = UICultureService.GetProperty("TextInputSecondName");
                        SecondNameTextBox.SetCurrentValue(ForegroundProperty, Brushes.Gray);
                    }
                    if (LoginTextBox.Text.Trim() == string.Empty)
                    {
                        //LoginTextBox.Text = UICultureService.GetProperty("TextInputLogin");
                        LoginTextBox.SetCurrentValue(ForegroundProperty, Brushes.Gray);
                    }
                    if (PassTextBox.Text.Trim() == string.Empty)
                    {
                        //PassTextBox.Text = UICultureService.GetProperty("TextInputPassword");
                        PassTextBox.SetCurrentValue(ForegroundProperty, Brushes.Gray);
                    }
                    break;
            }
        }
    }
}
