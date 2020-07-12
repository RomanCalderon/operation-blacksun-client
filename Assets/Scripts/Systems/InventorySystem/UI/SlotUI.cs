using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using InventorySystem.Slots;
using System.Security.Permissions;

public class SlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public string Id = string.Empty;
    public Slot Slot { get; private set; } = null;

    private Color m_normalColor = Color.red;
    [SerializeField]
    private Color m_vacantColor = new Color ();
    [SerializeField]
    private Color m_occupiedColor = new Color ();
    [SerializeField]
    private Color m_highlightColor = new Color ();
    [SerializeField]
    private Image m_slotImage = null;
    [SerializeField]
    private Image m_contentImage = null;
    [SerializeField]
    private Text m_contentNameText = null;
    [SerializeField]
    private Text m_contentStackSizeText = null;

    private void Awake ()
    {
        // Set the default colors
        m_slotImage.color = m_normalColor = m_vacantColor;
    }

    public void UpdateSlot ( Slot slot )
    {
        // NRE is checked in InventoryManager

        // Check if a slot reference is null
        if ( Slot == null )
        {
            Slot = slot;
        }

        // Check for matching IDs
        if ( Id != slot.Id )
        {
            Debug.LogError ( "Slot IDs do not match." );
            return;
        }

        DisplayContents ( true );

        // Update slot ui
        if ( slot.IsEmpty () )
        {
            m_slotImage.color = m_normalColor = m_vacantColor;
            m_contentImage.enabled = false;
            m_contentImage.sprite = null;
            m_contentNameText.text = string.Empty;
            m_contentStackSizeText.text = string.Empty;
        }
        else
        {
            m_slotImage.color = m_normalColor = m_occupiedColor;
            m_contentImage.enabled = true;
            m_contentImage.sprite = slot.PlayerItem.Image;
            m_contentNameText.text = slot.PlayerItem.Name;
            if ( slot.IsStackable )
            {
                m_contentStackSizeText.text = $"{slot.StackSize}/{slot.PlayerItem.StackLimit}";
            }
            else
            {
                m_contentStackSizeText.text = string.Empty;
            }
        }
    }

    public void DisplayContents ( bool state )
    {
        m_contentImage.enabled = state;
        m_contentNameText.enabled = state;
        m_contentStackSizeText.enabled = state;
    }

    public void ChangeContents ( PointerEventData.InputButton button )
    {
        switch ( button )
        {
            case PointerEventData.InputButton.Left: // Moving whole slot
                DisplayContents ( false ); // Hide all contents
                break;
            case PointerEventData.InputButton.Right: // Moving one item from slot
                if ( Slot.IsStackable && Slot.StackSize > 1 )
                {
                    m_contentStackSizeText.text = $"{Slot.StackSize - 1}/{Slot.PlayerItem.StackLimit}"; // Display the stack size minus one
                }
                else
                {
                    DisplayContents ( false ); // Hide all contents
                }
                break;
            case PointerEventData.InputButton.Middle: // Moving half the stack size
                m_contentStackSizeText.text = $"{Mathf.CeilToInt ( Slot.StackSize / 2f )}/{Slot.PlayerItem.StackLimit}"; // Display half the stack size
                break;
            default:
                break;
        }
    }

    public Sprite GetContentImage ()
    {
        return m_contentImage.sprite;
    }

    public void OnPointerEnter ( PointerEventData eventData )
    {
        m_slotImage.color = m_highlightColor;
    }

    public void OnPointerExit ( PointerEventData eventData )
    {
        m_slotImage.color = m_normalColor;
    }

    public void OnPointerDown ( PointerEventData eventData )
    {
        if ( Slot != null && !Slot.IsEmpty () )
        {
            InventoryManager.Instance.OnSlotSelected ( this, eventData );
        }
    }

    public void OnBeginDrag ( PointerEventData eventData )
    {
        if ( Slot != null && !Slot.IsEmpty () )
        {
            InventoryManager.Instance.SlotBeginDrag ( eventData );
        }
    }

    public void OnDrag ( PointerEventData eventData )
    {
        if ( Slot != null && !Slot.IsEmpty () )
        {
            InventoryManager.Instance.SlotDrag ( eventData.position );
        }
    }

    public void OnEndDrag ( PointerEventData eventData )
    {
        if ( Slot != null && !Slot.IsEmpty () )
        {
            InventoryManager.Instance.SlotEndDrag ( eventData );
        }
    }

    public void OnDrop ( PointerEventData eventData )
    {
        Debug.Assert ( Slot != null, "Slot is null." );
        if ( Slot != null )
        {
            InventoryManager.Instance.SlotOnDrop ( Id, eventData );
        }
    }
}
