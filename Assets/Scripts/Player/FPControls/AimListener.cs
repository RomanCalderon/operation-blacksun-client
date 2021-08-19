using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AimListener : MonoBehaviour
{
    #region Constants

    private const float ADS_POSITION_SPEED = 18f;
    private const float ADS_ROTATION_SPEED = 80;
    private const float AIM_OUT_SPEED_RATIO = 0.75f;

    #endregion

    #region Members

    [SerializeField]
    private Transform m_ADSPoint = null;
    [SerializeField]
    private string m_ADSTargetTag = "ADSTarget";
    private Transform m_currentADSTarget = null;
    [SerializeField]
    private TransformAlignmentUtil m_adsTransformAlignment = null;
    [SerializeField]
    private FollowTransform m_adsFollowTransform = null;
    private bool m_isAiming = false;

    [SerializeField]
    private Transform m_model = null;
    [SerializeField]
    private Transform m_modelContainer = null;
    private Vector3 m_originalPosition = Vector3.zero;
    private Vector3 m_offset = Vector3.zero;
    private Vector3 m_targetPosition = Vector3.zero;
    private Quaternion m_originalRotation = Quaternion.identity;
    private Quaternion m_targetRotation = Quaternion.identity;
    private Coroutine m_weaponAimCoroutine = null;

    public UnityEvent<bool> onAimStateChanged;
    public UnityEvent onAimTrue;
    public UnityEvent onAimFalse;

    #endregion

    #region Initialization

    private void Awake ()
    {
        m_originalPosition = m_model.localPosition;
        m_originalRotation = m_model.localRotation;

        UpdateADSPointTarget ();
    }

    private void OnEnable ()
    {
        AimController.OnAimStateUpdated += AimUpdate;
        UpdateADSPointTarget ();
    }

    private void OnDisable ()
    {
        AimController.OnAimStateUpdated -= AimUpdate;
    }

    #endregion

    // Update is called once per frame
    private void Update ()
    {
        float deltaTime = Time.deltaTime;

        // Position
        if ( m_isAiming )
        {
            m_modelContainer.localPosition = Vector3.Lerp ( m_modelContainer.localPosition, m_targetPosition, deltaTime * ADS_POSITION_SPEED );
            m_modelContainer.localRotation = Quaternion.RotateTowards ( m_modelContainer.localRotation, m_targetRotation, deltaTime * ADS_ROTATION_SPEED );
        }
        else
        {
            m_model.localPosition = Vector3.Lerp ( m_model.localPosition, m_originalPosition, deltaTime * ADS_POSITION_SPEED * AIM_OUT_SPEED_RATIO );
            m_model.localRotation = Quaternion.RotateTowards ( m_model.localRotation, m_originalRotation, deltaTime * ADS_ROTATION_SPEED * AIM_OUT_SPEED_RATIO );
        }
    }

    public Vector3 GetAimVector ()
    {
        return transform.forward; //m_ADSPoint.forward;
    }

    public void UpdateADSPointTarget ()
    {
        GameObject target = GameObject.FindWithTag ( m_ADSTargetTag );

        if ( target == null )
        {
            return;
        }

        if ( m_currentADSTarget == target )
        {
            return;
        }
        m_currentADSTarget = target.transform;

        if ( target != null )
        {
            m_adsFollowTransform.SetTarget ( target.transform );
            m_adsTransformAlignment.SetTarget ( target.transform );
            RealignADSPoint ();
        }
        else
        {
            Debug.LogWarning ( $"Couldn't find an ADSTarget with tag [{m_ADSTargetTag}]" );
        }
    }

    /// <summary>
    /// Realigns the ADS-Point to its currently-assigned ADS-Target.
    /// </summary>
    public void RealignADSPoint ()
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
        onAimStateChanged?.Invoke ( aimState );

        if ( aimState )
        {
            onAimTrue?.Invoke ();
            StartAim ();
        }
        else
        {
            onAimFalse?.Invoke ();
            StopAim ();
        }
    }

    private void StartAim ()
    {
        if ( m_weaponAimCoroutine != null )
            StopCoroutine ( m_weaponAimCoroutine );
        m_weaponAimCoroutine = StartCoroutine ( AimWeapon () );
    }

    private IEnumerator AimWeapon ()
    {
        yield return null;

        m_modelContainer.SetPositionAndRotation ( m_ADSPoint.position, m_ADSPoint.rotation );
        m_model.SetParent ( m_modelContainer );

        SetAimRotation ();
        SetAimPosition ();
        m_isAiming = true;
    }

    private void SetAimPosition ()
    {
        // Set target position
        Vector3 adsDiff = transform.InverseTransformDirection ( transform.position - m_modelContainer.position );
        m_offset = Vector3.zero;
        m_offset.z = Vector3.Distance ( m_ADSPoint.position, transform.position );
        m_targetPosition = m_modelContainer.localPosition + adsDiff + m_offset;
    }

    private void SetAimRotation ()
    {
        Quaternion difference = m_modelContainer.localRotation * Quaternion.Inverse ( transform.localRotation );
        m_targetRotation = difference * Quaternion.Inverse ( m_modelContainer.localRotation );
    }

    private void StopAim ()
    {
        m_model.SetParent ( transform );
        m_isAiming = false;
    }

    private void OnDrawGizmos ()
    {
        Gizmos.color = new Color ( 1, 0, 0, 0.75f );
        Gizmos.DrawSphere ( m_ADSPoint.position, 0.001f );
        Gizmos.color = Color.blue;
        Gizmos.DrawRay ( m_modelContainer.position, m_modelContainer.forward );
        Gizmos.color = Color.green;
        Gizmos.DrawRay ( transform.position, transform.forward );
    }
}
