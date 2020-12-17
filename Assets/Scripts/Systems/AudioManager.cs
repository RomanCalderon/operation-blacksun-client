using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public static class AudioManager
{
    private static bool m_isInitialized = false;
    private static GameAssets m_gameAssets = null;
    private static List<AudioSource> m_audioRegister = new List<AudioSource> ();

    public static void Initialize ()
    {
        if ( !m_isInitialized )
        {
            m_gameAssets = GameAssets.Instance;
            m_isInitialized = true;
        }
    }

    public static void PlaySound ( string clipName, float volume, bool loop )
    {
        Initialize ();
        GameObject soundGameObject = new GameObject ( $"Audio [{clipName}]" );
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource> ();
        m_audioRegister.Add ( audioSource );
        audioSource.volume = volume;
        audioSource.outputAudioMixerGroup = m_gameAssets.GetAudioMixerGroup ( clipName );
        audioSource.loop = loop;
        AudioClip clip = m_gameAssets.GetAudioClip ( clipName );
        audioSource.clip = clip;
        audioSource.Play ();
        if ( !loop )
        {
            Object.Destroy ( soundGameObject, clip.length );
        }
    }
    
    public static void PlaySound ( string clipName, float volume, Vector3 location, float spatialBlend = 1.0f, bool loop = false )
    {
        Initialize ();
        GameObject soundGameObject = new GameObject ( $"Audio [{clipName}]" );
        soundGameObject.transform.position = location;
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource> ();
        m_audioRegister.Add ( audioSource );
        audioSource.volume = volume;
        audioSource.outputAudioMixerGroup = m_gameAssets.GetAudioMixerGroup ( clipName );
        audioSource.loop = loop;
        audioSource.spatialBlend = spatialBlend;
        AudioClip clip = m_gameAssets.GetAudioClip ( clipName );
        audioSource.clip = clip;
        audioSource.Play ();
        if ( !loop )
        {
            Object.Destroy ( soundGameObject, clip.length );
        }
    }
    
    public static void PlaySound ( string clipName, float volume, Vector3 location, float minDistance, float maxDistance, float spatialBlend = 1.0f, bool loop = false )
    {
        Initialize ();
        GameObject soundGameObject = new GameObject ( $"Audio [{clipName}]" );
        soundGameObject.transform.position = location;
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource> ();
        m_audioRegister.Add ( audioSource );
        audioSource.volume = volume;
        audioSource.outputAudioMixerGroup = m_gameAssets.GetAudioMixerGroup ( clipName );
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.spatialBlend = spatialBlend;
        audioSource.loop = loop;
        AudioClip clip = m_gameAssets.GetAudioClip ( clipName );
        audioSource.clip = clip;
        audioSource.Play ();
        if ( !loop )
        {
            Object.Destroy ( soundGameObject, clip.length );
        }
    }

    public static void PlaySound ( string clipName, float volume, bool loop, float spatialBlend, Vector3 location )
    {
        Initialize ();
        GameObject soundGameObject = new GameObject ( $"Audio [{clipName}]" );
        soundGameObject.transform.position = location;
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource> ();
        m_audioRegister.Add ( audioSource );
        audioSource.clip = m_gameAssets.GetAudioClip ( clipName );
        audioSource.outputAudioMixerGroup = m_gameAssets.GetAudioMixerGroup ( clipName );
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.spatialBlend = spatialBlend;
        audioSource.Play ();
        if ( !loop )
        {
            Object.Destroy ( soundGameObject, audioSource.clip.length );
        }
    }

    public static void PlaySound ( AudioClip clip, AudioMixerGroup mixerGroup, float volume, bool loop )
    {
        GameObject soundGameObject = new GameObject ( $"Audio [{clip.name}]" );
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource> ();
        m_audioRegister.Add ( audioSource );
        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = mixerGroup;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.Play ();
        if ( !loop )
        {
            Object.Destroy ( soundGameObject, clip.length );
        }
    }

    /// <summary>
    /// Plays an AudioClip.
    /// </summary>
    /// <param name="clip">The AudioClip to play.</param>
    /// <param name="mixerGroup">The AudioMixerGroup used for this playback.</param>
    /// <param name="volume">Playback volume.</param>
    /// <param name="loop">Loop the AudioClip?</param>
    /// <param name="spatialBlend">0.0 makes the sound full 2D, 1.0 makes it full 3D.</param>
    /// <param name="location">Where the AudioSource is located in the scene.</param>
    public static void PlaySound ( AudioClip clip, AudioMixerGroup mixerGroup, float volume, bool loop, float spatialBlend, Vector3 location )
    {
        GameObject soundGameObject = new GameObject ( $"Audio [{clip.name}]" );
        soundGameObject.transform.position = location;
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource> ();
        m_audioRegister.Add ( audioSource );
        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = mixerGroup;
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
