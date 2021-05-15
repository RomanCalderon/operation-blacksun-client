using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerInput;

public class InteractionController : MonoBehaviour
{
    private const float CHECK_RADIUS = 2f;
    private const float CHECK_ANGLE = 15f;

    public bool CanInteract { get; set; } = true;

    // View
    [SerializeField]
    private InteractionView m_interactionView = null;

    [SerializeField]
    private Transform m_lookTransform = null;
    [SerializeField]
    private LayerMask m_interactableMask;

    private IInteractable m_target = null;
    private int m_clientId = 0;
    private string m_interactKeybind = null;

    private void OnEnable ()
    {
        // Subscribe to Interactable events
        Interactable.OnStartedHover += TargetStartHover;
        Interactable.OnStartedInteraction += TargetStartInteraction;
        Interactable.OnInteracted += TargetInteracted;
        Interactable.OnStoppedInteraction += TargetStopInteraction;
        Interactable.OnStoppedHover += TargetStopHover;
    }

    private void OnDisable ()
    {
        // Unsubscribe from Interactable events
        Interactable.OnStartedHover -= TargetStartHover;
        Interactable.OnStartedInteraction -= TargetStartInteraction;
        Interactable.OnInteracted -= TargetInteracted;
        Interactable.OnStoppedInteraction -= TargetStopInteraction;
        Interactable.OnStoppedHover -= TargetStopHover;
    }

    private void Awake ()
    {
        m_clientId = Client.instance.myId;
    }

    // Start is called before the first frame update
    private void Start ()
    {
        Debug.Assert ( m_interactionView != null, "m_interactionView is null." );
        Debug.Assert ( m_lookTransform != null, "m_lookTransform is null." );
    }

    private void Update ()
    {
        CheckClientInput ();
    }

    // Update is called once per frame
    private void FixedUpdate ()
    {
        if ( CanInteract = !InventoryManager.Instance.IsDisplayed )
        {
            LookForTarget ();
        }
        else
        {
            RemoveTarget ();
        }
    }

    private void CheckClientInput ()
    {
        if ( m_target == null )
        {
            return;
        }

        if ( PlayerInputController.GetKey ( PlayerInputController.InteractKey ) )
        {
            if ( !m_target.IsInteracting )
            {
                m_target.StartInteract ( m_clientId );
            }
        }
        else if ( m_target.IsInteracting )
        {
            m_target.StopInteract ();
        }
    }

    private void LookForTarget ()
    {
        // Check if the player can interact
        if ( !CanInteract )
        {
            if ( m_target != null )
            {
                m_target.StopInteract ();
                m_target = null;
            }
            return;
        }

        IInteractable target = GetTargetInteractable ();
        if ( IsNewTarget ( target ) )
        {
            RemoveTarget ();
            SetTarget ( target );
        }
    }

    private void SetTarget ( IInteractable newTarget )
    {
        // Assign new target
        m_target = newTarget;

        // Get interact keybind
        m_interactKeybind = PlayerInputController.InteractKey.ToString ();

        // Send target to view
        m_interactionView.SetTarget ( m_target, m_interactKeybind );

        // Invoke StartHover
        if ( m_target != null )
        {
            m_target.StartHover ();
        }
    }

    private void RemoveTarget ()
    {
        // Invoke StopHover
        if ( m_target != null )
        {
            m_target.StopHover ();
            m_target = null;
        }
    }

    #region Event Listeners

    private void TargetStartHover ()
    {
        m_interactionView.UpdateInteractionState ( InteractionView.InteractionStates.HOVER );
    }

    private void TargetStartInteraction ()
    {
        m_interactionView.UpdateInteractionState ( InteractionView.InteractionStates.INTERACTING );
    }

    private void TargetInteracted ()
    {
        m_interactionView.UpdateInteractionState ( InteractionView.InteractionStates.NONE );
    }

    private void TargetStopInteraction ()
    {
        m_interactionView.UpdateInteractionState ( InteractionView.InteractionStates.HOVER );
    }

    private void TargetStopHover ()
    {
        m_interactionView.UpdateInteractionState ( InteractionView.InteractionStates.NONE );
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

        // Set PlayerInput look direction
        PlayerInput.PlayerInputController.SetLookDirection ( lookDirection );

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
