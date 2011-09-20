using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Timers;
using Nalarium;
using NetInterop.Routing.Core;
using NetInterop.Routing.Image.Configuration;

namespace NetInterop.Routing.Image
{
    [Export(typeof(Module))]
    [ModuleMetadata("Image")]
    public class ImageModule : Module
    {
        [ImportMany]
        private List<Handler> _parserList;

        private Timer _timer;
        public byte Version { get; set; }

        public Map<string, byte[]> ImageDatabase { get; set; }

        public Map<string, string> VirtualPhysicalMap { get; set; }

        protected override void Initialize()
        {
            VirtualPhysicalMap = new Map<string, string>();
            ImageDatabase = new Map<string, byte[]>();
            foreach (MappingElement networkData in ImageSection.GetConfigSection().Mappings)
            {
                var v = networkData.Virtual;
                var p = networkData.Physical;
                VirtualPhysicalMap.Add(v, p);
                if (!ImageDatabase.Any(o => o.Key.Equals((o))))
                {
                    if (System.IO.File.Exists(p))
                    {
                        ImageDatabase.Add(p, System.IO.File.ReadAllBytes(p));
                    }
                }
            }
        }

        public void RequestReceived(object sender, ModuleEventArgs args)
        {
            HeaderPackage package = args.HeaderPackage;
            var imageRequestHeader = package.GetHeader<ImageHeader>();
            var ipHeader = package.GetHeader<IPHeader>();
            RouteMessage(args.DeviceID, new List<IHeader>(new IHeader[] { imageRequestHeader, ipHeader }));
        }

        protected override void AcceptHandoff(string deviceID, string nextHandlerName, PacketData packetData, Value[] parameterArray)
        {
            RouteMessage(deviceID, CatalogHeaderListInBranch(nextHandlerName, parameterArray), parameterArray);
        }

        internal void RouteMessage(string deviceID, List<IHeader> headerList, params Value[] parameterArray)
        {
            var imageHeader = (ImageHeader)headerList.SingleOrDefault(p => p is ImageHeader);
            if (imageHeader.Operation == 0)
            {
                HandleRequest(deviceID, imageHeader, parameterArray);
            }
            else
            {
                HandleResponse(deviceID, imageHeader, parameterArray);
            }
        }

        private void HandleRequest(string deviceID, ImageHeader imageHeader, params Value[] parameterArray)
        {
            Log.Write("IMAGE", "SendingRequest");

            PacketData packetData = GeneratePacketData(imageHeader);

            //++ send to system module for ip packaging
            Handoff(deviceID, typeof(UdpHeader), packetData,
                    Controller.OverrideDefaultValueData(parameterArray,
                    Value.Raw("UdpHeader:SourcePort", (ushort)5200),
                    Value.Raw("UdpHeader:DestinationPort", (ushort)5201),
                //TODO: what the heck? UDP should set this
                    Value.Raw("IPHeader:Protocol", (byte)17),
                //Value.Raw<byte>("IPHeader:TypeOfService", 0xc0),
                    Value.Raw("IPHeader:TTL", (byte)128),
                    Value.Raw("IPHeader:SourceAddress", Controller.DeviceConfigurationMap[deviceID].PrimaryIPConfiguration.Address))
                );
        }

        private void HandleResponse(string deviceID, ImageHeader imageHeader, params Value[] parameterArray)
        {
            var data = System.Text.UTF8Encoding.UTF8.GetString(imageHeader.Data);
            var lineArray = data.Split('\n');
            var name = string.Empty;
            foreach (var s in lineArray)
            {
                var partArray = s.Split('=');
                if (partArray[0].Equals("Name"))
                {
                    name = partArray[1];
                }
            }
            if (String.IsNullOrEmpty(name))
            {
                return;
            }

            byte[] buffer = null;
            var physical = VirtualPhysicalMap[name];
            if (String.IsNullOrEmpty(physical))
            {
                return;
            }
            if (ImageDatabase.Any(p => p.Key.Equals((physical))))
            {
                buffer = ImageDatabase[physical];
            }
            if (buffer == null)
            {
                return;
            }
            Log.Write("IMAGE", "SendingResponse.", "Size:" + buffer.Length.ToString());
            //++ parsers build the header
            var imageResponse = (ImageHeader)Controller.GetSingletonHandler("Image").Build(
                Value.Raw("ImageHeader:Operation", (byte)1),
                Value.Raw("ImageHeader:Data", (byte[])buffer)
            );

            PacketData packetData = GeneratePacketData(imageResponse);

            //++ send to system module for ip packaging
            Handoff(deviceID, typeof(UdpHeader), packetData,
                    Controller.OverrideDefaultValueData(parameterArray,
                        Value.Raw("UdpHeader:SourcePort", (ushort)5201),
                        Value.Raw("UdpHeader:DestinationPort", (ushort)5200),
                //TODO: what the heck? UDP should set this
                        Value.Raw("IPHeader:Protocol", (byte)17),
                //Value.Raw<byte>("IPHeader:TypeOfService", 0xc0),
                        Value.Raw("IPHeader:TTL", (byte)128),
                        Value.Raw("IPHeader:SourceAddress", Controller.DeviceConfigurationMap[deviceID].PrimaryIPConfiguration.Address))
                );
        }
    }
}