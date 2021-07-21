using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneController
{
    #region Models

    [System.Serializable]
    public struct SceneLoadConfiguration
    {
        [Tooltip ( "Scene configured to process and display asynchronous scene loading info data" )]
        public int LoaderScene;
        [Tooltip ( "Target scene to load" )]
        public int TargetScene;
    }

    #endregion

    /// <summary>
    /// Controller for loading and transitioning between scenes.
    /// </summary>
    public class SceneController : PersistentLazySingleton<SceneController>
    {
        public float SceneLoadProgress { get => m_sceneLoadOperation != null ? m_sceneLoadOperation.progress : 0; }

        [SerializeField]
        private SceneLoadConfiguration m_sceneLoadConfiguration;

        private int m_activeSceneIndex = 0;
        private AsyncOperation m_sceneLoadOperation = null;

        private void OnEnable ()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable ()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        #region Scene Loading

        /// <summary>
        /// Immediately transitions application to loader scene to begin loading target scene.
        /// </summary>
        public void LoadScene ()
        {
            LoadScene ( m_sceneLoadConfiguration.TargetScene );
        }

        /// <summary>
        /// Immediately transitions application to loader scene to begin loading scene with <paramref name="targetSceneIndex"/>.
        /// </summary>
        /// <param name="targetSceneIndex"></param>
        public void LoadScene ( int targetSceneIndex )
        {
            if ( m_activeSceneIndex == targetSceneIndex )
            {
                Debug.Log ( "Can't load scene. Target scene is set to the active scene." );
                return;
            }
            if ( SceneManager.GetSceneByBuildIndex ( targetSceneIndex ) == null )
            {
                Debug.LogError ( $"Failed to load scene with build index {targetSceneIndex}. Check if the scene is added to the Build Settings." );
                return;
            }

            // Store target scene index
            m_sceneLoadConfiguration.TargetScene = targetSceneIndex;

            // Load loader scene
            SceneManager.LoadSceneAsync ( m_sceneLoadConfiguration.LoaderScene );
        }

        private void OnSceneLoaded ( Scene scene, LoadSceneMode mode )
        {
            // Keep track of the active scene index
            m_activeSceneIndex = SceneManager.GetActiveScene ().buildIndex;

            if ( m_activeSceneIndex == m_sceneLoadConfiguration.LoaderScene )
            {
                // Begin loading the target scene
                m_sceneLoadOperation = SceneManager.LoadSceneAsync ( m_sceneLoadConfiguration.TargetScene );
            }
        }

        #endregion
    }

}
