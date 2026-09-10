using PicRepo.Client.Models;
using System.Reflection;

namespace PicRepo.Client.ViewModels.Settings
{
    public class UcAboutViewModel : BindableBase, IConfig
    {
        public string ApplicationName { get; set; }
        public string? Version { get; set; }

        public UcAboutViewModel()
        {
            ApplicationName = "PicRepo";
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        }
    }
}