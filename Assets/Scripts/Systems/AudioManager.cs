using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AudioManager
{
    private static List<AudioSource> m_audioRegister = new List<AudioSource> ();

    public static void PlaySound ( string clipName, float volume, bool loop )
    {
        GameObject soundGameObject = new GameObject ( $"Audio [{clipName}]" );
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource> ();
        m_audioRegister.Add ( audioSource );
        audioSource.volume = volume;
        audioSource.outputAudioMixerGroup = GameAssets.Instance.GetAudioMixerGroup ( clipName );
        audioSource.loop = loop;
        AudioClip clip = GameAssets.Instance.GetAudioClip ( clipName );
        audioSource.clip = clip;
        audioSource.Play ();
        if ( !loop )
        {
            Object.Destroy ( soundGameObject, clip.length );
        }
    }

    public static void PlaySound ( AudioClip clip, float volume, bool loop )
    {
        GameObject soundGameObject = new GameObject ( $"Audio [{clip.name}]" );
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource> ();
        m_audioRegister.Add ( audioSource );
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.Play ();
        if ( !loop )
        {
            Object.Destroy ( soundGameObject, clip.length );
        }
    }

    public static void PlaySound ( AudioClip clip, float volume, bool loop, float spatialBlend, Vector3 location )
    {
        GameObject soundGameObject = new GameObject ( $"Audio [{clip.name}]" );
        soundGameObject.transform.position = location;
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource> ();
        m_audioRegister.Add ( audioSource );
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.spatialBlend = spatialBlend;
        audioSource.Play ();
        if ( !loop )
        {
            Object.Destroy ( soundGameObject, clip.length );
        }
    }

    public static void Stop ( string clipName )
    {
        PruneRegister ();

        AudioSource audioSource = m_audioRegister.FirstOrDefault ( s => s.clip.name == clipName );

        if ( audioSource != null )
        {
            Object.Destroy ( audioSource );
        }
    }

    public static void StopAll ()
    {
        foreach ( AudioSource source in m_audioRegister )
        {
            Object.Destroy ( source );
        }
        m_audioRegister.Clear ();
    }

    /// <summary>
    /// Removes all elements that are null from the audio register.
    /// </summary>
    private static void PruneRegister ()
    {
        m_audioRegister = m_audioRegister.Where ( s => s != null ).ToList ();
    }
}
