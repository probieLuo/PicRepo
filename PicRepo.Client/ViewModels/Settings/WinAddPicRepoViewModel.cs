using System.Windows;

namespace PicRepo.Client.ViewModels.Settings
{
    internal class WinAddPicRepoViewModel : BindableBase
    {
        public string NewPicRepoName { get; set; }

        public DelegateCommand AddPicRepoCommand { get; }

        public WinAddPicRepoViewModel()
        {
            AddPicRepoCommand = new DelegateCommand(AddPicRepo);
        }

        private void AddPicRepo()
        {
            if (string.IsNullOrWhiteSpace(NewPicRepoName)) { return; }
            Application.Current.Windows.Cast<Window>().First((Window s) => s.IsActive).DialogResult = true;
        }
    }
}