using UnityEngine;
using InventorySystem.PlayerItems;

public class ItemSpawner : MonoBehaviour
{
    public int SpawnerId { get => m_spawnerId; }
    public int ItemQuantity { get => m_quantity; }

    [SerializeField]
    private PickupInstance m_pickupInstancePrefab = null;

    private int m_spawnerId;
    private string m_itemId = null;
    private int m_quantity = 1;

    private PickupInstance m_instance = null;

    public void Initialize ( int spawnerId, string itemId, int quantity )
    {
        m_spawnerId = spawnerId;
        m_itemId = itemId;
        m_quantity = quantity;

        SpawnItem ();
    }

    public void DestroyItem ()
    {
        if ( m_instance != null )
        {
            Destroy ( m_instance );
        }
    }

    private void SpawnItem ()
    {
        m_instance = Instantiate ( m_pickupInstancePrefab, transform.position, transform.rotation, transform );
        m_instance.Initialize ( m_itemId, m_quantity );
    }
}
