using BiblioGest.Data;
using System.Windows;

namespace BiblioGest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Initialize database
            await DatabaseInitializer.InitializeAsync();
            
            // Configure global exception handling
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            LogException(ex, "AppDomain.CurrentDomain.UnhandledException");
            
            MessageBox.Show($"An unhandled exception occurred: {ex?.Message}\n\nThe application will now close.", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception, "Application.Current.DispatcherUnhandledException");
            
            MessageBox.Show($"An unhandled exception occurred: {e.Exception.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            
            e.Handled = true;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogException(e.Exception, "TaskScheduler.UnobservedTaskException");
            e.SetObserved();
        }

        private void LogException(Exception ex, string source)
        {
            // In a real app, you'd implement proper logging
            // For now, we'll just print to debug console
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] Exception in {source}: {ex?.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex?.StackTrace}");
        }
    }
}