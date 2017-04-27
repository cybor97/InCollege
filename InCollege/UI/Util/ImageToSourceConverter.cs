using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace InCollege.UI.Util
{
    class ImageToSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
                return new BitmapImage(new Uri($"pack://application:,,,/InCollege.Core;component/Resources/{value}.png"));
            else return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
