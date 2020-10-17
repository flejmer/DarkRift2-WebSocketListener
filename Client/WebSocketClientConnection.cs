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
    
    private readonly bool _isUsingSecureConnection;
    private readonly string _address;
    private readonly int _port;

    public WebSocketClientConnection(string address, int port, bool isUsingSecureConnection)
    {
        _isUsingSecureConnection = isUsingSecureConnection;
        _address = address;
        _port = port;

        RemoteEndPoints = new List<IPEndPoint> {new IPEndPoint(IPAddress.Parse("0.0.0.0"), port)};

#if UNITY_WEBGL && !UNITY_EDITOR
        _webSocketClient = JavaScriptWebsocketClient.Instance;
#else
        _webSocketClient = WebSocketSharpWebSocketClient.Instance;
#endif
        _connectionState = ConnectionState.Disconnected;
    }

    private void SubscribeToWebSocketCallbacks()
    {
        _webSocketClient.Connected += OnConnected;
        _webSocketClient.Disconnected += OnDisconnected;
        _webSocketClient.ReceivedByteArrayMessage += OnReceivedByteArrayMessage;
        _webSocketClient.ReceivedError += OnReceivedError;
    }
    
    private void UnsubscribeFromWebSocketCallbacks()
    {
        _webSocketClient.Connected -= OnConnected;
        _webSocketClient.Disconnected -= OnDisconnected;
        _webSocketClient.ReceivedByteArrayMessage -= OnReceivedByteArrayMessage;
        _webSocketClient.ReceivedError -= OnReceivedError;
    }
    
    public override IPEndPoint GetRemoteEndPoint(string name)
    {
        return RemoteEndPoints.First();
    }

    public override void Connect()
    {
        _connectionState = ConnectionState.Connecting;
        
        SubscribeToWebSocketCallbacks();
        
        _webSocketClient.ConnectToServer(_address, _port, _isUsingSecureConnection);
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
        
        UnsubscribeFromWebSocketCallbacks();
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

    private void OnReceivedError() { }
}
