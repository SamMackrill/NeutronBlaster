using System;
using System.Windows;

using NeutronBlaster.Models;

namespace NeutronBlaster
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string applicationName;

        private void Application_Start(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

            applicationName = typeof(App).Assembly.GetName().Name;

            var userProfilePath = Environment.GetEnvironmentVariable("USERPROFILE");
            if (string.IsNullOrWhiteSpace(Settings.Default.JournalFileLocation))
            {
                Settings.Default.JournalFileLocation = $@"{userProfilePath}\Saved Games\Frontier Developments\Elite Dangerous";
            }

            if (string.IsNullOrWhiteSpace(Settings.Default.RouteLocation))
            {
                Settings.Default.RouteLocation = $@"{userProfilePath}\Downloads";
            }

            var windowViewModel = new MainWindowViewModel();
            var window = new MainWindow
            {
                DataContext = windowViewModel
            };

            window.Show();

            windowViewModel.Begin();
        }

        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show((e.ExceptionObject as Exception)?.Message ?? "Unknown", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Settings.Default.Save();
        }
    }
}
