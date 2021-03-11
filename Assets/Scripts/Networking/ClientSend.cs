using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData ( Packet _packet )
    {
        _packet.WriteLength ();
        Client.instance.tcp.SendData ( _packet );
    }

    private static void SendUDPData ( Packet _packet )
    {
        _packet.WriteLength ();
        Client.instance.udp.SendData ( _packet );
    }

    #region Packets
    public static void WelcomeReceived ()
    {
        using ( Packet _packet = new Packet ( ( int ) ClientPackets.welcomeReceived ) )
        {
            _packet.Write ( Client.instance.myId );
            _packet.Write ( UIManager.instance.Username );

            SendTCPData ( _packet );
        }
    }

    public static void Ping ()
    {
        using ( Packet _packet = new Packet ( ( int ) ClientPackets.ping ) )
        {
            SendTCPData ( _packet );
        }
    }

    public static void SpawnPlayer ( string _username )
    {
        using ( Packet _packet = new Packet ( ( int ) ClientPackets.spawnPlayer ) )
        {
            _packet.Write ( Client.instance.myId );
            _packet.Write ( _username );

            SendTCPData ( _packet );
        }
    }

    public static void PlayerReady ()
    {
        using ( Packet _packet = new Packet ( ( int ) ClientPackets.playerReady ) )
        {
            _packet.Write ( Client.instance.myId );

            SendTCPData ( _packet );
        }
    }

    public static void PlayerInput ( byte [] _inputs )
    {
        using ( Packet _packet = new Packet ( ( int ) ClientPackets.playerInput ) )
        {
            _packet.Write ( _inputs.Length );
            _packet.Write ( _inputs );
            
            SendUDPData ( _packet );
        }
    }

    public static void PlayerShoot ( Vector3 _direction, float _damage, string _gunshotClip, float _gunshotVolume, float _minDistance, float _maxDistance )
    {
        using ( Packet _packet = new Packet ( ( int ) ClientPackets.playerShoot ) )
        {
            _packet.Write ( _direction );
            _packet.Write ( _damage );
            _packet.Write ( _gunshotClip );
            _packet.Write ( _gunshotVolume );
            _packet.Write ( _minDistance );
            _packet.Write ( _maxDistance );

            SendTCPData ( _packet );
        }
    }

    public static void PlayerTransferSlotContents ( string fromSlotId, string toSlotId, int transferMode )
    {
        using ( Packet _packet = new Packet ( ( int ) ClientPackets.playerTransferSlotContents ) )
        {
            _packet.Write ( fromSlotId );
            _packet.Write ( toSlotId );
            _packet.Write ( transferMode );

            SendTCPData ( _packet );
        }
    }

    public static void PlayerInventoryReduceItem ( string playerItemId, int reductionAmount )
    {
        using ( Packet _packet = new Packet ( ( int ) ClientPackets.playerInventoryReduceItem ) )
        {
            _packet.Write ( playerItemId );
            _packet.Write ( reductionAmount );

            SendTCPData ( _packet );
        }
    }
    #endregion
}