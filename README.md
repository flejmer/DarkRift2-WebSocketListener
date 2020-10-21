# DarkRift 2 - WebSocket listener

Listener for [DarkRift 2](https://darkriftnetworking.com/DarkRift2) that enables communication over WebSocket.

Allows you to use DarkRift2 networking in WebGL games.

Should be compatible with DarkRift2 version 2.6.0 and above.

## Notes

Server side uses [Fleck](https://github.com/statianzo/Fleck) implementation of WebSocket protocol. 

Client side uses [websocket-sharp](https://github.com/sta/websocket-sharp) for non-WebGL build.

## Prerequisites
- [DarkRift 2](https://darkriftnetworking.com/DarkRift2)
- [Fleck](https://github.com/statianzo/Fleck)
- [websocket-sharp](https://github.com/sta/websocket-sharp)

## Installation

### Server

#### Simple

Copy all `.dll` files that present in `/Server/Plugins` into your DarkRift2 server `Plugins` directory.

#### Self Build

To build `NetworkListener` yourself, follow the usual flow of creating plugins for DarkRift2 that is described in detail [here](https://darkriftnetworking.com/DarkRift2/Docs/2.6.0/getting_started/3_server_basics.html).

`NetworkListener` depends on `Fleck` NuGet package so remember to add it to your project.

#### Server Config

To create unsecured WebSocket server add this entry to server configuration `xml` file to allow it to listen to WebSocket connections.

```xml
<listener name="WebSocketListener" type="WebSocketNetworkListener" address="0.0.0.0" port="4201">
  <settings noDelay="true" />
</listener>
```

To create secure server, certificate file needs to be provided and placed in server working directory. Server configuration `xml` needs to include certificate file name and password.

```xml
<listener name="WebSocketListener Secure" type="WebSocketNetworkListener" address="0.0.0.0" port="4200">
  <settings noDelay="true" certificateName="certificate_file_name.pfx" certificatePassword="certificatePassword"/>
</listener>
```

### Client ###

Copy all the files that are present in `/Client/Plugins` into your Unity project `Plugins` directory. Then copy all the `.cs` files from `/Client` into your designated scripts directory.

To connect to websocket server you need to pass an instance of `WebSocketClientConnection` into `DarkRiftClient.Connect` method. You can use `UnityClient` class that is included is within DarkRift2 package and change it to fulfil your needs.

Example:

```csharp
public void Connect(string address, int port)
{
    Client.Connect(new WebSocketClientConnection(address, port, false));
}
```

`false` here means that you are connecting to unsecured WebSocket server.

##### Disclaimer #####

`Connect` is run in Unity main thread synchronously, but the `WebSocket` connects asynchronously. This means that in some cases connection is still being established, but log indicates that `Connection failed`.

Similarly, when server is not running and connection is attempted will cause a few seconds of freeze. Ideally, `ConnectInBackground` should be used, but current DarkRift version that is compatible with this listener (2.4.5) uses threathing for this method. Unfortunately threads are not available in WebGL builds.

#### Optional ####

##### Client #####

I added `WebSocketUnityClient` for easier client implementation. You can use it instead of package included `UnityClient` if all you care about is WebSocket connection.

To use it, copy `WebSocketUnityClient.cs` from `/Client/WebSocketUnityClient` folder and add to your Unity project. Similarly, copy `WebSocketUnityClientEditor.cs` form `/Client/WebSocketUnityClient/Editor` and add it to your Unity projects `Editor` folder.

`WebSocketUnityClient` should be compatible with DarkRift2 2.6.0 and above.

##### Server #####

For embedded secure server, to ensure that certificate file is placed in server working directory after build you can use provided `PostprocessBuild.cs`. When placed inside `Editor` Unity directory it will run after every build and search for certificate file in `Assets` directory and subdirectories. If found, certificate file will be copied to build output path directory inside `Plugins` folder.
