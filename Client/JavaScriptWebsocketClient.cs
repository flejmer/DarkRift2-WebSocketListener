using System;
using System.Net;
using System.Runtime.InteropServices;
using AOT;

public class JavaScriptWebsocketClient : IWebSocketClient
{
    private static readonly Lazy<JavaScriptWebsocketClient> Lazy = 
        new Lazy<JavaScriptWebsocketClient> (() => new JavaScriptWebsocketClient());

    public static JavaScriptWebsocketClient Instance => Lazy.Value;

    public event Action Connected;
    public event Action Disconnected;
    public event Action<byte[]> ReceivedByteArrayMessage;
    public event Action<string> ReceivedTextMessage;
    public event Action ReceivedError;
    
    private delegate void TextMessageCallback(IntPtr ptr);
    private delegate void ByteArrayMessageCallback(IntPtr buffer, IntPtr length);

    [DllImport("__Internal")]
    private static extern void ConnectWebSocket(string wsUri);
    
    [DllImport("__Internal")]
    private static extern void DisconnectWebSocket();
    
    [DllImport("__Internal")]
    private static extern void SetupConnectionOpenCallbackFunction(Action action);
    
    [DllImport("__Internal")]
    private static extern void SetupConnectionClosedCallbackFunction(Action action);
    
    [DllImport("__Internal")]
    private static extern void SetupReceivedByteArrayMessageCallbackFunction(ByteArrayMessageCallback callback);
    
    [DllImport("__Internal")]
    private static extern void SetupReceivedTextMessageCallbackFunction(TextMessageCallback callback);
    
    [DllImport("__Internal")]
    private static extern void SetupReceivedErrorCallbackFunction(Action action);
    
    [DllImport("__Internal")]
    private static extern void SendTextMessage(string str);
    
    [DllImport("__Internal")]
    private static extern void SendByteArrayMessage(byte[] array, int size);
    
    [DllImport("__Internal")]
    private static extern void HelloString(string str);

    
    private JavaScriptWebsocketClient()
    {
        SetupConnectionOpenCallbackFunction(ConnectionOpenCallback);
        SetupConnectionClosedCallbackFunction(ConnectionClosedCallback);
        SetupReceivedByteArrayMessageCallbackFunction(ReceivedByteArrayMessageCallback);
        SetupReceivedTextMessageCallbackFunction(ReceivedTextMessageCallback);
        SetupReceivedErrorCallbackFunction(ReceivedErrorCallback);
    }

    public void ConnectToServer(IPAddress ip, int port, bool isUsingSecureConnection)
    {
        var urlPrefix = isUsingSecureConnection ? "wss" : "ws";
        ConnectWebSocket($"{urlPrefix}://{ip}:{port}");
    }
    
    public void DisconnectFromServer()
    {
        DisconnectWebSocket();
    }
    
    public void SendMessageToServer(string text)
    {
        SendTextMessage(text);
    }
    
    public void SendMessageToServer(byte[] array, int size)
    {
        SendByteArrayMessage(array, size);
    }
    
    [MonoPInvokeCallback(typeof(Action))]
    private static void ConnectionOpenCallback()
    {
        Instance.Connected?.Invoke();
    }
    
    [MonoPInvokeCallback(typeof(Action))]
    private static void ConnectionClosedCallback()
    {
        Instance.Disconnected?.Invoke();
    }
    
    [MonoPInvokeCallback(typeof(ByteArrayMessageCallback))]
    private static void ReceivedByteArrayMessageCallback(IntPtr buffer, IntPtr length)
    {
        var readLength = length.ToInt32();
        var bytes = new byte[readLength];
        Marshal.Copy(buffer, bytes, 0, readLength);
        Instance.ReceivedByteArrayMessage?.Invoke(bytes);
    }
    
    [MonoPInvokeCallback(typeof(TextMessageCallback))]
    private static void ReceivedTextMessageCallback(IntPtr ptr)
    {
        var text = Marshal.PtrToStringAuto(ptr);
        Instance.ReceivedTextMessage?.Invoke(text);
    }
    
    [MonoPInvokeCallback(typeof(Action))]
    private static void ReceivedErrorCallback()
    {
        Instance.ReceivedError?.Invoke();
    }
}
