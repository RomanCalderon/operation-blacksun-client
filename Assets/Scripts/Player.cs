using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent ( typeof ( PlayerModelController ) )]
public class Player : MonoBehaviour
{
    private int m_id;
    private string m_username;

    public float Health
    {
        get
        {
            return m_health;
        }
    }
    [SerializeField]
    private float m_health;
    private float m_maxHealth = 100;
    private PlayerModelController m_modelController = null;

    private void Start ()
    {
        m_modelController = GetComponent<PlayerModelController> ();
    }

    public void Initialize ( int _id, string _username )
    {
        m_id = _id;
        m_username = _username;
        m_health = m_maxHealth;
    }

    // Update is called once per frame
    void Update ()
    {

    }

    public void SetMovementVector ( Vector2 movement )
    {
        m_modelController.SetMovementVector ( movement );
    }

    public void SetHealth ( float _health )
    {
        m_health = _health;

        if ( m_health <= 0f )
        {
            Die ();
        }
    }

    public void Die ()
    {
        // FIXME: deal with model reappearing on respawn
        // maybe instantiate a ragdoll when player dies
        if ( Client.instance.myId != m_id )
        {
            m_modelController.HideModel ( true );
        }
    }

    public void Respawn ()
    {
        m_modelController.HideModel ( false );
        SetHealth ( m_maxHealth );
    }
}
