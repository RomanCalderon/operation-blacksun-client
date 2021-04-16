using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    [SerializeField]
    private Transform m_target = null;
    [Space]
    [SerializeField]
    private bool m_followPosition = true;
    [SerializeField]
    private bool m_followRotation = true;
    [SerializeField]
    private bool m_keepOffset = true;
    private Vector3 m_positionalOffset;
    private Vector3 m_rotationalOffset;

    private void OnEnable ()
    {
        if ( m_target == null )
        {
            return;
        }

        if ( m_keepOffset )
        {
            m_positionalOffset = transform.position - m_target.position;
            m_rotationalOffset = transform.eulerAngles - m_target.eulerAngles;
        }
        else
        {
            m_positionalOffset = Vector3.zero;
        }
    }

    void LateUpdate ()
    {
        if ( m_target == null )
        {
            return;
        }
        transform.position = m_target.position + m_positionalOffset;
        transform.rotation = Quaternion.Euler ( m_target.eulerAngles + m_rotationalOffset );
        //if ( m_followPosition )
        //{
        //    transform.position = m_target.position + m_positionalOffset;
        //}
        //if ( m_followRotation )
        //{
        //    transform.rotation = Quaternion.Euler ( m_target.eulerAngles + m_rotationalOffset );
        //}
    }

    public void SetTarget ( Transform target )
    {
        m_target = target;
    }

    public void UpdatePositionalOffset ()
    {
        m_positionalOffset = transform.position - m_target.position;
    }
}
