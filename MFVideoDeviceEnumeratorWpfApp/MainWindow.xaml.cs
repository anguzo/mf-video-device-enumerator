using System.Windows;

namespace MFVideoDeviceEnumeratorWpfApp
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindow()
        {
            DataContext = _viewModel = new MainWindowViewModel();
            InitializeComponent();
            Application.Current.Exit += OnApplicationExit;
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            _viewModel.Dispose();
        }
    }
}
