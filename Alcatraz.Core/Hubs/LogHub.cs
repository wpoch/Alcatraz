using System;
using System.Linq;
using Alcatraz.Core.Log;
using Raven.Client;
using SignalR.Hubs;

namespace Alcatraz.Core.Hubs
{
    [HubName("logHub")]
    public class LogHub : Hub
    {
        private readonly IDocumentStore _documentStore;

        public LogHub(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        //BUG: Unbound result...
        public LogMessage[] GetLogs()
        {
            using (var session = _documentStore.OpenSession())
            {
                return session.Query<LogMessage>()
                    .Where(x => x.TimeStamp > DateTime.Now.AddDays(-1))
                    .OrderByDescending(x => x.TimeStamp)
                    .Take(500)
                    .ToArray();
            }
        }
    }
}