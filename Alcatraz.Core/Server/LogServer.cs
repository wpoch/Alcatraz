﻿using System;
using System.Diagnostics;
using Alcatraz.Core.Hubs;
using Alcatraz.Core.Log;
using Alcatraz.Core.Receivers;
using Raven.Client;
using Raven.Client.Embedded;
using SignalR;
using SignalR.Hubs;
using log4net;

namespace Alcatraz.Core.Server
{
    public class LogServer : IDisposable
    {
        public static EmbeddableDocumentStore DocumentStore;

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
            DocumentStore.Configuration.Port = 8099;
            DocumentStore.Initialize();

            if (_log.IsDebugEnabled) _log.Debug("... RavenDB initialized!");
        }

        private void InitBroadcaster()
        {
            if (_log.IsDebugEnabled) _log.Debug("Init SignalR...");

            const string url = "http://localhost:8081/";

            _server = new RemoteOriginServer(url);
            _server.DependencyResolver.Register(typeof (IJavaScriptProxyGenerator),
                                                () => new JsProxyGenerator(_server.DependencyResolver, url));

            // Enable the hubs route (/signalr)
            _server.EnableHubs();
            _server.Start();

            var connectionManager =
                _server.DependencyResolver.GetService(typeof (IConnectionManager)) as IConnectionManager;
            _clients = connectionManager.GetClients<LogHub>();

            if (_log.IsDebugEnabled) _log.Debug("...SignalR initialized!");
        }

        private void InitReceivers()
        {
            if (_log.IsDebugEnabled) _log.Debug("Init UDP Receivers...");

            var receiver = new UdpReceiver {Port = 44444, OnLogMessageReceived = AddLogMessage};
            receiver.Initialize();

            if (_log.IsDebugEnabled) _log.Debug("... UDP Receivers initialized!");
        }

        #endregion

        private readonly ILog _log = LogManager.GetLogger(typeof (LogServer));
        private dynamic _clients;
        private RemoteOriginServer _server;

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
            using (IDocumentSession session = DocumentStore.OpenSession())
            {
                session.Store(logMsg);
                session.SaveChanges();
            }

            //Broadcast the log to the clients
            _clients.received(logMsg);
        }
    }
}