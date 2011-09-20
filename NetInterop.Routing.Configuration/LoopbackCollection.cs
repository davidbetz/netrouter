using System;
using System.Configuration;
using Nalarium.Configuration;

namespace NetInterop.Routing.Configuration
{
    public class LoopbackCollection : CommentableCollection<LoopbackElement>
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
            return (element as LoopbackElement).ID;
        }
    }
}