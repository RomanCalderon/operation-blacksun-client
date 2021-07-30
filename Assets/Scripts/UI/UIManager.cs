using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region Models

    private enum States
    {
        SERVER_CONNECT,
        SERVER_CONNECTING,
        PLAYER_START,
        NONE
    }

    #endregion

    private const string USERNAME_PLAYERPREF = "Username";
    private const string SERVER_IP_PLAYERPREF = "ServerIP";
    private const string SERVER_CONNECT_LABEL = "CONNECT";
    private const string SERVER_CONNECTING_LABEL = "CONNECTING...";

    public static UIManager instance;

    [Header ( "UI Views" )]
    [SerializeField]
    private GameObject serverConnectView = null;
    [SerializeField]
    private GameObject playerSpawnView = null;
    private States m_currentState = States.SERVER_CONNECT;

    [Header ( "Server Connect View UI" )]
    [SerializeField]
    private InputField m_usernameField = null;
    [SerializeField]
    private InputField m_serverIPField = null;
    [SerializeField]
    private Button m_serverConnectButton = null;
    [SerializeField]
    private Text m_serverConnectButtonText = null;

    public string Username { get; private set; }
    public string ServerIP { get; private set; }

    private void OnEnable ()
    {
        Client.OnServerConnect += UpdateServerConnectionStatus;
    }

    private void OnDisable ()
    {
        Client.OnServerConnect -= UpdateServerConnectionStatus;
    }

    private void Awake ()
    {
        if ( instance == null )
        {
            instance = this;
        }
        else if ( instance != this )
        {
            Debug.Log ( "Instance already exists, destroying object!" );
            Destroy ( this );
        }
    }

    private void Start ()
    {
        m_usernameField.text = PlayerPrefs.GetString ( USERNAME_PLAYERPREF );
        m_serverIPField.text = PlayerPrefs.GetString ( SERVER_IP_PLAYERPREF );

        TransitionUIState ( States.SERVER_CONNECT );
    }

    #region Button Callbacks

    // DEBUG
    public void DebugConnect ()
    {
        Client.instance.ConnectToServer ( m_serverIPField.text, 26950 );
    }

    /// <summary>
    /// Spawns the player into the scene.
    /// </summary>
    public void SpawnPlayer ()
    {
        // Hide player loadout/spawn view
        TransitionUIState ( States.NONE );

        ClientSend.SpawnPlayer ( Client.instance.myUsername );
    }

    #endregion

    #region UI Management

    public void PlayerConnected ()
    {
        TransitionUIState ( States.PLAYER_START );
    }

    private void UpdateServerConnectionStatus ( int status )
    {
        switch ( status )
        {
            case 0: // Attempting to connect
                TransitionUIState ( States.SERVER_CONNECTING );
                break;
            case 2: // Connection timed out/failed
                TransitionUIState ( States.SERVER_CONNECT );
                break;
            default:
                break;
        }
    }

    private void TransitionUIState ( States state )
    {
        // Clear UI
        ClearUI ();

        // Set current state
        m_currentState = state;

        // Display state
        switch ( state )
        {
            case States.SERVER_CONNECT:
                serverConnectView.SetActive ( true );
                m_usernameField.interactable = true;
                m_serverConnectButton.interactable = true;
                m_serverConnectButtonText.text = SERVER_CONNECT_LABEL;
                break;
            case States.SERVER_CONNECTING:
                serverConnectView.SetActive ( false );
                m_usernameField.interactable = false;
                m_serverConnectButton.interactable = false;
                m_serverConnectButtonText.text = SERVER_CONNECTING_LABEL;
                break;
            case States.PLAYER_START:
                playerSpawnView.SetActive ( true );
                break;
            default:
                break;
        }
    }

    private void ClearUI ()
    {
        // Server connect
        serverConnectView.SetActive ( false );

        // Player start
        playerSpawnView.SetActive ( false );
    }

    #endregion
}
