using System;
using System.Net;

namespace WebSocketListener
{
    public interface IWebSocketConnection
    {
        IPEndPoint IpEndPoint { get; }

        event Action<IWebSocketConnection> ConnectionOpen;
        event Action ConnectionClosed;
        event Action<byte[]> BinaryMessageReceived;
        event Action<Exception> ConnectionError;

        void Send(byte[] buffer);
        void Close();
    }
}
