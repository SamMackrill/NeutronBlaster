using System;
using System.Windows;
using System.Windows.Controls.Primitives;
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

        private void CloseButtonRectangle_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
       
        private void CurrentSystem_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var context = this.DataContext as MainWindowViewModel;
            if (context == null) return;
            context.SetClipboard(context.CurrentSystem);
        }
        private void LastSystemOnRoute_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var context = this.DataContext as MainWindowViewModel;
            if (context == null) return;
            context.SetClipboard(context.LastSystemOnRoute);
        }
        private void TargetSystem_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var context = this.DataContext as MainWindowViewModel;
            if (context == null) return;
            context.SetClipboard(context.TargetSystem);
        }
    }
}
