using DynamicData.Binding;
using Microsoft.EntityFrameworkCore;
using PicRepo.Client.Data;
using PicRepo.Client.Helper;
using PicRepo.Client.Models;
using PicRepo.Client.Views;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;

namespace PicRepo.Client.ViewModels
{
    internal class WinHistoryViewModel : BindableBase
    {
        private readonly IContainerProvider container;
        private readonly IAppSettings appSettings;

        public string? Keyword { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        //[DependsOn(nameof(Items), nameof(Keyword), nameof(StartTime), nameof(EndTime),nameof(PageIndex),nameof(PageSize))]
        //public IEnumerable<HisItem>? ShowItems
        //{
        //    get
        //    {
        //        var query = Items.AsEnumerable();
        //        TotalCount = query.Count();
        //        if (!Items.Any())
        //        {
        //            return query;
        //        }
        //        query = query.Where(s => s.FileName.Contains(Keyword??"", StringComparison.OrdinalIgnoreCase));
        //        if(StartTime.HasValue)
        //        {
        //            query = query.Where(s => s.UploadTime >= StartTime.Value);
        //        }
        //        if(EndTime.HasValue)
        //        {
        //            query = query.Where(s => s.UploadTime <= EndTime.Value);
        //        }
        //        query = query.Skip((PageIndex - 1) * PageSize).Take(PageSize).OrderByDescending(h => h.UploadTime);
        //        NextPageCommand.RaiseCanExecuteChanged();
        //        PrevPageCommand.RaiseCanExecuteChanged();
        //        RaisePropertyChanged(nameof(PageInfo));
        //        return query;
        //    }
        //}
        public ObservableCollection<HisItem> Items { get; set; } = new ObservableCollection<HisItem>();

        public HisItem? SelectedItem { get; set; }

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
        public string PageInfo => $"{PageIndex}/{TotalPages}";

        public DelegateCommand LoadedCommand { get; }
        public DelegateCommand NextPageCommand { get; }
        public DelegateCommand PrevPageCommand { get; }
        public DelegateCommand ClearFilterCommand { get; }

        public DelegateCommand GetDefaultClipboardCommand { get; }
        public DelegateCommand GetMarkdownCommand { get; }
        public DelegateCommand GetHTMLCommand { get; }
        public DelegateCommand GetURLCommand { get; }
        public DelegateCommand GetImageCommand { get; }

        public WinHistoryViewModel(IContainerProvider container, IAppSettings appSettings)
        {
            this.container = container;
            this.appSettings = appSettings;

            LoadedCommand = new DelegateCommand(async () => await OnLoaded());
            NextPageCommand = new DelegateCommand(async () => await NextPage(), () => PageIndex < TotalPages);
            PrevPageCommand = new DelegateCommand(async () => await PrevPage(), () => PageIndex > 1);
            ClearFilterCommand = new DelegateCommand(OnClearFilter);
            GetDefaultClipboardCommand = new DelegateCommand(OnGetDefaultClipboard);
            GetMarkdownCommand = new DelegateCommand(OnGetMarkdown);
            GetHTMLCommand = new DelegateCommand(OnGetHTML);
            GetURLCommand = new DelegateCommand(OnGetURL);
            GetImageCommand = new DelegateCommand(OnGetImage);

            this.WhenPropertyChanged(t => t.PageSize)
                .Throttle(TimeSpan.FromMilliseconds(500), RxApp.TaskpoolScheduler)
                .Subscribe(async _ =>
            {
                PageIndex = 1;
                await LoadPageAsync();
            });
            this.WhenPropertyChanged(t => t.Keyword)
                .Throttle(TimeSpan.FromMilliseconds(500), RxApp.TaskpoolScheduler)
                .Subscribe(async _ =>
            {
                PageIndex = 1;
                await LoadPageAsync();
            });
            this.WhenPropertyChanged(t => t.StartTime)
                .Throttle(TimeSpan.FromMilliseconds(500), RxApp.TaskpoolScheduler)
                .Subscribe(async _ =>
                {
                    PageIndex = 1;
                    await LoadPageAsync();
                });
            this.WhenPropertyChanged(t => t.EndTime)
                .Throttle(TimeSpan.FromMilliseconds(500), RxApp.TaskpoolScheduler)
                .Subscribe(async _ =>
                {
                    PageIndex = 1;
                    await LoadPageAsync();
                });
        }

        private async Task OnLoaded()
        {
            //using (var db = new AppDbContext())
            //{
            //    Items =  db.UploadHistorys.ToList().Adapt<ObservableCollection<HisItem>>();
            //}
            await LoadPageAsync();
        }

        private void OnClearFilter()
        {
            Keyword = "";
            StartTime = null;
            EndTime = null;
        }

        public async Task LoadPageAsync()
        {
            using (var db = new AppDbContext())
            {
                var query = db.UploadHistorys.AsQueryable();
                if (!string.IsNullOrWhiteSpace(Keyword))
                    query = query.Where(h => EF.Functions.Like(h.FileName, $"%{Keyword}%"));
                if (StartTime.HasValue)
                    query = query.Where(h => h.UploadTime >= StartTime.Value);
                if (EndTime.HasValue)
                    query = query.Where(h => h.UploadTime <= EndTime.Value);
                TotalCount = await query.CountAsync();
                var data = await query.OrderByDescending(h => h.UploadTime)
                    .Skip((PageIndex - 1) * PageSize)
                    .Take(PageSize)
                    .Select(h => new HisItem
                    {
                        Url = h.Url,
                        FileName = h.FileName,
                        UploadTime = h.UploadTime,
                        FileSize = h.FileSize
                    })
                    .ToListAsync();

                Items = new ObservableCollection<HisItem>(data);
            }
            NextPageCommand.RaiseCanExecuteChanged();
            PrevPageCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged(nameof(PageInfo));
        }

        private async Task NextPage()
        {
            if (PageIndex < TotalPages)
            {
                PageIndex++;
                await LoadPageAsync();
            }
        }

        private async Task PrevPage()
        {
            if (PageIndex > 1)
            {
                PageIndex--;
                await LoadPageAsync();
            }
        }

        private void OnGetImage()
        {
            if (SelectedItem == null) return;
            var win = container.Resolve<WinImage>();
            var vm = container.Resolve<WinImageViewModel>();
            vm.Url = SelectedItem.Url;
            win.DataContext = vm;
            win.Show();
        }

        private void OnGetURL()
        {
            if (SelectedItem == null) return;
            Clipboard.SetText($"{SelectedItem.Url}");
        }

        private void OnGetHTML()
        {
            if (SelectedItem == null) return;
            Clipboard.SetText($"<img src=\"{SelectedItem.Url}\" alt=\"{SelectedItem.FileName}\" />");
        }

        private void OnGetMarkdown()
        {
            if (SelectedItem == null) return;
            Clipboard.SetText($"![]({SelectedItem.Url})");
        }

        private void OnGetDefaultClipboard()
        {
            if (SelectedItem == null) return;
            switch (appSettings.CopyType)
            {
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
    }
}