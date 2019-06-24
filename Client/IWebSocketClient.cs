using System;
using System.Net;

public interface IWebSocketClient
{
    event Action Connected;
    event Action Disconnected;
    event Action<byte[]> ReceivedByteArrayMessage;
    event Action<string> ReceivedTextMessage;
    event Action ReceivedError;

    void ConnectToServer(IPAddress ip, int port);
    void DisconnectFromServer();
    void SendMessageToServer(string text);
    void SendMessageToServer(byte[] array, int size);
}
