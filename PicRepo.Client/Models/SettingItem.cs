using System.Windows.Controls;

namespace PicRepo.Client.Models
{
    public class SettingItem
    {
        public string Name { get; set; }
        public UserControl? View { get; set; }
        public IConfig ViewModel { get; set; }

        public SettingItem(string name, UserControl? view, IConfig viewModel)
        {
            Name = name;
            View = view;
            ViewModel = viewModel;
        }
    }
}