using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[RequireComponent ( typeof ( CanvasGroup ) )]
public class CanvasGroupFader : MonoBehaviour
{

    private CanvasGroup m_canvasGroup = null;
    private bool m_isActive = true;

    [SerializeField, Tooltip ( "Fades in if true, fades out if false." )]
    private bool m_fadeIn = true;
    [SerializeField, Tooltip ( "Alternates between fading in and out." )]
    private bool m_pingPong = true;
    [SerializeField, Tooltip ( "Fade speed multiplier." )]
    private float m_fadeSpeed = 1.0f;

    private float m_alphaValue = 1f;
    private bool m_pingPongUp = true;

    private void Awake ()
    {
        m_canvasGroup = GetComponent<CanvasGroup> ();
    }

    // Start is called before the first frame update
    void Start ()
    {
        m_canvasGroup.alpha = m_alphaValue = m_fadeIn ? 0f : 1f;
    }

    // Update is called once per frame
    void Update ()
    {
        float deltaTime = Time.deltaTime;

        if ( m_isActive )
        {
            if ( m_pingPong )
            {
                if ( m_pingPongUp )
                {
                    m_alphaValue += deltaTime * m_fadeSpeed;

                    if ( m_alphaValue >= 1f )
                    {
                        m_pingPongUp = false;
                    }
                }
                else
                {
                    m_alphaValue -= deltaTime * m_fadeSpeed;

                    if ( m_alphaValue <= 0f )
                    {
                        m_pingPongUp = true;
                    }
                }
            }
            else
            {
                if ( m_fadeIn && m_alphaValue < 1f )
                {
                    m_alphaValue += deltaTime * m_fadeSpeed;
                }
                else if ( m_alphaValue > 0f )
                {
                    m_alphaValue -= deltaTime * m_fadeSpeed;
                }
            }

            // Update canvas alpha value
            m_alphaValue = Mathf.Clamp01 ( m_alphaValue );
            m_canvasGroup.alpha = m_alphaValue;
        }
    }

    public void SetActive ( bool value )
    {
        m_isActive = value;
    }

    public void SetFade ( bool value )
    {
        m_fadeIn = value;
    }
}
