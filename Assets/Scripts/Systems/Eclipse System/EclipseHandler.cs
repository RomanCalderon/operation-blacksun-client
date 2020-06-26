using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class EclipseHandler : MonoBehaviour
{
    [SerializeField]
    private Transform m_sun = null;
    [SerializeField]
    private Transform m_moon = null;
    private Quaternion m_moonTargetRotation;
    [SerializeField]
    private float m_moonRotationDelta = 10f;
    public float AngleOffset
    {
        get { return m_angleOffest; }
    }
    [SerializeField]
    private float m_angleOffest = 0.0f;
    [SerializeField]
    private float m_overlapAngleOffset = 8.0f;
    [SerializeField]
    private bool m_isOverlaping = false;

    // Start is called before the first frame update
    void Start ()
    {
        // Check if the sun and moon transforms are assigned
        Debug.Assert ( m_sun != null );
        Debug.Assert ( m_moon != null );
    }

    // Update is called once per frame
    void Update ()
    {
        if ( m_sun != null && m_moon != null )
        {
            // Rotate the moon transform towards its target rotation
            m_moon.rotation = Quaternion.RotateTowards ( m_moon.rotation, m_moonTargetRotation, m_moonRotationDelta );

            // Get the new angle offset to check for eclipse
            m_angleOffest = Quaternion.Angle ( m_sun.rotation, m_moon.rotation );
            m_angleOffest -= m_moon.eulerAngles.z;
            m_angleOffest = Mathf.Abs ( m_angleOffest );
            m_isOverlaping = Mathf.Abs ( m_angleOffest ) < m_overlapAngleOffset;
        }
    }

    /// <summary>
    /// Sets the moon's target rotation.
    /// </summary>
    /// <param name="targetRotation">Target rotation value as Quaternion.</param>
    public void SetMoonRotation ( Quaternion targetRotation )
    {
        m_moonTargetRotation = targetRotation;
    }
}
