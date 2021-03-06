using System;

namespace NetInterop.Routing
{
    public static class DataConverter
    {
        public static System.Net.IPAddress ReverseIpAddress(System.Net.IPAddress iPAddress)
        {
            byte[] byteArray = iPAddress.GetAddressBytes();
            Array.Reverse(byteArray);
            return new System.Net.IPAddress(byteArray);
        }
    }
}