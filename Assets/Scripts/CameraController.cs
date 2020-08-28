using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [SerializeField]
    private Player player;

    [SerializeField]
    private float m_clampAngle = 85f;
    [SerializeField]
    private Vector3 m_crouchTarget = new Vector3 ( 0, -0.5f, 0 );
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

        InitializeSensitivity ();

        m_verticalRotation = transform.localEulerAngles.x;
        m_horizontalRotation = player.transform.eulerAngles.y;

        m_headOffset = transform.localPosition;

        SetCursorMode ( CursorLockMode.Locked );
    }

    private void InitializeSensitivity ()
    {
        // Set Sens
        // TODO: Set sens from settings
        m_sensitivity = m_normalSensitivity = 125f;

        // Set Aim Sens
        // TODO: Set aim sens from settings
        m_aimSensitivity = 50f;

    }

    private void Update ()
    {
        float deltaTime = Time.deltaTime;

        if ( Cursor.lockState == CursorLockMode.Locked )
        {
            Look ( deltaTime );
        }

        // Update head crouch
        SetCrouch ( Input.GetKey ( KeyCode.C ) ); // TODO: Switch to KeybindManager
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

    public void AddRecoil ( float verticalRecoil, float horizontalRecoil )
    {
        m_verticalRotation -= verticalRecoil;
        m_horizontalRotation -= horizontalRecoil;
    }

    private void SetCrouch ( bool value )
    {
        if ( value )
        {
            m_headPositionTarget = m_crouchTarget;
            m_headTiltTarget = m_crouchHeadTiltAmount;
        }
        else
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
