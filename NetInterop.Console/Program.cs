using System;
using System.Text;
using Nalarium;
using NetInterop.Routing;
using System.Threading;
using NetInterop.Routing.Core;
using Monitor = NetInterop.Routing.Monitor;

namespace NetInterop.Console
{
    public class Program
    {
        public void ReportStatus(string message)
        {
            System.Console.Write(message);
        }

        public void SaveMessage(string message)
        {
        }

        public void WriteNewLine()
        {
            System.Console.WriteLine();
        }

        public void Begin()
        {
            //ParseController pc = ParseController.Create();
            ////pc.Parsed += (s, e) => ShowParsedData(e.HandlerData);
            //pc.Parsed += (s, e) => System.Console.WriteLine(e.Summary);
            ////_pc.VerboseOutput = new Write(ReportStatus);
            ////WinpcapBridge.Bridge.BeforeCallback = new WinpcapBridge.Bridge.CallbackPlain(WriteNewLine);
            //Monitor.ActiveCallbackIntPtr = (length, array) =>
            //                                   {
            //                                       //pc.Begin(length, array);
            //                                       Task.Factory.StartNew(() => pc.Begin(length, array));
            //                                   };
            ////WinpcapBridge.Bridge.AfterCallback = new WinpcapBridge.Bridge.CallbackPlain(WriteNewLine);
            ////NetInterop.Bridge.Monitor.RunIt("ip proto 89");
            //Dictionary<int, string> deviceList = Monitor.GetDeviceList();
            //foreach (var item in deviceList)
            //{
            //    System.Console.WriteLine("{0} {1}", item.Key, item.Value);
            //}
            ////NetInterop.Bridge.Monitor.RunIt(1, "ip proto 88");
            //Monitor.RunIt(4, null);
            //var process = Process.GetCurrentProcess();
            //process.ProcessorAffinity = (IntPtr)(((Int64)process.ProcessorAffinity) & 0x0002);
            //var wh = new AutoResetEvent(false);
            //var a = NetInterop.Routing.Monitor.GetDeviceDictionary();
            var a = Monitor.GetDeviceDictionary();
            RoutingController controller = RoutingController.Create();
            var monitor = new Thread(new ThreadStart(() =>
            {
                System.Console.CancelKeyPress += (s, e) =>
                {
                    controller.Disable();
                    e.Cancel = true;
                };
                RoutingController.VerboseOutput = ReportStatus;
                //controller.EnableExtensiveOutput = true;
                controller.Parsed += controller_Parsed;
                controller.Enable("not tcp port 3389");
                //controller.Enable(4, "udp port 520");//, "tcp dst port 179 or tcp src port 179 or tcp dst port 676");
            }));
            monitor.Start();
            //++ h
            //const string deviceID = "F063AD2D-214E-4CE2-AA42-3CFE31456FB4";
            //++ w
            const string deviceID = "DA2C93D2-9B36-41D7-8BB1-00D74E22A71C";
            //++ wv
            //const string deviceID = "24290CE3-F5E3-4988-9108-A22614CBEBE5";
            //var destination = IPAddress.From("10.1.155.1");
            ////var destination = ip_address.From("224.0.0.140");
            //var handler = controller.GetRequestHandler("Image:Image");
            //var header = handler.BuildRequest(Value.Create("Name", "image"));
            //handler.Submit(header, destination, deviceID);

            var destination = IPAddress.From("10.1.1.254");
            //var destination = ip_address.From("224.0.0.140");
            ushort i = 1;
            while (true)
            {
                System.Console.ReadLine();
                //controller.Submit(deviceID, "Image:Image", new[] { 
                //    Value.Raw("ImageHeader:Operation", (byte)0),
                //    Value.Raw("ImageHeader:Data", UTF8Encoding.UTF8.GetBytes("Name=image")),
                //    Value.Raw("IPHeader:DestinationAddress", destination),
                //});
                /*
                controller.Submit(deviceID, "Image:Image", new[] { 
                    Value.Raw("ImageHeader:Operation", (byte)1),
                    Value.Raw("ImageHeader:Data", UTF8Encoding.UTF8.GetBytes("Name=image")),
                    Value.Raw("IPHeader:DestinationAddress", destination),
                   
                }); */
                //controller.Submit(deviceID, "Icmp:IcmpEcho", new[]
                //                                             {
                //                                                 Value.Raw("IPHeader:DestinationAddress", destination),
                //                                                 Value.Raw("IcmpHeader:Type", (byte) 8),
                //                                                 Value.Raw("IcmpHeader:Code", (byte) 0),
                //                                                 Value.Raw("IcmpEchoHeader:Identifier", (ushort) 1),
                //                                                 Value.Raw("IcmpEchoHeader:SequenceNumber", i++)
                //                                             });
            }
        }

        private void controller_Parsed(object sender, ParsedEventArgs args)
        {
            System.Console.WriteLine(args.Summary);
        }

        private void ShowParsedData(HandlerData data)
        {
            foreach (HandlerDataValue item in data.PropertyList)
            {
                System.Console.WriteLine(string.Format("{0}={1}", item.Name, item.Value));
            }
            foreach (HandlerData item in data.Children)
            {
                ShowParsedData(item);
            }
        }

        public static void Main()
        {
            //++ h
            //const string deviceID = "F063AD2D-214E-4CE2-AA42-3CFE31456FB4";
            //++ w
            const string deviceID = "DA2C93D2-9B36-41D7-8BB1-00D74E22A71C";
            //++ wv
            //const string deviceID = "24290CE3-F5E3-4988-9108-A22614CBEBE5";
            /*
            var context = NetInterop.Connection.Context.Create();
            NetInterop.Connection.Context.VerboseOutput += System.Console.WriteLine;
            var deviceDictionary = context.GetDeviceDictionary();
            var indexDictionary = context.GetDeviceIndexDictionary();
            context.Received += Begin;
            context.Run(deviceID);
            return;*/
            new Program().Begin();
        }

        private static void Begin(String deviceID, int size, IntPtr data)
        {
            System.Console.WriteLine(String.Format("Data from {0} of size {1}.", deviceID, size));
        }

        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
        }
    }
}