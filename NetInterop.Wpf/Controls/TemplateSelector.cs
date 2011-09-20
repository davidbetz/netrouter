using System.Windows;
using System.Windows.Controls;
using NetInterop.Routing;

namespace NetInterop.Wpf.Controls
{
    public class TemplateSelector : DataTemplateSelector
    {
        public DataTemplate SimpleTemplate { get; set; }
        public DataTemplate HasStandardFormatTemplate { get; set; }
        public DataTemplate IsHeaderTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var parserDataValue = (item as HandlerDataValue);
            if (parserDataValue == null)
            {
                return SimpleTemplate;
            }
            if (parserDataValue.Value is IHasStandardFormat)
            {
                return HasStandardFormatTemplate;
            }
            if (parserDataValue.Value is IHeader)
            {
                return IsHeaderTemplate;
            }
            return SimpleTemplate;
        }
    }
}