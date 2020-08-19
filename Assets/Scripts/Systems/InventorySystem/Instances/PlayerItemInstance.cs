using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.PlayerItems;

public class PlayerItemInstance : MonoBehaviour
{
    public virtual PlayerItem PlayerItem
    {
        get
        {
            return m_playerItem;
        }
    }
    [SerializeField]
    protected PlayerItem m_playerItem = null;

    public void SetActive ( bool value )
    {
        gameObject.SetActive ( value );
    }
}
