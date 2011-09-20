using System.Windows;

namespace NetInterop.Wpf
{
    internal class CoreApplication : Application
    {
        private readonly MainWindow window;

        internal CoreApplication()
        {
            window = new MainWindow();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            window.Show();
        }
    }
}