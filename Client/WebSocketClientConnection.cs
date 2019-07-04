using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DarkRift;
using DarkRift.Client;

public class WebSocketClientConnection : NetworkClientConnection
{
    public override ConnectionState ConnectionState => _connectionState;
    public override IEnumerable<IPEndPoint> RemoteEndPoints { get; }

    private ConnectionState _connectionState;
    private readonly IWebSocketClient _webSocketClient;

    public WebSocketClientConnection(IPAddress ipAddress, int port)
    {
        var endPoint = new IPEndPoint(ipAddress, port);

        RemoteEndPoints = new List<IPEndPoint> {endPoint};

#if UNITY_WEBGL && !UNITY_EDITOR
        _webSocketClient = JavaScriptWebsocketClient.Instance;
#else
        _webSocketClient = WebSocketSharpWebSocketClient.Instance;
#endif
        _connectionState = ConnectionState.Disconnected;

        SetUpWebSocketCallbacks();
    }

    private void SetUpWebSocketCallbacks()
    {
        _webSocketClient.Connected += OnConnected;
        _webSocketClient.Disconnected += OnDisconnected;
        _webSocketClient.ReceivedByteArrayMessage += OnReceivedByteArrayMessage;
        _webSocketClient.ReceivedTextMessage += OnReceivedTextMessage;
        _webSocketClient.ReceivedError += OnReceivedError;
    }
    
    public override IPEndPoint GetRemoteEndPoint(string name)
    {
        return RemoteEndPoints.First();
    }

    public override void Connect()
    {
        _connectionState = ConnectionState.Connecting;

        var endPoint = RemoteEndPoints.First();
        _webSocketClient.ConnectToServer(endPoint.Address, endPoint.Port);
    }

    public override bool SendMessageReliable(MessageBuffer message)
    {
        return SendMessage(message);
    }

    public override bool SendMessageUnreliable(MessageBuffer message)
    {
        return SendMessage(message);
    }
    
    private bool SendMessage(MessageBuffer message)
    {
        if (_connectionState == ConnectionState.Disconnected)
            return false;
        
        _webSocketClient.SendMessageToServer(message.Buffer, message.Count);
        return true;
    }

    public override bool Disconnect()
    {
        _webSocketClient.DisconnectFromServer();
        return true;
    }
    
    private void OnConnected()
    {
        _connectionState = ConnectionState.Connected;
    }

    private void OnDisconnected()
    {
        _connectionState = ConnectionState.Disconnected;
        HandleDisconnection();
    }

    private void OnReceivedByteArrayMessage(byte[] bytes)
    {
        using (var messageBuffer = MessageBuffer.Create(bytes.Length))
        {
            Buffer.BlockCopy(bytes, 0, messageBuffer.Buffer, 0, bytes.Length);
            messageBuffer.Count = bytes.Length;
            HandleMessageReceived(messageBuffer, SendMode.Reliable);
        }
    }
    
    private void OnReceivedTextMessage(string message) { }
    
    private void OnReceivedError() { }
}
