using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoaderView : MonoBehaviour
{
    [SerializeField]
    private Slider m_progressbar = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void UpdateProgressBar ( float value )
    {
        if ( m_progressbar != null )
        {
            m_progressbar.value = Mathf.Clamp01 ( value );
        }
    }
}
