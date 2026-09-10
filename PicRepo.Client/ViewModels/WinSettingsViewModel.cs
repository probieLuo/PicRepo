using PicRepo.Client.Helper;
using PicRepo.Client.Models;
using PicRepo.Client.ViewModels.Settings;
using PicRepo.Client.Views.Settings;
using System.Collections.ObjectModel;
using System.Windows;

namespace PicRepo.Client.ViewModels
{
    public class WinSettingsViewModel : BindableBase
    {
        private readonly IContainerProvider container;
        private readonly IAppSettings appSettings;

        public ObservableCollection<SettingItem> Items { get; set; }
        public SettingItem? SelectedItem { get; set; }

        public DelegateCommand SubmitCommand { get; }
        public DelegateCommand CancelCommand { get; }

        public WinSettingsViewModel(IContainerProvider container, IAppSettings appSettings)
        {
            this.container = container;
            this.appSettings = appSettings;
            Items = [
                new SettingItem("通用", container.Resolve<UcGeneralSettings>(), container.Resolve<UcGeneralSettingsViewModel>()),
                new SettingItem("图床", container.Resolve<UcPicRepoSettings>(), container.Resolve<UcPicRepoSettingsViewModel>()),
                new SettingItem("关于", container.Resolve<UcAbout>(), container.Resolve<UcAboutViewModel>()),
                ];
            SelectedItem = Items.First();

            SubmitCommand = new DelegateCommand(OnSubmit);
            CancelCommand = new DelegateCommand(OnCancel);
        }

        private void OnCancel()
        {
        }

        private void OnSubmit()
        {
            Application.Current.Windows.Cast<Window>().First((Window s) => s.IsActive).DialogResult = true;
        }
    }
}