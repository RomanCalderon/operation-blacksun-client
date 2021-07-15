using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebServiceCommunications;

[RequireComponent ( typeof ( WebServiceCommunication ) )]
[RequireComponent ( typeof ( ServerBrowserView ) )]
public class ServerBrowser : MonoBehaviour
{
    #region Models

    [System.Serializable]
    public class GameServerCollection
    {
        [System.Serializable]
        public struct EmbeddedData
        {
            public GameServer [] gameServerList;
        }

        public EmbeddedData _embedded;
    }

    [System.Serializable]
    public class GameServer
    {
        [System.Serializable]
        public class LinkContainer
        {
            [System.Serializable]
            public struct Link
            {
                public string href;
            }

            public Link self;
            public Link gameservers;
        }

        public int id;
        public string name;
        public string ip;
        public short port;
        public string sceneIndex;
        public string playerCount;
        public string maxPlayers;
        public LinkContainer _links;

        public GameServer ( int id, string name, string ip, short port, string sceneIndex, string playerCount, string maxPlayers, LinkContainer links )
        {
            this.id = id;
            this.name = name;
            this.ip = ip;
            this.port = port;
            this.sceneIndex = sceneIndex;
            this.playerCount = playerCount;
            this.maxPlayers = maxPlayers;
            _links = links;
        }

        public override string ToString ()
        {
            return $"Server registration response: id={id} | name={name}";
        }
    }

    #endregion

    private const string GAME_SERVERS_URL_RELATIVE = "/gameservers";

    private WebServiceCommunication m_wsc = null;
    private ServerBrowserView m_view = null;

    [ SerializeField]
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

        m_wsc.Get ( GAME_SERVERS_URL_RELATIVE, ServerListResponse );
    }

    private void ServerListResponse ( string responseBody )
    {
        if ( string.IsNullOrEmpty ( responseBody ) )
        {
            return;
        }
        Debug.Log ( responseBody );

        m_gameServerCollection = JsonUtility.FromJson<GameServerCollection> ( responseBody );
        m_view.LoadServerList ( m_gameServerCollection, ConnectToServer );
    }

    private void ConnectToServer ( string ip, short port )
    {
        Debug.Log ( $"Connect to server: {ip}:{port}" );
    }
}
