using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SetGlobalVectorPropertyUtil : MonoBehaviour
{
    #region Models

    private enum TransformTrackTypes
    {
        POSITION,
        ROTATION,
        SCALE
    }

    private enum Directions
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        FORWARDS,
        BACKWARD
    }

    #endregion

    #region Members

    [SerializeField]
    private string m_globalVectorPropertyName = null;
    [Header ( "Vector Details" )]
    [SerializeField]
    private bool m_isGlobal = false;
    [SerializeField]
    private TransformTrackTypes m_transformTrackType = TransformTrackTypes.POSITION;
    [SerializeField]
    private Directions m_direction = Directions.UP;
    [Space]
    [SerializeField]
    private bool m_stayUpdated = true;

    #endregion

    #region Methods

    // Start is called before the first frame update
    void Start ()
    {
        if ( !string.IsNullOrEmpty ( m_globalVectorPropertyName ) )
        {
            Vector3 v = GetGlobalVector ();
            Shader.SetGlobalVector ( m_globalVectorPropertyName, v );
        }
    }

    // Update is called once per frame
    void Update ()
    {
        if ( m_stayUpdated )
        {
            if ( !string.IsNullOrEmpty ( m_globalVectorPropertyName ) )
            {
                Vector3 v = GetGlobalVector ();
                Shader.SetGlobalVector ( m_globalVectorPropertyName, v );
            }
        }
    }

    private Vector3 GetGlobalVector ()
    {
        switch ( m_transformTrackType )
        {
            case TransformTrackTypes.POSITION:
                return ( m_isGlobal ) ? transform.position : transform.localPosition;
            case TransformTrackTypes.ROTATION:
                switch ( m_direction )
                {
                    case Directions.UP:
                        return ( m_isGlobal ) ? Vector3.up : transform.up;
                    case Directions.DOWN:
                        return ( m_isGlobal ) ? Vector3.down : -transform.up;
                    case Directions.LEFT:
                        return ( m_isGlobal ) ? Vector3.left : -transform.right;
                    case Directions.RIGHT:
                        return ( m_isGlobal ) ? Vector3.right : transform.right;
                    case Directions.FORWARDS:
                        return ( m_isGlobal ) ? Vector3.forward : transform.forward;
                    case Directions.BACKWARD:
                        return ( m_isGlobal ) ? Vector3.back : -transform.forward;
                    default:
                        break;
                }
                break;
            case TransformTrackTypes.SCALE:
                return ( m_isGlobal ) ? transform.lossyScale : transform.localScale;
            default:
                break;
        }

        return Vector3.zero;
    }

    #endregion
}
