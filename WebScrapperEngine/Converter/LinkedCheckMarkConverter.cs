using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using WebScrapperEngine.Entity;

namespace WebScrapperEngine.Converter
{
    public class LinkedCheckMarkConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
                return false;

            var bookmarkCreations = value as IEnumerable<BookmarkCreation>;

            if (bookmarkCreations == null)
                return false;

            return bookmarkCreations.Count() > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
