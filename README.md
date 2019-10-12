# DarkRift 2 - WebSocket listener

Listener for [DarkRift 2](https://darkriftnetworking.com/DarkRift2) that enables communication over WebSocket.

This allows you to use DarkRift2 networking in WebGL games.

Should be compatible with DarkRift2 version 2.4.0 and above.

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

Add this entry to server configuration `xml` file to allow it to listen to WebSocket connections.

```xml
<listener name="WebSocketListener" type="WebSocketNetworkListener" address="0.0.0.0" port="4296">
  <settings noDelay="true" />
</listener>
```

##### Disclaimer #####

If you are using Unity embeded server, there is a bug within DarkRift that causes listeners to not be detected. There is a temporary fix for that described [here](https://github.com/DarkRiftNetworking/DarkRift/issues/58).

### Client ###

Copy all the files that are present in `/Client/Plugins` into your Unity project `Plugins` directory. Then copy all the `.cs` files from `/Client` into your designated scripts directory.

To connect to websocket server you need to pass an instance of `WebSocketClientConnection` into `DarkRiftClient.Connect` method. You can use `UnityClient` class that is included is within DarkRift2 package and change it to fulfil your needs.

Example:

```csharp
public void Connect(IPAddress ip, int port, IPVersion ipVersion)
{
    // Client.Connect(ip, port, ipVersion);
    Client.Connect(new WebSocketClientConnection(ip, port, false));
            
    if (ConnectionState == ConnectionState.Connected)
        Debug.Log("Connected to " + ip + " on port " + port + " using " + ipVersion + ".");
    else
        Debug.Log("Connection failed to " + ip + " on port " + port + " using " + ipVersion + ".");
}
```

`false` here means that you are connecting to unsecured WebSocket server. 

##### Disclaimer #####

`Connect` is run in Unity main thread synchronously, but the `WebSocket` connects asynchronously. This means that in some cases connection is still being established, but log indicates that `Connection failed`.

Similarly, when server is not running and connection is attempted will cause a few seconds of freeze. Ideally, `ConnectInBackground` should be used, but current DarkRift version that is compatible with this listener (2.4.5) uses threathing for this method. Unfortunately threads are not available in WebGL builds.

#### Optional ####

I added `WebSocketUnityClient` for easier client implementation. You can use it instead of package included `UnityClient` if all you care about is WebSocket connection.

To use it, copy `WebSocketUnityClient.cs` from `/Client/WebSocketUnityClient` folder and add to your Unity project. Similarly, copy `WebSocketUnityClientEditor.cs` form `/Client/WebSocketUnityClient/Editor` and add it to your Unity projects `Editor` folder.
