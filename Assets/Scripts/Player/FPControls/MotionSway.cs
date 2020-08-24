using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionSway : MonoBehaviour
{
    private const float SWAY_REDUCTION_FACTOR = 16f;

    [Header ( "Values" )]
    public float SwayAmountX = 0.007f;
    public float SwayAmountY = 0.02f;
    public float MaxSwayAmount = 0.02f;
    public float SwaySmooth = 8f;
    [Space]
    public float SmoothRotation = 3f;
    public float TiltAngle = 1.5f;

    private Vector3 def;
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
        def = transform.localPosition;
    }

    private void Update ()
    {
        if ( !InventoryManager.Instance.IsDisplayed )
        {
            UpdateSway ( Input.GetAxis ( "Mouse X" ), Input.GetAxis ( "Mouse Y" ) );
        }
        else
        {
            UpdateSway ( 0f, 0f );
        }
    }

    public void UpdateSway ( float x, float y )
    {
        float factorX = -x;
        float factorY = -y;
        float tiltAroundZ = -x * TiltAngle;
        float tiltAroundX = y * TiltAngle * 2f;

        factorX *= SwayAmountX;
        factorY *= SwayAmountY;

        if ( m_isAiming )
        {
            factorX = Mathf.Clamp ( factorX / SWAY_REDUCTION_FACTOR, -MaxSwayAmount / SWAY_REDUCTION_FACTOR, MaxSwayAmount / SWAY_REDUCTION_FACTOR );
            factorY = Mathf.Clamp ( factorY / SWAY_REDUCTION_FACTOR, -MaxSwayAmount / SWAY_REDUCTION_FACTOR, MaxSwayAmount / SWAY_REDUCTION_FACTOR );
            tiltAroundZ /= SWAY_REDUCTION_FACTOR;
            tiltAroundX /= SWAY_REDUCTION_FACTOR;
        }
        else
        {
            factorX = Mathf.Clamp ( factorX, -MaxSwayAmount, MaxSwayAmount );
            factorY = Mathf.Clamp ( factorY, -MaxSwayAmount, MaxSwayAmount );
        }

        // Position
        Vector3 final = new Vector3 ( def.x + factorX, def.y + factorY, def.z );
        transform.localPosition = Vector3.Lerp ( transform.localPosition, final, Time.deltaTime * SwaySmooth );

        // Rotation
        Quaternion target = Quaternion.Euler ( tiltAroundX, tiltAroundZ, 0 );
        transform.localRotation = Quaternion.Slerp ( transform.localRotation, target, Time.deltaTime * SmoothRotation );
    }

    private void AimUpdated ( bool state )
    {
        m_isAiming = state;
    }
}
