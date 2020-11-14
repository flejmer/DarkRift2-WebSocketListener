using System;
using System.Net;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebSocketListener
{
    public class ListenerWebSocketBehavior : WebSocketBehavior, IWebSocketConnection
    {
        public IPEndPoint IpEndPoint => Context.UserEndPoint;
        
        public event Action<IWebSocketConnection> ConnectionOpen;
        public event Action ConnectionClosed;
        public event Action<byte[]> BinaryMessageReceived;
        public event Action<Exception> ConnectionError;

        protected override void OnOpen()
        {
            base.OnOpen();
            ConnectionOpen?.Invoke(this);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);
            BinaryMessageReceived?.Invoke(e.RawData);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);
            ConnectionError?.Invoke(e.Exception);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            ConnectionClosed?.Invoke();
            base.OnClose(e);
        }
        
        public new void Send(byte[] buffer)
        {
            if (ConnectionState != WebSocketState.Open)
                return;
            
            base.Send(buffer);
        }

        public new void Close()
        {
            if (ConnectionState != WebSocketState.Open)
                return;
            
            base.Close();
        }
    }
}
