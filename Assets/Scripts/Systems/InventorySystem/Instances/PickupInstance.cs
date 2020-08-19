using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.PlayerItems;

public class PickupInstance : PlayerItemInstance
{
    [Header ( "UI" )]
    [SerializeField]
    private PickupInstancePopup m_detailsPopup = null;


    private void OnEnable ()
    {
        m_detailsPopup.SetActive ( true );
    }

    public void Initialize ( PlayerItem playerItem )
    {
        if ( playerItem == null )
        {
            Debug.LogWarning ( "playerItem is null." );
            return;
        }

        m_playerItem = playerItem;
        m_detailsPopup.UpdateDetails ( playerItem );
    }
}
