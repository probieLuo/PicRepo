using Hardcodet.Wpf.TaskbarNotification;
using NLog;
using PicRepo.Client.Models;
using Prism.Commands;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;

namespace PicRepo.Client.ViewModels.Settings
{
    public class UcAboutViewModel : BindableBase, IConfig
	{
		private TaskbarIcon? trayIcon;
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public string ApplicationName { get; set; }
        public string? Version { get; set; }
        public string HelpUrl { get; set; } = "https://github.com/probieLuo/PicRepo#%E4%BD%BF%E7%94%A8%E6%96%B9%E6%B3%95";
        public string Description { get; set; } = "PicRepo 是一个轻量、实用的桌面端图片上传工具，适合需要高频上传图片并快速复制链接的场景";
		public DelegateCommand<RequestNavigateEventArgs> OpenHelpCommand { get; }

        public UcAboutViewModel()
		{
			trayIcon = Application.Current.FindResource("TrayIcon") as TaskbarIcon;
			ApplicationName = "PicRepo";
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

            OpenHelpCommand = new DelegateCommand<RequestNavigateEventArgs>(OnOpenHelp);
        }

        private void OnOpenHelp(RequestNavigateEventArgs e)
        {
            var url = e?.Uri?.ToString() ?? HelpUrl;
            try
            {
                var psi = new ProcessStartInfo { FileName = url, UseShellExecute = true };
                Process.Start(psi);
            }
			catch (Exception ex)
			{
				trayIcon?.ShowBalloonTip("错误", $"无法打开帮助链接：{ex.Message}", BalloonIcon.Error);
				_logger.Error(ex, "打开帮助链接失败");
			}
		}
    }
}