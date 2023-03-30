using System;
using System.Windows;
using System.Windows.Threading;

namespace MFVideoDeviceEnumeratorWpfApp
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            PerformCleanup();
        }

        private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            PerformCleanup();
        }

        private void PerformCleanup()
        {
            var mainWindow = (MainWindow)MainWindow;
            (mainWindow?.DataContext as MainWindowViewModel)?.Dispose();
        }
    }
}