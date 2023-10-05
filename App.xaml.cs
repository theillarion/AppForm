using MySql.Data.MySqlClient;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Mysqlx.Notice;
using Xk7.Helper.Exceptions;
using Xk7.pages;
using Xk7.Services;

namespace Xk7
{
    public partial class App : Application
    {
        internal readonly IDbAsyncService _dbAsyncService;
        internal static readonly NavigationWindow MainFrame = new();
        internal static readonly DbAsyncService? dbService;
        internal static string language = "en";
        internal delegate void Error(string? message);
        internal App()
        {
            MainFrame.ShowsNavigationUI = false;
            //TODO: fix
            MainFrame.Width = 1248;
            MainFrame.Height = 702;
            //
            UICultureService.SetCulture(new CultureInfo(language));
            var dbService = ConfigureDefaultDbService(FatalError);
            if (dbService != null)
                MainFrame.Navigate(new Auth(dbService));
            else
                FatalError(null);
            MainFrame.Show();
        }
        internal static void FatalError(string? message)
        {
            MessageBox.Show(message ?? "Unknown error.", "Fatal error", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(-1);
        }
        internal static DbAsyncService? ConfigureDefaultDbService(Error error)
        {
            if (!DbSettingsService.DbSettingsFileExists())
                error.Invoke(UICultureService.GetProperty("ErrorNotFoundConfig"));

            var settings = DbSettingsService.LoadDbSettings();
            try
            {
                return new DbAsyncService(new MySqlConnection(settings.ConnectionString));
            }
            catch (ConnectionException)
            {
                error.Invoke(UICultureService.GetProperty("ExceptionConnectionRefused"));
            }
            return null;
        }
    }
}