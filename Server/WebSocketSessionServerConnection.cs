using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DarkRift;
using DarkRift.Server;
using SuperWebSocket;

namespace WebSocketListener
{
    public class WebSocketSessionServerConnection : NetworkServerConnection
    {
        public override ConnectionState ConnectionState => _connectionState;
        public override IEnumerable<IPEndPoint> RemoteEndPoints { get; }

        public event Action<WebSocketSession> OnDisconnect; 

        private bool _disposedValue;
        
        private ConnectionState _connectionState;
        private readonly WebSocketSession _webSocketSession;

        public WebSocketSessionServerConnection(WebSocketSession webSocketSession, IPEndPoint endPoint)
        {
            RemoteEndPoints = new List<IPEndPoint> {endPoint};
            
            _webSocketSession = webSocketSession;
            _connectionState = ConnectionState.Connected;
        }

        public override IPEndPoint GetRemoteEndPoint(string name)
        {
            return RemoteEndPoints.First();
        }

        public override void StartListening() { }

        public void MessageRecievedHandler(byte[] buffer)
        {
            using (var messageBuffer = MessageBuffer.Create(buffer.Length))
            {
                Buffer.BlockCopy(buffer, 0, messageBuffer.Buffer, 0, buffer.Length);
                messageBuffer.Count = buffer.Length;

                HandleMessageReceived(messageBuffer, SendMode.Reliable);
            }
        }
        
        public override bool SendMessageReliable(MessageBuffer message)
        {
            return SendMessage(message);
        }

        public override bool SendMessageUnreliable(MessageBuffer message)
        {
            return SendMessage(message);
        }

        private bool SendMessage(IMessageBuffer message)
        {
            if (_connectionState == ConnectionState.Disconnected)
            {
                message.Dispose();
                return false;
            }

            _webSocketSession.Send(message.Buffer, 0, message.Count);
            message.Dispose();

            return true;
        }

        public override bool Disconnect()
        {
            if (_connectionState == ConnectionState.Disconnected)
                return false;
            
            CloseConnection();
            return true;
        }
        
        private void CloseConnection()
        {
            _connectionState = ConnectionState.Disconnected;
            
            OnDisconnect?.Invoke(_webSocketSession);
            _webSocketSession.Close();
        }
        
        public void ClientDisconnected()
        {
            HandleDisconnection();
        }
        
        protected override void Dispose(bool disposing)
        {
            base.Dispose(true);
            
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
