using System;

namespace NetInterop.Wpf
{
    public sealed class Program
    {
        [STAThread, LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        public static void Main(String[] args)
        {
            new CoreApplication().Run();
        }
    }
}