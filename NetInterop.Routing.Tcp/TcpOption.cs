using System;

namespace NetInterop.Routing.Tcp
{
    public class TcpOption
    {
        public Byte Size { get; set; }
        public String Name { get; set; }
        public Int32 RFC { get; set; }
        public String Description { get; set; }

        public static TcpOption Create(Byte size, String name, Int32 rfc, String description)
        {
            return new TcpOption
            {
                Size = size,
                Name = name,
                RFC = rfc,
                Description = description
            };
        }
    }
}