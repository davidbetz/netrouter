using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using NetInterop.Routing;

namespace NetInterop.Wpf.Converters
{
    public class DataTableValueConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            Type type = value.GetType();
            PropertyInfo[] propertyArray = type.GetProperties();
            var list = new List<HandlerDataValue>();
            foreach (PropertyInfo item in propertyArray)
            {
                list.Add(new HandlerDataValue
                         {
                             Name = item.Name,
                             Value = item.GetValue(value, null)
                         });
            }
            return list;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}