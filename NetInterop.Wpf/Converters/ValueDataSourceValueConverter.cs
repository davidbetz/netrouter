using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Nalarium;
using Nalarium.Reflection;
using NetInterop.Routing;
using IValueConverter = System.Windows.Data.IValueConverter;

namespace NetInterop.Wpf.Converters
{
    public class ValueDataSourceValueConverter : IValueConverter
    {
        private static Type _objectType = typeof(Object);
        private static Type _parserDataValueType = typeof(HandlerDataValue);

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            return ConvertDataToHandlerDataValue(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        private object ConvertDataToHandlerDataValue(Object rawData)
        {
            if (rawData is List<HandlerDataValue>)
            {
                return rawData;
            }
            var data = new List<HandlerDataValuePresentation>();
            Type type = rawData.GetType();
            FieldInfo[] fieldInfoArray = type.GetFields();
            foreach (FieldInfo fieldInfo in fieldInfoArray)
            {
                object value = fieldInfo.GetValue(rawData);
                if (value != null)
                {
                    data.Add(new HandlerDataValuePresentation
                             {
                                 Name = fieldInfo.Name,
                                 Value = value,
                                 //Type = _objectType
                             });
                }
            }
            PropertyInfo[] propertyInfoArray = type.GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfoArray)
            {
                object[] fieldOverrideAttributeArray =
                    AttributeReader.ReadPropertyAttributeArray<FieldOverrideAttribute>(propertyInfo);
                object[] fieldLabelAttributeArray =
                    AttributeReader.ReadPropertyAttributeArray<FieldLabelAttribute>(propertyInfo);
                String name;
                if (!Collection.IsNullOrEmpty(fieldLabelAttributeArray))
                {
                    name = (fieldLabelAttributeArray[0] as FieldLabelAttribute).Label;
                }
                else
                {
                    name = propertyInfo.Name;
                }
                String fieldOverride = String.Empty;
                if (!Collection.IsNullOrEmpty(fieldOverrideAttributeArray))
                {
                    fieldOverride = (fieldOverrideAttributeArray[0] as FieldOverrideAttribute).FieldName;
                }
                object value = propertyInfo.GetValue(rawData, null);
                if (value != null)
                {
                    data.Add(new HandlerDataValuePresentation
                             {
                                 Name = name,
                                 Value = value,
                                 ReplacedFieldName = fieldOverride
                             });
                }
            }
            IEnumerable<string> hasReplacedFieldName =
                data.Where(p => !String.IsNullOrEmpty(p.ReplacedFieldName)).Select(p => p.ReplacedFieldName);
            data.RemoveAll((b) =>
                           {
                               return hasReplacedFieldName.Contains(b.Name);
                           });
            return data;
        }

        #region Nested type: HandlerDataValuePresentation

        public class HandlerDataValuePresentation : HandlerDataValue
        {
            public String ReplacedFieldName { get; set; }
        }

        #endregion
    }
}