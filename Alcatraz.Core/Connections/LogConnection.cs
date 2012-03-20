using System;
using System.Threading.Tasks;
using SignalR;
using SignalR.Hosting;

namespace Alcatraz.Core.Connections
{
    public class LogConnection : PersistentConnection
    {
        protected override Task OnConnectedAsync(IRequest request, string connectionId)
        {
            return Connection.Broadcast(String.Format("{0} connected from {1}", connectionId, request.Headers["User-Agent"]));
        }

        protected override Task OnReceivedAsync(string connectionId, string data)
        {
            return Connection.Broadcast(data);
        }
    }
}