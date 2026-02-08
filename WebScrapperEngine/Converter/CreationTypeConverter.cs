using System;
using System.Globalization;
using System.Windows.Data;

namespace WebScrapperEngine.Converter
{
    public class CreationTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (CreationType)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
