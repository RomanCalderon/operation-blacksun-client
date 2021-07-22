using System.Collections.Generic;
using UnityEngine;

public class ItemSpawnerManager : LazySingleton<ItemSpawnerManager>
{
    public static Dictionary<int, ItemSpawner> ItemSpawners = null;

    [SerializeField]
    private GameObject m_itemSpawnerPrefab = null;


    protected override void Awake ()
    {
        base.Awake ();

        ItemSpawners = new Dictionary<int, ItemSpawner> ();
    }

    public void CreateItemSpawner ( byte [] data )
    {
        // Deserialize spawner data
        ItemSpawner.SpawnerData spawnerData = ItemSpawner.SpawnerData.FromArray ( data );

        // Create and initialize item spawner
        GameObject spawner = Instantiate ( m_itemSpawnerPrefab, spawnerData.SpawnerPosition, Quaternion.Euler ( spawnerData.SpawnerRotation ), transform );
        ItemSpawner itemSpawner = spawner.GetComponent<ItemSpawner> ();
        itemSpawner.Initialize ( spawnerData );
        ItemSpawners.Add ( spawnerData.SpawnerId, itemSpawner );
    }

    public void SpawnItem ( byte [] spawnerData )
    {
        // TODO: Create an ItemSpawner with spawnerData
        // and spawn PlayerItem with given quantity.
        throw new System.NotImplementedException ();
    }
}
