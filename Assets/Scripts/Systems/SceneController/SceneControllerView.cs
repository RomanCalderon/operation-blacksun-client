using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneControllerView : MonoBehaviour
{
    [Header ( "General" )]
    [SerializeField, Range ( 0.01f, 8f )]
    private float m_fadeInDuration = 0.1f, m_fadeOutDuration = 0.1f;

    [Header ( "View" )]
    [SerializeField]
    private CanvasGroup m_canvasFader = null;

    // Start is called before the first frame update
    void Start ()
    {
        m_canvasFader.gameObject.SetActive ( false );
    }

    public void FadeIn ( Action onFadeIn )
    {
        StartCoroutine ( FadeInEnum ( onFadeIn ) );
    }

    public void FadeOut ( Action onFadeOut )
    {
        StartCoroutine ( FadeOutEnum ( onFadeOut ) );
    }

    #region Coroutines

    private IEnumerator FadeInEnum ( Action onFadeIn )
    {
        m_canvasFader.alpha = 0f;
        m_canvasFader.gameObject.SetActive ( true );

        while ( m_canvasFader.alpha < 1f )
        {
            m_canvasFader.alpha += Time.deltaTime / m_fadeInDuration;
            yield return null;
        }
        m_canvasFader.gameObject.SetActive ( false );
        onFadeIn?.Invoke ();
    }

    private IEnumerator FadeOutEnum ( Action onFadeOut )
    {
        m_canvasFader.alpha = 1f;
        m_canvasFader.gameObject.SetActive ( true );

        while ( m_canvasFader.alpha > 0f )
        {
            m_canvasFader.alpha -= Time.deltaTime / m_fadeOutDuration;
            yield return null;
        }
        m_canvasFader.gameObject.SetActive ( false );
        onFadeOut?.Invoke ();
    }

    #endregion
}
