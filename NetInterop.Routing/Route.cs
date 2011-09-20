using System;

namespace NetInterop.Routing
{
    public class Route
    {
        public int AdministrativeDistance { get; set; }
        public IPAddress Network { get; set; }
        public IPAddress Mask { get; set; }
        public IPAddress NextHop { get; set; }
        public uint Metric { get; set; }
        public IPAddress Via { get; set; }
        public DateTime TimeUpdated { get; set; }
        public Module OwningModule { get; set; }

        public String ModuleName
        {
            get
            {
                return OwningModule.Name;
            }
        }

        public TimeSpan Age
        {
            get
            {
                return DateTime.Now - TimeUpdated;
            }
        }
    }
}