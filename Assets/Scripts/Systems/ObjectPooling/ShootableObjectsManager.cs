using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootableObjectsManager : MonoBehaviour
{
    #region Models

    public enum ShootableObjects
    {
        SmallBullet,
        MediumBullet,
        LargeBullet,
        Debug
    }

    [Serializable]
    public class ShootableObjectPool
    {
        [HideInInspector]
        public string ObjectName;
        public ShootableObjects Tag;
        public GameObject Prefab;
        public int Size;

        public void OnValidate ()
        {
            ObjectName = Tag.ToString ();
        }
    }

    public enum HitObjects
    {
        Asphalt,
        Bricks,
        Concrete,
        Glass,
        Ground,
        Metal1,
        Metal2,
        Mud,
        Plywood,
        Rock,
        Sand,
        Skin,
        Tiles,
        Water,
        Wood,
        Debug
    }

    [Serializable]
    public class HitObjectsPool
    {
        [HideInInspector]
        public string ObjectName;
        public HitObjects Tag;
        public GameObject Prefab;
        public int Size;

        public void OnValidate ()
        {
            ObjectName = Tag.ToString ();
        }
    }

    #endregion

    public static ShootableObjectsManager Instance;

    [Header ( "Shootable Objects" ), SerializeField]
    private List<ShootableObjectPool> m_shootableObjectPools;
    private Dictionary<int, Queue<GameObject>> m_shootableObjectPool;

    [Header ( "Hit Objects" ), SerializeField]
    private List<HitObjectsPool> m_hitObjectPools;
    private Dictionary<int, Queue<GameObject>> m_hitObjectPool;


    private void Awake ()
    {
        if ( Instance == null )
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start ()
    {
        InitializePools ();
    }

    private void InitializePools ()
    {
        m_shootableObjectPool = new Dictionary<int, Queue<GameObject>> ();
        m_hitObjectPool = new Dictionary<int, Queue<GameObject>> ();

        // Shootable object pool
        foreach ( ShootableObjectPool pool in m_shootableObjectPools )
        {
            Queue<GameObject> objectPool = new Queue<GameObject> ();
            for ( int i = 0; i < pool.Size; i++ )
            {
                GameObject obj = Instantiate ( pool.Prefab );
                obj.SetActive ( false );
                objectPool.Enqueue ( obj );
            }
            m_shootableObjectPool.Add ( ( int ) pool.Tag, objectPool );
        }

        // Hit object pool
        foreach ( HitObjectsPool pool in m_hitObjectPools )
        {
            Queue<GameObject> objectPool = new Queue<GameObject> ();
            for ( int i = 0; i < pool.Size; i++ )
            {
                GameObject obj = Instantiate ( pool.Prefab );
                obj.SetActive ( false );
                objectPool.Enqueue ( obj );
            }
            m_hitObjectPool.Add ( ( int ) pool.Tag, objectPool );
        }
    }

    public GameObject SpawnFromPool ( ShootableObjects tag, Vector3 position, Vector3 normal )
    {
        if ( !m_shootableObjectPool.ContainsKey ( ( int ) tag ) )
        {
            Debug.LogWarning ( $"Shootable Object Pool with tag [{tag}] doesn't exist." );
            return null;
        }

        GameObject objectToSpawn = m_shootableObjectPool [ ( int ) tag ].Dequeue ();
        objectToSpawn.SetActive ( true );
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = Quaternion.Euler ( normal );

        IPooledObject pooledObject = objectToSpawn.GetComponent<IPooledObject> ();
        if ( pooledObject != null )
        {
            pooledObject.OnObjectSpawn ();
        }

        m_shootableObjectPool [ ( int ) tag ].Enqueue ( objectToSpawn );

        return objectToSpawn;
    }

    public GameObject SpawnFromPool ( HitObjects tag, Vector3 position, Vector3 normal )
    {
        //Debug.Log ("SpawnFromPool()");

        if ( !m_hitObjectPool.ContainsKey ( ( int ) tag ) )
        {
            Debug.LogWarning ( $"Hit Object Pool with tag [{tag}] doesn't exist." );
            return null;
        }

        GameObject objectToSpawn = m_hitObjectPool [ ( int ) tag ].Dequeue ();
        objectToSpawn.SetActive ( true );
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = Quaternion.LookRotation ( normal, Vector3.up );

        IPooledObject[] pooledObjects = objectToSpawn.GetComponents<IPooledObject> ();
        foreach ( IPooledObject pooledObject in pooledObjects )
        {
            pooledObject.OnObjectSpawn ();
        }

        m_hitObjectPool [ ( int ) tag ].Enqueue ( objectToSpawn );

        return objectToSpawn;
    }

    private void OnValidate ()
    {
        foreach ( ShootableObjectPool pool in m_shootableObjectPools )
        {
            pool.OnValidate ();
        }

        foreach ( HitObjectsPool pool1 in m_hitObjectPools )
        {
            pool1.OnValidate ();
        }
    }
}
