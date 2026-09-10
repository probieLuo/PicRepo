using Hardcodet.Wpf.TaskbarNotification;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using NLog;
using PicRepo.Client.Data;
using PicRepo.Client.Events;
using PicRepo.Client.Helper;
using PicRepo.Client.Models;
using PicRepo.Client.Services;
using PicRepo.Client.Views;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace PicRepo.Client.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private TaskbarIcon? trayIcon;
        private readonly IContainerProvider container;
        private readonly IAppSettings appSettings;
        private IEventAggregator aggregator;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public string Title { get; set; }
        public bool ShowInTaskbar { get; set; } = true;
        public WindowState WindowState { get; set; }
        public string? Keyword { get; set; }
        public ObservableCollection<HisItem> HisItems { get; set; }
        public HisItem? SelectedHisItem { get; set; }
        [DependsOn(nameof(HisItems), nameof(Keyword))]
        public IEnumerable<HisItem>? ShowItems
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Keyword) || !HisItems.Any())
                {
                    return HisItems;
                }

                return HisItems.Where(s => s.FileName.Contains(Keyword, StringComparison.OrdinalIgnoreCase));
            }
        }
        public bool IsUploading { get; set; } = false;

        public DelegateCommand LoadedCommand { get; }
        public DelegateCommand UploadImageCommand { get; }
        public DelegateCommand<DragEventArgs> PreviewDragOverCommand { get; }
        public DelegateCommand<DragEventArgs> UploadImageDropCommand { get; }
        public DelegateCommand OpenSetWinCommand { get; }
        public DelegateCommand OpenHelpWinCommand { get; }
        public DelegateCommand OpenHistoryCommand { get; }
        public DelegateCommand OpenAboutWinCommand { get; }
        public DelegateCommand<CancelEventArgs> ClosingCommand { get; }
        public DelegateCommand ExitCommand { get; }
        public DelegateCommand GetDefaultClipboardCommand { get; }
        public DelegateCommand GetMarkdownCommand { get; }
        public DelegateCommand GetHTMLCommand { get; }
        public DelegateCommand GetURLCommand { get; }
        public DelegateCommand GetImageCommand { get; }

        public MainWindowViewModel(IContainerProvider container, IAppSettings appSettings, IEventAggregator aggregator)
        {
            this.container = container;
            this.appSettings = appSettings;
            this.aggregator = aggregator;

            Title = "PicRepo";
            trayIcon = Application.Current.FindResource("TrayIcon") as TaskbarIcon;

            LoadedCommand = new DelegateCommand(OnLoaded);
            UploadImageCommand = new DelegateCommand(OnUploadImage);
            PreviewDragOverCommand = new DelegateCommand<DragEventArgs>(OnPreviewDragOver);
            UploadImageDropCommand = new DelegateCommand<DragEventArgs>(OnUploadImageDrop);
            OpenSetWinCommand = new DelegateCommand(OnOpenSetWin);
            OpenHelpWinCommand = new DelegateCommand(OnOpenHelpWin);
            OpenHistoryCommand = new DelegateCommand(OnOpenHistory);
            OpenAboutWinCommand = new DelegateCommand(OnOpenAboutWin);
            ClosingCommand = new DelegateCommand<CancelEventArgs>(OnClosing);
            ExitCommand = new DelegateCommand(OnExit);
            GetDefaultClipboardCommand = new DelegateCommand(OnGetDefaultClipboard);
            GetMarkdownCommand = new DelegateCommand(OnGetMarkdown);
            GetHTMLCommand = new DelegateCommand(OnGetHTML);
            GetURLCommand = new DelegateCommand(OnGetURL);
            GetImageCommand = new DelegateCommand(OnGetImage);

            aggregator.GetEvent<ClearHisEvent>().Subscribe(() =>
            {
                HisItems?.Clear();
            });
        }

        public async Task Refresh()
        {
            using (var db = new AppDbContext())
            {
                var hisItems = await db.UploadHistorys.Select(i => new HisItem
                {
                    Url = i.Url,
                    FileName = i.FileName,
                    UploadTime = i.UploadTime,
                    FileSize = i.FileSize
                }).OrderByDescending(h => h.UploadTime).Take(100).ToListAsync();
                HisItems = new ObservableCollection<HisItem>(hisItems);//hisItems.Adapt<ObservableCollection<HisItem>>();
            }
        }

        private void OnGetImage()
        {
            if (SelectedHisItem == null) return;
            var win = container.Resolve<WinImage>();
            var vm = container.Resolve<WinImageViewModel>();
            vm.Url = SelectedHisItem.Url;
            win.DataContext = vm;
            //win.Owner = Application.Current.Windows.Cast<Window>().First((Window i) => i.IsActive);
            win.Show();
        }

        private void OnGetURL()
        {
            if(SelectedHisItem == null) return;
            Clipboard.SetText($"{SelectedHisItem.Url}");
        }

        private void OnGetHTML()
        {
            if(SelectedHisItem == null) return;
            Clipboard.SetText($"<img src=\"{SelectedHisItem.Url}\" alt=\"{SelectedHisItem.FileName}\" />");
        }

        private void OnGetMarkdown()
        {
            if(SelectedHisItem == null) return;
            Clipboard.SetText($"![]({SelectedHisItem.Url})");
        }

        private void OnGetDefaultClipboard()
        {
            switch(appSettings.CopyType)
            {
                case CopyType.URL:
                    OnGetURL();
                    break;
                case CopyType.HTML:
                    OnGetHTML();
                    break;
                case CopyType.Markdown:
                    OnGetMarkdown();
                    break;
                default:
                    OnGetURL();
                    break;
            }
        }
        private void GetDefaultClipboard(string url)
        {
            switch (appSettings.CopyType)
            {
                case CopyType.URL:
                    Clipboard.SetText($"{url}");
                    break;
                case CopyType.HTML:
                    Clipboard.SetText($"<img src=\"{url}\" alt=\"alt\" />");
                    break;
                case CopyType.Markdown:
                    Clipboard.SetText($"![]({url})");
                    break;
                default:
                    Clipboard.SetText($"{url}");
                    break;
            }
        }
        private void OnLoaded()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(async () => await Refresh()),
                System.Windows.Threading.DispatcherPriority.Background);
        }

        private void OnPreviewDragOver(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && (files[0].EndsWith(".jpg") || files[0].EndsWith(".png") || files[0].EndsWith(".jpeg") || files[0].EndsWith(".gif")))
                {
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private async void OnUploadImageDrop(DragEventArgs e)
        {
            if (IsUploading) return;
            IsUploading = true;
            try
            {
                if (e == null) return;
                if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

                if (e.Data.GetData(DataFormats.FileDrop) is not string[] files || files.Length == 0) return;

                // 仅处理第一个文件
                string filePath = files[0];
                if (!File.Exists(filePath)) return;

                GitHubPicRepoConfig? config = appSettings.PicRepoConfigs.FirstOrDefault(c => c.IsDefault && c.PicRepoType == PicRepoType.GitHub) as GitHubPicRepoConfig;
                if (config != null)
                {
                    var token = PicRepo.Client.Helper.EncryptionHelper.DecryptString(config.Token);
                    GithubApiService apiService = new(config.ProductHeader, token, config.Owner, config.Repo);
                    var result = await apiService.UploadImageFromFileAsync(filePath, $"{Path.GetFileName(filePath)}", "Add image", config.Branch);

                    if (!string.IsNullOrEmpty(result.url))
                    {
                        trayIcon?.ShowBalloonTip("提示", $"图片上传成功！地址已复制到粘贴板", BalloonIcon.Info);
                        GetDefaultClipboard(result.url);
                    }
                    if (result.hisItem != null)
                    {
                        HisItems.Insert(0, result.hisItem.Adapt<HisItem>());
                    }
                }
            }
            catch(Exception ex)
            {
                trayIcon?.ShowBalloonTip("错误", $"图片上传失败！{ex.Message}", BalloonIcon.Error);
                _logger.Error(ex, "图片上传失败");
            }
            finally
            {
                IsUploading = false;
            }
        }

        private void OnExit()
        {
            Application.Current.Shutdown();
        }

        private void OnClosing(CancelEventArgs e)
        {
            if (appSettings.MinimizeToTrayOnClose)
            {
                ShowInTaskbar = false;
                //WindowState = WindowState.Minimized;
                Application.Current.MainWindow.Visibility = Visibility.Collapsed;
                if (appSettings.MiniWindowingOnClose)
                {
                    var win = container.Resolve<WinMini>();
                    win.Show();
                }
                e.Cancel = true;
            }
            else
            {
                var win = container.Resolve<WinMini>();
                win.Close();
            }
        }

        private void OnOpenHelpWin()
        {
        }

        private void OnOpenHistory()
        {
            var win = container.Resolve<WinHistory>();
            var vm = container.Resolve<WinHistoryViewModel>();
            win.DataContext = vm;
            win.Owner = Application.Current.Windows.Cast<Window>().First((Window i) => i.IsActive);
            win.Show();
        }

        private void OnOpenAboutWin()
        {
            var win = container.Resolve<WinSettings>();
            var vm = container.Resolve<WinSettingsViewModel>();
            vm.SelectedItem = vm.Items.First(i => i.Name.Equals("关于"));
            win.DataContext = vm;
            win.Owner = Application.Current.Windows.Cast<Window>().First((Window i) => i.IsActive);
            win.ShowDialog();
        }

        private void OnOpenSetWin()
        {
            var win = container.Resolve<WinSettings>();
            win.Owner = Application.Current.Windows.Cast<Window>().First((Window i) => i.IsActive);
            if (win.ShowDialog() == true)
            {
            }
        }

        private async void OnUploadImage()
        {
            if (IsUploading) return;
            IsUploading = true;
            try
            {
                var dlg = new OpenFileDialog() { Filter = "图片文件|*.jpg;*.jpeg;*.png;*.gif" };
                if (dlg.ShowDialog() == true)
                {
                    string filePath = dlg.FileName;
                    GitHubPicRepoConfig? config = appSettings.PicRepoConfigs.FirstOrDefault(c => c.IsDefault && c.PicRepoType == PicRepoType.GitHub) as GitHubPicRepoConfig;
                    if (config != null)
                    {
                        var token = PicRepo.Client.Helper.EncryptionHelper.DecryptString(config.Token);
                        GithubApiService apiService = new(config.ProductHeader, token, config.Owner, config.Repo);
                        var result = await apiService.UploadImageFromFileAsync(filePath, $"{Path.GetFileName(filePath)}", "Add image", config.Branch);

                        if (!string.IsNullOrEmpty(result.url))
                        {
                            trayIcon?.ShowBalloonTip("提示", $"图片上传成功！地址已复制到粘贴板", BalloonIcon.Info);
                            GetDefaultClipboard(result.url);
                        }
                        if (result.hisItem != null)
                        {
                            HisItems.Insert(0, result.hisItem.Adapt<HisItem>());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                trayIcon?.ShowBalloonTip("错误", $"图片上传失败！{e.Message}", BalloonIcon.Error);
                _logger.Error(e, "图片上传失败");
            }
            finally
            {
                IsUploading = false;
            }
        }
    }
}