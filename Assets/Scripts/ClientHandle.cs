using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome ( Packet _packet )
    {
        string _msg = _packet.ReadString ();
        int _myId = _packet.ReadInt ();

        Debug.Log ( $"Message from server: {_msg}" );
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived ();

        // Now that we have the client's id, connect UDP
        Client.instance.udp.Connect ( ( ( IPEndPoint ) Client.instance.tcp.socket.Client.LocalEndPoint ).Port );
    }

    #region Player

    public static void ConnectPlayer ( Packet _packet )
    {
        int _id = _packet.ReadInt ();
        string _username = _packet.ReadString ();

        GameManager.instance.PlayerConnected ( _id, _username );
    }

    public static void Ping ( Packet _packet )
    {
        int elapsedTime = _packet.ReadInt ();
        string serverBounceTime = _packet.ReadString ();

        DateTime serverTime = DateTime.ParseExact ( serverBounceTime, "o", CultureInfo.CurrentCulture );
        int travelTimeSpan = DateTime.Now.Millisecond - serverTime.Millisecond;
        int rtt = travelTimeSpan + elapsedTime;
        Client.instance.UpdatePing ( rtt );
    }

    public static void SpawnPlayer ( Packet _packet )
    {
        int _id = _packet.ReadInt ();
        Vector3 _position = _packet.ReadVector3 ();
        Quaternion _rotation = _packet.ReadQuaternion ();

        GameManager.instance.SpawnPlayer ( _id, _position, _rotation );
    }

    public static void PlayerPosition ( Packet _packet )
    {
        int _id = _packet.ReadInt ();
        // Ignore if Player hasn't been assigned yet
        if ( GameManager.players [ _id ].Player == null )
        {
            return;
        }

        Vector3 _position = _packet.ReadVector3 ();
        Vector3 oldPosition = GameManager.players [ _id ].Player.transform.position;

        // Interpolate the players position
        GameManager.players [ _id ].Player.transform.position = Vector3.Lerp ( oldPosition, _position, Time.deltaTime * 32f );
    }

    public static void PlayerRotation ( Packet _packet )
    {
        int _id = _packet.ReadInt ();
        // Ignore if Player hasn't been assigned yet
        if ( GameManager.players [ _id ].Player == null )
        {
            return;
        }

        Quaternion _rotation = _packet.ReadQuaternion ();

        GameManager.players [ _id ].Player.transform.rotation = _rotation;
    }

    public static void PlayerMovementVector ( Packet _packet )
    {
        int _id = _packet.ReadInt ();
        float _playerMovementX = _packet.ReadFloat ();
        float _playerMovementY = _packet.ReadFloat ();

        GameManager.players [ _id ].SetMovementVector ( new Vector2 ( _playerMovementX, _playerMovementY ) );
    }

    public static void PlayerDisconnected ( Packet _packet )
    {
        int _id = _packet.ReadInt ();

        if ( GameManager.players [ _id ].Player != null )
        {
            Destroy ( GameManager.players [ _id ].Player.gameObject );
        }
        GameManager.players.Remove ( _id );
    }

    public static void PlayerHealth ( Packet _packet )
    {
        int _id = _packet.ReadInt ();
        float _health = _packet.ReadFloat ();

        GameManager.players [ _id ].SetHealth ( _health );
    }

    public static void PlayerRespawned ( Packet _packet )
    {
        int _id = _packet.ReadInt ();

        GameManager.players [ _id ].Respawn ();
    }

    #endregion
}