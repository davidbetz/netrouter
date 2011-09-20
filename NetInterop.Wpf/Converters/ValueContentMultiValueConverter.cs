using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using NetInterop.Routing;

namespace NetInterop.Wpf.Converters
{
    public class ValueContentMultiValueConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
            {
                return null;
            }
            if (values.Length != 5)
            {
                return null;
            }
            object value = values[0];
            if (value == null)
            {
                return null;
            }
            var SimpleTemplate = values[1] as DataTemplate;
            var HasStandardFormatTemplate = values[2] as DataTemplate;
            var IsHeaderTemplate = values[3] as DataTemplate;
            var IsDataTableTemplate = values[4] as DataTemplate;

            var parserDataValue = (value as HandlerDataValue);
            if (parserDataValue == null)
            {
                return SimpleTemplate.LoadContent();
            }
            if (parserDataValue.Value is IHasStandardFormat)
            {
                return HasStandardFormatTemplate.LoadContent();
            }
            if (parserDataValue.Value is IHeader)
            {
                return IsHeaderTemplate.LoadContent();
            }
            Type type = parserDataValue.Value.GetType();
            object[] attributeArray = type.GetCustomAttributes(false);
            if (attributeArray.Any(p => p is DataTableAttribute))
            {
                return IsDataTableTemplate.LoadContent();
            }
            return SimpleTemplate.LoadContent();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}