using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollJointController : MonoBehaviour
{
    [SerializeField]
    private Transform m_targetJoint = null;
    [SerializeField]
    private bool m_isKinematicOnFollow = true;
    [SerializeField]
    private bool m_lateUpdate = true;

    private bool m_followTarget = true;
    private bool m_jointContainsRigidbody = false;
    private Rigidbody m_rigidbody = null;

    // Start is called before the first frame update
    void Start ()
    {
        m_jointContainsRigidbody = ( m_rigidbody = GetComponent<Rigidbody> () ) != null;

        SetFollowTarget ( m_followTarget );
    }

    // Update is called once per frame
    void Update ()
    {
        if ( !m_lateUpdate && m_followTarget && m_targetJoint != null )
        {
            transform.SetPositionAndRotation ( m_targetJoint.position, m_targetJoint.rotation );
            transform.localScale = m_targetJoint.localScale;
        }
    }

    private void LateUpdate ()
    {
        if ( m_lateUpdate && m_followTarget && m_targetJoint != null )
        {
            transform.SetPositionAndRotation ( m_targetJoint.position, m_targetJoint.rotation );
            transform.localScale = m_targetJoint.localScale;
        }
    }

    public void SetFollowTarget ( bool followTarget )
    {
        m_followTarget = followTarget;

        if ( m_jointContainsRigidbody && m_isKinematicOnFollow )
        {
            m_rigidbody.isKinematic = m_followTarget;
        }
    }
}
