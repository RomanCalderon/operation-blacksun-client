using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;
using InventorySystem.Presets;
using InventorySystem.PlayerItems;

public class InventoryManager : MonoBehaviour
{
    [SerializeField]
    private Inventory m_inventory = null;
    private PlayerItemDatabase m_playerItemDatabase = null;

    private void Awake ()
    {
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
        m_inventory.SetSlot ( slotId, playerItem, quantity );
        m_inventory.OnValidate ();
    }
}
