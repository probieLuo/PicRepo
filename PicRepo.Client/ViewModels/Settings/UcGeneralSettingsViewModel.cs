using DynamicData.Binding;
using PicRepo.Client.Data;
using PicRepo.Client.Events;
using PicRepo.Client.Helper;
using PicRepo.Client.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace PicRepo.Client.ViewModels.Settings
{
    public class UcGeneralSettingsViewModel : BindableBase, IConfig
    {
        private readonly IAppSettings settings;
        private readonly IEventAggregator aggregator;

        public ObservableCollection<ThemeModeItem> Themes { get; set; }
        public ThemeModeItem? SelectedTheme { get; set; }
        public bool MinimizeToTrayOnClose { get; set; }
        public bool MiniWindowingOnClose { get; set; }
        public List<CopyType> CopyTypes { get; set; }
        public CopyType? SelectedCopyType { get; set; }

        public DelegateCommand ClearCacheCommand { get; }

        public UcGeneralSettingsViewModel(IAppSettings settings, IEventAggregator aggregator)
        {
            this.settings = settings;
            this.aggregator = aggregator;

            Themes = [
                new ThemeModeItem("浅色", ThemeMode.Light),
                new ThemeModeItem("深色", ThemeMode.Dark),
                new ThemeModeItem("跟随系统", ThemeMode.System)
                ];
            SelectedTheme = Themes.FirstOrDefault(t => t.Value.ToString() == settings.Theme);
            MinimizeToTrayOnClose = settings.MinimizeToTrayOnClose;
            MiniWindowingOnClose = settings.MiniWindowingOnClose;
            CopyTypes = Enum.GetValues<CopyType>().ToList();
            SelectedCopyType = settings.CopyType;

            this.WhenPropertyChanged(s => s.SelectedTheme).Subscribe(e =>
            {
                if (e.Value == null) return;
                Application.Current.ThemeMode = e.Value.Value;
                settings.Theme = e.Value.Value.ToString();
                settings.SaveConfig();
            });

            this.WhenPropertyChanged(s => s.MinimizeToTrayOnClose).Subscribe(e =>
            {
                settings.MinimizeToTrayOnClose = e.Value;
                settings.SaveConfig();
            });

            this.WhenPropertyChanged(s => s.MiniWindowingOnClose).Subscribe(e =>
            {
                settings.MiniWindowingOnClose = e.Value;
                settings.SaveConfig();
            });

            this.WhenPropertyChanged(s => s.SelectedCopyType).Subscribe(e =>
            {
                if (e.Value == null) return;
                settings.CopyType = e.Value;
                settings.SaveConfig();
            });

            ClearCacheCommand = new DelegateCommand(OnClearCache);
        }

        private void OnClearCache()
        {
            using var db = new AppDbContext();
            db.UploadHistorys.RemoveRange(db.UploadHistorys);
            db.SaveChanges();
            aggregator.GetEvent<ClearHisEvent>().Publish();
        }
    }

    public class ThemeModeItem(string name, ThemeMode value)
    {
        public string Name { get; set; } = name;
        public ThemeMode Value { get; set; } = value;
    }
}