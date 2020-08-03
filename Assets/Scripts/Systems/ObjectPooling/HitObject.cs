using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent ( typeof ( AudioSource ) )]
public class HitObject : MonoBehaviour, IPooledObject
{
    private AudioSource m_audioSource = null;

    [SerializeField]
    private AudioClip [] m_impactSounds = null;

    private void Awake ()
    {
        m_audioSource = GetComponent<AudioSource> ();
    }

    public void OnObjectSpawn ()
    {
        m_audioSource.PlayOneShot ( m_impactSounds [ Random.Range ( 0, m_impactSounds.Length ) ] );
    }
}
