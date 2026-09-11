using PicRepo.Client.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PicRepo.Client.Views
{
    /// <summary>
    /// WinMini.xaml 的交互逻辑
    /// </summary>
    public partial class WinMini : Window
    {
        public WinMini()
        {
            InitializeComponent();
        }

        private Point _downPosition;
        private bool _isDragging;

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _downPosition = e.GetPosition(this);
            _isDragging = false;
            ((UIElement)sender).CaptureMouse();
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (!((UIElement)sender).IsMouseCaptured) return;

            var currentPosition = e.GetPosition(this);
            var offset = currentPosition - _downPosition;

            if (!_isDragging &&
                (Math.Abs(offset.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(offset.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                _isDragging = true;
            }

            if (_isDragging)
            {
                Left += offset.X;
                Top += offset.Y;
            }
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((UIElement)sender).ReleaseMouseCapture();

            if (!_isDragging)
            {
                if (DataContext is WinMiniViewModel vm)
                {
                    vm.UploadImageCommand.Execute();
                }
            }

            _isDragging = false;
        }
    }
}