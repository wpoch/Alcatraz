using System;
using Alcatraz.Core.Hubs;
using Alcatraz.Core.Log;
using Alcatraz.Core.Receivers;
using Alcatraz.Core.Settings;
using Raven.Client.Embedded;
using SignalR;
using SignalR.Hubs;
using log4net;

namespace Alcatraz.Core.Server
{
    public class LogServer : IDisposable
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(LogServer));
        private readonly ApplicationSettings _settings;
        private EmbeddableDocumentStore _documentStore;
        private RemoteOriginServer _server;
        private dynamic _clients;

        public LogServer(ApplicationSettings settings)
        {
            _settings = settings;
        }

        #region Initialization

        public void Start()
        {
            if (_log.IsDebugEnabled) _log.Debug("Starting Alcatraz engines...");
            InitLogStore();
            InitBroadcaster();
            InitReceivers();
            if (_log.IsDebugEnabled) _log.Debug("... Alcatraz running!");
        }

        public void Stop()
        {
            this.Dispose();
        }

        private void InitLogStore()
        {
            if (_log.IsDebugEnabled) _log.Debug("Init RavenDB...");

            _documentStore = new EmbeddableDocumentStore
                                {
                                    DataDirectory = "Data",
                                    UseEmbeddedHttpServer = true,
                                };
            _documentStore.Configuration.Port = _settings.DatabasePort;
            _documentStore.Initialize();

            if (_log.IsDebugEnabled) _log.Debug("... RavenDB initialized!");
        }

        private void InitBroadcaster()
        {
            if (_log.IsDebugEnabled) _log.Debug("Init SignalR...");

            _server = new RemoteOriginServer(_settings.BroadcastUrl);
            _server.DependencyResolver.Register(typeof(IJavaScriptProxyGenerator),
                                                () => new JsProxyGenerator(_server.DependencyResolver, _settings.BroadcastUrl));
            _server.DependencyResolver.Register(typeof(LogHub), () => new LogHub(_documentStore));

            // Enable the hubs route (/signalr)
            _server.EnableHubs();
            _server.Start();

            var connectionManager =
                _server.DependencyResolver.GetService(typeof(IConnectionManager)) as IConnectionManager;
            if(connectionManager == null)
                throw new NullReferenceException("Can't ");
            _clients = connectionManager.GetClients<LogHub>();

            if (_log.IsDebugEnabled) _log.Debug("...SignalR initialized!");
        }

        private void InitReceivers()
        {
            if (_log.IsDebugEnabled) _log.Debug("Init UDP Receivers...");
            
            foreach (var listeningPort in _settings.UdpPorts)
            {
                var receiver = new UdpReceiver { Port = listeningPort, OnLogMessageReceived = AddLogMessage };
                receiver.Initialize();                
            }

            if (_log.IsDebugEnabled) _log.Debug("... UDP Receivers initialized!");
        }

        #endregion        

        private void AddLogMessage(LogMessage logMsg)
        {
            if (_log.IsDebugEnabled) _log.Debug(logMsg);

            //Store the log
            using (var session = _documentStore.OpenSession())
            {
                session.Store(logMsg);
                session.SaveChanges();
            }

            //Broadcast the log to the clients
            _clients.received(logMsg);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_documentStore != null) _documentStore.Dispose();
            if (_server != null) _server.Stop();
        }

        #endregion
    }
}