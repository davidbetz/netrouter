using System;
using System.Collections.Generic;
using Nalarium.Reflection;

namespace NetInterop.Routing
{
    public class HandlerTrackingInformation
    {
        internal HandlerTrackingInformation()
        {
            PotentialNextHandlerList = new List<string>();
        }

        public Type Type { get; set; }
        public Handler SingletonHandler { get; set; }
        public ModuleTrackingInformation ModuleTrackingInformation { get; set; }
        public List<string> PotentialNextHandlerList { get; set; }
        public List<string> ParentNameList { get; set; }
        public String Name { get; set; }

        public List<String> HeaderList { get; set; }

        internal static HandlerTrackingInformation Create()
        {
            return new HandlerTrackingInformation();
        }

        internal static HandlerTrackingInformation Create(Handler parser, Type parserType, ModuleTrackingInformation moduleTrackingInformation)
        {
            var information = new HandlerTrackingInformation
                              {
                                  SingletonHandler = parser,
                                  Type = parserType,
                                  ModuleTrackingInformation = moduleTrackingInformation
                              };
            parser.HandlerTrackingInformation = information;
            var attribute = AttributeReader.ReadTypeAttribute<HandlerMetadataAttribute>(parser);
            if (attribute == null)
            {
                throw new InvalidOperationException(string.Format("Handler missing HandlerMetadataAttribute ({0}).", parserType.FullName));
            }
            information.Name = attribute.Name.ToUpper();
            information.ParentNameList = attribute.ParentNameList;
            return information;
        }
    }
}