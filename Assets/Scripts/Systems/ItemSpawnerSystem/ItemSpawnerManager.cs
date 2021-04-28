using System.Collections.Generic;
using UnityEngine;

public class ItemSpawnerManager : PersistentLazySingleton<ItemSpawnerManager>
{
    public static Dictionary<int, ItemSpawner> ItemSpawners = new Dictionary<int, ItemSpawner> ();

    [SerializeField]
    private GameObject m_itemSpawnerPrefab = null;


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
}
