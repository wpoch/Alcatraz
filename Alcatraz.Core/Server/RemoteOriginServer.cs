﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Alcatraz.Core.Helpers;
using SignalR;
using SignalR.Hosting;
using SignalR.Hosting.Self;
using SignalR.Hosting.Self.Infrastructure;
using SignalR.Hubs;
using SignalR.Infrastructure;

namespace Alcatraz.Core.Server
{
    public class RemoteOriginServer
    {
        private readonly Dictionary<string, Type> _connectionMapping = new Dictionary<string, Type>();
        private readonly HttpListener _listener;
        private readonly Uri _url;
        private bool _hubsEnabled;

        public RemoteOriginServer(Uri url)
            : this(url, new DefaultDependencyResolver())
        {
        }

        public RemoteOriginServer(Uri url, IDependencyResolver resolver)
        {
            _url = url;
            _listener = new HttpListener();
            _listener.Prefixes.Add(url.ToString());
            DependencyResolver = resolver;
        }

        public Action<HostContext> OnProcessRequest { get; set; }

        public IDependencyResolver DependencyResolver { get; private set; }

        public void Start()
        {
            _listener.Start();

            ReceiveLoop();
        }

        public void Stop()
        {
            _listener.Stop();
        }

        public void MapConnection<T>(string path) where T : PersistentConnection
        {
            if (!_connectionMapping.ContainsKey(path))
            {
                _connectionMapping.Add(path, typeof (T));
            }
        }

        public void EnableHubs()
        {
            _hubsEnabled = true;
        }

        public bool TryGetConnection(string path, out PersistentConnection connection)
        {
            connection = null;

            if (_hubsEnabled && path.StartsWith("/signalr", StringComparison.OrdinalIgnoreCase))
            {
                connection = new HubDispatcher("/signalr");
                return true;
            }

            return TryGetMappedConnection(path, out connection);
        }

        private void ReceiveLoop()
        {
            _listener.BeginGetContext(ar =>
                                          {
                                              HttpListenerContext context;
                                              try
                                              {
                                                  context = _listener.EndGetContext(ar);
                                              }
                                              catch (Exception)
                                              {
                                                  return;
                                              }

                                              ReceiveLoop();

                                              // Process the request async
                                              ProcessRequestAsync(context).ContinueWith(task =>
                                                                                            {
                                                                                                if (task.IsFaulted)
                                                                                                {
                                                                                                    Exception ex =
                                                                                                        task.Exception.
                                                                                                            GetBaseException
                                                                                                            ();
                                                                                                    context.Response.
                                                                                                        ServerError(ex).
                                                                                                        Catch();

                                                                                                    Debug.WriteLine(
                                                                                                        ex.Message);
                                                                                                }

                                                                                                context.Response.
                                                                                                    CloseSafe();
                                                                                            });
                                          }, null);
        }

        private Task ProcessRequestAsync(HttpListenerContext context)
        {
            try
            {
                Debug.WriteLine("Incoming request to {0}.", context.Request.Url);

                PersistentConnection connection;

                string path = ResolvePath(context.Request.Url);

                if (TryGetConnection(path, out connection))
                {
                    var request = new HttpListenerRequestWrapper(context.Request);

                    context.Response.AddHeader("Access-Control-Allow-Origin", "*");
                    var response = new HttpListenerResponseWrapper(context.Response);

                    var hostContext = new HostContext(request, response, context.User);

                    if (OnProcessRequest != null)
                    {
                        OnProcessRequest(hostContext);
                    }
#if DEBUG
                    hostContext.Items[HostConstants.DebugMode] = true;
#endif
                    hostContext.Items["System.Net.HttpListenerContext"] = context;

                    // Initialize the connection
                    connection.Initialize(DependencyResolver);
                    return connection.ProcessRequestAsync(hostContext);
                }

                return context.Response.NotFound();
            }
            catch (Exception ex)
            {
                return TaskAsyncHelper.FromError(ex);
            }
        }

        private bool TryGetMappedConnection(string path, out PersistentConnection connection)
        {
            connection = null;

            foreach (var pair in _connectionMapping)
            {
                // If the url matches then create the connection type
                if (path.StartsWith(pair.Key, StringComparison.OrdinalIgnoreCase))
                {
                    var factory = new PersistentConnectionFactory(DependencyResolver);
                    connection = factory.CreateInstance(pair.Value);
                    return true;
                }
            }

            return false;
        }

        private string ResolvePath(Uri url)
        {
            string baseUrl = url.GetComponents(UriComponents.Scheme | UriComponents.HostAndPort | UriComponents.Path,
                                               UriFormat.SafeUnescaped);

            if (!baseUrl.StartsWith(_url.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Unable to resolve path");
            }

            string path = baseUrl.Substring(_url.ToString().Length);
            if (!path.StartsWith("/"))
            {
                return "/" + path;
            }

            return path;
        }
    }
}