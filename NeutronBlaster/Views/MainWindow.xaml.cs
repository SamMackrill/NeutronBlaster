using System.Windows;
using System.Windows.Input;

namespace NeutronBlaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
       
        private void CurrentSystem_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var context = DataContext as MainWindowViewModel;
            context?.SetClipboard(context.CurrentSystem);
        }

        private void LastSystemOnRoute_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var context = DataContext as MainWindowViewModel;
            context?.SetClipboard(context.LastSystemOnRoute);
        }

        private void TargetSystem_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var context = DataContext as MainWindowViewModel;
            context?.SetClipboard(context.TargetSystem);
        }
    }
}
