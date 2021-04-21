using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionSway : MonoBehaviour
{
    private const float SWAY_STRENGTH = 0.2f;

    [SerializeField]
    private PlayerMovementController m_playerMovementController = null;

    [Header ( "Values" )]
    public float SwayAmountX = 0.007f;
    public float SwayAmountY = 0.02f;
    public float MaxSwayAmount = 0.02f;
    public float SwaySmooth = 16f;
    [Space]
    public float SmoothRotation = 8;
    public float TiltAngle = 1.5f;

    private Vector3 m_default;
    [SerializeField]
    private bool m_showGizmos = false;
    [SerializeField]
    private Vector3 m_runPositionOffset;
    [SerializeField]
    private Vector3 m_runRotationOffset = Vector3.forward;

    private bool m_isAiming = false;


    private void OnEnable ()
    {
        AimController.OnAimStateUpdated += AimUpdated;
    }

    private void OnDisable ()
    {
        AimController.OnAimStateUpdated -= AimUpdated;
    }

    private void Start ()
    {
        m_default = transform.localPosition;
    }

    private void Update ()
    {
        float deltaTime = Time.deltaTime;
        if ( !InventoryManager.Instance.IsDisplayed && !m_isAiming )
        {
            UpdateSway ( Input.GetAxis ( "Mouse X" ), Input.GetAxis ( "Mouse Y" ), deltaTime );
        }
        else
        {
            UpdateSway ( 0f, 0f, deltaTime * 3f );
        }
    }

    public void UpdateSway ( float x, float y, float deltaTime )
    {
        float factorX = -x;
        float factorY = -y;
        float tiltAroundX = y * TiltAngle * 2f;
        float tiltAroundY = -x * TiltAngle;
        float playerSpeed = m_playerMovementController.Velocity.magnitude;
        float tiltAroundZ = -x * TiltAngle * playerSpeed / 4f;

        factorX *= SwayAmountX;
        factorY *= SwayAmountY;

        if ( m_isAiming )
        {
            factorX = Mathf.Clamp ( factorX * SWAY_STRENGTH, -MaxSwayAmount * SWAY_STRENGTH, MaxSwayAmount * SWAY_STRENGTH );
            factorY = Mathf.Clamp ( factorY * SWAY_STRENGTH, -MaxSwayAmount * SWAY_STRENGTH, MaxSwayAmount * SWAY_STRENGTH );
            tiltAroundX = tiltAroundX * SWAY_STRENGTH;
            tiltAroundY = tiltAroundY * SWAY_STRENGTH;
            tiltAroundZ = tiltAroundZ * SWAY_STRENGTH;
        }
        else
        {
            factorX = Mathf.Clamp ( factorX, -MaxSwayAmount, MaxSwayAmount );
            factorY = Mathf.Clamp ( factorY, -MaxSwayAmount, MaxSwayAmount );
        }

        // Position
        bool useRunOffset = m_playerMovementController.IsRunning &&
            m_playerMovementController.IsGrounded &&
            !m_playerMovementController.IsSliding &&
            !m_isAiming;
        Vector3 final = new Vector3 ( m_default.x + factorX, m_default.y + factorY, m_default.z ) + ( useRunOffset ? m_runPositionOffset : Vector3.zero );
        transform.localPosition = Vector3.Lerp ( transform.localPosition, final, deltaTime * SwaySmooth );

        // Rotation
        Quaternion target = Quaternion.Euler ( tiltAroundX, tiltAroundY, tiltAroundZ );
        if ( useRunOffset )
        {
            target *= Quaternion.LookRotation ( m_runRotationOffset, Vector3.up );
        }
        transform.localRotation = Quaternion.Slerp ( transform.localRotation, target, deltaTime * SmoothRotation );
    }

    private void AimUpdated ( bool state )
    {
        m_isAiming = state;
    }

    private void OnDrawGizmos ()
    {
        if ( !m_showGizmos )
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawRay ( transform.position, m_runPositionOffset );
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay ( transform.position + m_runPositionOffset, m_runRotationOffset.normalized );
    }
}
