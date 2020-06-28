using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private int id;
    [SerializeField]
    private string username;

    public float Health
    {
        get
        {
            return health;
        }
    }
    [SerializeField]
    private float health;
    [SerializeField]
    private float maxHealth = 100;
    [SerializeField]
    private GameObject model = null;

    private void Start ()
    {
        Debug.Assert ( model != null );
    }

    public void Initialize ( int _id, string _username )
    {
        id = _id;
        username = _username;
        health = maxHealth;
    }

    // Update is called once per frame
    void Update ()
    {

    }

    public void SetHealth ( float _health )
    {
        health = _health;

        if ( health <= 0f )
        {
            Die ();
        }
    }

    public void Die ()
    {
        model.SetActive ( false );
    }

    public void Respawn ()
    {
        model.SetActive ( true );
        SetHealth ( maxHealth );
    }
}
