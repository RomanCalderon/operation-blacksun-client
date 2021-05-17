using PlayerInput;
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
        Client.instance.SetClientId ( _myId );
        ClientSend.WelcomeReceived ();

        // Now that we have the client's id, connect UDP
        Client.instance.udp.Connect ( ( ( IPEndPoint ) Client.instance.tcp.socket.Client.LocalEndPoint ).Port );

        // Update UI
        UIManager.instance.PlayerConnected ();
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
        Client.instance.PingReceived ();
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

        // Ignore if Player is null
        if ( !GameManager.players.ContainsKey ( _id ) || GameManager.players [ _id ].Player == null )
        {
            return;
        }

        // Update the players new position
        Vector3 newPosition = _packet.ReadVector3 ();
        GameManager.players [ _id ].UpdatePlayerPosition ( newPosition );
    }

    public static void PlayerRotation ( Packet _packet )
    {
        int _id = _packet.ReadInt ();

        // Ignore if Player is null
        if ( !GameManager.players.ContainsKey ( _id ) || GameManager.players [ _id ].Player == null )
        {
            return;
        }

        // Update the remote players' rotation
        Quaternion newRotation = _packet.ReadQuaternion ();
        GameManager.players [ _id ].UpdatePlayerRotation ( newRotation );
    }

    public static void PlayerMovement ( Packet _packet )
    {
        int _id = _packet.ReadInt ();

        // Ignore if Player is null
        if ( !GameManager.players.ContainsKey ( _id ) || GameManager.players [ _id ].Player == null )
        {
            return;
        }

        int moveInputX = _packet.ReadInt ();
        int moveInputY = _packet.ReadInt ();
        float moveSpeed = _packet.ReadFloat ();
        bool _playerRun = _packet.ReadBool ();
        bool _playerCrouch = _packet.ReadBool ();

        // Used for visual effects
        GameManager.players [ _id ].SetMovementValues ( moveInputX, moveInputY, moveSpeed, _playerRun, _playerCrouch );
    }

    public static void PlayerInputProcessed ( Packet _packet )
    {
        int playerId = _packet.ReadInt ();

        // Ignore if Player is null
        if ( !GameManager.players.ContainsKey ( playerId ) || GameManager.players [ playerId ].Player == null )
        {
            return;
        }

        int length = _packet.ReadInt ();
        byte [] request = _packet.ReadBytes ( length );
        GameManager.players [ playerId ].OnServerFrame ( request );
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

    public static void PlayerUpdateInventorySlot ( Packet _packet )
    {
        string _slotId = _packet.ReadString ();
        string _playerItemId = _packet.ReadString ();
        int _quantity = _packet.ReadInt ();

        InventoryManager.Instance.UpdateSlot ( _slotId, _playerItemId, _quantity );
    }

    #endregion

    public static void SpawnHitObject ( Packet _packet )
    {
        int _hitType = _packet.ReadInt ();
        Vector3 _hitPosition = _packet.ReadVector3 ();
        Vector3 _hitNormal = _packet.ReadVector3 ();

        ShootableObjectsManager.Instance.SpawnFromPool ( ( ShootableObjectsManager.HitObjects ) _hitType, _hitPosition, _hitNormal );
    }

    public static void PlayAudioClip ( Packet _packet )
    {
        int _id = _packet.ReadInt ();
        string _audioClipName = _packet.ReadString ();
        float _volume = _packet.ReadFloat ();
        Vector3 _location = _packet.ReadVector3 ();
        float _minDistance = _packet.ReadFloat ();
        float _maxDistance = _packet.ReadFloat ();

        AudioManager.PlaySound ( _audioClipName, _volume, _location, _minDistance, _maxDistance );
    }

    public static void Hitmarker ( Packet _packet )
    {
        int _id = _packet.ReadInt ();
        int _hitmarkerType = _packet.ReadInt ();

        GameManager.players [ _id ].Player.Hitmarker ( _hitmarkerType );
    }

    #region Item Spawner

    public static void CreateItemSpawner ( Packet _packet )
    {
        int _spawnerDataLength = _packet.ReadInt ();
        byte [] _spawnerData = _packet.ReadBytes ( _spawnerDataLength );

        ItemSpawnerManager.Instance.CreateItemSpawner ( _spawnerData );
    }

    public static void SpawnItem ( Packet _packet )
    {
        int _spawnerDataLength = _packet.ReadInt ();
        byte [] _spawnerData = _packet.ReadBytes ( _spawnerDataLength );

        ItemSpawnerManager.Instance.SpawnItem ( _spawnerData );
    }

    public static void DestroyItem ( Packet _packet )
    {
        int _spawnerId = _packet.ReadInt ();

        ItemSpawnerManager.ItemSpawners [ _spawnerId ].DestroyItem ();
    }

    #endregion

    #region Networked Rigidbody

    public static void NetworkedRigidbodyData ( Packet _packet )
    {
        int _dataLength = _packet.ReadInt ();
        byte [] _data = _packet.ReadBytes ( _dataLength );

        NetworkedRigidbodyManager.Instance.UpdateBody ( _data );
    }

    #endregion
}