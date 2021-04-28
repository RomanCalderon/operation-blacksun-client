using UnityEngine;
using InventorySystem.PlayerItems;

[RequireComponent ( typeof ( BoxCollider ) )]
public class PickupInstance : Interactable
{
    public PlayerItem PlayerItem { get => m_playerItem; }
    public int Quantity { get => m_quantity; }

    private PlayerItem m_playerItem = null;
    private int m_quantity = 0;
    [SerializeField]
    private Transform m_container = null;
    private BoxCollider m_boxCollider = null;

    private void Awake ()
    {
        m_boxCollider = GetComponent<BoxCollider> ();
    }

    public void Initialize ( byte [] interactableData, string itemId, int quantity )
    {
        if ( string.IsNullOrEmpty ( itemId ) )
        {
            Debug.LogWarning ( "itemId is null or empty." );
            return;
        }

        base.Initialize ( interactableData );

        // Get PlayerItem from database
        m_playerItem = GameAssets.Instance.GetPlayerItem ( itemId );
        m_quantity = quantity;
        GameObject itemObject = GameAssets.Instance.GetPlayerItemObject ( itemId );
        Instantiate ( itemObject, m_container.position, m_container.rotation, m_container );

        SetColliderBounds ( gameObject );

        //m_detailsPopup.UpdateDetails ( playerItem );
    }

    #region Interactable

    public override void StartHover ()
    {
        // Display Interactable access UI
        Debug.Log ( $"Interactable [{m_playerItem}] - StartHover" );
    }

    public override void StartInteract ( int clientId, string accessKey = null )
    {
        base.StartInteract ( clientId, accessKey );
        if ( !IsInteracting )
        {
            return;
        }

        // Display Interactable interaction UI
        Debug.Log ( $"Interactable [{m_playerItem}] - StartInteract" );
    }

    public override void StartInteract ( int clientId, string [] accessKeys )
    {
        base.StartInteract ( clientId, accessKeys );
        if ( !IsInteracting )
        {
            return;
        }

        // Display Interactable interaction UI
        Debug.Log ( $"Interactable [{m_playerItem}] - StartInteract" );
    }

    protected override void OnInteract ()
    {
        Debug.Log ( $"Interactable [{m_playerItem}] - OnInteract" );
    }

    public override void StopInteract ()
    {
        base.StopInteract ();

        // Hide Interactable interaction UI
        Debug.Log ( $"Interactable [{m_playerItem}] - StopInteract" );
    }

    public override void StopHover ()
    {
        base.StopInteract ();

        // Hide Interactable UI
        Debug.Log ( $"Interactable [{m_playerItem}] - StopHover" );
    }

    #endregion

    #region Util

    protected void SetColliderBounds ( GameObject assetModel )
    {
        var pos = assetModel.transform.localPosition;
        var rot = assetModel.transform.localRotation;
        var scale = assetModel.transform.localScale;

        // Need to clear out transforms while encapsulating bounds
        assetModel.transform.localPosition = Vector3.zero;
        assetModel.transform.localRotation = Quaternion.identity;
        assetModel.transform.localScale = Vector3.one;

        // Start with root object's bounds
        var bounds = new Bounds ( Vector3.zero, Vector3.zero );
        if ( assetModel.transform.TryGetComponent<Renderer> ( out var mainRenderer ) )
        {
            // New Bounds() will include 0,0,0 which you may not want to Encapsulate
            // because the vertices of the mesh may be way off the model's origin,
            // so instead start with the first renderer bounds and Encapsulate from there
            bounds = mainRenderer.bounds;
        }

        var descendants = assetModel.GetComponentsInChildren<Transform> ();
        foreach ( Transform desc in descendants )
        {
            if ( desc.TryGetComponent<Renderer> ( out var childRenderer ) )
            {
                if ( bounds.extents == Vector3.zero )
                    bounds = childRenderer.bounds;
                bounds.Encapsulate ( childRenderer.bounds );
            }
        }

        // Apply bounds to box collider
        m_boxCollider.center = bounds.center - assetModel.transform.position;
        m_boxCollider.size = bounds.size;

        // Restore transforms
        assetModel.transform.localPosition = pos;
        assetModel.transform.localRotation = rot;
        assetModel.transform.localScale = scale;
    }

    #endregion
}
