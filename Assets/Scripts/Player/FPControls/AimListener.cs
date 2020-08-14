using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class AimListener : MonoBehaviour
{
    [SerializeField]
    private Transform m_ADSPoint = null;
    [SerializeField]
    private string m_ADSTargetTag = "ADSTarget";
    private Transform m_currentADSTarget = null;
    private TransformAlignmentUtil m_adsTransformAlignment = null;
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
        Debug.Assert ( m_ADSPoint != null, "Please assign an ADS Point." );
        m_adsFollowTransform = m_ADSPoint.GetComponent<FollowTransform> ();
        m_adsTransformAlignment = m_ADSPoint.GetComponent<TransformAlignmentUtil> ();

        m_originalPosition = m_model.localPosition;
        m_originalRotation = m_model.localRotation;
        m_offset.z = Vector3.Distance ( m_ADSPoint.position, transform.position );
    }

    private void OnEnable ()
    {
        AimController.OnAimStateUpdated += AimUpdate;

        SetADSPointTarget ();
    }

    private void OnDisable ()
    {
        AimController.OnAimStateUpdated -= AimUpdate;
    }

    // Update is called once per frame
    private void Update ()
    {
        // Position
        m_model.localPosition = Vector3.SmoothDamp ( m_model.localPosition, m_targetPosition, ref m_currVelocity, m_smoothTime * Time.deltaTime );
    }

    private void FixedUpdate ()
    {
        if ( AimController.AimState )
        {
            Vector3 adsDiff = ( transform.localPosition + m_offset ) - m_ADSPoint.localPosition;
            m_targetPosition = m_model.localPosition + adsDiff;
        }
    }

    public Vector3 GetAimVector ()
    {
        return m_ADSPoint.forward;
    }

    // TODO: Call this method when the active sight attachment changes
    private void SetADSPointTarget ()
    {
        Transform target = GameObject.FindWithTag ( m_ADSTargetTag ).transform;

        if ( m_currentADSTarget == target )
        {
            return;
        }
        m_currentADSTarget = target;

        if ( target != null )
        {
            m_adsFollowTransform.SetTarget ( target );
            m_adsTransformAlignment.SetTarget ( target );
            RealignADSPoint ();
            AimUpdate ( AimController.AimState );
        }
        else
        {
            Debug.LogWarning ( $"Couldn't find an ADSTarget with tag [{m_ADSTargetTag}]" );
        }
    }

    /// <summary>
    /// Realigns the ADS-Point to its currently-assigned ADS-Target.
    /// </summary>
    private void RealignADSPoint ()
    {
        if ( m_ADSPoint == null )
        {
            Debug.LogWarning ( "m_ADSPoint is null." );
            return;
        }
        m_adsFollowTransform.enabled = false;
        m_adsTransformAlignment.AlignPosition ();
        m_adsTransformAlignment.AlignRotation ();
        m_adsFollowTransform.enabled = true;
    }

    private void AimUpdate ( bool aimState )
    {
        if ( aimState )
        {
            RealignADSPoint ();

            // Rotation
            float angle = Vector3.Angle ( m_ADSPoint.up, m_model.transform.up );
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
    }

    private void OnDrawGizmos ()
    {
        Gizmos.color = new Color ( 1, 0, 0, 0.75f );
        Gizmos.DrawSphere ( transform.position + m_offset, 0.001f );
    }
}
