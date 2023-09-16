using DriveFileSearcher.Helpers;
using DriveFileSearcher.VM;
using System.Windows;

namespace DriveFileSearcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ViewModel vm;

        public MainWindow() 
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            vm = new ViewModel();
            DataContext = vm;
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await DispatcherHelper.RunAsync(() => vm.ListDrives());
        }
    }
}
