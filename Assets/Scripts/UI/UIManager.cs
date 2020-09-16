using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header ( "UI Views" )]
    [SerializeField]
    private GameObject serverConnectView = null;
    [SerializeField]
    private GameObject playerSpawnView = null;

    [Header ( "Server Connect View UI" )]
    public InputField usernameField;
    public InputField serverIPField;


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
        usernameField.text = PlayerPrefs.GetString ( "Username" );
        //serverIPField.text = PlayerPrefs.GetString ( "ServerIP" );
    }

    /// <summary>
    /// Connects to a game server with a pre-defined IP address and port.
    /// </summary>
    public void ConnectToServer ()
    {
        string serverIPAddress = Client.instance.ip;
        Match match = Regex.Match ( serverIPAddress, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b" );
        if ( match.Success )
        {
            serverIPAddress = match.Value;
            Debug.Log ( $"Valid IP address: {serverIPAddress}" );
            PlayerPrefs.SetString ( "Username", usernameField.text );
            PlayerPrefs.SetString ( "ServerIP", serverIPAddress );
        }
        else
        {
            Debug.LogError ( "Invalid IP address." );
            return;
        }

        // Hide server connect view
        serverConnectView.SetActive ( false );
        usernameField.interactable = false;
        serverIPField.interactable = false;
        Client.instance.ConnectToServer ();

        // Display player loadout/spawn view
        playerSpawnView.SetActive ( true );
    }

    /// <summary>
    /// Spawns the player into the scene.
    /// </summary>
    public void SpawnPlayer ()
    {
        ClientSend.SpawnPlayer ( usernameField.text );

        // Hide player loadout/spawn view
        playerSpawnView.SetActive ( false );
    }
}
