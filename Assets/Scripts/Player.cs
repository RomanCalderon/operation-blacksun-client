using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent ( typeof ( PlayerModelController ) )]
public class Player : MonoBehaviour
{
    private int m_id;
    private string m_username;

    private PlayerModelController m_modelController = null;

    private void Awake ()
    {
        m_modelController = GetComponent<PlayerModelController> ();
    }

    public void Initialize ( int _id, string _username )
    {
        m_id = _id;
        m_username = _username;
    }

    public void SetMovementVector ( Vector2 movement )
    {
        m_modelController.SetMovementVector ( movement );
    }

    public void Die ()
    {
        m_modelController.ShowModel ( false );
        
        // TODO: instantiate a ragdoll when player dies
    }

    public void Respawn ()
    {
        m_modelController.ShowModel ( true );
    }
}
