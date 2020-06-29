using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using UnityEngine;
using UnityEngine.UI;

public class InfoBarHandler : MonoBehaviour
{
    private const float FPS_REFRESH_INTERVAL = 0.2f;

    [SerializeField]
    private GameObject m_infoBarView = null;
    private bool m_infoBarDisplayed = false;
    [SerializeField]
    private Text m_pingText = null;
    [SerializeField]
    private Text m_fpsText = null;
    [SerializeField]
    private bool m_fastFps = false;
    private int m_frameCounter = 0;
    private float m_timeCounter = 0.0f;
    private float m_lastFramerate = 0f;

    private int m_lastPing = 0;
    private int m_ping
    {
        get { return m_lastPing; }
        set
        {
            if ( value != m_lastPing )
            {
                m_lastPing = value;
                m_pingText.text = $"ping {value}ms";
            }
        }
    }

    private int m_lastFps = 0;
    private int m_fps
    {
        get { return m_lastFps; }
        set
        {
            if ( value != m_lastFps )
            {
                m_lastFps = value;
                m_fpsText.text = $"fps {value}";
            }
        }
    }


    // Start is called before the first frame update
    private void Start ()
    {
        Debug.Assert ( m_infoBarView != null );
        Debug.Assert ( m_pingText != null );
        Debug.Assert ( m_fpsText != null );
    }

    // Update is called once per frame
    private void Update ()
    {
        // Toggle the info bar view
        if ( Input.GetKeyDown ( KeyCode.I ) )
        {
            m_infoBarDisplayed = !m_infoBarDisplayed;
            m_infoBarView.SetActive ( m_infoBarDisplayed );
        }

        UpdateInfoBar ();
    }

    /// <summary>
    /// Updates the UI for displaying the client's ping and framerate.
    /// </summary>
    private void UpdateInfoBar ()
    {
        m_ping = ( int ) Client.instance.Ping;

        // Update the fps value every frame
        if ( m_fastFps )
        {
            m_fps = ( int ) ( 1f / Time.deltaTime );
        }
        else
        {
            // Only update fps after every FPS_CHECK_INTERVAL seconds
            if ( m_timeCounter < FPS_REFRESH_INTERVAL )
            {
                m_timeCounter += Time.deltaTime;
                m_frameCounter++;
            }
            else
            {
                m_lastFramerate = m_frameCounter / m_timeCounter;
                m_frameCounter = 0;
                m_timeCounter = 0.0f;

                m_fps = ( int ) m_lastFramerate;
            }
        }
    }
}
