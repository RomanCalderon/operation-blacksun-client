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
        /// Determines decal can be rotated or not.
        /// </summary>
        [Tooltip ( "Determines decal can be rotated or not" )] [SerializeField] private bool canRotate;

        /// <summary>
        /// Determines decal can be rotated or not.
        /// </summary>
        public bool CanRotate
        {
            get
            {
                return canRotate;
            }
        }

        [SerializeField]
        private MeshRenderer m_decalRenderer = null;
        [SerializeField]
        private ParticleSystem [] m_particleSystems = null;

        public void OnObjectSpawn ()
        {
            if ( m_decalRenderer )
            {
                float xOffset = Mathf.RoundToInt ( Random.value ) + 0.5f;
                float yOffset = Mathf.RoundToInt ( Random.value ) + 0.5f;
                m_decalRenderer.material.SetTextureOffset ( "_BaseMap", new Vector2 ( xOffset, yOffset ) );
                m_decalRenderer.material.SetTextureScale ( "_BaseMap", Vector2.one * 0.5f );
            }

            foreach ( ParticleSystem particleSystem in m_particleSystems )
            {
                particleSystem.Stop ();
                particleSystem.Play ();
            }

            if ( canRotate )
            {
                transform.Rotate ( Vector3.forward, Random.value * 360, Space.Self );
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