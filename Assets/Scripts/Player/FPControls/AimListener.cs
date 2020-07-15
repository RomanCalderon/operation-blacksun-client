using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class AimListener : MonoBehaviour
{
    private bool m_aimState = false;

    [SerializeField]
    private Transform m_ADSPoint = null;
    [SerializeField]
    private Transform m_model = null;
    private Vector3 m_originalPosition;
    private Quaternion m_originalRotation;
    [SerializeField]
    private Vector3 m_offset = new Vector3 ( 0, 0, 0.075f );
    private Quaternion m_targetRotation;
    private Vector3 m_targetPosition;
    private Vector3 m_currVelocity;
    [SerializeField]
    private float m_smoothTime = 5f;

    private void Awake ()
    {
        m_originalPosition = m_model.localPosition;
        m_originalRotation = m_model.localRotation;
    }

    private void OnEnable ()
    {
        AimController.OnAimUpdated += AimUpdate;
    }

    private void OnDisable ()
    {
        AimController.OnAimUpdated -= AimUpdate;
    }

    // Start is called before the first frame update
    void Start ()
    {
        AimUpdate ( false );
    }

    // Update is called once per frame
    void Update ()
    {
        // Position
        m_model.localPosition = Vector3.SmoothDamp ( m_model.localPosition, m_targetPosition, ref m_currVelocity, m_smoothTime * Time.deltaTime );
    }

    private void AimUpdate ( bool aimState )
    {
        if ( aimState )
        {
            // Rotation
            float angle = Vector3.Angle ( m_ADSPoint.up, m_model.up );
            m_model.RotateAround ( m_ADSPoint.position, m_ADSPoint.forward, -angle );

            // Position
            Vector3 adsDiff = ( transform.localPosition + m_offset ) - m_ADSPoint.localPosition;
            m_targetPosition = m_model.localPosition + adsDiff;
        }
        else
        {
            m_targetPosition = m_originalPosition;
            m_model.localRotation = m_originalRotation;
        }
        m_aimState = aimState;
    }

    private void OnDrawGizmos ()
    {
        Gizmos.color = new Color ( 1, 0, 0, 0.5f );
        Gizmos.DrawSphere ( transform.position + m_offset, 0.005f );
    }
}
