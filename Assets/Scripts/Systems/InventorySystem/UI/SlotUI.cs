using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using InventorySystem.Slots;

public class SlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string Id = string.Empty;
    private Color m_normalColor = new Color();
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


    public void UpdateSlot ( Slot slot )
    {
        if ( Id != slot.Id )
        {
            Debug.LogError ( "Slot ids do not match." );
            return;
        }

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

    public void OnPointerEnter ( PointerEventData eventData )
    {
        m_slotImage.color = m_highlightColor;
    }

    public void OnPointerExit ( PointerEventData eventData )
    {
        m_slotImage.color = m_normalColor;
    }
}
