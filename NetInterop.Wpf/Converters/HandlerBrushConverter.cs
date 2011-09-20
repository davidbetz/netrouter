using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NetInterop.Wpf.Converters
{
    public class HandlerBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush whiteBrush = new SolidColorBrush(Colors.White);
        private static readonly SolidColorBrush blackBrush = new SolidColorBrush(Colors.Black);

        static HandlerBrushConverter()
        {
            HandlerColorDictionary = new Dictionary<String, Tuple<Brush, Brush>>();
            HandlerColorDictionary.Add("ARP",
                                       Tuple.Create<Brush, Brush>(
                                           new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d6e8ff")),
                                           blackBrush));
            HandlerColorDictionary.Add("TCP",
                                       Tuple.Create<Brush, Brush>(
                                           new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8dff7f")),
                                           blackBrush));
            HandlerColorDictionary.Add("OSPFHELLO",
                                       Tuple.Create<Brush, Brush>(
                                           new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff3d6")),
                                           blackBrush));
            HandlerColorDictionary.Add("ICMP",
                                       Tuple.Create<Brush, Brush>(blackBrush,
                                                                  new SolidColorBrush(
                                                                      (Color)ColorConverter.ConvertFromString("#00ff0e"))));
            HandlerColorDictionary.Add("UDP",
                                       Tuple.Create<Brush, Brush>(
                                           new SolidColorBrush((Color)ColorConverter.ConvertFromString("#70e0ff")),
                                           blackBrush));
            HandlerColorDictionary.Add("STP",
                                       Tuple.Create<Brush, Brush>(whiteBrush,
                                                                  new SolidColorBrush(
                                                                      (Color)ColorConverter.ConvertFromString("#ad8080"))));
        }

        private static Dictionary<String, Tuple<Brush, Brush>> HandlerColorDictionary { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            //+
            String valueString = value.ToString().Trim();
            String[] partArray = valueString.Split(' ');
            String finalPart = partArray[partArray.Length - 1];
            //+
            int parameterInt32;
            Int32.TryParse((parameter ?? String.Empty).ToString(), out parameterInt32);
            Tuple<Brush, Brush> tuple;
            if (!HandlerColorDictionary.ContainsKey(finalPart))
            {
                if (parameterInt32 == 1)
                {
                    return blackBrush;
                }
                else
                {
                    return whiteBrush;
                }
            }
            tuple = HandlerColorDictionary[finalPart];
            if (HandlerColorDictionary.ContainsKey(finalPart))
            {
                tuple = HandlerColorDictionary[finalPart];
            }
            if (parameterInt32 == 1)
            {
                return tuple.Item2;
            }
            else
            {
                return tuple.Item1;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        #endregion
    }
}