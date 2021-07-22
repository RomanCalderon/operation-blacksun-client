using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PauseMenuManager : LazySingleton<PauseMenuManager>
{
    public bool Pause { get; private set; }

    [SerializeField]
    public UnityEvent onPause = null;
    [SerializeField]
    public UnityEvent onUnpause = null;

    [SerializeField]
    private int m_mainMenuSceneIndex = 0;

    private void Start ()
    {
        // Default mode set to unpause
        PauseMenuActive ( false );
    }

    public void PauseMenuActive ( bool state )
    {
        Pause = state;

        if ( Pause )
            onPause?.Invoke ();
        else
            onUnpause?.Invoke ();

        // Toggle cursor lock mode
        CursorLockMode lockMode = ( Pause ? CursorLockMode.None : CursorLockMode.Locked );
        CameraController.Instance.SetCursorMode ( lockMode );
    }

    public void LeaveGame ()
    {
        // Disconnect from the server
        Client.instance.Disconnect ();

        SceneController.Instance.LoadScene ( m_mainMenuSceneIndex );
    }
}
