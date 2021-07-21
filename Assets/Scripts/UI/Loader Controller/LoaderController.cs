using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent ( typeof ( LoaderView ) )]
public class LoaderController : MonoBehaviour
{
    private LoaderView m_view = null;
    private SceneController m_sceneController = null;

    private void Awake ()
    {
        m_view = GetComponent<LoaderView> ();
    }

    // Start is called before the first frame update
    void Start ()
    {
        m_sceneController = SceneController.Instance;
    }

    private void FixedUpdate ()
    {
        if ( m_sceneController != null )
        {
            float progress = m_sceneController.SceneLoadProgress;
            m_view.UpdateProgressBar ( progress );
        }
    }
}
