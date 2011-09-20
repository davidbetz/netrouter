using System;
using System.Globalization;
using System.Windows.Data;
using NetInterop.Routing;

namespace NetInterop.Wpf.Converters
{
    public class StandardFormatValueConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            if (value is IHasStandardFormat)
            {
                var hasStandardFormat = (IHasStandardFormat)value;
                return hasStandardFormat.StandardFormat;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}