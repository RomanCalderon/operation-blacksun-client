using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GameAssets : MonoBehaviour
{
    private static GameAssets m_instance;
    public static GameAssets Instance
    {
        get
        {
            if ( m_instance == null )
            {
                m_instance = Instantiate ( Resources.Load<GameAssets> ( "GameAssets" ) );
                return m_instance;
            }
            return m_instance;
        }
    }

    [Header ( "Audio Clips" )]
    [SerializeField]
    private AudioClipReference [] AudioClips = null;

    public AudioClip GetAudioClip ( string name )
    {
        if ( string.IsNullOrEmpty ( name ) )
        {
            return null;
        }
        return AudioClips.FirstOrDefault ( c => c.Name == name ).AudioClip;
    }

    public AudioMixerGroup GetAudioMixerGroup ( string name )
    {
        if ( string.IsNullOrEmpty ( name ) )
        {
            return null;
        }
        return AudioClips.FirstOrDefault ( c => c.Name == name ).MixerGroup;
    }
}

[System.Serializable]
public struct AudioClipReference
{
    public string Name;
    public AudioClip AudioClip;
    public AudioMixerGroup MixerGroup;
}
