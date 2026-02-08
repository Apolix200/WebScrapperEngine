using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using WebScrapperEngine.Entity;

namespace WebScrapperEngine.Converter
{
    public class LinkedCreationConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null || values[0] == DependencyProperty.UnsetValue)
                return false;

            if (values[1] == null || values[1] == DependencyProperty.UnsetValue)
                return false;

            int creationId = System.Convert.ToInt32(values[0]);
            var bookmarkCreations = values[1] as IEnumerable<BookmarkCreation>;

            if (bookmarkCreations == null)
                return false;

            return bookmarkCreations.Any(bc => bc.CreationId == creationId);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
