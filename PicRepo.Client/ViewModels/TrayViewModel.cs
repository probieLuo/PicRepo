using PicRepo.Client.Views;
using System.Windows;

namespace PicRepo.Client.ViewModels
{
    public class TrayViewModel : BindableBase
    {
        private readonly IContainerProvider container;

        public DelegateCommand ShowWindowCommand { get; }
        public DelegateCommand ExitCommand { get; }

        public TrayViewModel(IContainerProvider container)
        {
            ShowWindowCommand = new DelegateCommand(ShowWindow);
            ExitCommand = new DelegateCommand(Exit);
            this.container = container;
        }

        private void ShowWindow()
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow == null)
                {
                    mainWindow = container.Resolve<MainWindow>();
                    Application.Current.MainWindow = mainWindow;
                }
                var vm = mainWindow.DataContext as MainWindowViewModel;
                if (vm != null)
                {
                    vm.ShowInTaskbar = true;
                    //vm.WindowState = WindowState.Normal;
                    await vm.Refresh();
                }
                var miniWin = container.Resolve<WinMini>();
                miniWin.Hide();
                mainWindow.Activate();
                mainWindow.Visibility = Visibility.Visible;
            });
        }

        private void Exit()
        {
            Application.Current.Shutdown();
        }
    }
}