using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
    private const float MAX_HEALTH = 100;
    private const float POSITION_INTERPOLATION_SPEED = 32f;
    private const float ROTATION_INTERPOLATION_SPEED = 32f;

    public delegate void HealthHandler ( int playerId, float currentHealth, float maxHealth );
    public static HealthHandler OnHealthUpdated;

    public int Id { get; private set; }
    public string Username { get; private set; }
    public Player Player { get; private set; }
    public float Health { get; private set; }
    public bool IsInitialized { get; private set; }


    public PlayerManager ( int _id, string _username )
    {
        Id = _id;
        Username = _username;

        Player = null; // Player will be assigned when the player spawns into the scene
    }

    public void InitializePlayer ( Player _player )
    {
        IsInitialized = true;
        Player = _player;
        Player.Initialize ( Id, Username );
        SetHealth ( MAX_HEALTH );

        // Tell the server that this player has spawned
        ClientSend.PlayerReady ();
    }

    public void UpdatePlayerPosition ( Vector3 position )
    {
        Player.SetUpdatedPosition ( position );
    }

    public void UpdatePlayerRotation ( Quaternion rotation )
    {
        Player.SetUpdatedRotation ( rotation );
    }

    public void SetMovementValues ( Vector3 movementVelocity, Vector2 inputVelocity, bool run, bool crouch, bool prone )
    {
        if ( Player == null )
        {
            Debug.LogError ( "Player is null." );
            return;
        }
        Player.SetMovementValues ( movementVelocity, inputVelocity, run, crouch, prone );
    }

    public void OnServerFrame ( byte [] processedRequest )
    {
        Player.OnServerFrame ( processedRequest );
    }

    public void SetHealth ( float _health )
    {
        if ( Player == null )
        {
            Debug.LogError ( "Player is null." );
            return;
        }

        Health = _health;
        OnHealthUpdated?.Invoke ( Id, Health, MAX_HEALTH );

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
