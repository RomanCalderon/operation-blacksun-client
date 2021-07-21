using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controller for loading and transitioning between scenes.
/// </summary>
[RequireComponent ( typeof ( SceneControllerView ) )]
public class SceneController : PersistentLazySingleton<SceneController>
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

    public float SceneLoadProgress { get => m_sceneLoadOperation != null ? m_sceneLoadOperation.progress : 0; }

    private SceneControllerView m_view = null;

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

    protected override void Awake ()
    {
        base.Awake ();

        DontDestroyOnLoad ( gameObject );

        m_view = GetComponent<SceneControllerView> ();
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

        // Start fade in
        m_view.FadeIn ( TransitionToLoader );
    }

    private void TransitionToLoader ()
    {
        // Transition to loader scene
        SceneManager.LoadSceneAsync ( m_sceneLoadConfiguration.LoaderScene );
    }

    private void OnSceneLoaded ( Scene scene, LoadSceneMode mode )
    {
        m_view.ShowFade ( false );

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
