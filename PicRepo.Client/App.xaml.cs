using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.EntityFrameworkCore;
using NLog;
using Notification.Core;
using Notification.Wpf;
using PicRepo.Client.Data;
using PicRepo.Client.Helper;
using PicRepo.Client.ViewModels;
using PicRepo.Client.Views;
using Splat;
using System.Windows;
using System.Windows.Threading;

namespace PicRepo.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Prism.Unity.PrismApplication
    {
        private static TaskbarIcon trayIcon;

        public App()
        {
            GetUnhandledException();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<AppDbContext>(factory =>
            {
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlite("Data Source=app.db")
                    .Options;
                return new AppDbContext(options);
            });

            containerRegistry.RegisterSingleton<IAppSettings, AppSettings>();
            containerRegistry.RegisterSingleton<TrayViewModel>();
            containerRegistry.RegisterSingleton<WinMini>();
            containerRegistry.RegisterSingleton<WinMiniViewModel>();
        }

        protected override Window CreateShell()
        {
            using var db = Container.Resolve<AppDbContext>();
            var isDatabaseExists = db.Database.CanConnect();

            if (!isDatabaseExists)
            {
                db.Database.Migrate();
            }
            var settings = Container.Resolve<IAppSettings>();
            settings.LoadConfig();
            ThemeMode = settings.Theme switch
            {
                "Light" => ThemeMode.Light,
                "Dark" => ThemeMode.Dark,
                _ => ThemeMode.System
            };
            trayIcon = (TaskbarIcon)FindResource("TrayIcon");
            trayIcon.DataContext = Container.Resolve<TrayViewModel>();
            return Container.Resolve<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            trayIcon?.Dispose();
            base.OnExit(e);
        }

        #region
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static NotificationManager? _notificationManager;

        public static NotificationManager NotificationManager
        {
            get
            {
                if (_notificationManager == null)
                {
                    _notificationManager = new NotificationManager();
                }

                return _notificationManager;
            }
        }

        public static void ShowNotification(string title, string message, NotificationType type = NotificationType.Information, string areaName = "", TimeSpan? timeSpan = null)
        {
            NotificationManager.Show(title, message, type, areaName, timeSpan);
        }

        public static void ShowMessage(string message, TimeSpan? timeSpan = null)
        {
            ShowNotification("提示", message, NotificationType.Information, "", timeSpan);
        }

        public static void ShowError(string message, TimeSpan? timeSpan = null)
        {
            ShowNotification("错误", message, NotificationType.Error, "", timeSpan);
        }

        public static void ShowNotification(string title, string message, BalloonIcon type = BalloonIcon.Info)
        {
            trayIcon?.ShowBalloonTip(title, message, type);
        }

        public static void ShowMessage(string message)
        {
            trayIcon?.ShowBalloonTip("提示", message, BalloonIcon.Info);
        }

        public static void ShowError(string message)
        {
            trayIcon?.ShowBalloonTip("错误", message, BalloonIcon.Error);
        }

        private static void GetUnhandledException()
        {
            Application.Current.DispatcherUnhandledException += delegate (object s, DispatcherUnhandledExceptionEventArgs e)
            {
                _logger.Error(e.Exception);
                trayIcon?.ShowBalloonTip("错误", e.Exception.Message, BalloonIcon.Error);
                e.Handled = true;
            };
            TaskScheduler.UnobservedTaskException += delegate (object? s, UnobservedTaskExceptionEventArgs e)
            {
                _logger.Error(e.Exception);
                trayIcon?.ShowBalloonTip("错误", e.Exception.Message, BalloonIcon.Error);
                e.SetObserved();
            };
            AppDomain.CurrentDomain.UnhandledException += delegate (object s, UnhandledExceptionEventArgs e)
            {
                if (e.ExceptionObject is Exception ex)
                {
                    _logger.Error(ex);
                    trayIcon?.ShowBalloonTip("错误", ex.Message, BalloonIcon.Error);
                }
                else
                {
                    string message = e.ExceptionObject.ToString() ?? "获取错误信息失败";
                    _logger.Error(message);
                    trayIcon?.ShowBalloonTip("错误", message, BalloonIcon.Error);
                }
            };
        }

        #endregion
    }
}