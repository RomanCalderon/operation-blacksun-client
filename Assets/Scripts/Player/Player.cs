using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerInput;

[RequireComponent ( typeof ( ClientPredictionHandler ) )]
[RequireComponent ( typeof ( PlayerMovementController ) )]
[RequireComponent ( typeof ( PlayerInputController ) )]
[RequireComponent ( typeof ( PlayerModelController ) )]
[RequireComponent ( typeof ( InventoryManager ) )]
[RequireComponent ( typeof ( HitmarkerController ) )]
public class Player : MonoBehaviour
{
    private int m_id;
    private string m_username;
    public bool IsDead { get; private set; }
    [SerializeField]
    private Camera m_playerCamera = null;
    private AudioListener m_playerAudioListener = null;
    [SerializeField]
    private Camera m_fpCamera = null;
    private ClientPredictionHandler m_clientPredictionHandler = null;
    private PlayerMovementController m_playerMovementController = null;
    public InventoryManager InventoryManager { get; private set; }
    public WeaponsController WeaponsController { get; private set; }
    [SerializeField]
    private GameObject m_weaponsController = null;
    [SerializeField]
    private CameraController m_cameraController = null;
    [SerializeField]
    private HitmarkerController m_hitmarkerController = null;
    private PlayerModelController m_modelController = null;
    [SerializeField]
    private RagdollMasterJointController m_ragdollController = null;
    [SerializeField]
    private Camera m_ragdollCamera = null;
    [SerializeField]
    private Transform m_ragdollParent = null;

    public Vector3 MovementVelocity
    {
        get
        {
            return m_playerMovementController.Velocity;
        }
    }

    // Interpolation data
    public float PositionLerpProgress { get; private set; } = 0f;
    public float RotationLerpProgress { get; private set; } = 0f;
    private Vector3 m_targetPosition;
    private Quaternion m_targetRotation = Quaternion.identity;


    private void Awake ()
    {
        m_clientPredictionHandler = GetComponent<ClientPredictionHandler> ();
        m_playerMovementController = GetComponent<PlayerMovementController> ();
        m_modelController = GetComponent<PlayerModelController> ();
        InventoryManager = GetComponent<InventoryManager> ();
    }

    private void Start ()
    {
        Debug.Assert ( m_ragdollController != null, "Ragdoll controller is null." );
        Debug.Assert ( m_ragdollParent != null, "Ragdoll parent is null." );
    }

    private void Update ()
    {
        InterpolateTransform ( Time.deltaTime );
    }

    public void Initialize ( int _id, string _username )
    {
        m_id = _id;
        m_username = _username;

        if ( Client.instance.myId == m_id )
        {
            // Initializations
            m_playerAudioListener = m_playerCamera.GetComponent<AudioListener> ();
            WeaponsController = m_weaponsController.GetComponent<WeaponsController> ();

            m_playerCamera.enabled = true;
            m_cameraController.CanControl ( true );
            m_fpCamera.enabled = true;
            m_ragdollCamera.gameObject.SetActive ( false );

            // Reset feedback
            m_hitmarkerController.ResetHitmarkers ();
        }
        else
        {
            // Initialize animator states
            SetMovementAnimationValues ( 0, 0, 0, false, false );
        }
    }

    #region Movement/Positioning

    public void SetUpdatedPosition ( Vector3 pos )
    {
        PositionLerpProgress = 0f;
        m_targetPosition = pos;
    }

    public void SetUpdatedRotation ( Quaternion rot )
    {
        RotationLerpProgress = 0f;
        m_targetRotation = rot;
    }

    private void InterpolateTransform ( float deltaTime )
    {
        // Increment progress trackers
        PositionLerpProgress += deltaTime * Constants.INTERP_POSITION_SPEED;
        RotationLerpProgress += deltaTime * Constants.INTERP_ROTATION_SPEED;

        // Clamp values since the player will only move up to
        // the most recent state receive from server
        PositionLerpProgress = Mathf.Clamp01 ( PositionLerpProgress );
        RotationLerpProgress = Mathf.Clamp01 ( RotationLerpProgress );

        // Interpolate remote player position and rotation
        if ( Client.instance.myId != m_id )
        {
            transform.position = Vector3.Lerp ( transform.position, m_targetPosition, PositionLerpProgress );
            transform.rotation = Quaternion.Lerp ( transform.rotation, m_targetRotation, RotationLerpProgress );
        }
    }

    public void SetMovementAnimationValues ( int moveInputX, int moveInputY, float moveSpeed, bool runInput, bool crouchInput )
    {
        if ( Client.instance.myId != m_id )
        {
            // Eventually this will be executed for the local player as well
            m_modelController.SetMovementVector ( moveInputX, moveInputY, moveSpeed );
            m_modelController.SetRun ( runInput );
            m_modelController.SetCrouch ( crouchInput );
        }
    }

    public void OnServerFrame ( byte [] simulationState )
    {
        m_clientPredictionHandler.OnServerSimulationStateReceived ( simulationState );
    }

    #endregion

    #region Feedback

    public void Hitmarker ( int type )
    {
        m_hitmarkerController.ShowHitmarker ( type );
    }

    #endregion

    public void Die ()
    {
        IsDead = true;
        m_modelController.ShowModel ( false );
        if ( Client.instance.myId == m_id )
        {
            m_playerCamera.enabled = false;
            m_playerAudioListener.enabled = false;
            m_cameraController.CanControl ( false );
            m_fpCamera.enabled = false;
            m_ragdollCamera.gameObject.SetActive ( true );
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
            m_playerAudioListener.enabled = true;
            m_cameraController.CanControl ( true );
            m_fpCamera.enabled = true;
            m_ragdollCamera.gameObject.SetActive ( false );

            // Reset feedback
            m_hitmarkerController.ResetHitmarkers ();
        }

        // Reset and disable the player ragdoll
        m_ragdollController.transform.SetPositionAndRotation ( m_ragdollParent.position, m_ragdollParent.rotation );
        m_ragdollController.transform.SetParent ( m_ragdollParent );
        m_ragdollController.EnableMeshRenderers ( false );
        m_ragdollController.EnableFollowTarget ( true );
    }
}
