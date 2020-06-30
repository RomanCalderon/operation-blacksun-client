using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollJointController : MonoBehaviour
{
    [SerializeField]
    private Transform m_targetJoint = null;
    [SerializeField]
    private bool m_followTarget = true;
    [SerializeField]
    private bool m_isKinematicOnFollow = true;
    private bool m_jointContainsRigidbody = false;
    private Rigidbody m_rigidbody = null;

    // Start is called before the first frame update
    void Start ()
    {
        m_jointContainsRigidbody = ( m_rigidbody = GetComponent<Rigidbody> () ) != null;

        if ( m_targetJoint == null )
        {
            Debug.LogError ( "m_targetJoint has not been assigned a joint. Please ensure a joint is assigned to this component." );
        }
        else
        {
            EnableFollowTarget ( m_followTarget );
        }
    }

    // Update is called once per frame
    void Update ()
    {
        if ( m_followTarget )
        {
            transform.position = m_targetJoint.position;
            transform.rotation = m_targetJoint.rotation;
            transform.localScale = m_targetJoint.localScale;
        }
    }

    public void EnableFollowTarget ( bool followTarget )
    {
        m_followTarget = followTarget;

        if ( m_jointContainsRigidbody && m_isKinematicOnFollow )
        {
            m_rigidbody.isKinematic = m_followTarget;
        }
    }
}
