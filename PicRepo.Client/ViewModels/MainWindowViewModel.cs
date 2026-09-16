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
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Media.Imaging;

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
		public DelegateCommand ClipboardUploadCommand { get; }

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
			ClipboardUploadCommand = new DelegateCommand(OnClipboardUpload);

			aggregator.GetEvent<ClearHisEvent>().Subscribe(() =>
			{
				HisItems?.Clear();
			});
		}

		private static string SaveBitmapSourceToTempFile(BitmapSource bitmapSource)
		{
			string path = Path.Combine(Path.GetTempPath(), $"picrepo-{Guid.NewGuid():N}.png");
			var encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
			using var stream = File.Create(path);
			encoder.Save(stream);
			return path;
		}

		private static bool IsImageUrl(string? text)
		{
			if (string.IsNullOrWhiteSpace(text)) return false;

			var trimmed = text.Trim();
			if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)) return false;

			if (!uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
				&& !uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			var extension = Path.GetExtension(uri.AbsolutePath);
			return !string.IsNullOrWhiteSpace(extension)
				&& IsImageFile(extension);
		}

		private static async Task<string?> DownloadImageUrlToTempFileAsync(string url)
		{
			using var client = new HttpClient();
			byte[] data = await client.GetByteArrayAsync(url);
			var extension = Path.GetExtension(new Uri(url).AbsolutePath);
			if (string.IsNullOrWhiteSpace(extension))
			{
				extension = ".png";
			}

			var path = Path.Combine(Path.GetTempPath(), $"picrepo-{Guid.NewGuid():N}{extension}");
			await File.WriteAllBytesAsync(path, data);
			return path;
		}

		private async void OnClipboardUpload()
		{
			if (IsUploading) return;
			IsUploading = true;
			try
			{
				var clipboardData = Clipboard.GetDataObject();
				if (clipboardData == null)
				{
					trayIcon?.ShowBalloonTip("提示", "剪贴板中没有可上传的图片内容。", BalloonIcon.Info);
					return;
				}

				var filePaths = new List<string>();

				if (clipboardData.GetDataPresent(DataFormats.FileDrop))
				{
					if (clipboardData.GetData(DataFormats.FileDrop) is string[] dropFiles)
					{
						filePaths.AddRange(dropFiles.Where(IsImageFile));
					}
				}

				if (clipboardData.GetDataPresent(DataFormats.Text))
				{
					var text = clipboardData.GetData(DataFormats.Text)?.ToString();
					if (!string.IsNullOrWhiteSpace(text))
					{
						var trimmed = text.Trim().Trim('"', '\'');
						if (IsImageUrl(trimmed))
						{
							var imageUrlFile = await DownloadImageUrlToTempFileAsync(trimmed);
							if (!string.IsNullOrWhiteSpace(imageUrlFile))
							{
								filePaths.Add(imageUrlFile);
							}
						}
						else if (File.Exists(trimmed) && IsImageFile(trimmed))
						{
							filePaths.Add(trimmed);
						}
					}
				}

				if (Clipboard.ContainsImage())
				{
					var image = Clipboard.GetImage();
					if (image != null)
					{
						filePaths.Add(SaveBitmapSourceToTempFile(image));
					}
				}

				var validFiles = filePaths.Where(File.Exists).Distinct().ToList();
				if (validFiles.Count == 0)
				{
					trayIcon?.ShowBalloonTip("提示", "剪贴板中没有图片文件、图片链接或截图。", BalloonIcon.Info);
					return;
				}

				await UploadFilesAsync(validFiles);
			}
			catch (Exception ex)
			{
				trayIcon?.ShowBalloonTip("错误", $"从剪贴板上传失败：{ex.Message}", BalloonIcon.Error);
				_logger.Error(ex, "从剪贴板上传失败");
			}
			finally
			{
				IsUploading = false;
			}
		}

		public async Task Refresh()
		{
			var hisItems = await Task.Run(async () =>
			{
				using AppDbContext db = new();
				return await db.UploadHistorys.Select(i => new HisItem
				{
					Url = i.Url,
					FileName = i.FileName,
					UploadTime = i.UploadTime,
					FileSize = i.FileSize
				}).OrderByDescending(h => h.UploadTime).Take(100).ToListAsync();
			});
			HisItems = new ObservableCollection<HisItem>(hisItems);
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
			if (SelectedHisItem == null) return;
			Clipboard.SetText($"{SelectedHisItem.Url}");
		}

		private void OnGetHTML()
		{
			if (SelectedHisItem == null) return;
			Clipboard.SetText($"<img src=\"{SelectedHisItem.Url}\" alt=\"{SelectedHisItem.FileName}\" />");
		}

		private void OnGetMarkdown()
		{
			if (SelectedHisItem == null) return;
			Clipboard.SetText($"![]({SelectedHisItem.Url})");
		}

		private void OnGetDefaultClipboard()
		{
			switch (appSettings.CopyType)
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

		private async void OnLoaded()
		{
			await Refresh();
			//Application.Current.Dispatcher.BeginInvoke(new Action(async () => await Refresh()), System.Windows.Threading.DispatcherPriority.Background);
		}

		private static bool IsImageFile(string filePath)
		{
			var extension = Path.GetExtension(filePath);
			if (string.IsNullOrWhiteSpace(extension)) return false;

			return string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(extension, ".gif", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(extension, ".bmp", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(extension, ".webp", StringComparison.OrdinalIgnoreCase);
		}

		private void OnPreviewDragOver(DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				if (files.Length > 0 && files.Any(IsImageFile))
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

		private async Task UploadFilesAsync(IEnumerable<string> filePaths)
		{
			var files = filePaths.Where(File.Exists).Distinct().ToList();
			if (files.Count == 0) return;

			var config = appSettings.PicRepoConfigs.FirstOrDefault(c => c.IsDefault);
			if (config == null)
			{
				trayIcon?.ShowBalloonTip("错误", "请先配置默认图床后再上传图片。", BalloonIcon.Error);
				return;
			}

			foreach (var filePath in files)
			{
				var repoService = new PicRepoService();
				var result = await repoService.UploadAsync(config, filePath);

				if (!string.IsNullOrEmpty(result.url))
				{
					GetDefaultClipboard(result.url);
				}

				if (result.hisItem != null)
				{
					HisItems.Insert(0, result.hisItem.Adapt<HisItem>());
				}
			}

			if (files.Count > 1)
			{
				trayIcon?.ShowBalloonTip("提示", $"已批量上传 {files.Count} 张图片，链接已复制到剪贴板。", BalloonIcon.Info);
			}
			else
			{
				trayIcon?.ShowBalloonTip("提示", "图片上传成功！地址已复制到粘贴板", BalloonIcon.Info);
			}
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

				var imageFiles = files.Where(IsImageFile).ToList();
				if (imageFiles.Count == 0) return;

				await UploadFilesAsync(imageFiles);
			}
			catch (Exception ex)
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
			var url = "https://github.com/probieLuo/PicRepo#%E4%BD%BF%E7%94%A8%E6%96%B9%E6%B3%95";
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
				var dlg = new OpenFileDialog()
				{
					Filter = "图片文件|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.webp",
					Multiselect = true
				};
				if (dlg.ShowDialog() == true)
				{
					await UploadFilesAsync(dlg.FileNames);
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