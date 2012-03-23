using System;
using System.IO;
using System.Linq;
using Alcatraz.Core.Server;
using Alcatraz.Core.Settings;
using Alcatraz.Service.Properties;
using Topshelf;
using log4net;
using log4net.Config;

namespace Alcatraz.Service
{
    internal class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        private static void Main(string[] args)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("log4net.config"));

            var settings = new ApplicationSettings
            {
                BroadcastUrl = Settings.Default.BORADCAST_URL,
                DatabasePort = Settings.Default.DATABASE_PORT
            };

            try
            {
                settings.UdpPorts = Settings.Default.UDPRECEIVERS_PORTS
                                        .Cast<string>()
                                        .Select(x => Convert.ToInt32(x))
                                        .ToArray();
            }
            catch (Exception ex)
            {
                Log.Fatal("Can't recover the UDP Receivers ports.", ex);
            }

            HostFactory.Run(x =>
            {
                x.Service<LogServer>(s =>
                {
                    s.SetServiceName("alcatraz");
                    s.ConstructUsing(name => new LogServer(settings));
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.SetDescription("Alcatraz: All Your Log Are Belong To Us.");
                x.SetDisplayName("Alcatraz");
                x.SetServiceName("Alcatraz");
            });
        }
    }
}