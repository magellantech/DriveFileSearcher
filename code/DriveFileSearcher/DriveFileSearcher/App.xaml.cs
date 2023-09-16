using DriveFileSearcher.Helpers;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows;
using System;
using NLog;

namespace DriveFileSearcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ILogger _logger;

        public App()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += UnobservedTaskUnhandledException;
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
            Dispatcher.CurrentDispatcher.UnhandledExceptionFilter += DispatcherUnhandledExceptionFilter;
        }

        private void DispatcherUnhandledExceptionFilter(object? sender, DispatcherUnhandledExceptionFilterEventArgs e)
        {
            _logger.Log(LogLevel.Fatal, e.Exception, StrConsts.UnhandledException);
        }

        private void OnDispatcherUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Log(LogLevel.Fatal, e.Exception, StrConsts.UnhandledException);
        }

        private void UnobservedTaskUnhandledException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            _logger.Log(LogLevel.Fatal, e.Exception, StrConsts.UnhandledException);
        }

        private void CurrentDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            _logger.Log(LogLevel.Fatal, StrConsts.UnhandledException);
        }
    }
}
