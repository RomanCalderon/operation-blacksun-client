using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Knife.Effects;

[RequireComponent(typeof(ParticleGroupPlayer))]
public class DebugParticleGroupPlayer : MonoBehaviour
{
    [SerializeField]
    private bool m_loop = true;
    [SerializeField]
    private float m_interval = 1.0f;
    private ParticleGroupPlayer m_particleGroupPlayer = null;

    private void Awake ()
    {
        m_particleGroupPlayer = GetComponent<ParticleGroupPlayer> ();
    }

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        while ( m_loop )
        {
            m_particleGroupPlayer.Play ();
            yield return new WaitForSeconds ( m_interval );
        }
    }
}
