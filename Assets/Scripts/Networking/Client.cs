using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    #region Models

    private enum IP_ADDRESSES
    {
        LOCAL,
        PRIVATE,
        PUBLIC
    }

    #endregion

    #region Constants

    private const float SERVER_CONNECT_TIMEOUT = 15f;
    private const float PING_CHECK_INTERVAL = 1f;

    /// <summary>
    /// Local Host IP:          127.0.0.1
    /// Private Network IP:     192.168.1.67
    /// Public Network IP:      99.24.219.47
    /// </summary>
    private const string LOCAL_IP_ADDRESS = "127.0.0.1";
    private const string PRIVATE_IP_ADDRESS = "192.168.1.67";
    private const string PUBLIC_IP_ADDRESS = "99.24.219.47";

    #endregion

    #region Members

    public static Client instance;
    public static short dataBufferSize = 4096;

    [SerializeField]
    private IP_ADDRESSES m_activeIpAddress;
    public string ip { get; private set; } = LOCAL_IP_ADDRESS;
    public int port = 26950;
    public int myId { get; private set; } = 0;
    public string myUsername { get; private set; } = string.Empty;

    public TCP tcp;
    public UDP udp;
    private bool isConnected = false;
    public delegate void ServerConnectionHandler ( int status );
    public static ServerConnectionHandler OnServerConnect;
    public uint ServerTick;

    private delegate void PacketHandler ( Packet _packet );
    private static Dictionary<int, PacketHandler> packetHandlers;

    // Timeout
    private Coroutine m_serverConnectTimeout = null;

    // Ping
    public long Ping { get; private set; } = 0L;
    private float m_pingCheckTimer = 0f;
    private Stopwatch m_pingStopwatch;

    #endregion


    private void Awake ()
    {
        if ( instance == null )
        {
            instance = this;
        }
        else if ( instance != this )
        {
            UnityEngine.Debug.Log ( "Instance already exists, destroying object!" );
            Destroy ( this );
        }
    }

    private void Start ()
    {
        // Initialize ping stopwatch
        m_pingStopwatch = new Stopwatch ();

        switch ( m_activeIpAddress )
        {
            case IP_ADDRESSES.LOCAL:
                ip = LOCAL_IP_ADDRESS;
                break;
            case IP_ADDRESSES.PRIVATE:
                ip = PRIVATE_IP_ADDRESS;
                break;
            case IP_ADDRESSES.PUBLIC:
                ip = PUBLIC_IP_ADDRESS;
                break;
            default:
                ip = LOCAL_IP_ADDRESS;
                break;
        }
    }

    private void Update ()
    {
        GetPing ();
    }

    private void OnApplicationQuit ()
    {
        Disconnect (); // Disconnect when the game is closed
    }

    /// <summary>Attempts to connect to the server.</summary>
    public void ConnectToServer ( string ip, ushort port )
    {
        this.ip = ip;
        this.port = port;

        tcp = new TCP ();
        udp = new UDP ();

        InitializeClientData ();

        isConnected = true;
        tcp.Connect (); // Connect tcp, udp gets connected once tcp is done
    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private Packet receivedData;
        private byte [] receiveBuffer;

        /// <summary>Attempts to connect to the server via TCP.</summary>
        public void Connect ()
        {
            instance.StartServerConnection ();

            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte [ dataBufferSize ];
            socket.BeginConnect ( instance.ip, instance.port, ConnectCallback, socket );
        }

        /// <summary>Initializes the newly connected client's TCP-related info.</summary>
        private void ConnectCallback ( IAsyncResult _result )
        {
            socket.EndConnect ( _result );

            if ( !socket.Connected )
            {
                UnityEngine.Debug.Log ( "TCP socket not connected." );
                return;
            }

            stream = socket.GetStream ();

            receivedData = new Packet ();

            stream.BeginRead ( receiveBuffer, 0, dataBufferSize, ReceiveCallback, null );
        }

        /// <summary>Sends data to the client via TCP.</summary>
        /// <param name="_packet">The packet to send.</param>
        public void SendData ( Packet _packet )
        {
            UnityEngine.Debug.Assert ( _packet != null );
            try
            {
                if ( socket != null )
                {
                    stream.BeginWrite ( _packet.ToArray (), 0, _packet.Length (), null, null ); // Send data to server
                }
            }
            catch ( Exception _ex )
            {
                UnityEngine.Debug.Log ( $"Error sending data to server via TCP: {_ex}" );
            }
        }

        /// <summary>Reads incoming data from the stream.</summary>
        private void ReceiveCallback ( IAsyncResult _result )
        {
            try
            {
                int _byteLength = stream.EndRead ( _result );
                if ( _byteLength <= 0 )
                {
                    instance.Disconnect ();
                    return;
                }

                byte [] _data = new byte [ _byteLength ];
                Array.Copy ( receiveBuffer, _data, _byteLength );

                receivedData.Reset ( HandleData ( _data ) ); // Reset receivedData if all data was handled
                stream.BeginRead ( receiveBuffer, 0, dataBufferSize, ReceiveCallback, null );
            }
            catch
            {
                Disconnect ();
            }
        }

        /// <summary>Prepares received data to be used by the appropriate packet handler methods.</summary>
        /// <param name="_data">The recieved data.</param>
        private bool HandleData ( byte [] _data )
        {
            int _packetLength = 0;

            receivedData.SetBytes ( _data );

            if ( receivedData.UnreadLength () >= 4 )
            {
                // If client's received data contains a packet
                _packetLength = receivedData.ReadInt ();
                if ( _packetLength <= 0 )
                {
                    // If packet contains no data
                    return true; // Reset receivedData instance to allow it to be reused
                }
            }

            while ( _packetLength > 0 && _packetLength <= receivedData.UnreadLength () )
            {
                // While packet contains data AND packet data length doesn't exceed the length of the packet we're reading
                byte [] _packetBytes = receivedData.ReadBytes ( _packetLength );
                ThreadManager.ExecuteOnMainThread ( () =>
                  {
                      using ( Packet _packet = new Packet ( _packetBytes ) )
                      {
                          int _packetId = _packet.ReadInt ();
                          packetHandlers [ _packetId ] ( _packet ); // Call appropriate method to handle the packet
                      }
                  } );

                _packetLength = 0; // Reset packet length
                if ( receivedData.UnreadLength () >= 4 )
                {
                    // If client's received data contains another packet
                    _packetLength = receivedData.ReadInt ();
                    if ( _packetLength <= 0 )
                    {
                        // If packet contains no data
                        return true; // Reset receivedData instance to allow it to be reused
                    }
                }
            }

            if ( _packetLength <= 1 )
            {
                return true; // Reset receivedData instance to allow it to be reused
            }

            return false;
        }

        /// <summary>Disconnects from the server and cleans up the TCP connection.</summary>
        private void Disconnect ()
        {
            instance.Disconnect ();

            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP ()
        {
            endPoint = new IPEndPoint ( IPAddress.Parse ( instance.ip ), instance.port );
        }

        /// <summary>Attempts to connect to the server via UDP.</summary>
        /// <param name="_localPort">The port number to bind the UDP socket to.</param>
        public void Connect ( int _localPort )
        {
            socket = new UdpClient ( _localPort );

            socket.Connect ( endPoint );
            socket.BeginReceive ( ReceiveCallback, null );

            using ( Packet _packet = new Packet () )
            {
                SendData ( _packet );
            }
        }

        /// <summary>Sends data to the client via UDP.</summary>
        /// <param name="_packet">The packet to send.</param>
        public void SendData ( Packet _packet )
        {
            try
            {
                _packet.InsertInt ( instance.myId ); // Insert the client's ID at the start of the packet
                if ( socket != null )
                {
                    socket.BeginSend ( _packet.ToArray (), _packet.Length (), null, null );
                }
            }
            catch ( Exception _ex )
            {
                UnityEngine.Debug.Log ( $"Error sending data to server via UDP: {_ex}" );
            }
        }

        /// <summary>Receives incoming UDP data.</summary>
        private void ReceiveCallback ( IAsyncResult _result )
        {
            try
            {
                byte [] _data = socket.EndReceive ( _result, ref endPoint );
                socket.BeginReceive ( ReceiveCallback, null );

                if ( _data.Length < 4 )
                {
                    instance.Disconnect ();
                    return;
                }

                HandleData ( _data );
            }
            catch
            {
                Disconnect ();
            }
        }

        /// <summary>Prepares received data to be used by the appropriate packet handler methods.</summary>
        /// <param name="_data">The recieved data.</param>
        private void HandleData ( byte [] _data )
        {
            using ( Packet _packet = new Packet ( _data ) )
            {
                int _packetLength = _packet.ReadInt ();
                _data = _packet.ReadBytes ( _packetLength );
            }

            ThreadManager.ExecuteOnMainThread ( () =>
              {
                  using ( Packet _packet = new Packet ( _data ) )
                  {
                      int _packetId = _packet.ReadInt ();
                      packetHandlers [ _packetId ] ( _packet ); // Call appropriate method to handle the packet
                  }
              } );
        }

        /// <summary>Disconnects from the server and cleans up the UDP connection.</summary>
        private void Disconnect ()
        {
            instance.Disconnect ();

            endPoint = null;
            socket = null;
        }
    }

    /// <summary>Initializes all necessary client data.</summary>
    private void InitializeClientData ()
    {
        packetHandlers = new Dictionary<int, PacketHandler> ()
        {
            { (int)ServerPackets.welcome, ClientHandle.Welcome },
            { (int)ServerPackets.playerConnected, ClientHandle.ConnectPlayer },
            { (int)ServerPackets.ping, ClientHandle.Ping },
            { (int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },
            { (int)ServerPackets.playerPosition, ClientHandle.PlayerPosition },
            { (int)ServerPackets.playerRotation, ClientHandle.PlayerRotation },
            { (int)ServerPackets.playerMovement, ClientHandle.PlayerMovement },
            { (int)ServerPackets.playerInputProcessed, ClientHandle.PlayerInputProcessed },
            { (int)ServerPackets.playerDisconnected, ClientHandle.PlayerDisconnected },
            { (int)ServerPackets.playerHealth, ClientHandle.PlayerHealth },
            { (int)ServerPackets.playerRespawned, ClientHandle.PlayerRespawned },
            { (int)ServerPackets.playerUpdateInventorySlot, ClientHandle.PlayerUpdateInventorySlot },
            { (int)ServerPackets.spawnHitObject, ClientHandle.SpawnHitObject },
            { (int)ServerPackets.playAudioClip, ClientHandle.PlayAudioClip },
            { (int)ServerPackets.hitmarker, ClientHandle.Hitmarker },
            { (int)ServerPackets.createItemSpawner, ClientHandle.CreateItemSpawner },
            { (int)ServerPackets.destroyItem, ClientHandle.DestroyItem },
            { (int)ServerPackets.networkedRigidbodyData, ClientHandle.NetworkedRigidbodyData }
        };
        UnityEngine.Debug.Log ( "Initialized packets." );
    }

    #region Setters

    public void SetClientId ( int id )
    {
        myId = id;

        // DEBUG
        myUsername = "PLAYER " + myId;
    }

    #endregion

    #region Server Connect Timeout

    private void StartServerConnection ()
    {
        if ( m_serverConnectTimeout != null )
        {
            StopCoroutine ( m_serverConnectTimeout );
        }
        m_serverConnectTimeout = StartCoroutine ( ServerConnectTimeoutEnum () );
        OnServerConnect?.Invoke ( 0 );
    }

    private IEnumerator ServerConnectTimeoutEnum ()
    {
        yield return new WaitForSeconds ( SERVER_CONNECT_TIMEOUT );

        if ( isConnected && myId != 0 )
        {
            yield break;
        }

        // Timeout
        Disconnect ();
        OnServerConnect?.Invoke ( 2 );
    }

    #endregion

    #region Ping

    private void GetPing ()
    {
        if ( isConnected && myId != 0 )
        {
            // Wait for ping check interval
            if ( m_pingCheckTimer > 0f )
            {
                m_pingCheckTimer -= Time.deltaTime;
            }
            else
            {
                // Start watch
                m_pingStopwatch.Start ();
                // Ping the server
                ClientSend.Ping ();
                m_pingCheckTimer = PING_CHECK_INTERVAL;
            }
        }
    }

    public void PingReceived ()
    {
        // Stop ping stopwatch
        m_pingStopwatch.Stop ();
        Ping = m_pingStopwatch.ElapsedMilliseconds;
        // Reset stopwatch
        m_pingStopwatch.Reset ();
    }

    #endregion

    #region Disconnect

    /// <summary>Disconnects from the server and stops all network traffic.</summary>
    private void Disconnect ()
    {
        if ( isConnected )
        {
            isConnected = false;
            tcp.socket.Close ();
            if ( udp.socket != null )
            {
                udp.socket.Close ();
            }

            UnityEngine.Debug.Log ( "Disconnected from server." );
        }
    }

    #endregion
}