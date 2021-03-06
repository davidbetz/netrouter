using System;
using System.ComponentModel.Composition;
using NetInterop.Routing.Core;
using NetInterop.Routing.Tcp;

namespace NetInterop.Routing.Http
{
    [Export(typeof(Handler))]
    [HandlerMetadata("HTTP", "TCP")]
    public class HttpHandler : Handler
    {
        public static GlobalProperty HttpHeaderProperty = GlobalProperty.Register("HttpHeader", typeof(HttpHeader), typeof(HttpHandler));

        public override ushort LayerID
        {
            get
            {
                return 80;
            }
        }

        protected override Boolean CheckForNext()
        {
            if (HandlerNameStack.Contains("TCP") || HandlerNameStack.Contains("UDP"))
            {
                var identification = LoadAndScroll<HttpIdentification>();
                return identification == HttpIdentification.HttpIdentifier;
            }
            return false;
        }

        public override Handler Parse()
        {
            var header = new HttpHeader();

            SetValue(HttpHeaderProperty, header);

            return GetNextHandler();
        }
    }
}