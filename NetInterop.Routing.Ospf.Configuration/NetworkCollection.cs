using System;
using System.Configuration;
using Nalarium.Configuration;

namespace NetInterop.Routing.Ospf.Configuration
{
    public class NetworkCollection : CommentableCollection<NetworkElement>
    {
        ////- ~RootPath -//
        //[ConfigurationProperty("rootPath", DefaultValue = "/Sequence_/")]
        //public String RootPath
        //{
        //    get
        //    {
        //        return (String)this["rootPath"];
        //    }
        //}

        //+
        //- #GetElementKey -//
        protected override Object GetElementKey(ConfigurationElement element)
        {
            var e = (element as NetworkElement);
            return e.Area + e.Network + e.Mask;
        }
    }
}