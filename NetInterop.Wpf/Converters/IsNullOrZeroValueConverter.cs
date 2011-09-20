using System;
using System.Globalization;
using System.Windows.Data;

namespace NetInterop.Wpf.Converters
{
    public class IsNullOrZeroValueConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return true;
            }
            int valueInt32;
            Int32.TryParse((value ?? string.Empty).ToString(), out valueInt32);
            if (valueInt32 == 0)
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        #endregion
    }
}