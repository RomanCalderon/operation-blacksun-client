using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class AimListener : MonoBehaviour
{
    private bool m_aimState = false;

    [SerializeField]
    private Transform m_ADSPoint = null;
    private TransformAlignmentUtil m_asdTransformAlignment = null;
    private FollowTransform m_adsFollowTransform = null;
    [SerializeField]
    private Transform m_model = null;
    private Vector3 m_originalPosition = Vector3.zero;
    private Quaternion m_originalRotation = Quaternion.identity;
    private Vector3 m_offset = Vector3.zero;
    private Vector3 m_targetPosition = Vector3.zero;
    private Vector3 m_currVelocity = Vector3.zero;
    [SerializeField]
    private float m_smoothTime = 5f;

    private void Awake ()
    {
        m_adsFollowTransform = m_ADSPoint.GetComponent<FollowTransform> ();
        m_asdTransformAlignment = m_ADSPoint.GetComponent<TransformAlignmentUtil> ();

        m_originalPosition = m_model.localPosition;
        m_originalRotation = m_model.localRotation;
        m_offset.z = Vector3.Distance ( m_ADSPoint.position, transform.position );
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
        m_adsFollowTransform.enabled = false;
        //m_adsFollowTransform.UpdatePositionalOffset ();
        m_asdTransformAlignment.AlignPosition ();
        m_adsFollowTransform.enabled = true;
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
            //m_adsFollowTransform.UpdatePositionalOffset ();
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
        Gizmos.color = new Color ( 1, 0, 0, 0.75f );
        Gizmos.DrawSphere ( transform.position + m_offset, 0.001f );
    }
}
