using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent ( typeof ( PlayerModelController ) )]
[RequireComponent ( typeof ( InventoryManager ) )]
public class Player : MonoBehaviour
{
    private int m_id;
    private string m_username;
    [SerializeField]
    private Camera m_playerCamera = null;
    [SerializeField]
    private Camera m_fpCamera = null;
    public InventoryManager InventoryManager { get; private set; }
    public WeaponsController WeaponsController { get; private set; }
    [SerializeField]
    private GameObject m_weaponsController = null;
    [SerializeField]
    private CameraController m_cameraController = null;
    private PlayerModelController m_modelController = null;
    [SerializeField]
    private RagdollMasterJointController m_ragdollController = null;
    [SerializeField]
    private Camera m_ragdollCamera = null;
    [SerializeField]
    private Transform m_ragdollParent = null;

    private void Awake ()
    {
        m_modelController = GetComponent<PlayerModelController> ();
        InventoryManager = GetComponent<InventoryManager> ();
        if ( Client.instance.myId == m_id )
        {
            WeaponsController = m_weaponsController.GetComponent<WeaponsController> ();
        }
    }

    private void Start ()
    {
        Debug.Assert ( m_ragdollController != null, "Ragdoll controller is null." );
        Debug.Assert ( m_ragdollParent != null, "Ragdoll parent is null." );
    }

    public void Initialize ( int _id, string _username )
    {
        m_id = _id;
        m_username = _username;

        if ( Client.instance.myId == m_id )
        {
            m_cameraController.CanControl ( true );
        }
    }

    public void SetMovementVector ( Vector2 movement )
    {
        m_modelController.SetMovementVector ( movement );
    }

    public void SetRun ( bool value )
    {
        m_modelController.SetRun ( value );
    }

    public void SetCrouch ( bool value )
    {
        m_modelController.SetCrouch ( value );
    }

    public void SetProne ( bool value )
    {
        m_modelController.SetProne ( value );
    }

    public void Die ()
    {
        m_modelController.ShowModel ( false );
        if ( Client.instance.myId == m_id )
        {
            m_playerCamera.enabled = false;
            m_cameraController.CanControl ( false );
            m_fpCamera.enabled = false;
            m_ragdollCamera.enabled = true;
        }

        // Enable the player ragdoll
        m_ragdollController.transform.SetParent ( null );
        m_ragdollController.EnableMeshRenderers ( true );
        m_ragdollController.EnableFollowTarget ( false );
    }

    public void Respawn ()
    {
        m_modelController.ShowModel ( true );
        if ( Client.instance.myId == m_id )
        {
            m_playerCamera.enabled = true;
            m_cameraController.CanControl ( true );
            m_fpCamera.enabled = true;
            m_ragdollCamera.enabled = false;
        }

        // Reset and disable the player ragdoll
        m_ragdollController.transform.SetPositionAndRotation ( m_ragdollParent.position, m_ragdollParent.rotation );
        m_ragdollController.transform.SetParent ( m_ragdollParent );
        m_ragdollController.EnableMeshRenderers ( false );
        m_ragdollController.EnableFollowTarget ( true );
    }
}
