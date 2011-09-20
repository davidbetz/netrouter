using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace NetInterop.Wpf.Controls
{
    public class PropertyListViewer : ContentControl
    {
        public static readonly DependencyProperty HeaderValueProperty = DependencyProperty.Register("HeaderValue",
                                                                                                    typeof(Object),
                                                                                                    typeof(
                                                                                                        PropertyListViewer
                                                                                                        )
                                                                                                    ,
                                                                                                    new PropertyMetadata
                                                                                                        ((o, e) =>
                                                                                                         {
                                                                                                             Debug.
                                                                                                                 Write
                                                                                                                 ("");
                                                                                                         }));

        static PropertyListViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyListViewer),
                                                     new FrameworkPropertyMetadata(typeof(PropertyListViewer)));
        }

        public Object HeaderValue
        {
            get
            {
                return base.GetValue(HeaderValueProperty);
            }
            set
            {
                base.SetValue(HeaderValueProperty, value);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}