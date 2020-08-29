using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
    private const float MAX_HEALTH = 100;

    public int Id { get; private set; }
    public string Username { get; private set; }
    public Player Player { get; private set; }
    public float Health { get; private set; }

    public PlayerManager ( int _id, string _username )
    {
        Id = _id;
        Username = _username;

        Player = null; // Player will be assigned when the player spawns into the scene
    }

    public void InitializePlayer ( Player _player )
    {
        Player = _player;
        Player.Initialize ( Id, Username );
        SetHealth ( MAX_HEALTH );

        // Tell the server that this player has spawned
        ClientSend.PlayerReady ();
    }

    public void SetMovementValues ( Vector2 movement, bool run, bool crouch, bool prone )
    {
        if ( Player == null )
        {
            Debug.LogError ( "Player is null." );
            return;
        }
        Player.SetMovementVector ( movement );
        Player.SetRun ( run );
        Player.SetCrouch ( crouch );
        Player.SetProne ( prone );
    }

    public void SetHealth ( float _health )
    {
        if ( Player == null )
        {
            Debug.LogError ( "Player is null." );
            return;
        }

        Health = _health;

        if ( Health <= 0f )
        {
            Die ();
        }
    }

    private void Die ()
    {
        if ( Player == null )
        {
            Debug.LogError ( "Player is null." );
            return;
        }

        Player.Die ();
    }

    public void Respawn ()
    {
        if ( Player == null )
        {
            Debug.LogError ( "Player is null." );
            return;
        }

        SetHealth ( MAX_HEALTH );
        Player.Respawn ();
    }
}
