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
            m_angleOffest = Quaternion.Angle ( m_sun.rotation, m_moon.rotation );
        }

        m_isOverlaping = m_angleOffest < m_overlapAngleOffset;
    }
}
