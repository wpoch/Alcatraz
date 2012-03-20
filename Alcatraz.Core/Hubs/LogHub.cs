using System;
using SignalR.Hubs;

namespace Alcatraz.Core.Hubs
{
    [HubName("log")]
    public class LogHub : Hub
    {
        public void Ping()
        {
            throw new NotImplementedException();
        }
    }
}