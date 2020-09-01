using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent ( typeof ( PlayerModelController ) )]
[RequireComponent ( typeof ( InventoryManager ) )]
public class Player : MonoBehaviour
{
    private int m_id;
    private string m_username;
    public bool IsDead { get; private set; }
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

    public Vector3 MovementVelocity
    {
        get
        {
            return m_movementVelocity;
        }
    }
    private Vector3 m_movementVelocity = Vector3.zero;

    public void Initialize ( int _id, string _username )
    {
        m_id = _id;
        m_username = _username;

        if ( Client.instance.myId == m_id )
        {
            m_cameraController.CanControl ( true );
        }
    }

    public void SetMovementValues ( Vector3 movementVelocity, Vector2 inputVelocity, bool runInput, bool crouchInput, bool proneInput )
    {
        m_movementVelocity = movementVelocity;

        if ( Client.instance.myId == m_id )
        {
            m_cameraController.ApplySlideShake ( crouchInput );
        }
        else
        {
            // Eventually this will be executed for the local player as well
            m_modelController.SetMovementVector ( inputVelocity );
            m_modelController.SetRun ( runInput );
            m_modelController.SetCrouch ( crouchInput );
            m_modelController.SetProne ( proneInput );
        }
    }

    public void Die ()
    {
        IsDead = true;
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
        IsDead = false;
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
