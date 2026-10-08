using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using VivaCSharp.Services;

namespace VivaCSharp
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                Logger.Initialize();

                Logger.Log("========================================");
                Logger.Log("VivaC# starting.");
                Logger.Log("App constructor started.");
                Logger.Log("========================================");

                DispatcherUnhandledException +=
                    App_DispatcherUnhandledException;

                AppDomain.CurrentDomain.UnhandledException +=
                    CurrentDomain_UnhandledException;

                TaskScheduler.UnobservedTaskException +=
                    TaskScheduler_UnobservedTaskException;

                Logger.Log(
                    "Crash handlers registered.");
            }
            catch (Exception ex)
            {
                try
                {
                    Logger.Error(
                        "App constructor failure",
                        ex);
                }
                catch
                {
                }
            }
        }

        protected override void OnStartup(
            StartupEventArgs e)
        {
            try
            {
                Logger.Log(
                    "OnStartup entered.");

                Logger.Log(
                    "StartupUri = MainWindow.xaml");

                base.OnStartup(e);

                Logger.Log(
                    "base.OnStartup completed.");
            }
            catch (Exception ex)
            {
                HandleException(
                    "Application Startup Failure",
                    ex);

                Shutdown(1);
            }
        }

        private void App_DispatcherUnhandledException(
            object sender,
            DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(
                "WPF Dispatcher Unhandled Exception",
                e.Exception);

            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(
            object sender,
            UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                HandleException(
                    "AppDomain Unhandled Exception",
                    exception);
            }
        }

        private void TaskScheduler_UnobservedTaskException(
            object? sender,
            UnobservedTaskExceptionEventArgs e)
        {
            HandleException(
                "Unobserved Task Exception",
                e.Exception);

            e.SetObserved();
        }

        private void HandleException(
            string type,
            Exception exception)
        {
            try
            {
                Logger.Error(
                    type,
                    exception);
            }
            catch
            {
            }

            MessageBox.Show(
                BuildExceptionDetails(exception) +
                "\n\nFull report saved to:" +
                "\nCrashLogs\\Errors",
                "VivaC# - ERROR",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private static string BuildExceptionDetails(
            Exception exception)
        {
            var builder =
                new StringBuilder();

            Exception? current = exception;

            int level = 0;

            while (current != null)
            {
                if (level == 0)
                {
                    builder.AppendLine(
                        "Exception:");

                    builder.AppendLine(
                        current.GetType().FullName);

                    builder.AppendLine();

                    builder.AppendLine(
                        "Message:");

                    builder.AppendLine(
                        current.Message);
                }
                else
                {
                    builder.AppendLine();

                    builder.AppendLine(
                        $"InnerException {level}:");

                    builder.AppendLine(
                        current.GetType().FullName);

                    builder.AppendLine();

                    builder.AppendLine(
                        current.Message);
                }

                current = current.InnerException;
                level++;
            }

            return builder.ToString();
        }
    }
}
