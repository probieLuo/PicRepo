using PicRepo.Client.Helper;
using PicRepo.Client.Models;
using PicRepo.Client.Views.Settings;
using System.Collections.ObjectModel;
using System.Windows;

namespace PicRepo.Client.ViewModels.Settings
{
    public class UcPicRepoSettingsViewModel : BindableBase, IConfig
    {
        private readonly IContainerProvider container;
        private readonly IAppSettings appSettings;

        public ObservableCollection<GitHubPicRepoConfig> PicRepoConfigs { get; set; }
        public GitHubPicRepoConfig? SelectedConfig { get; set; }

        public DelegateCommand AddConfigCommand { get; }
        public DelegateCommand SaveConfigCommand { get; }
        public DelegateCommand SetDefaultCommand { get; }
        public DelegateCommand DeleteConfigCommand { get; }

        public UcPicRepoSettingsViewModel(IContainerProvider container, IAppSettings appSettings)
        {
            this.container = container;
            this.appSettings = appSettings;

            // Use UI copies of stored configs so we don't mutate appSettings instances (avoid wiping stored/encrypted tokens)
            PicRepoConfigs = new ObservableCollection<GitHubPicRepoConfig>(
                appSettings.PicRepoConfigs
                    .OfType<GitHubPicRepoConfig>()
                    .Select(c => new GitHubPicRepoConfig
                    {
                        PicRepoType = c.PicRepoType,
                        IsDefault = c.IsDefault,
                        Name = c.Name,
                        Token = string.Empty, // do not expose stored/encrypted token in UI
                        ProductHeader = c.ProductHeader,
                        Owner = c.Owner,
                        Repo = c.Repo,
                        Branch = c.Branch
                    })
            );
            SelectedConfig = PicRepoConfigs.FirstOrDefault(i => i.IsDefault == true);

            AddConfigCommand = new DelegateCommand(OnAddConfig);
            SaveConfigCommand = new DelegateCommand(OnSaveConfig);
            SetDefaultCommand = new DelegateCommand(OnSetDefault);
            DeleteConfigCommand = new DelegateCommand(OnDeleteConfig);
        }

        private void OnDeleteConfig()
        {
            if (SelectedConfig != null)
            {
                var existingConfig = appSettings.PicRepoConfigs.FirstOrDefault(i => i.Name == SelectedConfig.Name && i.PicRepoType == SelectedConfig.PicRepoType);
                if (existingConfig != null)
                {
                    appSettings.PicRepoConfigs.Remove(existingConfig);
                }
                appSettings.SaveConfig();
                PicRepoConfigs.Remove(SelectedConfig);
                SelectedConfig = PicRepoConfigs.FirstOrDefault();
                OnSetDefault();
            }
        }

        private void OnSetDefault()
        {
            if (SelectedConfig != null)
            {
                var existingConfig = appSettings.PicRepoConfigs.FirstOrDefault(i => i.Name == SelectedConfig.Name && i.PicRepoType == SelectedConfig.PicRepoType);
                if (existingConfig != null)
                {
                    existingConfig.IsDefault = true;
                    SelectedConfig.IsDefault = true;
                    foreach (var config in appSettings.PicRepoConfigs.Where(i => i != existingConfig && i.PicRepoType == existingConfig.PicRepoType))
                    {
                        config.IsDefault = false;
                    }
                    foreach (var config in PicRepoConfigs.Where(i => i != SelectedConfig && i.PicRepoType == SelectedConfig.PicRepoType))
                    {
                        config.IsDefault = false;
                    }
                }
                appSettings.SaveConfig();
            }
        }

        private void OnSaveConfig()
        {
            if (SelectedConfig != null)
            {
                var existingConfig = appSettings.PicRepoConfigs.FirstOrDefault(i => i.Name == SelectedConfig.Name && i.PicRepoType == SelectedConfig.PicRepoType) as GitHubPicRepoConfig;
                if (existingConfig != null)
                {
                    // Only overwrite stored token when user provided a new one in UI
                    if (!string.IsNullOrEmpty(SelectedConfig.Token))
                    {
                        existingConfig.Token = EncryptionHelper.EncryptString(SelectedConfig.Token);
                    }
                    existingConfig.Owner = SelectedConfig.Owner;
                    existingConfig.Repo = SelectedConfig.Repo;
                    existingConfig.Branch = SelectedConfig.Branch;
                }
                else
                {
                    // For new config, encrypt token before storing
                    if (!string.IsNullOrEmpty(SelectedConfig.Token))
                    {
                        SelectedConfig.Token = EncryptionHelper.EncryptString(SelectedConfig.Token);
                    }
                    appSettings.PicRepoConfigs.Add(SelectedConfig);
                }
                appSettings.SaveConfig();
            }
        }

        private void OnAddConfig()
        {
            var win = container.Resolve<WinAddPicRepo>();
            var vm = container.Resolve<WinAddPicRepoViewModel>();
            win.DataContext = vm;
            win.Owner = Application.Current.Windows.Cast<Window>().First((Window i) => i.IsActive);
            if (win.ShowDialog() == true)
            {
                if (PicRepoConfigs.Any(i => i.Name == vm.NewPicRepoName))
                {
                    SelectedConfig = PicRepoConfigs.FirstOrDefault(i => i.Name == vm.NewPicRepoName);
                }
                else
                {
                    var newConfig = new GitHubPicRepoConfig { Name = vm.NewPicRepoName };
                    PicRepoConfigs.Add(newConfig);
                    SelectedConfig = newConfig;
                }
            }
        }
    }
}