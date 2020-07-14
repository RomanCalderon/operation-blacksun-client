using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformAlignmentUtil : MonoBehaviour
{
    [SerializeField]
    private Transform m_target = null;

    public void AlignPosition ()
    {
        if ( m_target == null )
        {
            Debug.LogWarning ( "Alignment failed. Please assign a Transform to 'Target'." );
            return;
        }
        transform.position = m_target.position;
    }

    public void AlignRotation ()
    {
        if ( m_target == null )
        {
            Debug.LogWarning ( "Alignment failed. Please assign a Transform to 'Target'." );
            return;
        }
        transform.rotation = m_target.rotation;
    }

    public void AlignScale ()
    {
        if ( m_target == null )
        {
            Debug.LogWarning ( "Alignment failed. Please assign a Transform to 'Target'." );
            return;
        }
        transform.localScale = m_target.lossyScale;
    }
}
