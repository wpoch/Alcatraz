using System;

namespace Alcatraz.Core.Settings
{
    public class ApplicationSettings
    {
        public Uri BroadcastUrl { get; set; }
        public int DatabasePort { get; set; }
        public int[] UdpPorts { get; set; }
    }
}