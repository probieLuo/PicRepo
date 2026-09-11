using PicRepo.Client.Helper;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PicRepo.Client.Converters
{
    internal class PicRepoTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string parameterStr && value is PicRepoType type)
            {
                switch (parameterStr)
                {
                    case "Github":
                        return type == PicRepoType.GitHub ? Visibility.Visible : Visibility.Collapsed;
                    case "Gitee":
                        return type == PicRepoType.Gitee ? Visibility.Visible : Visibility.Collapsed;
                    default:
                        return Visibility.Collapsed;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
