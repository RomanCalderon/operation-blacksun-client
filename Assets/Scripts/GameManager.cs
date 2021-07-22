using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager> ();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

    [SerializeField]
    private GameObject m_mainCamera = null;

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

        Debug.Assert ( m_mainCamera != null, "m_mainCamera is null." );
    }

    /// <summary>
    /// Adds a new PlayerManager to players for the new player that connected.
    /// </summary>
    /// <param name="id">The player's id.</param>
    /// <param name="username">The player's username.</param>
    public void AddPlayer ( int id, string username )
    {
        if ( players.ContainsKey ( id ) )
        {
            RemovePlayer ( id );
        }
        Debug.Log ( $"[{username}][{id}] has joined the game." );

        PlayerManager _playerManager = new PlayerManager ( id, username );
        players.Add ( id, _playerManager );
    }

    public void RemovePlayer ( int id )
    {
        if ( !players.ContainsKey ( id ) )
        {
            Debug.LogError ( $"Dictionary does not contain player with id [{id}]" );
            return;
        }
        if ( players [ id ].Player != null )
        {
            Destroy ( players [ id ].Player.gameObject );
        }
        if ( !players.Remove ( id ) )
        {
            Debug.LogError ( $"Error removing player [{id}] from the dictionary." );
        }
    }

    /// <summary>Spawns a player.</summary>
    /// <param name="_id">The player's ID.</param>
    /// <param name="_position">The player's starting position.</param>
    /// <param name="_rotation">The player's starting rotation.</param>
    public void SpawnPlayer ( int _id, Vector3 _position, Quaternion _rotation )
    {
        GameObject _player;
        if ( _id == Client.instance.myId )
        {
            _player = Instantiate ( localPlayerPrefab, _position, _rotation );

            // Disable the main camera
            m_mainCamera.SetActive ( false );
        }
        else
        {
            _player = Instantiate ( playerPrefab, _position, _rotation );
        }
        // Initialize this PlayerManager with a username and Player component
        players [ _id ].InitializePlayer ( _player.GetComponent<Player> () );

    }
}