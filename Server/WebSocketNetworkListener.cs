using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using DarkRift.Server;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebSocketListener
{
    public class WebSocketNetworkListener : NetworkListener
    {
        public override Version Version => new Version(3, 0, 0);

        private readonly WebSocketServer _serverSocket;

        private readonly Dictionary<IWebSocketConnection, WebSocketSessionServerConnection> _connections =
            new Dictionary<IWebSocketConnection, WebSocketSessionServerConnection>();

        public WebSocketNetworkListener(NetworkListenerLoadData pluginLoadData) : base(pluginLoadData)
        {
            var certificate = GetCertificate(
                pluginLoadData.Settings["certificateName"],
                pluginLoadData.Settings["certificatePassword"]
                );
            
            var isSecure = certificate != null;

            var urlPrefix = isSecure ? "wss" : "ws";
            var address = pluginLoadData.Address.ToString();
            var port = pluginLoadData.Port.ToString();

            _serverSocket = new WebSocketServer($"{urlPrefix}://{address}:{port}");
            ConfigureNoDelay(pluginLoadData);

            if (isSecure)
            {
                _serverSocket.SslConfiguration.ServerCertificate = certificate;
                _serverSocket.SslConfiguration.EnabledSslProtocols =
                    SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Ssl2 | SslProtocols.Ssl3;
            }

            _serverSocket.AddWebSocketService<ListenerWebSocketBehavior>(
                "/Listener",
                behaviour => behaviour.ConnectionOpen += NewSessionConnectedHandler
                );

            if (bool.Parse(pluginLoadData.Settings["debug"] ?? "false")) _serverSocket.Log.Level = LogLevel.Debug;

            Logger.Info(
                $"{(isSecure ? "Secure" : "Unsecured")} websocket server mounted, listening on port {port}");
        }

        public override void StartListening()
        {
            _serverSocket.Start();
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
                
                UnregisterClientSession(session);
            }
        }

        private void ConfigureNoDelay(PluginBaseLoadData pluginLoadData)
        {
            try
            {
                var field = typeof(WebSocketServer).GetField("_listener", BindingFlags.NonPublic);
                if (field != null && field.GetValue(_serverSocket) is TcpListener listener)
                {
                    listener.Server.NoDelay = bool.Parse(pluginLoadData.Settings["noDelay"] ?? "false");
                }
            }
            catch (Exception exception)
            {
                Logger.Error($"Couldn't disable Nagle's algorithm error: {exception.Message}");
            }
        }
        
        private X509Certificate2 GetCertificate(string certificateName, string certificatePassword)
        {
            if (certificateName == null || certificatePassword == null) return null;

            var certificates = Directory.GetFiles(
                Environment.CurrentDirectory, 
                certificateName, 
                SearchOption.AllDirectories
            );

            var certificatePath = certificates.First(path => path.EndsWith(certificateName));
            
            return certificatePath != null ? new X509Certificate2(certificatePath, certificatePassword) : null;
        }
    }
}
