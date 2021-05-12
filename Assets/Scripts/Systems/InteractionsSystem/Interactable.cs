using System;
using System.IO;
using UnityEngine;
using InteractionData;

public class Interactable : MonoBehaviour, IInteractable
{
    #region Delegates

    public delegate void InteractionEventHandler ();
    public static InteractionEventHandler OnStartedHover;
    public static InteractionEventHandler OnStartedInteraction;
    public static InteractionEventHandler OnInteracted;
    public static InteractionEventHandler OnStoppedInteraction;
    public static InteractionEventHandler OnStoppedHover;

    #endregion

    public string InstanceId { get => m_instanceId; }
    public int InteractionType { get => m_interactionType; }
    public bool IsInteractable { get => m_isInteractable; }
    public string InteractionContext { get => m_interactionContext; }
    public Color InteractionColor { get => m_interactionLabelColor; }
    public bool IsInteracting { get => m_isInteracting; }
    public float InteractTime { get => m_interactTime; }
    public bool HasAccessKey { get => m_hasAccessKey; }
    public int ClientId { get => m_clientId; }
    public float InteractTimer { get => m_interactTimer; }

    private string m_instanceId = null;
    private int m_interactionType = 0;
    private bool m_isInteractable = false;
    private string m_interactionContext;
    private Color m_interactionLabelColor;
    private bool m_isInteracting = false;
    private float m_interactTime = 0f;
    private bool m_hasAccessKey = false;
    private int m_clientId = 0;
    private bool m_hasInteracted = false;
    private float m_interactTimer = 0f;

    #region Interface

    /// <summary>
    /// Initializes Interactable with InteractableData <paramref name="data"/>.
    /// </summary>
    /// <param name="data">Interactable data containing interactability flag and access key.</param>
    public virtual void Initialize ( byte [] data )
    {
        InteractableData interactableData = InteractableData.FromArray ( data );
        m_instanceId = interactableData.InstanceId;
        m_interactionType = interactableData.InteractionType;
        m_isInteractable = interactableData.IsInteractable;
        m_interactionContext = interactableData.InteractionContext;
        m_interactionLabelColor = interactableData.InteractionLabelColor;
        m_interactTime = interactableData.InteractTime;
        m_hasAccessKey = interactableData.HasAccessKey;
    }

    /// <summary>
    /// Called when Interactable becomes accessible.
    /// </summary>
    public virtual void StartHover ()
    {
        OnStartedHover?.Invoke ();
    }

    /// <summary>
    /// Called when Interactable begins interaction.
    /// </summary>
    /// <param name="clientId">The ID of the client interacting with Interactable.</param>
    /// <param name="accessKey">An access key used to compare against Interactable's AccessKey.</param>
    public virtual void StartInteract ( int clientId )
    {
        if ( !IsInteractable )
        {
            return;
        }
        if ( m_isInteracting )
        {
            StopInteract ();
            return;
        }
        if ( !HasAccessKey )
        {
            StopInteract ();
            return;
        }
        m_clientId = clientId;
        m_isInteracting = true;
        m_interactTimer = m_interactTime;
        OnStartedInteraction?.Invoke ();
    }

    /// <summary>
    /// Called when Interactable interaction timer is complete.
    /// </summary>
    protected virtual void OnInteract ()
    {
        OnInteracted?.Invoke ();
    }

    /// <summary>
    /// Called when Interactable interaction has ended or got interrupted.
    /// </summary>
    public virtual void StopInteract ()
    {
        m_isInteracting = m_hasInteracted = false;
        m_interactTimer = 0f;
        OnStoppedInteraction?.Invoke ();
    }

    /// <summary>
    /// Called when Interactable becomes inaccessible.
    /// </summary>
    public virtual void StopHover ()
    {
        m_isInteracting = m_hasInteracted = false;
        m_interactTimer = 0f;
        OnStoppedHover?.Invoke ();
    }

    #endregion

    #region Runtime

    private void Update ()
    {
        CheckInteractTime ();
    }

    private void CheckInteractTime ()
    {
        if ( m_isInteracting )
        {
            if ( m_hasInteracted )
            {
                return;
            }
            // Increment interact timer
            if ( m_interactTimer > 0f )
            {
                m_interactTimer -= Time.deltaTime;
                return;
            }
            // Interaction timer is complete
            OnInteract ();
            m_hasInteracted = true;
        }
    }

    private void OnDestroy ()
    {
        StopInteract ();
        StopHover ();
    }

    #endregion
}
