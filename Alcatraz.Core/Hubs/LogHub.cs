using System;
using SignalR.Hubs;

namespace Alcatraz.Core.Hubs
{
    [HubName("logHub")]
    public class LogHub : Hub
    {
        public void Ping()
        {
            throw new NotImplementedException();
        }
    }
}