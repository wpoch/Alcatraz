using System;
using System.Linq;
using Alcatraz.Core.Log;
using Alcatraz.Core.Server;
using Raven.Client;
using SignalR.Hubs;

namespace Alcatraz.Core.Hubs
{
    [HubName("logHub")]
    public class LogHub : Hub
    {
        public LogMessage[] GetLogs()
        {
            using (IDocumentSession session = LogServer.DocumentStore.OpenSession())
            {
                return session.Query<LogMessage>()
                    .Where(x => x.TimeStamp > DateTime.Now.AddDays(-1))
                    .OrderByDescending(x => x.TimeStamp).ToArray();
            }
        }
    }
}