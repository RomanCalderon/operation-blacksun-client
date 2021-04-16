using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerInput;
using EZCameraShake;

public class CameraController : MonoBehaviour
{
    private const float CROUCH_SPEED = 4f;
    private const float CROUCH_TARGET_HEIGHT = -0.5f;
    private const float SLIDE_SHAKE_THRESHOLD = 5.0f;
    private const float SLIDE_SHAKE_FADEIN = 0.2f;

    public static CameraController Instance;

    private bool m_canControl = false;

    [SerializeField]
    private Player player;
    [SerializeField]
    private PlayerMovementController m_movementController = null;

    [SerializeField]
    private float m_clampAngle = 85f;
    private Vector3 m_headPositionTarget;
    private float m_horizontalHeadTilt = 0f;
    private float m_verticalHeadTilt = 0f;
    private float m_horizontalHeadTiltTarget = 0f;
    private float m_verticalHeadTiltTarget = 0f;
    private Vector3 m_headOffset;

    private float m_sensitivity;
    private float m_normalSensitivity;
    private float m_aimSensitivity;
    private float m_verticalRotation;
    private float m_horizontalRotation;

    // Sliding params
    private bool m_isShaking = false;
    private CameraShakeInstance m_shakeInstance = null;


    private void OnEnable ()
    {
        AimController.OnAimStateUpdated += SetSensitivity;
    }

    private void OnDisable ()
    {
        AimController.OnAimStateUpdated -= SetSensitivity;
    }

    private void Start ()
    {
        if ( Instance == null )
        {
            Instance = this;
        }

        m_headOffset = transform.localPosition;

        Initialize ();
    }

    public void CanControl ( bool value )
    {
        m_canControl = value;

        // Reinitialize PlayerController inputs
        PlayerInputController.Initialize ();
    }

    private void Initialize ()
    {
        // Set Sens
        // TODO: Set sens from settings
        m_sensitivity = m_normalSensitivity = 125f;

        // Set Aim Sens
        // TODO: Set aim sens from settings
        m_aimSensitivity = 50f;

        // Set rotations
        m_verticalRotation = transform.localEulerAngles.x;
        m_horizontalRotation = player.transform.eulerAngles.y;

        // Lock cursor
        SetCursorMode ( CursorLockMode.Locked );

        CanControl ( true );
    }

    private void Update ()
    {
        float deltaTime = Time.deltaTime;

        if ( Cursor.lockState == CursorLockMode.Locked && m_canControl )
        {
            m_horizontalHeadTilt = Mathf.Lerp ( m_horizontalHeadTilt, m_horizontalHeadTiltTarget, deltaTime * 80f );
            m_verticalHeadTilt = Mathf.Lerp ( m_verticalHeadTilt, m_verticalHeadTiltTarget, deltaTime * 80f );
            m_horizontalHeadTiltTarget = Mathf.Max ( 0, m_horizontalHeadTiltTarget - deltaTime * 10 );
            m_verticalHeadTiltTarget = Mathf.Max ( 0, m_verticalHeadTiltTarget - deltaTime * 10 );
            Look ( deltaTime );
        }

        // Update crouch height
        UpdateMovementChanges ( PlayerInputController.CrouchInput, deltaTime );
    }

    private void Look ( float deltaTime )
    {
        float mouseHorizontalInput = Input.GetAxis ( Constants.MOUSE_HORIZONTAL_INPUT );
        float mouseVerticalInput = -Input.GetAxis ( Constants.MOUSE_VERTICAL_INPUT );

        m_horizontalRotation += ( mouseHorizontalInput - m_horizontalHeadTilt ) * m_sensitivity * deltaTime;
        m_verticalRotation += ( mouseVerticalInput - m_verticalHeadTilt ) * m_sensitivity * deltaTime;
        m_verticalRotation = Mathf.Clamp ( m_verticalRotation, -m_clampAngle, m_clampAngle );

        transform.localRotation = Quaternion.Euler ( m_verticalRotation, 0f, 0f );
        m_movementController.SetRotation ( Quaternion.Euler ( 0f, m_horizontalRotation, 0f ) );
    }

    private void ApplySlideShake ( bool crouchInput )
    {
        float playerSpeed = player.MovementVelocity.magnitude;
        float shakeMagnitude = Mathf.Sqrt ( playerSpeed / 60f );
        float shakeRoughness = playerSpeed / 3f;

        if ( !m_isShaking && playerSpeed >= SLIDE_SHAKE_THRESHOLD && crouchInput )
        {
            m_isShaking = true;
            m_shakeInstance = CameraShaker.Instance.StartShake ( shakeMagnitude, shakeRoughness, SLIDE_SHAKE_FADEIN );
        }
        if ( m_isShaking )
        {
            m_shakeInstance.UpdateShake ();

            if ( playerSpeed < SLIDE_SHAKE_THRESHOLD || !crouchInput )
            {
                m_isShaking = false;
                m_shakeInstance.StartFadeOut ( 0.1f );
            }
        }
    }

    public void AddRecoil ( float verticalRecoil, float horizontalRecoil )
    {
        m_verticalHeadTiltTarget += verticalRecoil;
        m_horizontalHeadTiltTarget += horizontalRecoil;
    }

    public void UpdateMovementChanges ( bool crouch, float deltaTime )
    {
        if ( !m_canControl )
        {
            return;
        }

        // Slide shake effect
        ApplySlideShake ( crouch );

        if ( crouch ) // Crouch
        {
            m_headPositionTarget = new Vector3 ( 0, CROUCH_TARGET_HEIGHT, 0 );
        }
        else // Standing
        {
            m_headPositionTarget = Vector3.zero;
        }

        // Update head positioning
        transform.localPosition = Vector3.MoveTowards ( transform.localPosition, m_headPositionTarget + m_headOffset, deltaTime * CROUCH_SPEED );
    }

    /// <summary>
    /// Called when the aim state changes.
    /// </summary>
    /// <param name="aimState">The new aim state.</param>
    private void SetSensitivity ( bool aimState )
    {
        m_sensitivity = aimState ? m_aimSensitivity : m_normalSensitivity;
    }

    public void SetCursorMode ( CursorLockMode lockMode )
    {
        Cursor.lockState = lockMode;
        Cursor.visible = lockMode == CursorLockMode.None;
    }
}
