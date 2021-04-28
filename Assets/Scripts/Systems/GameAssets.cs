using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using InventorySystem.PlayerItems;

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

    [Header ( "Player Items" )]
    [SerializeField]
    private PlayerItemData [] m_playerItems = null;


    #region Audio

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

    #endregion

    #region Player Items

    public PlayerItem GetPlayerItem ( string itemId )
    {
        if ( string.IsNullOrEmpty ( itemId ) )
        {
            return null;
        }
        return m_playerItems.FirstOrDefault ( i => i.PlayerItem.Id == itemId ).PlayerItem;
    }

    public GameObject GetPlayerItemObject ( string itemId )
    {
        if ( string.IsNullOrEmpty ( itemId ) )
        {
            return null;
        }
        return m_playerItems.FirstOrDefault ( i => i.PlayerItem.Id == itemId ).Object;
    }

    #endregion

    #region Util

    private void OnValidate ()
    {
        for ( int i = 0; i < m_playerItems.Length; i++ )
        {
            m_playerItems [ i ].Name = m_playerItems [ i ].PlayerItem.ToString ();
        }
    }

    #endregion
}

#region Models

[System.Serializable]
public struct AudioClipReference
{
    public string Name;
    public AudioClip AudioClip;
    public AudioMixerGroup MixerGroup;
}

[System.Serializable]
public struct PlayerItemData
{
    public string Name;
    public PlayerItem PlayerItem;
    public GameObject Object;
}

#endregion

