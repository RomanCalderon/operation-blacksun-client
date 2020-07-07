using InventorySystem.Slots;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour
{
    public string Id = string.Empty;
    [SerializeField]
    private Color m_vacantColor = new Color ();
    [SerializeField]
    private Color m_occupiedColor = new Color ();
    [SerializeField]
    private Image m_slotBackgroundImage = null;
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
            m_slotBackgroundImage.color = m_vacantColor;
            m_contentImage.enabled = false;
            m_contentNameText.text = string.Empty;
            m_contentStackSizeText.text = string.Empty;
        }
        else
        {
            m_slotBackgroundImage.color = m_occupiedColor;
            // TODO: set content image to PlayerItem Image
            m_contentImage = null;
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
}
