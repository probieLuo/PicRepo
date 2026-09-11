using PicRepo.Client.Helper;
using System.Globalization;
using System.Windows.Data;

namespace PicRepo.Client.Converters
{
    public class DataUnitValuesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ulong dataLen && dataLen > 0)
            {
                return $"{DataUnitHelper.GetLength(dataLen)}";
            }

            return "0 MB";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}