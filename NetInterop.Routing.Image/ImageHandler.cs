using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Nalarium;
using NetInterop.Routing.Core;

namespace NetInterop.Routing.Image
{
    [Export(typeof(Handler))]
    [HandlerMetadata("Image", "UDP")]
    public class ImageHandler : Handler
    {
        public static GlobalProperty ImageHeaderProperty = GlobalProperty.Register("ImageHeader", typeof(ImageHeader),
                                                                                         typeof(ImageHandler));

        protected override Boolean CheckForNext()
        {
            return ((UdpHeader)GetValue(UdpHandler.UdpHeaderProperty)).DestinationPort == 5200;
        }

        public override Handler Parse()
        {
            //var buffer = base.LoadUntilCharacterFound(',', ';');
            //var buffer = base.LoadUntilCharacterFound(new char[] { ',', ';' });
            //var buffer = base.LoadUntilCharacterFound("\r\n");

            //var data = System.Text.UTF8Encoding.UTF8.GetString(base.LoadRest());
            //var partArray = data.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            var header = LoadHeader<ImageHeader>();
            if (header.Data == null)
            {
                return null;
            }

            SetValue(ImageHeaderProperty, header);

            return GetNextHandler();
        }

        public override void Initialize(Module module)
        {
            SeriesCompleted += (module as ImageModule).RequestReceived;
        }

        protected override IHeader Build(Module module, params Value[] parameterArray)
        {
            var imageRequestHeader = CreateHeader<ImageHeader>(parameterArray);
            return imageRequestHeader;
        }

        public override PacketData GetBytes(IHeader header, PacketData packetData)
        {
            var imageHeader = (ImageHeader)header;
            var currentData = new List<byte>();
            currentData.Add(imageHeader.Operation);
            currentData.AddRange(imageHeader.Data);
            return packetData.UpdateData(currentData);
        }
    }
}