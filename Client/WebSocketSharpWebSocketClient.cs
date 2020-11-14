#if !UNITY_WEBGL || UNITY_EDITOR

using System.Net.Sockets;
using System.Reflection;
using System;
using WebSocketSharp;

public class WebSocketSharpWebSocketClient : IWebSocketClient
{
    private static readonly Lazy<WebSocketSharpWebSocketClient> Lazy = 
        new Lazy<WebSocketSharpWebSocketClient> (() => new WebSocketSharpWebSocketClient());

    public static WebSocketSharpWebSocketClient Instance => Lazy.Value;
    
    public event Action Connected;
    public event Action Disconnected;
    public event Action<byte[]> ReceivedByteArrayMessage;
    public event Action ReceivedError;

    private WebSocket _webSocketConnection;
    
    private WebSocketSharpWebSocketClient() { }
    
    public void ConnectToServer(string address, int port, bool isUsingSecureConnection)
    {
        if (_webSocketConnection != null) return;

        var urlPrefix = isUsingSecureConnection ? "wss" : "ws";

        _webSocketConnection = new WebSocket($"{urlPrefix}://{address}:{port}/Listener");

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
            if (args.IsBinary)
            {
                ReceivedByteArrayMessage?.Invoke(args.RawData);
            }
        };

        _webSocketConnection.OnError += (sender, args) =>
        {
            ReceivedError?.Invoke();
        };
        
        _webSocketConnection.Connect();
        
        ConfigureNoDelay();
    }

    public void DisconnectFromServer()
    {
        _webSocketConnection?.Close();
        _webSocketConnection = null;
    }

    public void SendMessageToServer(byte[] array, int size)
    {
        _webSocketConnection?.Send(array);
    }
    
    private void ConfigureNoDelay()
    {
        try
        {
            var field = typeof(WebSocket).GetField("_tcpClient", BindingFlags.NonPublic);
            if (field != null && field.GetValue(_webSocketConnection) is TcpClient tcpClient)
            {
                tcpClient.NoDelay = true;
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Couldn't disable Nagle's algorithm error: {exception.Message}");
        }
    }
}
#endif