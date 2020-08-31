using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    private const float SLIDE_SHAKE_THRESHOLD = 5.0f;
    private const float SLIDE_SHAKE_ROUGHNESS = 12.0f;
    private const float SLIDE_SHAKE_FADEIN = 0.2f;

    private bool m_canControl = false;

    [SerializeField]
    private Player player;

    [SerializeField]
    private float m_clampAngle = 85f;
    private Vector3 m_crouchTarget = new Vector3 ( 0, -0.75f, 0 );
    private Vector3 m_proneTarget = new Vector3 ( 0, -1.5f, 0 );
    [SerializeField]
    private float m_crouchSpeed = 3f;
    private Vector3 m_headPositionTarget;
    private float m_crouchHeadTiltAmount = 0.5f;
    private float m_headTilt = 0f;
    private float m_headTiltTarget = 0f;
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
        PlayerController.Initialize ();
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
            Look ( deltaTime );
        }

        // Update head crouch/prone positioning
        bool crouchInput = PlayerController.CrouchInput;
        bool proneInput = PlayerController.ProneInput;
        SetCrouchProne ( crouchInput, proneInput );
        transform.localPosition = Vector3.MoveTowards ( transform.localPosition, m_headPositionTarget + m_headOffset, deltaTime * m_crouchSpeed );
    }

    private void Look ( float deltaTime )
    {
        float mouseVertical = -Input.GetAxis ( "Mouse Y" );
        float mouseHorizontal = Input.GetAxis ( "Mouse X" );

        m_verticalRotation += mouseVertical * m_sensitivity * deltaTime;
        m_horizontalRotation += mouseHorizontal * m_sensitivity * deltaTime;

        m_verticalRotation = Mathf.Clamp ( m_verticalRotation, -m_clampAngle, m_clampAngle );

        m_headTilt = Mathf.Lerp ( m_headTilt, m_headTiltTarget, deltaTime * 16f );
        transform.localRotation = Quaternion.Euler ( m_verticalRotation - m_headTilt, 0f, 0f );
        player.transform.rotation = Quaternion.Euler ( 0f, m_horizontalRotation, 0f );
    }

    public void ApplySlideShake ( bool crouchInput )
    {
        float playerSpeed = player.MovementVelocity.magnitude;
        float shakeMagnitude = playerSpeed / 75f;

        if ( !m_isShaking && playerSpeed >= SLIDE_SHAKE_THRESHOLD && crouchInput )
        {
            m_isShaking = true;
            m_shakeInstance = CameraShaker.Instance.StartShake ( shakeMagnitude, SLIDE_SHAKE_ROUGHNESS, SLIDE_SHAKE_FADEIN );
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
        m_verticalRotation -= verticalRecoil;
        m_horizontalRotation -= horizontalRecoil;
    }

    private void SetCrouchProne ( bool crouch, bool prone )
    {
        if ( !m_canControl )
        {
            return;
        }

        if ( crouch ) // Crouch
        {
            m_headPositionTarget = m_crouchTarget;
            m_headTiltTarget = m_crouchHeadTiltAmount;
        }
        else if ( prone ) // Prone
        {
            m_headPositionTarget = m_proneTarget;
            m_headTiltTarget = m_crouchHeadTiltAmount;
        }
        else // Standing
        {
            m_headPositionTarget = Vector3.zero;
            m_headTiltTarget = 0f;
        }
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
