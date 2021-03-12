using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent ( typeof ( PlayerMovementController ) )]
public class FootstepController : MonoBehaviour
{
    #region Models

    [System.Serializable]
    public struct TerrainData
    {
        public string Label;
        public int TextureIndex;
        public AudioClip [] AudioClips;
    }

    [System.Serializable]
    public struct GroundData
    {
        public string GroundTag;
        public AudioClip [] AudioClips;
    }

    #endregion

    #region Constants

    private const float FOOTSTEP_VOLUME_MULTIPLIER = 0.01f;
    private const string UNTAGGED_COLLIDER = "Untagged";

    #endregion

    #region Members

    private PlayerMovementController m_playerMovementController = null;
    private TerrainDetector terrainDetector = null;

    [SerializeField]
    private List<TerrainData> m_terrainData;
    [SerializeField]
    private List<GroundData> m_groundData;
    [Space]
    [SerializeField]
    private AudioMixerGroup m_audioMixerGroup;
    [SerializeField]
    private AudioClip [] m_defaultStepSounds;

    [SerializeField]
    private float m_strideLengthMultiplier = 1.0f;
    private Vector3 m_previousStepPosition;
    private bool m_previousGroundedState = false;

    #endregion

    private void Awake ()
    {
        m_playerMovementController = GetComponent<PlayerMovementController> ();
        terrainDetector = new TerrainDetector ();
    }

    private void Start ()
    {
        m_previousStepPosition = transform.position;
        m_previousGroundedState = m_playerMovementController.IsGrounded;
    }

    private void FixedUpdate ()
    {
        if ( m_playerMovementController.IsGrounded )
        {
            if ( m_previousGroundedState != m_playerMovementController.IsGrounded )
            {
                m_previousGroundedState = m_playerMovementController.IsGrounded;
                m_previousStepPosition = transform.position;

                // Play ground impact sound
                float stepVolume = 1f;
                Step ( stepVolume );
            }

            float playerSpeed = m_playerMovementController.Velocity.magnitude;
            if ( Vector3.Magnitude ( transform.position - m_previousStepPosition ) > Mathf.Sqrt ( playerSpeed * m_strideLengthMultiplier ) )
            {
                // Update previous step position
                m_previousStepPosition = transform.position;

                // Play footstep sound
                float stepVolume = Mathf.Sqrt ( playerSpeed * FOOTSTEP_VOLUME_MULTIPLIER );
                Step ( stepVolume );
            }
        }
        else if ( m_previousGroundedState != m_playerMovementController.IsGrounded )
        {
            m_previousGroundedState = m_playerMovementController.IsGrounded;

            // Play jump sound
            float stepVolume = 0.4f;
            Step ( stepVolume );
        }
    }

    private void Step ( float volume )
    {
        Collider groundCollider = m_playerMovementController.GetGroundCollision ();
        if ( groundCollider == null ) return;
        AudioClip clip = ( groundCollider is TerrainCollider ) ? GetRandomClipTerrain () : GetRandomClipGround ( groundCollider.tag );
        if ( clip == null ) clip = m_defaultStepSounds [ Random.Range ( 0, m_defaultStepSounds.Length ) ];
        AudioManager.PlaySound ( clip, m_audioMixerGroup, volume, false );
    }

    private AudioClip GetRandomClipGround ( string colliderTag )
    {
        if ( colliderTag == UNTAGGED_COLLIDER ) return null;
        GroundData data = m_groundData.FirstOrDefault ( gd => gd.GroundTag.Equals ( colliderTag ) );
        if ( data.AudioClips == null || data.AudioClips.Length == 0 ) return null;
        return data.AudioClips [ Random.Range ( 0, data.AudioClips.Length ) ];
    }

    private AudioClip GetRandomClipTerrain ()
    {
        int terrainTextureIndex = terrainDetector.GetActiveTerrainTextureIdx ( transform.position );
        TerrainData data = m_terrainData.FirstOrDefault ( fd => fd.TextureIndex == terrainTextureIndex );
        if ( data.AudioClips == null || data.AudioClips.Length == 0 ) return null;
        return data.AudioClips [ Random.Range ( 0, data.AudioClips.Length ) ];
    }
}
