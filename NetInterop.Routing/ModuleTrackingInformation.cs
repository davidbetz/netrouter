using System;
using System.Collections.Generic;
using System.Threading;
using Nalarium;
using Nalarium.Reflection;

namespace NetInterop.Routing
{
    public class ModuleTrackingInformation
    {
        private ModuleTrackingInformation()
        {
            HandlerTrackingInformationMap = new Map<string, HandlerTrackingInformation>();
            ModuleCallbackDictionary = new Map<string, List<ModuleEvent>>();
        }

        public Type Type { get; set; }
        public String Namespace { get; set; }
        public Thread Thread { get; set; }
        public Module Module { get; set; }
        public Map<string, HandlerTrackingInformation> HandlerTrackingInformationMap { get; set; }
        public Map<string, List<ModuleEvent>> ModuleCallbackDictionary { get; set; }

        public static ModuleTrackingInformation Create(Module module, Type moduleType)
        {
            var information = new ModuleTrackingInformation
                              {
                                  Module = module,
                                  Type = moduleType,
                                  Namespace = moduleType.Namespace
                              };
            module.ModuleTrackingInformation = information;
            var attribute = AttributeReader.ReadAssemblyAttribute<AssemblyModuleAttribute>(module);
            if (attribute == null)
            {
                throw new InvalidOperationException(string.Format("Module missing AssemblyModuleAttribute ({0}).", moduleType.FullName));
            }
            module.Name = attribute.Name.ToUpper();
            //var attribute = AttributeReader.ReadTypeAttribute<ModuleMetadataAttribute>(module);
            //if (attribute == null)
            //{
            //    throw new InvalidOperationException(string.Format("Module missing ModuleMetadataAttribute ({0}).", moduleType.FullName));
            //}
            //module.Name = attribute.Name.ToUpper();
            return information;
        }
    }
}