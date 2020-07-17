using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.PlayerItems;

public class PlayerItemInstance : MonoBehaviour
{
    public PlayerItem PlayerItem
    {
        get
        {
            return m_playerItem;
        }
    }
    [SerializeField]
    private PlayerItem m_playerItem = null;


    // Start is called before the first frame update
    void Start ()
    {

    }

    // Update is called once per frame
    void Update ()
    {

    }

    public void SetActive ( bool value )
    {
        gameObject.SetActive ( value );
    }
}
