using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using DarkRift.Server;
using Fleck;

namespace WebSocketListener
{
    public class WebSocketNetworkListener : NetworkListener
    {
        public override Version Version => new Version(2, 0, 0);

        private readonly WebSocketServer _serverSocket;

        private readonly Dictionary<IWebSocketConnection, WebSocketSessionServerConnection> _connections =
            new Dictionary<IWebSocketConnection, WebSocketSessionServerConnection>();

        public WebSocketNetworkListener(NetworkListenerLoadData pluginLoadData) : base(pluginLoadData)
        {
            if (bool.Parse(pluginLoadData.Settings["debug"] ?? "false")) FleckLog.Level = LogLevel.Debug;

            var certificateName = pluginLoadData.Settings["certificateName"];
            var certificatePassword = pluginLoadData.Settings["certificatePassword"];
            var certificatePath = Environment.CurrentDirectory + $"/Plugins/{certificateName}";
            
            var isSecureServer = certificateName != null && certificatePassword != null && File.Exists(certificatePath);

            var urlPrefix = isSecureServer ? "wss" : "ws";
            var address = pluginLoadData.Address.ToString();
            var port = pluginLoadData.Port.ToString();

            _serverSocket = new WebSocketServer($"{urlPrefix}://{address}:{port}")
            {
                EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Ssl3 | SslProtocols.Tls11 | SslProtocols.Tls,
                ListenerSocket = { NoDelay = bool.Parse(pluginLoadData.Settings["noDelay"] ?? "false") },
                Certificate = isSecureServer ? 
                    new X509Certificate2(certificatePath, certificatePassword) : null
            };
        }

        public override void StartListening()
        {
            _serverSocket.Start(connection => connection.OnOpen = () => NewSessionConnectedHandler(connection));
        }

        private void NewSessionConnectedHandler(IWebSocketConnection session)
        {
            var serverConnection = new WebSocketSessionServerConnection(session);
            serverConnection.OnDisconnect += socketSession => { SessionClosedHandler(session); };
            
            RegisterClientSession(session, serverConnection);
            RegisterConnection(serverConnection);
        }

        private void RegisterClientSession(IWebSocketConnection session, WebSocketSessionServerConnection connection)
        {
            lock (_connections)
            {
                if (_connections.ContainsKey(session))
                    return;
            
                _connections[session] = connection;
            }
        }

        private void UnregisterClientSession(IWebSocketConnection session)
        {
            lock (_connections)
            {
                if (!_connections.ContainsKey(session))
                    return;
            
                _connections.Remove(session);
            }
        }

        private void SessionClosedHandler(IWebSocketConnection session)
        {
            lock (_connections)
            {
                if (!_connections.ContainsKey(session))
                    return;
            
                _connections[session].ClientDisconnected();
                UnregisterClientSession(session);
            }
        }
    }
}
