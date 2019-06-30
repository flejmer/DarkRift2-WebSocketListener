# DarkRift 2 - WebSocket listener

Listener for [DarkRift 2](https://darkriftnetworking.com/DarkRift2) that enables communication over WebSocket.

This allows you to use DarkRift2 networking in WebGL games.

## Notes

Server side uses a [SuperWebSocket](https://archive.codeplex.com/?p=superwebsocket) implementation of WebSocket protocol. 

Client side uses [websocket-sharp](https://github.com/sta/websocket-sharp) for non-WebGL build.

## Prerequisites
- [DarkRift 2](https://darkriftnetworking.com/DarkRift2)
- [SuperWebSocket](https://archive.codeplex.com/?p=superwebsocket)
- [websocket-sharp](https://github.com/sta/websocket-sharp)

## Installation

### Server

#### Simple

Copy all `.dll` files that present in `/Server/Plugins` into your DarkRift2 server `Plugins` directory.

#### Self Build

To build `NetworkListener` yourself, follow the usual flow of creating plugins for DarkRift2 that is described in detail [here](https://darkriftnetworking.com/DarkRift2/Docs/2.4.4/getting_started/3_server_basics.html).

`NetworkListener` depends on `SuperWebSocketNETServer` NuGet package so remember to add it to your project.

#### Server Config

```xml
<listener name="WebSocketListener" type="WebSocketNetworkListener" address="0.0.0.0" port="4296">
  <settings noDelay="true" />
</listener>
```

### Client ###

Copy all the files that are present in `/Client/Plugins` into your Unity project `Plugins` directory. Then copy all the `.cs` files from `/Client` into your designated scripts directory.

To connect to websocket server you need to pass an instance of `WebSocketClientConnection` into `DarkRiftClient.Connect` method. You can use `UnityClient` class that is included is within DarkRift2 package and change it to fulfil your needs.

Example:

```csharp
public void Connect(IPAddress ip, int port, IPVersion ipVersion)
{
    Client.Connect(new WebSocketClientConnection(ip, port));

    if (_connectionProcessCoroutine != null) return;
            
    _connectionProcessCoroutine = ConnectionProcessCoroutine(ip, port);
    StartCoroutine(_connectionProcessCoroutine);
}

IEnumerator ConnectionProcessCoroutine(IPAddress ip, int port)
{
    while (ConnectionState == ConnectionState.Connecting)
    {
        yield return null;
    }

    _connectionProcessCoroutine = null;
            
    if (ConnectionState == ConnectionState.Connected)
        Debug.Log("Connected to " + ip + " on port " + port + " using " + ipVersion + ".");
    else
        Debug.Log("Connection failed to " + ip + " on port " + port + " using " + ipVersion + ".");
            
    ConnectionProcessFinished?.Invoke();
}
```

