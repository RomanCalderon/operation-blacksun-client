using Michsky.UI.Shift;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerBrowserView : MonoBehaviour
{
    [SerializeField]
    private Transform m_serverButtonContainer = null;
    [SerializeField]
    private ServerButton m_serverButtonPrefab = null;

    [SerializeField]
    private ModalWindowManager m_selectedServerModal = null;
    [SerializeField]
    private Button m_connectButton = null;
    [SerializeField]
    private Button m_cancelButton = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ClearServerList ()
    {
        ClearContainer ();
    }

    public void LoadServerList ( ServerBrowser.GameServerCollection gameServerCollection, Action<string, ushort> connectCallback )
    {
        ClearContainer ();

        if ( gameServerCollection == null )
        {
            Debug.LogError ( "GameServerCollection is null." );
            return;
        }
        if ( gameServerCollection.gameServerList == null || gameServerCollection.gameServerList.Length == 0 )
        {
            // No game servers
            return;
        }

        foreach ( ServerBrowser.GameServer gameServer in gameServerCollection.gameServerList )
        {
            ServerButton serverButton = Instantiate ( m_serverButtonPrefab, m_serverButtonContainer );
            string name = gameServer.name;
            int playerCount = gameServer.playerCount;
            int maxPlayers = gameServer.maxPlayers;
            serverButton.Initialize ( name, playerCount, maxPlayers );
            serverButton.button.onClick.AddListener ( () => ServerButtonClicked ( gameServer, connectCallback ) );
        }
    }

    private void ServerButtonClicked ( ServerBrowser.GameServer gameServer, Action<string, ushort> connectCallback )
    {
        m_selectedServerModal.ModalWindowIn ();
        m_connectButton.onClick.AddListener ( () => connectCallback?.Invoke ( gameServer.ip, gameServer.port ) );
        m_cancelButton.onClick.AddListener ( () => m_connectButton.onClick.RemoveAllListeners() );
    }

    private void ClearContainer ()
    {
        foreach ( Transform serverButton in m_serverButtonContainer )
        {
            Destroy ( serverButton.gameObject );
        }
    }
}
