var WebSocketJavaScriptLibrary = {

  $ConnectionOpen: {},
  $ConnectionClosed: {},
  $ReceivedByteArrayMessage: {},
  $ReceivedTextMessage: {},
  $ReceivedError: {},

  ConnectWebSocket: function init(wsUri) {
    var url = Pointer_stringify(wsUri);

    websocket = new WebSocket(url);
    websocket.binaryType = "arraybuffer";

    websocket.onopen = function(evt) {
      Runtime.dynCall('v', ConnectionOpen.callback, 0);
    };

    websocket.onclose = function(evt) {
      Runtime.dynCall('v', ConnectionClosed.callback, 0);
    };

    websocket.onmessage = function(evt) {
      if (evt.data instanceof ArrayBuffer) {
        var byteArray = new Uint8Array(evt.data);
        var buffer = _malloc(byteArray.byteLength);

        HEAPU8.set(byteArray, buffer / byteArray.BYTES_PER_ELEMENT);
        Runtime.dynCall('vii', ReceivedByteArrayMessage.callback, [buffer, byteArray.length]);
        _free(buffer);
      } else if (typeof evt.data === "string") {
        var buffer = _malloc(lengthBytesUTF8(evt.data) + 1);

        stringToUTF8(evt.data, buffer, lengthBytesUTF8(evt.data) + 1);
        Runtime.dynCall('vi', ReceivedTextMessage.callback, [buffer]);
        _free(buffer);
      }
    };

    websocket.onerror = function(evt) {
      Runtime.dynCall('v', ReceivedError.callback, 0);
    };
  },

  DisconnectWebSocket: function () {
    websocket.close();
  },

  SetupConnectionOpenCallbackFunction: function (obj) {
    ConnectionOpen.callback = obj;
  },

  SetupConnectionClosedCallbackFunction: function (obj) {
    ConnectionClosed.callback = obj;
  },

  SetupReceivedByteArrayMessageCallbackFunction: function (obj) {
    ReceivedByteArrayMessage.callback = obj;
  },

  SetupReceivedTextMessageCallbackFunction: function (obj) {
    ReceivedTextMessage.callback = obj;
  },

  SetupReceivedErrorCallbackFunction: function (obj) {
    ReceivedError.callback = obj;
  },

  SendTextMessage: function (textMessage) {
    var message = Pointer_stringify(textMessage);
    websocket.send(message);
  },

  SendByteArrayMessage: function (array, size) {
    var byteArray = new Uint8Array(size);

    for(var i = 0; i < size; i++) {
      var byte = HEAPU8[array + i];
      byteArray[i] = byte;
    }

    websocket.send(byteArray.buffer);
  },
};

autoAddDeps(WebSocketJavaScriptLibrary, '$ConnectionOpen');
autoAddDeps(WebSocketJavaScriptLibrary, '$ConnectionClosed');
autoAddDeps(WebSocketJavaScriptLibrary, '$ReceivedByteArrayMessage');
autoAddDeps(WebSocketJavaScriptLibrary, '$ReceivedTextMessage');
autoAddDeps(WebSocketJavaScriptLibrary, '$ReceivedError');
mergeInto(LibraryManager.library, WebSocketJavaScriptLibrary);
