using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawnerManager : PersistentLazySingleton<ItemSpawnerManager>
{
    public static Dictionary<int, ItemSpawner> ItemSpawners = new Dictionary<int, ItemSpawner> ();

    [SerializeField]
    private GameObject m_itemSpawnerPrefab = null;


    public void CreateItemSpawner ( int spawnerId, Vector3 spawnerPosition, Vector3 spawnerRotation, string itemId, int quantity )
    {
        GameObject spawner = Instantiate ( m_itemSpawnerPrefab, spawnerPosition, Quaternion.Euler ( spawnerRotation ), transform );
        ItemSpawner itemSpawner = spawner.GetComponent<ItemSpawner> ();
        itemSpawner.Initialize ( spawnerId, itemId, quantity );
        ItemSpawners.Add ( spawnerId, itemSpawner );
    }
}
