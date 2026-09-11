using PicRepo.Client.ViewModels;
using System.Windows;

namespace PicRepo.Client.Views
{
    /// <summary>
    /// WinImage.xaml 的交互逻辑
    /// </summary>
    public partial class WinImage : Window
    {
        public WinImage()
        {
            InitializeComponent();
            this.Loaded += async (sender, e) =>
            {
                await webView.EnsureCoreWebView2Async();
                if (this.DataContext is WinImageViewModel vm)
                {
                    await vm.SetWebView(webView);
                }
            };
        }
    }
}