using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Alcatraz.Core.Adapters;
using Alcatraz.Core.Connections;
using Alcatraz.Core.Hubs;
using Alcatraz.Core.Log;
using Alcatraz.Core.Receivers;
using Alcatraz.Core.Server;
using SignalR;
using SignalR.Hosting.Self;
using log4net;
using SignalR.Hubs;

namespace Alcatraz.Service
{
    [HubName("log")]
    public class LogServer : ILogMessageNotifiable
    {
        private Queue<LogMessage> _eventQueue;
        private bool _pauseLog;
        private Timer _logMsgTimer;

        private readonly ILog _log = LogManager.GetLogger(typeof(LogServer));
        private RemoteOriginServer _server;
        private dynamic _clients;

        public void Run()
        {
            if (_log.IsDebugEnabled) _log.Debug("Inicializando...");
            
            if (_log.IsDebugEnabled) _log.Debug("Inicializando UDP Receivers...");

            _eventQueue = new Queue<LogMessage>();

            var receiver = new UdpReceiver { Port = 44444 };
            receiver.Attach(this);
            receiver.Initialize();

            // Start the timer to process event logs in batch mode
            _logMsgTimer = new Timer(OnLogMessageTimer, null, 1000, 100);

            if (_log.IsDebugEnabled) _log.Debug("Inicializando SignalR...");
            Debug.Listeners.Add(new Log4NetTraceListener());
            Debug.AutoFlush = true;
            string url = "http://localhost:8081/";

            _server = new RemoteOriginServer(url);
            _server.DependencyResolver.Register(typeof(IJavaScriptProxyGenerator),
                () => new JsProxyGenerator(_server.DependencyResolver, url));

            // Map /echo to the persistent connection
            _server.MapConnection<LogConnection>("/logs");

            // Enable the hubs route (/signalr)
            _server.EnableHubs();
            _server.Start();

            var connectionManager = _server.DependencyResolver.GetService(typeof(IConnectionManager)) as IConnectionManager;
            _clients = connectionManager.GetClients<LogHub>();

            if (_log.IsDebugEnabled) _log.Debug("- END -");
        }

        #region ILogMessageNotifiable Members

        /// <summary>
        /// Transforms the notification into an asynchronous call.
        /// The actual method called to add log messages is 'AddLogMessages'.
        /// </summary>
        public void Notify(LogMessage[] logMsgs)
        {
            lock (_eventQueue)
            {
                foreach (var logMessage in logMsgs)
                {
                    _eventQueue.Enqueue(logMessage);
                }
            }
        }

        /// <summary>
        /// Transforms the notification into an asynchronous call.
        /// The actual method called to add a log message is 'AddLogMessage'.
        /// </summary>
        public void Notify(LogMessage logMsg)
        {
            lock (_eventQueue)
            {
                _eventQueue.Enqueue(logMsg);
            }
        }

        #endregion

        /// <summary>
        /// Adds a new log message, synchronously.
        /// </summary>
        private void AddLogMessages(IEnumerable<LogMessage> logMsgs)
        {
            if (_pauseLog)
                return;

            //logListView.BeginUpdate();

            foreach (LogMessage msg in logMsgs)
                AddLogMessage(msg);

            //logListView.EndUpdate();
        }

        /// <summary>
        /// Adds a new log message, synchronously.
        /// </summary>
        private void AddLogMessage(LogMessage logMsg)
        {
            if (_pauseLog)
                return;

            //RemovedLogMsgsHighlight();

            //_addedLogMessage = true;

            var sms = string.Format("{0}-{1}| {2}. {3}", logMsg.Level, logMsg.LoggerName, logMsg.Message,
                                    logMsg.ExceptionString);

            if (_log.IsInfoEnabled) _log.InfoFormat(sms);

            _clients.received(logMsg);
        }

        private void OnLogMessageTimer(object sender)
        {
            LogMessage[] messages;

            lock (_eventQueue)
            {
                // Do a local copy to minimize the lock
                messages = _eventQueue.ToArray();
                _eventQueue.Clear();
            }

            // Process logs if any
            if (messages.Length > 0)
            {
                AddLogMessages(messages);
            }
        }         
    }
}