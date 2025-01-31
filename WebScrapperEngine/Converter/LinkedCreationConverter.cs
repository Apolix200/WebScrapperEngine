using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WebScrapperEngine.Converter
{
    public class LinkedCreationConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string creationId = (values[0] != null && values[0] != DependencyProperty.UnsetValue) ? System.Convert.ToString(values[0]) : null;
            string connectedId = (values[1] != null && values[1] != DependencyProperty.UnsetValue) ? System.Convert.ToString(values[1]) : null;
            return creationId == connectedId;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
