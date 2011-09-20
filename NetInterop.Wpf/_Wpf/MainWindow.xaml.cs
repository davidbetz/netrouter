using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using NetInterop.Routing;

namespace NetInterop.Wpf
{
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty IsBigProperty = DependencyProperty.Register("IsBig", typeof(Boolean),
                                                                                              typeof(MainWindow),
                                                                                              new FrameworkPropertyMetadata
                                                                                                  (false,
                                                                                                   FrameworkPropertyMetadataOptions
                                                                                                       .AffectsRender));

        public RoutedUICommand RefreshFilterCommand = new RoutedUICommand("RefreshFilter", "RefreshFilter",
                                                                          typeof(MainWindow));

        private Thread _thread;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            HandlerData = new ObservableCollection<Tuple<Int32, List<HandlerData>, String>>();

            DataView.Filter = (o) =>
                              {
                                  var data = o as Tuple<Int32, List<HandlerData>, String>;
                                  string summary = data.Item3.ToUpper();
                                  String text = txtFilter.Text.ToUpper().Trim();
                                  String[] partArray = text.Split(' ');
                                  IEnumerable<string> allowed = partArray.Where(p => !p.StartsWith("!"));
                                  IEnumerable<string> blocked = partArray.Where(p => p.StartsWith("!"));
                                  if (String.IsNullOrEmpty(text))
                                  {
                                      return true;
                                  }
                                  foreach (string item in blocked)
                                  {
                                      string comparison = item.Substring(1, item.Length - 1);
                                      if (summary.Contains(comparison))
                                      {
                                          return false;
                                      }
                                      continue;
                                  }
                                  foreach (string item in allowed)
                                  {
                                      if (!summary.Contains(item))
                                      {
                                          return false;
                                      }
                                      return true;
                                  }
                                  return false;
                              };

            //btnFilter.Click += (s, e) =>
            //{
            //    DataView.Refresh();
            //};

            Closed += MainWindow_Closed;
            Loaded += MainWindow_Loaded;
        }

        public Boolean IsBig
        {
            get
            {
                return (Boolean)base.GetValue(IsBigProperty);
            }
            set
            {
                base.SetValue(IsBigProperty, value);
            }
        }

        private ObservableCollection<Tuple<Int32, List<HandlerData>, String>> HandlerData { get; set; }

        public ICollectionView DataView
        {
            get
            {
                return CollectionViewSource.GetDefaultView(HandlerData);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Begin();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _thread.Abort();
        }

        public void Begin()
        {
            RoutingController rc = RoutingController.Create();
            rc.Parsed += (s, e) =>
                         Dispatcher.Invoke(
                             new Action(() =>
                                        HandlerData.Insert(0,
                                                           new Tuple<Int32, List<HandlerData>, String>(e.Index, e.HandlerData, e.Summary))));
            _thread = new Thread(new ParameterizedThreadStart(delegate
                                                              {
                                                                  rc.Enable();
                                                              }));
            _thread.Start();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DataView.Refresh();
        }

        private void CommandBinding_Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IsBig = !IsBig;
        }

        //private void CollectionViewSource_Filter(object sender, System.Windows.Data.FilterEventArgs e)
        //{
        //    if (String.IsNullOrEmpty(txtFilter.Text))
        //    {
        //        e.Accepted = true;
        //        return;
        //    }
        //    var data = e.Item as Tuple<List<HandlerData>, String>;
        //    if (data.Item2.ToUpper().Contains(txtFilter.Text.ToUpper()))
        //    {
        //        e.Accepted = true;
        //    }
        //    else
        //    {
        //        e.Accepted = false;
        //    }
        //}

        //private void btnFilter_Click(object sender, RoutedEventArgs e)
        //{
        //}
    }
}