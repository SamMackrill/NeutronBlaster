using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NeutronBlaster
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static string applicationName;

        private void Application_Start(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

            applicationName = typeof(App).Assembly.GetName().Name;

            var window = new MainWindow
            {
                //DataContext = null
            };

            window.Show();

            try
            {
                var destinationSetter = new DestinationSetter(@"C:\Users\Sam\Downloads");
                var watcher = new LocationWatcher(@"C:\Users\Sam\Saved Games\Frontier Developments\Elite Dangerous");
                destinationSetter.SetClipWhenLocationChanges(watcher);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show((e.ExceptionObject as Exception)?.Message ?? "Unknown", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            //Settings.Default.Save();
        }
    }
}
