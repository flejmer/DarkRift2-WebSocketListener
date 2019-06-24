using System;
using System.Collections.Generic;
using System.Net;
using DarkRift.Server;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperWebSocket;

namespace WebSocketListener
{
    public class WebSocketNetworkListener : NetworkListener
    {
        public override Version Version => new Version(1, 0, 0);

        private readonly WebSocketServer _serverSocket;

        private readonly IPEndPoint _ipEndPoint;

        private readonly Dictionary<WebSocketSession, WebSocketSessionServerConnection> _connections =
            new Dictionary<WebSocketSession, WebSocketSessionServerConnection>();

        public WebSocketNetworkListener(NetworkListenerLoadData pluginLoadData) : base(pluginLoadData)
        {
            _ipEndPoint = new IPEndPoint(pluginLoadData.Address, pluginLoadData.Port);
            
            _serverSocket = new WebSocketServer();

            var serverConfig = new ServerConfig
            {
                Port = pluginLoadData.Port,
                Ip = pluginLoadData.Address.ToString(),
                Mode = SocketMode.Tcp,
                MaxConnectionNumber = 1000,
                Name = "WebSocket Server",
                ReceiveBufferSize = 16384,
                SendBufferSize = 16384
            };

            _serverSocket.Setup(
                new RootConfig(),
                serverConfig
            );
        }

        public override void StartListening()
        {
            _serverSocket.NewSessionConnected += NewSessionConnectedHandler;
            _serverSocket.NewMessageReceived += NewMessageReceivedHandler;
            _serverSocket.NewDataReceived += NewDataReceivedHandler;
            _serverSocket.SessionClosed += SessionClosedHandler;
            _serverSocket.Start();
        }

        private void NewSessionConnectedHandler(WebSocketSession session)
        {
            var serverConnection = new WebSocketSessionServerConnection(session, _ipEndPoint);
            serverConnection.OnDisconnect += ServerSessionEndHandler;
            
            RegisterClientSession(session, serverConnection);
            RegisterConnection(serverConnection);
        }

        private void RegisterClientSession(WebSocketSession session, WebSocketSessionServerConnection connection)
        {
            if (_connections.ContainsKey(session))
                return;
            
            _connections[session] = connection;
        }

        private void UnregisterClientSession(WebSocketSession session)
        {
            if (!_connections.ContainsKey(session))
                return;
            
            _connections.Remove(session);
        }

        private void ServerSessionEndHandler(WebSocketSession session)
        {
            SessionClosedHandler(session, CloseReason.ServerClosing);
        }

        private void SessionClosedHandler(WebSocketSession session, CloseReason closeReason)
        {
            if (!_connections.ContainsKey(session))
                return;
            
            _connections[session].ClientDisconnected();
            UnregisterClientSession(session);
        }

        private void NewMessageReceivedHandler(WebSocketSession session, string message)
        {
            Console.WriteLine("Text message received: " + message);
            session.Send("Echo - " + message);
        }

        private void NewDataReceivedHandler(WebSocketSession session, byte[] dataBuffer)
        {
            if (!_connections.ContainsKey(session)) return;
            _connections[session].MessageRecievedHandler(dataBuffer);
        }
    }
}
