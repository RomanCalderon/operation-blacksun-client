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
        if ( Cursor.lockState == CursorLockMode.Locked )
        {
            Look ();
        }

        Debug.DrawRay ( transform.position, transform.forward * 2, Color.red );
    }

    private void Look ()
    {
        float mouseVertical = -Input.GetAxis ( "Mouse Y" );
        float mouseHorizontal = Input.GetAxis ( "Mouse X" );

        m_verticalRotation += mouseVertical * m_sensitivity * Time.deltaTime;
        m_horizontalRotation += mouseHorizontal * m_sensitivity * Time.deltaTime;

        m_verticalRotation = Mathf.Clamp ( m_verticalRotation, -m_clampAngle, m_clampAngle );

        transform.localRotation = Quaternion.Euler ( m_verticalRotation, 0f, 0f );
        player.transform.rotation = Quaternion.Euler ( 0f, m_horizontalRotation, 0f );
    }

    public void AddRecoil ( float verticalRecoil, float horizontalRecoil )
    {
        m_verticalRotation -= verticalRecoil;
        m_horizontalRotation -= horizontalRecoil;
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
