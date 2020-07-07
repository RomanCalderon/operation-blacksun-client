using System.Collections.Generic;
using UnityEngine;
using InventorySystem;
using InventorySystem.PlayerItems;
using InventorySystem.Slots;
using InventorySystem.Slots.Results;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    [SerializeField]
    private Inventory m_inventory = null;
    private PlayerItemDatabase m_playerItemDatabase = null;

    [Header ( "Slot UI" ), SerializeField]
    private List<SlotUI> m_slotUI = new List<SlotUI> ();

    private void Awake ()
    {
        m_inventory = new Inventory ();
        m_playerItemDatabase = Resources.Load<PlayerItemDatabase> ( "PlayerItemDatabase" );
        Debug.Assert ( m_playerItemDatabase != null, "PlayerItemDatabase is null." );
    }

    public void SetSlot ( string slotId, string playerItemId, int quantity )
    {
        if ( m_inventory == null )
        {
            return;
        }
        PlayerItem playerItem = m_playerItemDatabase.GetPlayerItem ( playerItemId );
        InsertionResult result = m_inventory.SetSlot ( slotId, playerItem, quantity );
        if ( result.Result == InsertionResult.Results.SUCCESS || result.Result == InsertionResult.Results.OVERFLOW )
        {
            UpdateSlotUI ( result.Slot );
        }
        m_inventory.OnValidate ();
    }

    private void UpdateSlotUI ( Slot slot )
    {
        SlotUI slotUI = m_slotUI.FirstOrDefault ( s => s.Id == slot.Id );

        if ( slotUI == null )
        {
            Debug.LogError ( "SlotUI is null." );
            return;
        }
        slotUI.UpdateSlot ( slot );
    }
}
