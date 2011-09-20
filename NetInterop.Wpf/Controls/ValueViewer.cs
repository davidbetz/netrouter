using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using NetInterop.Routing;

namespace NetInterop.Wpf.Controls
{
    public class ValueViewer : ContentControl
    {
        public static readonly DependencyProperty PropertyListProperty = DependencyProperty.Register("PropertyList",
                                                                                                     typeof(
                                                                                                         List
                                                                                                         <
                                                                                                         HandlerDataValue
                                                                                                         >),
                                                                                                     typeof(ValueViewer
                                                                                                         )
                                                                                                     ,
                                                                                                     new PropertyMetadata
                                                                                                         ((o, e) =>
                                                                                                          {
                                                                                                              Debug.
                                                                                                                  Write
                                                                                                                  ("");
                                                                                                          }));

        public static readonly DependencyProperty HeaderValueProperty = DependencyProperty.Register("HeaderValue",
                                                                                                    typeof(Object),
                                                                                                    typeof(ValueViewer));

        static ValueViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ValueViewer),
                                                     new FrameworkPropertyMetadata(typeof(ValueViewer)));
        }

        public List<HandlerDataValue> PropertyList
        {
            get
            {
                return (List<HandlerDataValue>)base.GetValue(PropertyListProperty);
            }
            set
            {
                base.SetValue(PropertyListProperty, value);
            }
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
            //DataContext = this;
        }

        //public ValueViewer()
        //{
        //    DataContextChanged += new DependencyPropertyChangedEventHandler(ValueViewer_DataContextChanged);
        //}

        //void ValueViewer_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    HeaderValue = e.NewValue;
        //}
    }
}