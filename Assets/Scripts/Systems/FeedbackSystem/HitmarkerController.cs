using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class HitmarkerController : MonoBehaviour
{
    private const float SHOW_DURATION_STANDARD = 0.1f;
    private const float SHOW_DURATION_KILL = 0.14f;

    [SerializeField]
    private GameObject m_standardHitmarker = null;
    [SerializeField]
    private GameObject m_killHitmarker = null;
    [SerializeField]
    private AudioMixerGroup m_mixerGroup = null;
    [SerializeField]
    private AudioClip m_standardHitSound = null;
    [SerializeField]
    private AudioClip m_killHitSound = null;

    private Coroutine m_hitmarkerCoroutine = null;

    private void Start ()
    {
        ResetHitmarkers ();
    }

    public void ShowHitmarker ( int type )
    {
        if ( m_hitmarkerCoroutine != null )
        {
            StopCoroutine ( m_hitmarkerCoroutine );
            ResetHitmarkers ();
        }
        m_hitmarkerCoroutine = StartCoroutine ( ShowHitmarkerEnum ( type ) );
    }

    private IEnumerator ShowHitmarkerEnum ( int type )
    {
        if ( type == 0 )
        {
            AudioManager.PlaySound ( m_standardHitSound, m_mixerGroup, 1f, false, 0f, transform.position );
            m_standardHitmarker.SetActive ( true );
            yield return new WaitForSeconds ( SHOW_DURATION_STANDARD );
            m_standardHitmarker.SetActive ( false );
        }
        else if ( type == 1 )
        {
            AudioManager.PlaySound ( m_killHitSound, m_mixerGroup, 1f, false, 0f, transform.position );
            m_killHitmarker.SetActive ( true );
            yield return new WaitForSeconds ( SHOW_DURATION_KILL );
            m_killHitmarker.SetActive ( false );
        }
    }

    public void ResetHitmarkers ()
    {
        m_standardHitmarker.SetActive ( false );
        m_killHitmarker.SetActive ( false );
    }
}
