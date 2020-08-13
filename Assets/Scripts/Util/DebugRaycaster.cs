using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugRaycaster : MonoBehaviour
{
    [SerializeField]
    private float m_raycastDistance = 100f;

    [SerializeField]
    private GameObject m_debugObject = null;
    private GameObject m_debugObjectInstance = null;

    // Start is called before the first frame update
    void Start ()
    {
        m_debugObjectInstance = Instantiate ( m_debugObject, transform.position, Quaternion.identity );
        m_debugObjectInstance.SetActive ( false );
    }

    // Update is called once per frame
    void Update ()
    {
        if ( Physics.Raycast ( transform.position, transform.forward, out RaycastHit hit, m_raycastDistance ) )
        {
            if ( m_debugObjectInstance != null )
            {
                m_debugObjectInstance.SetActive ( true );
                m_debugObjectInstance.transform.position = hit.point;
            }
        }
        else
        {
            m_debugObjectInstance.SetActive ( false );
        }
    }
}
