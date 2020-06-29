using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
    public int Id { get; private set; }
    public string Username { get; private set; }
    public Player Player { get; private set; }


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
    }

    public void SetHealth ( float _health )
    {
        if ( Player == null )
        {
            Debug.LogError ( "Player component is not assigned." );
            return;
        }

        Player.SetHealth ( _health );

        if ( Player.Health <= 0f )
        {
            Die ();
        }
    }

    public void SetMovementVector ( Vector2 movement )
    {
        Player.SetMovementVector ( movement );
    }

    public void Die ()
    {
        if ( Player == null )
        {
            Debug.LogError ( "Player component is not assigned." );
            return;
        }

        Player.Die ();
    }

    public void Respawn ()
    {
        if ( Player == null )
        {
            Debug.LogError ( "Player component is not assigned." );
            return;
        }

        Player.Respawn ();
    }
}
