using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuEnvironmentController : MonoBehaviour
{
    [SerializeField]
    private GameObject m_environment = null;

    // Start is called before the first frame update
    void Start()
    {
        ShowEnvironment ( false );
    }

    public void ShowEnvironment ( bool state )
    {
        m_environment.SetActive ( state );
    }
}
