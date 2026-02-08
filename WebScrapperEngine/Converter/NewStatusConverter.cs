using System;
using System.Globalization;
using System.Windows.Data;

namespace WebScrapperEngine.Converter
{
    public class NewStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (NewStatus)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
