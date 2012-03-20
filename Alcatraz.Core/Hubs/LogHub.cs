using System;
using Alcatraz.Core.Log;
using Alcatraz.Core.Server;
using SignalR.Hubs;
using System.Linq;

namespace Alcatraz.Core.Hubs
{
    [HubName("logHub")]
    public class LogHub : Hub
    {
        public LogMessage[] GetLogs()
        {
            using (var session = LogServer.DocumentStore.OpenSession())
            {
                return session.Query<LogMessage>()
                    .Where(x => x.TimeStamp > DateTime.Now.AddDays(-1))
                    .OrderByDescending(x => x.TimeStamp).ToArray();
            }
        }
    }
}