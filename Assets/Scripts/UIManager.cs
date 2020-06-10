using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject startMenu;
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
        serverIPField.text = PlayerPrefs.GetString ( "ServerIP" );
    }

    public void ConnectToServer ()
    {
        string serverIPAddress = serverIPField.text;
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

        startMenu.SetActive ( false );
        usernameField.interactable = false;
        serverIPField.interactable = false;
        Client.instance.ConnectToServer ( serverIPField.text );
    }
}
