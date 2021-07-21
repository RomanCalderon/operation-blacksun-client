using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Payloads;

public class ClientConnectManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetServerAddress ();
    }

    private void GetServerAddress ()
    {
        string serverIPAddress = ServerClientConnectPayload.Instance.Ip;
        ushort port = ServerClientConnectPayload.Instance.Port;

        // Sanity check ip and port values
        Match match = Regex.Match ( serverIPAddress, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b" );
        if ( match.Success )
        {
            serverIPAddress = match.Value;
        }
        else
        {
            Debug.LogError ( $"Failed connecting to game server: Invalid IP address ({serverIPAddress}:{port})" );
            return;
        }

        ConnectToServer ( serverIPAddress, port );
    }

    private void ConnectToServer ( string ip, ushort port ) =>
        // Start connection to the server
        Client.instance.ConnectToServer ( ip, port );
}
