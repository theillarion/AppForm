using Xk7.Views;
using Xk7.ViewModel;
using Xk7.Helper.Enums;
using Xk7.Helper.Exceptions;
using Xk7.Model;
using Xk7.Services;

using System;
using System.Windows;
using System.Windows.Controls;

namespace Xk7.pages
{
    public partial class Registration : Page
    {
        private readonly IDbAsyncService _dbAsyncService;
        private readonly IFileService _fileService;
        private readonly RegistrationViewModel _viewModel;
        internal Registration(IDbAsyncService dbAsyncService, IFileService fileService)
        {
            InitializeComponent();
            _dbAsyncService = dbAsyncService;
            RegistrationExceptionTextBox.Visibility = Visibility.Hidden;
            _fileService = fileService;

            _viewModel = new RegistrationViewModel(new DbUser());
            DataContext = _viewModel;
        }
        private void SetError(string? message)
        {
            RegistrationExceptionTextBox.Text = message ?? "Unknown error";
            RegistrationExceptionTextBox.Visibility = Visibility.Visible;
        }
        private void FirstNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Text == string.Empty)
                _viewModel.ErrorFirstName = UICultureService.GetProperty("ErrorIncorrectFirstName") ?? "Unknown error";
            else
                _viewModel.ErrorFirstName = string.Empty;
        }
        private void SecondNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Text == string.Empty)
                _viewModel.ErrorSecondName = UICultureService.GetProperty("ErrorIncorrectSecondName") ?? "Unknown error";
            else
                _viewModel.ErrorSecondName = string.Empty;
        }
        private async void LoginTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = sender as TextBox;
                
                if (textBox != null && textBox.Text == string.Empty)
                    _viewModel.ErrorLogin = UICultureService.GetProperty("ErrorIncorrectLogin") ?? "Unknown error";
                else if (textBox != null && await _dbAsyncService.ExistsUserAsync(textBox.Text))
                    _viewModel.ErrorLogin = UICultureService.GetProperty("ErrorUserExists") ?? "Unknown error";
                else
                    _viewModel.ErrorLogin = string.Empty;
            }
            catch (ConnectionException)
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
        private void PasswordTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Text == string.Empty)
                _viewModel.ErrorPassword = UICultureService.GetProperty("ErrorIncorrectPassword") ?? "Unknown error";
            else
                _viewModel.ErrorPassword = string.Empty;
        }
        private void DateBirthdayTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is DatePicker dataPicker && dataPicker.SelectedDate != null)
                {
                    var result = DateOnly.Parse(DateOnly.FromDateTime(dataPicker.SelectedDate.Value).ToString()).ToString("yyyy-MM-dd");
                    _viewModel.ErrorDateBirthday = string.Empty;
                }
                else
                    _viewModel.ErrorDateBirthday = UICultureService.GetProperty("ErrorIncorrectDateBirthday") ?? "Unknown error";
            }
            catch (FormatException)
            {
                _viewModel.ErrorDateBirthday = UICultureService.GetProperty("ErrorIncorrectDateBirthday") ?? "Unknown error";
            }
        }
        private async void regRegistrationClick(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.AllCorrect())
            {
                SetError(UICultureService.GetProperty("RegistrationIncorrectData"));
                return;
            }
            try
            {
                var result = await _dbAsyncService.AddUserAsync(_viewModel.User);
                switch (result)
                {
                    case AddUserResult.Success:
                        App.MainFrame.Navigate(new UserProfile(_dbAsyncService, _fileService, _viewModel.User));
                        await _dbAsyncService.AddLogAsync(_viewModel.User.Login, LoggingType.SuccessRegistration);
                        break;
                    case AddUserResult.UserExists:
                        SetError(UICultureService.GetProperty("ErrorUserExists"));
                        break;
                    default:
                        SetError(UICultureService.GetProperty("UnknownError"));
                        break;
                }
            }
            catch (ConnectionException)
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
    }
}