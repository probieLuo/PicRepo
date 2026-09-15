using DynamicData.Binding;
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

        public ObservableCollection<IPicRepoConfig> PicRepoConfigs { get; set; }
        public IPicRepoConfig? SelectedConfig { get; set; }
        public GitHubPicRepoConfig? SelectedGithubConfig { get; set;}
        public GiteePicRepoConfig? SelectedGiteeConfig { get; set; }

        public DelegateCommand AddConfigCommand { get; }
        public DelegateCommand SaveConfigCommand { get; }
        public DelegateCommand SetDefaultCommand { get; }
        public DelegateCommand DeleteConfigCommand { get; }

        public UcPicRepoSettingsViewModel(IContainerProvider container, IAppSettings appSettings)
        {
            this.container = container;
            this.appSettings = appSettings;

            // Use UI copies of stored configs so we don't mutate appSettings instances (avoid wiping stored/encrypted tokens)
            PicRepoConfigs = new ObservableCollection<IPicRepoConfig>(appSettings.PicRepoConfigs.ToArray());
            SelectedConfig = PicRepoConfigs.FirstOrDefault(i => i.IsDefault == true);

            AddConfigCommand = new DelegateCommand(OnAddConfig);
            SaveConfigCommand = new DelegateCommand(OnSaveConfig);
            SetDefaultCommand = new DelegateCommand(OnSetDefault);
            DeleteConfigCommand = new DelegateCommand(OnDeleteConfig);

            this.WhenPropertyChanged(t => t.SelectedConfig).Subscribe(config =>
            {
                if(config!=null && config.Value != null)
                {
                    switch(config.Value.PicRepoType)
                    {
                        case PicRepoType.GitHub:
                            if(config.Value is GitHubPicRepoConfig githubconfig)
                            {
                                SelectedGithubConfig = githubconfig;
                                SelectedGithubConfig.Token = EncryptionHelper.DecryptString(SelectedGithubConfig.Token);
                                SelectedGiteeConfig = null;
                            }
                            break;
                        case PicRepoType.Gitee:
                            if (config.Value is GiteePicRepoConfig giteeconfig)
                            {
                                SelectedGiteeConfig = giteeconfig;
                                SelectedGiteeConfig.Token = EncryptionHelper.DecryptString(SelectedGiteeConfig.Token);
                                SelectedGithubConfig = null;
                            }
                            break;
                    }
                }
                else
				{
					SelectedGithubConfig = null;
					SelectedGiteeConfig = null;
				}
            });
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
                    foreach (var config in appSettings.PicRepoConfigs.Where(i => i != existingConfig))
                    {
                        config.IsDefault = false;
                    }
                    foreach (var config in PicRepoConfigs.Where(i => i != SelectedConfig))
                    {
                        config.IsDefault = false;
                    }
                }
                appSettings.SaveConfig();
            }
        }

        private void OnSaveConfig()
        {
            if (SelectedGithubConfig != null)
            {
                var existingConfig = appSettings.PicRepoConfigs.FirstOrDefault(i => i.Name == SelectedGithubConfig.Name && i.PicRepoType == SelectedGithubConfig.PicRepoType) as GitHubPicRepoConfig;
                if (existingConfig != null)
                {
                    // Only overwrite stored token when user provided a new one in UI
                    if (!string.IsNullOrEmpty(SelectedGithubConfig.Token))
                    {
                        existingConfig.Token = EncryptionHelper.EncryptString(SelectedGithubConfig.Token);
                    }
                    existingConfig.Owner = SelectedGithubConfig.Owner;
                    existingConfig.Repo = SelectedGithubConfig.Repo;
                    existingConfig.Branch = SelectedGithubConfig.Branch;
                }
                else
                {
                    // For new config, encrypt token before storing
                    if (!string.IsNullOrEmpty(SelectedGithubConfig.Token))
                    {
                        SelectedGithubConfig.Token = EncryptionHelper.EncryptString(SelectedGithubConfig.Token);
                    }
                    appSettings.PicRepoConfigs.Add(SelectedGithubConfig);
                }
                appSettings.SaveConfig();
                SetDefaultCommand.Execute();
            }

            if (SelectedGiteeConfig != null)
            {
                var existingConfig = appSettings.PicRepoConfigs.FirstOrDefault(i => i.Name == SelectedGiteeConfig.Name && i.PicRepoType == SelectedGiteeConfig.PicRepoType) as GiteePicRepoConfig;
                if (existingConfig != null)
                {
                    // Only overwrite stored token when user provided a new one in UI
                    if (!string.IsNullOrEmpty(SelectedGiteeConfig.Token))
                    {
                        existingConfig.Token = EncryptionHelper.EncryptString(SelectedGiteeConfig.Token);
                    }
                    existingConfig.Owner = SelectedGiteeConfig.Owner;
                    existingConfig.Repo = SelectedGiteeConfig.Repo;
                    existingConfig.Branch = SelectedGiteeConfig.Branch;
                }
                else
                {
                    // For new config, encrypt token before storing
                    if (!string.IsNullOrEmpty(SelectedGiteeConfig.Token))
                    {
                        SelectedGiteeConfig.Token = EncryptionHelper.EncryptString(SelectedGiteeConfig.Token);
                    }
                    appSettings.PicRepoConfigs.Add(SelectedGiteeConfig);
                }
                appSettings.SaveConfig();
                SetDefaultCommand.Execute();
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
                if (PicRepoConfigs.Any(i => i.Name == vm.NewPicRepoName && i.PicRepoType == vm.SelectedItem))
                {
                    SelectedConfig = PicRepoConfigs.FirstOrDefault(i => i.Name == vm.NewPicRepoName && i.PicRepoType == vm.SelectedItem);
                }
                else
                {
                    switch (vm.SelectedItem)
                    {
                        case PicRepoType.GitHub:
                            var newgithubConfig = new GitHubPicRepoConfig { Name = vm.NewPicRepoName };
                            PicRepoConfigs.Add(newgithubConfig);
                            SelectedConfig = newgithubConfig;
                            break;
                        case PicRepoType.Gitee:
                            var newgiteeConfig = new GiteePicRepoConfig { Name = vm.NewPicRepoName };
                            PicRepoConfigs.Add(newgiteeConfig);
                            SelectedConfig = newgiteeConfig;
                            break;
                    }
                    
                }
            }
        }
    }
}