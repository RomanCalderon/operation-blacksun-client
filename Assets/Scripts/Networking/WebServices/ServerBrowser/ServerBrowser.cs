using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebServiceCommunications;
using Payloads;

[RequireComponent ( typeof ( WebServiceCommunication ) )]
[RequireComponent ( typeof ( ServerBrowserView ) )]
public class ServerBrowser : MonoBehaviour
{
    #region Models

    [System.Serializable]
    public class GameServerCollection
    {
        public GameServer [] gameServerList;
    }

    [System.Serializable]
    public class GameServer
    {
        public int id;
        public string name;
        public string ip;
        public ushort port;
        public byte clientSceneIndex;
        public byte playerCount;
        public byte maxPlayers;

        public GameServer ( int id, string name, string ip, ushort port, byte clientSceneIndex, byte playerCount, byte maxPlayers )
        {
            this.id = id;
            this.name = name;
            this.ip = ip;
            this.port = port;
            this.clientSceneIndex = clientSceneIndex;
            this.playerCount = playerCount;
            this.maxPlayers = maxPlayers;
        }

        public override string ToString ()
        {
            return $"Server registration response: id={id} | name={name}";
        }
    }

    #endregion

    private const string REQUEST_URL_RELATIVE = "/api/gameserver";

    private WebServiceCommunication m_wsc = null;
    private ServerBrowserView m_view = null;

    [SerializeField]
    private GameServerCollection m_gameServerCollection = null;

    private void Awake ()
    {
        m_wsc = GetComponent<WebServiceCommunication> ();
        m_view = GetComponent<ServerBrowserView> ();
    }

    // Start is called before the first frame update
    void Start ()
    {

    }

    /// <summary>
    /// Retrieves a collection of all registered game servers from the master server. Clears the list of all previous game servers immediately.
    /// </summary>
    public void GetServerList ()
    {
        m_view.ClearServerList ();

        m_wsc.Get ( REQUEST_URL_RELATIVE, ServerListResponse );
    }

    private void ServerListResponse ( string responseBody )
    {
        if ( string.IsNullOrEmpty ( responseBody ) )
        {
            return;
        }
        Debug.Log ( responseBody );

        m_gameServerCollection = JsonUtility.FromJson<GameServerCollection> ( "{\"gameServerList\":" + responseBody + "}" );
        m_view.LoadServerList ( m_gameServerCollection, ConnectToServer );
    }

    private void ConnectToServer ( string ip, ushort port, int clientSceneIndex )
    {
        Debug.Log ( $"Connect to server: {ip}:{port}" );

        // Send ip and port to payload
#if UNITY_EDITOR
        ServerClientConnectPayload.Instance.Ip = "192.168.1.67";
#else
        ServerClientConnectPayload.Instance.Ip = ip;
#endif
        ServerClientConnectPayload.Instance.Port = port;

        TransitionToGameScene ( clientSceneIndex );
    }

    private void TransitionToGameScene ( int clientSceneIndex )
    {
        // Transition to game scene
        SceneController.Instance.LoadScene ( clientSceneIndex );
    }
}
