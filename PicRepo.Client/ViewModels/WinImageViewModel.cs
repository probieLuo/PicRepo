using Microsoft.Web.WebView2.Wpf;
using NLog;

namespace PicRepo.Client.ViewModels
{
    public class WinImageViewModel:BindableBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private WebView2? webView;

        public string Url { get; set; }
        public bool IsLoading { get; set; }

        public DelegateCommand SearchCommand { get; }
        public WinImageViewModel()
        {
            Url = "";
            SearchCommand = new DelegateCommand(OnSearch);
        }

        private async void OnSearch()
        {
            if (webView == null) return;
            try
            {
                IsLoading = true;
                var tcs = new TaskCompletionSource<bool>();
                webView.CoreWebView2.NavigationCompleted += (s, e) =>
                {
                    if (e.IsSuccess)
                        tcs.TrySetResult(true);
                    else
                        tcs.TrySetException(new Exception($"导航失败: {e.WebErrorStatus}"));
                };
                webView?.CoreWebView2.Navigate(Url);
                await tcs.Task;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally { IsLoading = false; }
        }

        public async Task SetWebView(WebView2 webView)
        {
            try
            {

                IsLoading = true;
                this.webView = webView; 
                var tcs = new TaskCompletionSource<bool>();
                webView.CoreWebView2.NavigationCompleted += (s, e) =>
                {
                    if (e.IsSuccess)
                        tcs.TrySetResult(true);
                    else
                        tcs.TrySetException(new Exception($"导航失败: {e.WebErrorStatus}"));
                };
                webView?.CoreWebView2.Navigate(Url);
                await tcs.Task;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally { IsLoading = false; }
        }
    }
}
