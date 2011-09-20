using System;
using System.Globalization;
using System.Windows.Data;
using NetInterop.Routing;

namespace NetInterop.Wpf.Converters
{
    public class HasStandardFormatValueConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }
            if (value is IHasStandardFormat)
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}