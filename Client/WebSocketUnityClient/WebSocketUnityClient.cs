using DarkRift.Dispatching;
using System;
using System.Net;
using UnityEngine;

namespace DarkRift.Client.Unity
{
    [AddComponentMenu("DarkRift/WebSocket Client")]
	public sealed class WebSocketUnityClient : MonoBehaviour
	{
        [SerializeField]
        [Tooltip("The address of the server to connect to.")]
        public string address = IPAddress.Loopback.ToString(); //Unity requires a serializable backing field so use string

        [SerializeField]
		[Tooltip("The port the server is listening on.")]
		ushort port = 4296;

        [SerializeField]
        [Tooltip("Specifies whether the client will connect to the server using WebSocket connection over TLS.")]
        private bool isUsingSecureConnection = false;

        [SerializeField]
        [Tooltip("Indicates whether the client will connect to the server in the Start method.")]
        private bool autoConnect = true;

        [SerializeField]
        [Tooltip("Specifies whether DarkRift should log all data to the console.")]
        private volatile bool sniffData = false;

        #region Cache settings

        /// <summary>
        ///     The object cache settings in use.
        /// </summary>
        public ObjectCacheSettings ObjectCacheSettings { get; set; }

        /// <summary>
        ///     Serialisable version of the object cache settings for Unity.
        /// </summary>
        [SerializeField]
        SerializableObjectCacheSettings objectCacheSettings = new SerializableObjectCacheSettings();
        #endregion

        /// <summary>
        ///     Event fired when a message is received.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        ///     Event fired when we disconnect form the server.
        /// </summary>
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        ///     The ID the client has been assigned.
        /// </summary>
        public ushort ID => Client.ID;

        /// <summary>
        ///     Returns the state of the connection with the server.
        /// </summary>
        public ConnectionState ConnectionState
        {
            get
            {
                return Client.ConnectionState;
            }
        }

        /// <summary>
        /// 	The actual client connecting to the server.
        /// </summary>
        /// <value>The client.</value>
        public DarkRiftClient Client
        {
            get
            {
                return client;
            }
        }

        DarkRiftClient client;

        /// <summary>
        ///     The dispatcher for moving work to the main thread.
        /// </summary>
        public Dispatcher Dispatcher { get; private set; }
        
        void Awake()
        {
            ObjectCacheSettings = objectCacheSettings.ToObjectCacheSettings();

            client = new DarkRiftClient(ObjectCacheSettings);

            //Setup dispatcher
            Dispatcher = new Dispatcher(true);

            //Setup routing for events
            Client.MessageReceived += Client_MessageReceived;
            Client.Disconnected += Client_Disconnected;
        }

        void Start()
		{
            //If auto connect is true then connect to the server
            if (autoConnect)
			    Connect(address, port);
		}

        void Update()
        {
            //Execute all the queued dispatcher tasks
            Dispatcher.ExecuteDispatcherTasks();
        }

        void OnDestroy()
        {
            //Remove resources
            Close();
        }

        void OnApplicationQuit()
        {
            //Remove resources
            Close();
        }

        /// <summary>
        ///     Connects to a remote server.
        /// </summary>
        /// <param name="ip">The IP address or domain of the server.</param>
        /// <param name="port">The port of the server.</param>
        public void Connect(string address, int port)
        {
            Client.Connect(new WebSocketClientConnection(address, port, isUsingSecureConnection));
        }

        /// <summary>
        ///     Sends a message to the server.
        /// </summary>
        /// <param name="message">The message template to send.</param>
        /// <returns>Whether the send was successful.</returns>
        public bool SendMessage(Message message, SendMode sendMode)
        {
            return Client.SendMessage(message, sendMode);
        }

        /// <summary>
        ///     Invoked when DarkRift receives a message from the server.
        /// </summary>
        /// <param name="sender">THe client that received the message.</param>
        /// <param name="e">The arguments for the event.</param>
        void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (sniffData)
                Debug.Log("Message Received");      //TODO more information!

            // DarkRift will recycle the message inside the event args when this method exits so make a copy now that we control the lifecycle of!
            Message message = e.GetMessage();
            MessageReceivedEventArgs args = MessageReceivedEventArgs.Create(message, e.SendMode);

            Dispatcher.InvokeAsync(
                () => 
                {
                    EventHandler<MessageReceivedEventArgs> handler = MessageReceived;
                    if (handler != null)
                    {
                        handler.Invoke(sender, args);
                    }

                    message.Dispose();
                    args.Dispose();
                }
            );
        }

        void Client_Disconnected(object sender, DisconnectedEventArgs e)
        {
            if (!e.LocalDisconnect)
                Debug.Log("Disconnected from server, error: " + e.Error);

            Dispatcher.InvokeAsync(
                () =>
                {
                    EventHandler<DisconnectedEventArgs> handler = Disconnected;
                    if (handler != null)
                    {
                        handler.Invoke(sender, e);
                    }
                }
            );
        }

        /// <summary>
        ///     Disconnects this client from the server.
        /// </summary>
        /// <returns>Whether the disconnect was successful.</returns>
        public bool Disconnect()
        {
            return Client.Disconnect();
        }

        /// <summary>
        ///     Closes this client.
        /// </summary>
        public void Close()
        {
            Client.MessageReceived -= Client_MessageReceived;
            Client.Disconnected -= Client_Disconnected;
            Client.Disconnect();

            Client.Dispose();
            Dispatcher.Dispose();
        }
	}
}
