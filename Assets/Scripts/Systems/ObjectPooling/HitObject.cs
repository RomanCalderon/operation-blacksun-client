using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent ( typeof ( AudioSource ) )]
public class HitObject : MonoBehaviour, IPooledObject
{
    private AudioSource m_audioSource = null;

    [Header ( "Audio Settings" )]
    [SerializeField]
    private float m_minDistance = 2;
    [SerializeField]
    private float m_maxDistance = 20;
    [SerializeField]
    private AudioClip [] m_impactSounds = null;

    private void Awake ()
    {
        m_audioSource = GetComponent<AudioSource> ();
        m_audioSource.spatialBlend = 2;
        m_audioSource.minDistance = m_minDistance;
        m_audioSource.maxDistance = m_maxDistance;
    }

    public void OnObjectSpawn ()
    {
        if ( m_impactSounds != null && m_impactSounds.Length > 0 )
        {
            m_audioSource.PlayOneShot ( m_impactSounds [ Random.Range ( 0, m_impactSounds.Length ) ] );
        }
    }
}
