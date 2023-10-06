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
        internal static IDbAsyncService _dbAsyncService;
        internal static IFileService _fileAsyncService;
        internal static bool _hasInstanceDbservice = false;
        internal static bool _hasInstanceFileservice = false;
        internal static readonly NavigationWindow MainFrame = new();
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
            var fileService = ConfigureDefaultFileService(FatalError);

            if (dbService != null && fileService != null)
            {
                MainFrame.Navigate(new Auth(dbService, fileService));
            }
            else
                FatalError(null);
            MainFrame.Show();
        }
        internal static void FatalError(string? message)
        {
            MessageBox.Show(message ?? "Unknown error.", "Fatal error", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(-1);
        }
        internal static IDbAsyncService ConfigureDefaultDbService(Error error)
        {
            if (!_hasInstanceDbservice)
            {
                if (!DbSettingsService.DbSettingsFileExists())
                    error.Invoke(UICultureService.GetProperty("ErrorNotFoundConfig"));

                var settings = DbSettingsService.LoadDbSettings();
                try
                {
                    _dbAsyncService = new DbAsyncService(new MySqlConnection(settings.ConnectionString));
                    _hasInstanceDbservice = true;
                }
                catch (ConnectionException)
                {
                    error.Invoke(UICultureService.GetProperty("ExceptionConnectionRefused"));
                }
            }
            return _dbAsyncService;
        }
        internal static IFileService ConfigureDefaultFileService(Error error)
        {
            if (!_hasInstanceFileservice)
            {
                if (!SftpSettingsService.FileExists())
                    error.Invoke(UICultureService.GetProperty("ErrorNotFoundConfig"));

                var settings = SftpSettingsService.LoadSettings();
                if (settings == null)
                    error.Invoke(UICultureService.GetProperty("ErrorIncorrectFileConfiguration"));
                try
                {
                    _fileAsyncService = new SftpService(settings);
                    _hasInstanceFileservice = true;
                }
                catch (ConnectionException)
                {
                    error.Invoke(UICultureService.GetProperty("ExceptionConnectionRefused"));
                }
            }
            return _fileAsyncService;
        }
    }
}