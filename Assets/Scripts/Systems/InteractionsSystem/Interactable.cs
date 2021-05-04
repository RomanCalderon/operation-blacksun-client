using System;
using System.IO;
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    #region Models

    [Serializable]
    public struct InteractableData
    {
        public int InteractionType;
        public bool IsInteractable;
        public string InteractionLabel;
        public float InteractTime;
        public bool HasAccessKey;

        public InteractableData ( int interactionType, bool isInteractable, string interactionLabel, float interactTime, bool hasAccessKey )
        {
            InteractionType = interactionType;
            IsInteractable = isInteractable;
            InteractionLabel = interactionLabel;
            InteractTime = interactTime;
            HasAccessKey = hasAccessKey;
        }

        public byte [] ToArray ()
        {
            MemoryStream stream = new MemoryStream ();
            BinaryWriterExtended writer = new BinaryWriterExtended ( stream );

            writer.Write ( InteractionType );
            writer.Write ( IsInteractable );
            writer.Write ( InteractionLabel );
            writer.Write ( InteractTime );
            writer.Write ( HasAccessKey );

            return stream.ToArray ();
        }

        public static InteractableData FromArray ( byte [] bytes )
        {
            BinaryReaderExtended reader = new BinaryReaderExtended ( new MemoryStream ( bytes ) );
            InteractableData s = default;

            s.InteractionType = reader.ReadInt32 ();
            s.IsInteractable = reader.ReadBoolean ();
            s.InteractionLabel = reader.ReadString ();
            s.InteractTime = reader.ReadSingle ();
            s.HasAccessKey = reader.ReadBoolean ();

            return s;
        }
    }

    #endregion

    #region Delegates

    public delegate void InteractionEventHandler ();
    public static InteractionEventHandler OnStartedHover;
    public static InteractionEventHandler OnStartedInteraction;
    public static InteractionEventHandler OnInteracted;
    public static InteractionEventHandler OnStoppedInteraction;
    public static InteractionEventHandler OnStoppedHover;

    #endregion

    public int InteractionType { get => m_interactionType; }
    public bool IsInteractable { get => m_isInteractable; }
    public string InteractionLabel { get => m_interactionLabel; }
    public bool IsInteracting { get => m_isInteracting; }
    public float InteractTime { get => m_interactTime; }
    public bool HasAccessKey { get => m_hasAccessKey; }
    public int ClientId { get => m_clientId; }
    public float InteractTimer { get => m_interactTimer; }

    private int m_interactionType = 0;
    private bool m_isInteractable = false;
    private string m_interactionLabel;
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
        m_interactionType = interactableData.InteractionType;
        m_isInteractable = interactableData.IsInteractable;
        m_interactionLabel = interactableData.InteractionLabel;
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
