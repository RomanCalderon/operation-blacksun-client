using Bearroll.UltimateDecals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Knife.Effects
{
    /// <summary>
    /// Simple decal component.
    /// </summary>
    public class SimpleDecal : MonoBehaviour, IDecal, IPooledObject
    {
        /// <summary>
        /// Determines if decal can be rotated or not.
        /// </summary>
        public bool CanRotate
        {
            get
            {
                return canRotate;
            }
        }
        /// <summary>
        /// Determines decal can be rotated or not.
        /// </summary>
        [Tooltip ( "Determines if decal can be rotated or not." ), SerializeField]
        private bool canRotate = true;
        [SerializeField]
        private UltimateDecal m_ultimateDecal = null;
        [SerializeField]
        private int m_atlasLength = 4;

        [SerializeField]
        private ParticleSystem [] m_particleSystems = null;

        public void OnObjectSpawn ()
        {
            foreach ( ParticleSystem particleSystem in m_particleSystems )
            {
                particleSystem.Stop ();
                particleSystem.Play ();
            }

            if ( canRotate )
            {
                transform.Rotate ( Vector3.forward, Random.value * 360, Space.Self );
            }

            if ( m_ultimateDecal != null )
            {
                m_ultimateDecal.atlasIndex = Random.Range ( 0, m_atlasLength );
            }
        }

        private void Start ()
        {
            OnObjectSpawn ();
        }
    }

    public interface IDecal
    {
        bool CanRotate { get; }
    }
}