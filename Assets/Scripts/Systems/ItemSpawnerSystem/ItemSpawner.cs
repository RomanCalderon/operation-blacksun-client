using System;
using System.IO;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    #region Models

    [Serializable]
    public struct SpawnerData
    {
        public int SpawnerId;
        public Vector3 SpawnerPosition;
        public Vector3 SpawnerRotation;
        public string ItemId;
        public int ItemQuantity;
        public byte [] InteractableData;

        public byte [] ToArray ()
        {
            MemoryStream stream = new MemoryStream ();
            BinaryWriterExtended writer = new BinaryWriterExtended ( stream );

            writer.Write ( SpawnerId );
            writer.Write ( SpawnerPosition );
            writer.Write ( SpawnerRotation );
            writer.Write ( ItemId );
            writer.Write ( ItemQuantity );
            writer.Write ( InteractableData.Length );
            writer.Write ( InteractableData );

            return stream.ToArray ();
        }

        public static SpawnerData FromArray ( byte [] bytes )
        {
            BinaryReaderExtended reader = new BinaryReaderExtended ( new MemoryStream ( bytes ) );
            SpawnerData s = default;

            s.SpawnerId = reader.ReadInt32 ();
            s.SpawnerPosition = reader.ReadVector3 ();
            s.SpawnerRotation = reader.ReadVector3 ();
            s.ItemId = reader.ReadString ();
            s.ItemQuantity = reader.ReadInt32 ();
            s.InteractableData = reader.ReadBytes ( reader.ReadInt32 () );

            return s;
        }
    }

    #endregion

    public int SpawnerId { get => m_spawnerId; }
    public int ItemQuantity { get => m_quantity; }

    [SerializeField]
    private PickupInstance m_pickupInstancePrefab = null;

    private int m_spawnerId;
    private string m_itemId = null;
    private int m_quantity = 1;

    private PickupInstance m_instance = null;

    public void Initialize ( SpawnerData spawnerData )
    {
        m_spawnerId = spawnerData.SpawnerId;
        m_itemId = spawnerData.ItemId;
        m_quantity = spawnerData.ItemQuantity;

        SpawnItem ( spawnerData.InteractableData );
    }

    public void DestroyItem ()
    {
        if ( m_instance != null )
        {
            Destroy ( m_instance.gameObject );
        }
    }

    private void SpawnItem ( byte [] interactableData )
    {
        m_instance = Instantiate ( m_pickupInstancePrefab, transform.position, transform.rotation, transform );
        m_instance.Initialize ( interactableData, m_itemId, m_quantity );
    }
}
