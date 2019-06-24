# DarkRift 2 - WebSocket listener

Listener for [DarkRift 2](https://darkriftnetworking.com/DarkRift2) that enables communication over WebSocket.
This allows you to use DarkRift2 networking in WebGL games.

## Notes

Server side uses a [SuperWebSocket](https://archive.codeplex.com/?p=superwebsocket) implementation of WebSocket protocol. 

## Prerequisites
- [DarkRift 2](https://darkriftnetworking.com/DarkRift2)
- [SuperWebSocket](https://archive.codeplex.com/?p=superwebsocket)

## Config

```xml
<listener name="WebSocketListener" type="WebSocketNetworkListener" address="0.0.0.0" port="4296">
  <settings noDelay="true" />
</listener>
```
