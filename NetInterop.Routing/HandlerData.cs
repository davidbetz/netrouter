using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NetInterop.Routing
{
    [DebuggerDisplay("{Name}")]
    public class HandlerData
    {
        public HandlerData()
        {
            PropertyList = new List<HandlerDataValue>();
            Children = new List<HandlerData>();
        }

        public String Name { get; set; }

        public List<HandlerDataValue> PropertyList { get; set; }
        public List<HandlerData> Children { get; set; }

        public Boolean HasMoreThanOneChild
        {
            get
            {
                return Children.Count > 1;
            }
        }

        public HandlerData FirstChild
        {
            get
            {
                return Children.Count > 0 ? Children[0] : null;
            }
        }
    }
}