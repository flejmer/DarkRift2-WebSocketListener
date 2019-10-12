#if !UNITY_WEBGL || UNITY_EDITOR

using System;
using System.Net;
using WebSocketSharp;

public class WebSocketSharpWebSocketClient : IWebSocketClient
{
    private static readonly Lazy<WebSocketSharpWebSocketClient> Lazy = 
        new Lazy<WebSocketSharpWebSocketClient> (() => new WebSocketSharpWebSocketClient());

    public static WebSocketSharpWebSocketClient Instance => Lazy.Value;
    
    public event Action Connected;
    public event Action Disconnected;
    public event Action<byte[]> ReceivedByteArrayMessage;
    public event Action<string> ReceivedTextMessage;
    public event Action ReceivedError;

    private WebSocket _webSocketConnection;
    
    private WebSocketSharpWebSocketClient() { }
    
    public void ConnectToServer(IPAddress ip, int port, bool isUsingSecureConnection)
    {
        if (_webSocketConnection != null) return;

        var urlPrefix = isUsingSecureConnection ? "wss" : "ws";
        _webSocketConnection = new WebSocket($"{urlPrefix}://{ip}:{port}");

        _webSocketConnection.OnOpen += (sender, args) =>
        {
            Connected?.Invoke();
        };
        
        _webSocketConnection.OnClose += (sender, args) =>
        {
            Disconnected?.Invoke();
        };
        
        _webSocketConnection.OnMessage += (sender, args) =>
        {
            if (args.IsText)
            {
                ReceivedTextMessage?.Invoke(args.Data);
            }
            else if (args.IsBinary)
            {
                ReceivedByteArrayMessage?.Invoke(args.RawData);
            }
        };

        _webSocketConnection.OnError += (sender, args) =>
        {
            ReceivedError?.Invoke();
        };
        
        _webSocketConnection.Connect();
    }

    public void DisconnectFromServer()
    {
        _webSocketConnection?.Close();
        _webSocketConnection = null;
    }

    public void SendMessageToServer(string text)
    {
        _webSocketConnection?.Send(text);
    }

    public void SendMessageToServer(byte[] array, int size)
    {
        _webSocketConnection?.Send(array);
    }
}
#endif