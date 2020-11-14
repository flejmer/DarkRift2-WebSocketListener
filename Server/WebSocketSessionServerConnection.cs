using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DarkRift;
using DarkRift.Server;

namespace WebSocketListener
{
    public class WebSocketSessionServerConnection : NetworkServerConnection
    {
        public override ConnectionState ConnectionState => _connectionState;
        public override IEnumerable<IPEndPoint> RemoteEndPoints { get; }

        public event Action<IWebSocketConnection> OnDisconnect; 

        private bool _disposedValue;
        
        private ConnectionState _connectionState;
        private readonly IWebSocketConnection _webSocketConnection;

        public WebSocketSessionServerConnection(IWebSocketConnection webSocketConnection)
        {
            RemoteEndPoints = new List<IPEndPoint> {
                webSocketConnection.IpEndPoint
            };
            
            _webSocketConnection = webSocketConnection;
            _connectionState = ConnectionState.Connected;

            webSocketConnection.BinaryMessageReceived += MessageReceivedHandler;
            webSocketConnection.ConnectionClosed += ConnectionClosedHandler;
        }

        public override IPEndPoint GetRemoteEndPoint(string name)
        {
            return RemoteEndPoints.First();
        }

        public override void StartListening() { }

        private void MessageReceivedHandler(byte[] buffer)
        {
            if (_connectionState != ConnectionState.Connected)
                return;

            using (var messageBuffer = MessageBuffer.Create(buffer.Length))
            {
                Buffer.BlockCopy(buffer, 0, messageBuffer.Buffer, 0, buffer.Length);
                messageBuffer.Count = buffer.Length;
                HandleMessageReceived(messageBuffer, SendMode.Reliable);
            }
        }
        
        public override bool SendMessageReliable(MessageBuffer message)
        {
            if (_connectionState == ConnectionState.Disconnected)
            {
                message.Dispose();
                return false;
            }

            var dataBuffer = new byte[message.Count];
            Buffer.BlockCopy(message.Buffer, 0, dataBuffer, 0, message.Count);
            
            _webSocketConnection.Send(dataBuffer);
            message.Dispose();

            return true;
        }

        public override bool SendMessageUnreliable(MessageBuffer message)
        {
            if (_connectionState == ConnectionState.Disconnected)
            {
                message.Dispose();
                return false;
            }

            var dataBuffer = new byte[message.Count];
            Buffer.BlockCopy(message.Buffer, 0, dataBuffer, 0, message.Count);
            
            _webSocketConnection.Send(dataBuffer);
            message.Dispose();

            return true;
        }

        public override bool Disconnect()
        {
            if (_connectionState == ConnectionState.Disconnecting)
            {
                _connectionState = ConnectionState.Disconnected;
            }
            else
            {
                _webSocketConnection.Close();
            }

            return true;
        }

        private void ConnectionClosedHandler()
        {
            _connectionState = ConnectionState.Disconnecting;
            OnDisconnect?.Invoke(_webSocketConnection);
            
            HandleDisconnection();
        }
        
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            
            if (_disposedValue)
                return;
            
            if (disposing)
            {
                Disconnect();
            }
            
            _disposedValue = true;
        }
    }
}
