using System;
using System.Threading.Tasks;
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
        private MainWindowViewModel context;
        private Task update = Task.FromResult(true);

        private async void Application_Start(object sender, StartupEventArgs e)
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

            context = new MainWindowViewModel();
            var window = new MainWindow
            {
                DataContext = context
            };

            window.Show();

            context.Begin();

            try
            {
                update = context.CheckForUpdates(e.Args);
                await update;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show((e.ExceptionObject as Exception)?.Message ?? "Unknown", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private async void Application_Exit(object sender, ExitEventArgs e)
        {
            Settings.Default.Save();
            await update.ContinueWith(ex => { });
            context?.Dispose();
        }
    }
}
