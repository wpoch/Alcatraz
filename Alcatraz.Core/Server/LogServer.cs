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
        public static EmbeddableDocumentStore DocumentStore;
        private dynamic _clients;
        private RemoteOriginServer _server;
        private readonly ApplicationSettings _settings;

        public LogServer(ApplicationSettings settings)
        {
            _settings = settings;
        }

        #region Initialization

        public void Run()
        {
            if (_log.IsDebugEnabled) _log.Debug("Starting Alcatraz engines...");
            InitReceivers();
            InitBroadcaster();
            InitLogStore();
            if (_log.IsDebugEnabled) _log.Debug("... Alcatraz running!");
        }

        private void InitLogStore()
        {
            if (_log.IsDebugEnabled) _log.Debug("Init RavenDB...");

            DocumentStore = new EmbeddableDocumentStore
                                {
                                    DataDirectory = "Data",
                                    UseEmbeddedHttpServer = true,
                                };
            DocumentStore.Configuration.Port = _settings.DatabasePort;
            DocumentStore.Initialize();

            if (_log.IsDebugEnabled) _log.Debug("... RavenDB initialized!");
        }

        private void InitBroadcaster()
        {
            if (_log.IsDebugEnabled) _log.Debug("Init SignalR...");

            _server = new RemoteOriginServer(_settings.BroadcastUrl);
            _server.DependencyResolver.Register(typeof(IJavaScriptProxyGenerator),
                                                () => new JsProxyGenerator(_server.DependencyResolver, _settings.BroadcastUrl));

            // Enable the hubs route (/signalr)
            _server.EnableHubs();
            _server.Start();

            var connectionManager =
                _server.DependencyResolver.GetService(typeof(IConnectionManager)) as IConnectionManager;
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

        #region IDisposable Members

        public void Dispose()
        {
            if (DocumentStore != null) DocumentStore.Dispose();
            if (_server != null) _server.Stop();
        }

        #endregion

        private void AddLogMessage(LogMessage logMsg)
        {
            if (_log.IsDebugEnabled) _log.Debug(logMsg);

            //Store the log
            using (var session = DocumentStore.OpenSession())
            {
                session.Store(logMsg);
                session.SaveChanges();
            }

            //Broadcast the log to the clients
            _clients.received(logMsg);
        }
    }
}