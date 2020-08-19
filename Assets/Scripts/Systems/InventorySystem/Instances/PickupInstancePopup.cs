using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InventorySystem.PlayerItems;

public class PickupInstancePopup : MonoBehaviour
{
    [SerializeField]
    private Text m_itemNameText = null;
    [SerializeField]
    private Text m_itemTypeText = null;
    [SerializeField]
    private Image m_itemImage = null;


    // Start is called before the first frame update
    void Start ()
    {

    }

    public void SetActive ( bool value )
    {
        gameObject.SetActive ( value );
    }

    public void UpdateDetails ( PlayerItem playerItem )
    {
        if ( playerItem == null )
        {
            m_itemNameText.text = m_itemTypeText.text = string.Empty;
            m_itemImage.sprite = null;
            return;
        }

        m_itemNameText.text = playerItem.Name.ToUpper ();
        m_itemTypeText.text = playerItem.GetType ().ToString ().ToUpper ();
        m_itemImage.sprite = playerItem.Image;
    }
}
