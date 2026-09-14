using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using PicRepo.Client.Helper;
using PicRepo.Client.Models;
using PicRepo.Client.Services;
using PicRepo.Client.Views;
using System.IO;
using System.Windows;

namespace PicRepo.Client.ViewModels
{
    public class WinMiniViewModel : BindableBase
    {
        private readonly IAppSettings appSettings;
        private readonly IContainerProvider container;
        private TaskbarIcon? trayIcon;

        public DelegateCommand UploadImageCommand { get; }
        public DelegateCommand OpenMainWinCommand { get; }
        public DelegateCommand HideWinCommand { get; }
        public DelegateCommand ExitCommand { get; }

        public WinMiniViewModel(IContainerProvider container, IAppSettings appSettings)
        {
            this.container = container;
            this.appSettings = appSettings;
            trayIcon = Application.Current.FindResource("TrayIcon") as TaskbarIcon;

            UploadImageCommand = new DelegateCommand(OnUploadImage);
            OpenMainWinCommand = new DelegateCommand(OnOpenMainWin);
            HideWinCommand = new DelegateCommand(OnHideWin);
            ExitCommand = new DelegateCommand(OnExit);
        }

        private void OnExit()
        {
            Application.Current.Shutdown();
        }

        private void OnHideWin()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var miniWin = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w is Views.WinMini);
                if (miniWin != null)
                {
                    miniWin.Hide();
                }
            });
        }

        private void OnOpenMainWin()
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow == null)
                {
                    mainWindow = container.Resolve<MainWindow>();
                    Application.Current.MainWindow = mainWindow;
                }
                var vm = mainWindow.DataContext as MainWindowViewModel;
                if (vm != null)
                {
                    vm.ShowInTaskbar = true;
                    await vm.Refresh();
                }
                var miniWin = container.Resolve<WinMini>();
                miniWin.Hide();
                mainWindow.Activate();
                mainWindow.Visibility = Visibility.Visible;
            });
        }

        private async void OnUploadImage()
        {
            var dlg = new OpenFileDialog() { Filter = "图片文件|*.jpg;*.jpeg;*.png;*.gif" };
            if (dlg.ShowDialog() == true)
            {
                string filePath = dlg.FileName;
                IPicRepoConfig? config = appSettings.PicRepoConfigs.FirstOrDefault(c => c.IsDefault);
                if (config != null)
                {
                    PicRepoService repoService = new PicRepoService();
                    var result = await repoService.UploadAsync(config, filePath);

                    if (!string.IsNullOrEmpty(result.url))
                    {
                        trayIcon?.ShowBalloonTip("提示", $"图片上传成功！地址已复制到粘贴板", BalloonIcon.Info);
                        switch (appSettings.CopyType)
                        {
                            case CopyType.URL:
                                Clipboard.SetText($"{result.url}");
                                break;

                            case CopyType.HTML:
                                Clipboard.SetText($"<img src=\"{result.url}\" alt=\"alt\" />");
                                break;

                            case CopyType.Markdown:
                                Clipboard.SetText($"![]({result.url})");
                                break;

                            default:
                                Clipboard.SetText($"{result.url}");
                                break;
                        }
                    }
                }
            }
        }
    }
}