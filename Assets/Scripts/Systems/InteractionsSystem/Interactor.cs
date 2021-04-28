using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerInput;

public class Interactor : MonoBehaviour
{
    private const float CHECK_RADIUS = 2f;
    private const float CHECK_ANGLE = 15f;

    [SerializeField]
    private Transform m_lookTransform = null;
    [SerializeField]
    private LayerMask m_interactableMask;

    private IInteractable m_target = null;
    private int m_clientId = 0;

    private void Awake ()
    {
        m_clientId = Client.instance.myId;
    }

    // Start is called before the first frame update
    private void Start ()
    {
        Debug.Assert ( m_lookTransform != null, "m_lookTransform is null." );
    }

    private void Update ()
    {
        CheckClientInput ();
    }

    // Update is called once per frame
    private void FixedUpdate ()
    {
        IInteractable target = GetTargetInteractable ();
        if ( IsNewTarget ( target ) )
        {
            if ( m_target != null )
            {
                m_target.StopHover ();
                m_target = null;
            }
            m_target = target;
            if ( m_target != null )
            {
                m_target.StartHover ();
            }
        }

        // DEBUG
        if ( m_target != null )
        {
            Vector3 lookDirection = m_lookTransform.forward;
            Vector3 headPosition = m_lookTransform.position;
            Debug.DrawRay ( headPosition, lookDirection * CHECK_RADIUS, Color.green, Time.fixedDeltaTime );
        }
    }

    #region Event Listeners

    private void CheckClientInput ()
    {
        if ( m_target == null )
        {
            return;
        }

        if ( PlayerInputController.GetKeyDown ( PlayerInputController.InteractKey ) )
        {
            m_target.StartInteract ( m_clientId, null as string );
        }
        else if ( PlayerInputController.GetKeyUp ( PlayerInputController.InteractKey ) )
        {
            m_target.StopInteract ();
        }
    }

    #endregion

    #region Util

    private bool IsNewTarget ( IInteractable target )
    {
        if ( m_target == null && target != null )
        {
            return true;
        }
        if ( m_target == target )
        {
            return false;
        }
        return true;
    }

    private IInteractable GetTargetInteractable ()
    {
        Collider [] allInteractables = Physics.OverlapSphere ( transform.position, CHECK_RADIUS, m_interactableMask );
        Vector3 lookDirection = m_lookTransform.forward;
        Vector3 headPosition = m_lookTransform.position;
        IInteractable target = null;
        float closest = CHECK_ANGLE;

        // Debug
        Debug.DrawRay ( headPosition, lookDirection * CHECK_RADIUS, Color.blue, Time.fixedDeltaTime );

        // Find closest Interactable
        foreach ( Collider collider in allInteractables )
        {
            float viewDistance = Vector3.Angle ( lookDirection, collider.transform.position - headPosition );
            if ( viewDistance < closest )
            {
                IInteractable interactable = collider.GetComponent<IInteractable> ();
                if ( interactable == null )
                {
                    Debug.LogError ( $"Collider ({collider.name}) with layer [Interactable] is missing an Interactable component." );
                    continue;
                }
                closest = viewDistance;
                target = interactable;
            }
        }
        return target;
    }

    #endregion
}
