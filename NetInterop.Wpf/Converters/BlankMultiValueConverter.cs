using System;
using System.Globalization;
using System.Windows.Data;

namespace NetInterop.Wpf.Converters
{
    public class BlankMultiValueConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[]
                   {
                   };
        }

        #endregion
    }
}