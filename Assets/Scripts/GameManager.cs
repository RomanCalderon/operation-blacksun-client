using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager> ();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

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

    /// <summary>
    /// Adds a new PlayerManager to players for the new player that connected.
    /// </summary>
    /// <param name="_id">The player's id.</param>
    /// <param name="_username">The player's username.</param>
    public void PlayerConnected ( int _id, string _username )
    {
        Debug.Log ( $"[{_username}] connected (ID: {_id}). Adding new PlayerManager to players Dictionary for {_username} at key {_id}." );
        PlayerManager _playerManager = new PlayerManager ( _id, _username );
        players.Add ( _id, _playerManager );
        Debug.Log ( $"players.Count is now {players.Count}" );
    }

    /// <summary>Spawns a player.</summary>
    /// <param name="_id">The player's ID.</param>
    /// <param name="_position">The player's starting position.</param>
    /// <param name="_rotation">The player's starting rotation.</param>
    public void SpawnPlayer ( int _id, Vector3 _position, Quaternion _rotation )
    {
        Debug.Log ( $"SpawnPlayer _id [{_id}] _username [{players [ _id ].Username}]" );
        GameObject _player;
        if ( _id == Client.instance.myId )
        {
            _player = Instantiate ( localPlayerPrefab, _position, _rotation );
        }
        else
        {
            _player = Instantiate ( playerPrefab, _position, _rotation );
        }
        // Initialize this PlayerManager with a username and Player component
        players [ _id ].InitializePlayer ( _player.GetComponent<Player> () );
    }
}