using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
    private const float MAX_HEALTH = 100;
    private const float POSITION_INTERPOLATION_SPEED = 32f;
    private const float ROTATION_INTERPOLATION_SPEED = 32f;

    public int Id { get; private set; }
    public string Username { get; private set; }
    public Player Player { get; private set; }
    public float Health { get; private set; }
    public bool IsInitialized { get; private set; }

    private Vector3 m_newPlayerPosition;
    private Quaternion m_newPlayerRotation;


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

    /// <summary>
    /// Main update loop called by GameManager to update any
    /// objects for this Player instance.
    /// </summary>
    public void Update ( float _deltaTime )
    {
        //Player.transform.position = Vector3.Lerp ( Player.transform.position, m_newPlayerPosition, _deltaTime * POSITION_INTERPOLATION_SPEED );
        //Player.transform.rotation = Quaternion.Slerp ( Player.transform.rotation, m_newPlayerRotation, _deltaTime * ROTATION_INTERPOLATION_SPEED );
    }

    public void SetPlayerPosition ( Vector3 newPosition )
    {
        m_newPlayerPosition = newPosition;

        // Direct assignment
        Player.transform.position = newPosition;
    }

    public void SetPlayerRotation ( Quaternion newRotation )
    {
        m_newPlayerRotation = newRotation;

        // Direct assignment
        Player.transform.rotation = newRotation;
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
