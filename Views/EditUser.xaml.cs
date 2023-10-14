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

using Xk7.Helper.Enums;
using Xk7.Model;
using Xk7.Services;

namespace Xk7.Views
{
    /// <summary>
    /// Логика взаимодействия для EditUser.xaml
    /// </summary>
    public partial class EditUser : Page
    {
        private readonly IDbAsyncService _dbAsyncService;
        private DbUser _user;
        private const string TitlePage = "EditUser";
        internal EditUser(IDbAsyncService dbAsyncService, DbUser user)
        {
            InitializeComponent();
            _dbAsyncService = dbAsyncService; 
            _user = user; 
            UserRoleTextBox.Text = user.IdUserRole.ToString();
            LoginTextBox.Text = user.Login;
            FirstNameTextBox.Text = user.FirstName;
            SecondNameTextBox.Text = user.SecondName;
            DateBirthTextBox.Text = user.DateBirthdayString;
        }
        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            DbUser EditedUser = (DbUser)_user.Clone();

            EditedUser.Login = LoginTextBox.Text;
            EditedUser.IdUserRole = uint.Parse(UserRoleTextBox.Text);
            EditedUser.FirstName = FirstNameTextBox.Text;
            EditedUser.SecondName = SecondNameTextBox.Text;
            EditedUser.DateBirthdayString = DateBirthTextBox.Text;


            if (!EditedUser.AllFieldsFilled())
            {
                MessageBox.Show(UICultureService.GetProperty("ErrorSomeFieldsBlank"), "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var result = await _dbAsyncService.UpdateUserByLoginAsync(_user.Login, EditedUser);
                if (result == UpdateUserResult.Success)
                    App.MainFrame.Navigate(await AdminPanel.CreateAsync());
                else
                    MessageBox.Show(EnumStrings.UpdateUserResult[(uint)result], "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.MainFrame.Navigate(await AdminPanel.CreateAsync());
        }
    }
}
