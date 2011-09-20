using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using NetInterop.Routing;

namespace NetInterop.Wpf.Converters
{
    public class ValueDataSourceMultiValueConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
            {
                return null;
            }
            if (values.Length != 2)
            {
                return null;
            }
            if (values[1] != null && !(values[1] == DependencyProperty.UnsetValue))
            {
                return ConvertDataToHandlerDataValue(values[1]);
            }
            return values[0];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[]
                   {
                   };
        }

        #endregion

        private List<HandlerDataValue> ConvertDataToHandlerDataValue(Object rawData)
        {
            var data = new List<HandlerDataValue>();
            Type type = rawData.GetType();
            FieldInfo[] fieldInfoArray = type.GetFields();
            foreach (FieldInfo fieldInfo in fieldInfoArray)
            {
                data.Add(new HandlerDataValue
                         {
                             Name = fieldInfo.Name,
                             Value = fieldInfo.GetValue(rawData)
                         });
            }
            PropertyInfo[] propertyInfoArray = type.GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfoArray)
            {
                data.Add(new HandlerDataValue
                         {
                             Name = propertyInfo.Name,
                             Value = propertyInfo.GetValue(rawData, null)
                         });
            }
            return data;
        }
    }
}