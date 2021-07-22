using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuManager : LazySingleton<PauseMenuManager>
{
    public bool Pause { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void PauseMenuActive ( bool state )
    {
        Pause = state;
        Debug.Log ( $"Pause={Pause}" );

        // Toggle cursor lock mode
        CursorLockMode lockMode = ( Pause ? CursorLockMode.None : CursorLockMode.Locked );
        CameraController.Instance.SetCursorMode ( lockMode );
    }
}
