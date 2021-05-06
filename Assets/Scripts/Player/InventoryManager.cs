using System.Collections.Generic;
using UnityEngine;
using InventorySystem;
using InventorySystem.PlayerItems;
using InventorySystem.Slots;
using InventorySystem.Slots.Results;
using System.Linq;
using UnityEngine.EventSystems;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance = null;

    public delegate void SlotHandler ( string slotId );
    public static SlotHandler OnSlotUpdated;

    public bool IsInteractable = true;
    public bool IsDisplayed { get; private set; } = false;

    public Inventory Inventory { get { return m_inventory; } }
    [SerializeField]
    private Inventory m_inventory = null;
    public PlayerItemDatabase PlayerItemDatabase { get; private set; } = null;

    [Header ( "UI" ), SerializeField]
    private GameObject m_inventoryView = null;
    [SerializeField]
    private List<SlotUI> m_slotUI = new List<SlotUI> ();
    private SlotUI m_selectedSlotUI = null;
    private bool IsSlotSelected
    {
        get
        {
            return m_selectedSlotUI != null;
        }
    }
    [SerializeField]
    private GameObject m_slotDragContentsPrefab = null;
    private SlotDragContents m_slotDragContentsInstance = null;
    private bool m_isDraggingSlot = false;

    private void Awake ()
    {
        if ( Instance == null )
        {
            Instance = this;
        }

        m_inventory = new Inventory ();
        PlayerItemDatabase = Resources.Load<PlayerItemDatabase> ( "PlayerItemDatabase" );
        InitializeSlots ();
        Debug.Assert ( PlayerItemDatabase != null, "PlayerItemDatabase is null." );
        Debug.Assert ( m_inventoryView != null, "m_inventoryView is null." );
        Debug.Assert ( m_slotDragContentsPrefab != null, "m_slotContentsPrefab is null." );
    }

    private void Start ()
    {
        m_inventoryView.SetActive ( false );
    }

    private void Update ()
    {
        if ( IsInteractable )
        {
            if ( Input.GetKeyDown ( KeyCode.Tab ) )
            {
                // Toggle display mode
                IsDisplayed = !IsDisplayed;

                // Set CanAim based on display
                AimController.CanAim = !IsDisplayed;

                // Show/hide inventory
                m_inventoryView.SetActive ( IsDisplayed );

                // Toggle cursor lock mode
                CursorLockMode lockMode = ( IsDisplayed ? CursorLockMode.None : CursorLockMode.Locked );
                CameraController.Instance.SetCursorMode ( lockMode );
            }
        }
    }

    private void InitializeSlots ()
    {
        foreach ( SlotUI slotUI in m_slotUI )
        {
            Slot slot = m_inventory.GetSlot ( slotUI.Id );
            slotUI.UpdateSlot ( slot );
        }
    }

    public int GetItemCount ( string playerItemId )
    {
        return Inventory.GetItemCount ( playerItemId );
    }

    /// <summary>
    /// Called from ClientHandle class.
    /// </summary>
    /// <param name="slotId">Slot ID</param>
    /// <param name="playerItemId">PlayerItem Id</param>
    /// <param name="quantity">Amount of this PlayerItem (Slot stack size)</param>
    public void UpdateSlot ( string slotId, string playerItemId, int quantity )
    {
        if ( m_inventory == null )
        {
            return;
        }

        PlayerItem playerItem = !string.IsNullOrEmpty ( playerItemId ) ? PlayerItemDatabase.GetPlayerItem ( playerItemId ) : null;
        InsertionResult result = m_inventory.SetSlot ( slotId, playerItem, quantity );

        // Update SlotUI
        SlotUI slotUI = m_slotUI.FirstOrDefault ( s => s.Id == slotId );
        if ( slotUI == null )
        {
            Debug.LogError ( "SlotUI is null." );
            return;
        }
        slotUI.UpdateSlot ( slotUI.Slot );
        m_inventory.OnValidate ();

        OnSlotUpdated?.Invoke ( slotId );
    }

    #region SlotUI Interactions

    public void OnSlotSelected ( SlotUI slotUI, PointerEventData eventData )
    {
        if ( slotUI == null )
        {
            Debug.LogWarning ( "slotUI is null." );
            return;
        }
        if ( IsSlotSelected )
        {
            return;
        }
        m_selectedSlotUI = slotUI;
    }

    public void SlotBeginDrag ( PointerEventData eventData )
    {
        if ( !IsSlotSelected )
        {
            return;
        }
        if ( m_isDraggingSlot )
        {
            return;
        }

        int contentDragAmount;

        m_isDraggingSlot = true;

        switch ( eventData.button )
        {
            case PointerEventData.InputButton.Left: // Moving whole slot
                contentDragAmount = m_selectedSlotUI.Slot.StackSize;
                break;
            case PointerEventData.InputButton.Right: // Moving one item from slot
                contentDragAmount = 1;
                break;
            case PointerEventData.InputButton.Middle: // Moving half the stack size
                contentDragAmount = Mathf.CeilToInt ( m_selectedSlotUI.Slot.StackSize / 2f );
                break;
            default:
                // Unknown input button
                return;
        }

        // Update SlotUI contents
        m_selectedSlotUI.ChangeContents ( eventData.button );

        // Assign SlotDragContent
        m_slotDragContentsInstance = Instantiate ( m_slotDragContentsPrefab, m_inventoryView.transform ).GetComponent<SlotDragContents> ();
        m_slotDragContentsInstance.Initialize ( m_selectedSlotUI.Slot.PlayerItem, contentDragAmount );
    }

    public void SlotDrag ( Vector2 dragPosition )
    {
        if ( !IsSlotSelected || m_slotDragContentsInstance == null )
        {
            return;
        }
        m_slotDragContentsInstance.transform.position = dragPosition;
    }

    public void SlotEndDrag ( PointerEventData eventData )
    {
        m_isDraggingSlot = false;

        // Remove SlotDragContents instance
        if ( m_slotDragContentsInstance != null )
        {
            Destroy ( m_slotDragContentsInstance.gameObject );
        }

        if ( !eventData.hovered.Any ( o => o.CompareTag ( "SlotUI" ) ) )
        {
            // Reset SlotUI contents
            m_selectedSlotUI.DisplayContents ( true );
            m_selectedSlotUI.UpdateSlot ( m_selectedSlotUI.Slot );
        }
        // Clear selected SlotUI object
        m_selectedSlotUI = null;
    }

    public void SlotOnDrop ( string droppedSlotId, PointerEventData eventData )
    {
        // Check if the DRAG slot is not null/empty
        if ( !IsSlotSelected )
        {
            return;
        }

        // Check if the DRAG slot is the same as the DROP slot
        if ( m_selectedSlotUI.Id == droppedSlotId )
        {
            // Reset SlotUI contents
            m_selectedSlotUI.DisplayContents ( true );
            m_selectedSlotUI.UpdateSlot ( m_selectedSlotUI.Slot );
        }
        else
        {
            ClientSend.PlayerTransferSlotContents ( m_selectedSlotUI.Id, droppedSlotId, ( int ) eventData.button );
        }
    }

    #endregion
}
