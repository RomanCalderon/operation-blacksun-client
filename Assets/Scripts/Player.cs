using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent ( typeof ( PlayerModelController ) )]
public class Player : MonoBehaviour
{
    private int m_id;
    private string m_username;
    private PlayerModelController m_modelController = null;

    [SerializeField]
    private GameObject m_modelRagdoll = null;

    private void Awake ()
    {
        m_modelController = GetComponent<PlayerModelController> ();
    }

    private void Start ()
    {
        Debug.Assert ( m_modelRagdoll != null, "Model ragdoll is null." );
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

        // Spawn a ragdoll that represents the player's dead body
        Instantiate ( m_modelRagdoll, transform.position, transform.rotation );
    }

    public void Respawn ()
    {
        m_modelController.ShowModel ( true );
    }
}
