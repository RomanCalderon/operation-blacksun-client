using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitTarget : MonoBehaviour
{
    [SerializeField]
    private Transform m_target = null;
    [SerializeField]
    private float m_rotationSpeedMultiplier = 1.0f;
    [SerializeField]
    private float m_smoothFactor = 10f;
    [SerializeField]
    private Vector3 m_cameraOffset = new Vector3 ( 0f, 2f, -4f );


    // Start is called before the first frame update
    void Start ()
    {
        //m_cameraOffset = transform.position - m_target.position;
    }

    // LateUpdate is called after UpdateMethods
    void LateUpdate ()
    {
        if ( m_target == null )
        {
            return;
        }

        Quaternion turnAngle = Quaternion.AngleAxis ( Input.GetAxis ( "Mouse X" ) * m_rotationSpeedMultiplier, Vector3.up );
        m_cameraOffset = turnAngle * m_cameraOffset;
        Vector3 newPosition = m_target.position + m_cameraOffset;
        transform.position = Vector3.Slerp ( transform.position, newPosition, Time.deltaTime * m_smoothFactor );
        transform.LookAt ( m_target );
    }

    public void SetTarget ( Transform target )
    {
        m_target = target;
    }
}
