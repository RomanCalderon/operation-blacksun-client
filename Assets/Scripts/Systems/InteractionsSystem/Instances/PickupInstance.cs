using UnityEngine;
using InventorySystem.PlayerItems;

[RequireComponent ( typeof ( BoxCollider ) )]
[RequireComponent ( typeof ( NetworkedRigidbody ) )]
public class PickupInstance : Interactable
{
    public PlayerItem PlayerItem { get => m_playerItem; }
    public int Quantity { get => m_quantity; }

    private PlayerItem m_playerItem = null;
    private int m_quantity = 0;
    [SerializeField]
    private Transform m_container = null;
    private BoxCollider m_boxCollider = null;
    private NetworkedRigidbody m_networkedRigidbody = null;

    private void Awake ()
    {
        m_boxCollider = GetComponent<BoxCollider> ();
        m_networkedRigidbody = GetComponent<NetworkedRigidbody> ();
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

        // Initialize instance Bounds
        SetColliderBounds ();

        // Initialize NetworkedRigidbody
        m_networkedRigidbody.Initialize ( InstanceId );
    }

    #region Util

    protected void SetColliderBounds ()
    {
        var pos = transform.localPosition;
        var rot = transform.localRotation;
        var scale = transform.localScale;

        // Need to clear out transforms while encapsulating bounds
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        // Start with root object's bounds
        var bounds = new Bounds ( Vector3.zero, Vector3.zero );
        if ( transform.TryGetComponent<Renderer> ( out var mainRenderer ) )
        {
            // New Bounds() will include 0,0,0 which you may not want to Encapsulate
            // because the vertices of the mesh may be way off the model's origin,
            // so instead start with the first renderer bounds and Encapsulate from there
            bounds = mainRenderer.bounds;
        }

        var descendants = GetComponentsInChildren<Transform> ();
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
        m_boxCollider.center = bounds.center - transform.position;
        m_boxCollider.size = bounds.size;

        // Restore transforms
        transform.localPosition = pos;
        transform.localRotation = rot;
        transform.localScale = scale;
    }

    #endregion
}
