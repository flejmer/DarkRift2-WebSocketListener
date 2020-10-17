using System;
using System.Net;

public interface IWebSocketClient
{
    event Action Connected;
    event Action Disconnected;
    event Action<byte[]> ReceivedByteArrayMessage;
    event Action ReceivedError;

    void ConnectToServer(string address, int port, bool isUsingSecureConnection);
    void DisconnectFromServer();
    void SendMessageToServer(byte[] array, int size);
}
