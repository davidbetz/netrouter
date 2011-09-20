using System.Windows;
using System.Windows.Controls;
using NetInterop.Routing;

namespace NetInterop.Wpf.Controls
{
    public class HandlerView : ContentControl
    {
        public static readonly DependencyProperty HandlerDataProperty = DependencyProperty.Register("HandlerData",
                                                                                                    typeof(HandlerData),
                                                                                                    typeof(HandlerView)
                                                                                                    ,
                                                                                                    new PropertyMetadata(
                                                                                                        (o, e) =>
                                                                                                        {
                                                                                                            //var parserData = e.NewValue as HandlerData;
                                                                                                            //StackPanel sp = new StackPanel();
                                                                                                            //var view = o as HandlerView;
                                                                                                            //view.Content = sp;
                                                                                                            //foreach (var item in parserData.Children)
                                                                                                            //{
                                                                                                            //    HandlerView pv = new HandlerView();
                                                                                                            //    pv.DataContext = item;
                                                                                                            //    pv.SetValue(HandlerView.HandlerDataProperty, item);
                                                                                                            //    sp.Children.Add(pv);
                                                                                                            //}
                                                                                                        }));

        //public static readonly DependencyProperty ColorSelectorProperty =
        //               DependencyProperty.RegisterAttached("ColorSelectorProperty", typeof(IColorSelector), typeof(DataBox),
        //               new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        //public static void SetHandlerDataSelector(UIElement element, HandlerData value)
        //{
        //    element.SetValue(HandlerDataProperty, value);
        //}
        //public static HandlerData GetHandlerDataSelector(UIElement element)
        //{
        //    return (HandlerData)element.GetValue(HandlerDataProperty);
        //}

        static HandlerView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HandlerView),
                                                     new FrameworkPropertyMetadata(typeof(HandlerView)));
        }

        public HandlerData HandlerData
        {
            get
            {
                return (HandlerData)base.GetValue(HandlerDataProperty);
            }
            set
            {
                base.SetValue(HandlerDataProperty, value);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        //public HandlerView()
        //{
        //    DefaultStyleKey = typeof(HandlerView);
        //}
    }
}