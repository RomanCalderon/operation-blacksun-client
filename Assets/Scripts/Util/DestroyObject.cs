using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    [SerializeField]
    private float m_destroyDelay = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        Destroy ( gameObject, m_destroyDelay );
    }
}
