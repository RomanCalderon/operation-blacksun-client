using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using InventorySystem.PlayerItems;

public class PlayerItemExporterEditor : EditorWindow
{
    #region Models

    [System.Serializable]
    public struct PlayerItemData
    {
        [HideInInspector]
        public string PlayerItemId;
        public GameObject Source { get => m_source; }
        private GameObject m_source;

        public PlayerItemData ( string playerItemId, GameObject source )
        {
            PlayerItemId = playerItemId;
            m_source = source;
        }
    }

    #endregion

    private const int MIN_WINDOW_SIZE_X = 540;
    private const int MIN_WINDOW_SIZE_Y = 360;

    private const int GUI_PADDING = 5;

    private const int STAGE_BUTTON_HEIGHT = 30;

    [SerializeField]
    public PlayerItem m_newPlayerItem = null;
    [SerializeField]
    public GameObject m_newSource = null;
    [SerializeField]
    public List<PlayerItemData> m_stagedPlayerItems = null;

    private bool m_displayErrorNewPlayerItemData = false;

    #region Initialization

    [MenuItem ( "Window/SDG/Player Item Exporter" )]
    public static void ShowWindow ()
    {
        EditorWindow window = GetWindow ( typeof ( PlayerItemExporterEditor ), true, "Player Item Exporter" );
        window.minSize = new Vector2 ( MIN_WINDOW_SIZE_X, MIN_WINDOW_SIZE_Y );
        window.Repaint ();
    }

    private void Awake ()
    {
        m_stagedPlayerItems = new List<PlayerItemData> ();
    }

    #endregion

    private void OnGUI ()
    {
        ScriptableObject scriptableObj = this;
        SerializedObject serializedObject = new SerializedObject ( scriptableObj );

        // Staged PlayerItemData list
        SerializedProperty stagedPlayerItems = serializedObject.FindProperty ( "m_stagedPlayerItems" );

        Rect screenRect = new Rect ( GUI_PADDING, GUI_PADDING, position.width - GUI_PADDING * 2, position.height - GUI_PADDING * 2 );
        GUILayout.BeginArea ( screenRect );

        #region GUI

        #region New PlayerItem Data

        GUILayout.BeginVertical ( "New Player Item", "window" );

        // Help box
        EditorGUILayout.HelpBox ( "Add your PlayerItem and related GameObject assets then click \"Stage\" to stage it.", MessageType.Info, true );
        EditorGUILayout.Separator ();

        m_newPlayerItem = ( PlayerItem ) EditorGUILayout.ObjectField ( "Player Item", m_newPlayerItem, typeof ( PlayerItem ), false );
        m_newSource = ( GameObject ) EditorGUILayout.ObjectField ( "Source Object", m_newSource, typeof ( GameObject ), false );

        // Error message
        if ( m_displayErrorNewPlayerItemData )
        {
            EditorGUILayout.HelpBox ( "PlayerItem or Source is null. Please add the required Objects then press \"Stage\".", MessageType.Error, true );
        }

        EditorGUILayout.Separator ();
        if ( GUILayout.Button ( "Stage", GUILayout.Height ( STAGE_BUTTON_HEIGHT ) ) )
        {
            serializedObject.ApplyModifiedProperties ();

            // Null check
            if ( m_newPlayerItem == null || m_newSource == null )
            {
                m_displayErrorNewPlayerItemData = true;
                return;
            }
            m_displayErrorNewPlayerItemData = false;

            // Add new PlayerItem data to stage list
            m_stagedPlayerItems.Add ( new PlayerItemData ( m_newPlayerItem.Id, m_newSource ) );
            serializedObject.ApplyModifiedProperties ();

            // Clear new object fields
            m_newPlayerItem = null;
            m_newSource = null;
        }

        GUILayout.EndVertical ();

        #endregion

        EditorGUILayout.Separator ();

        #region Staged PlayerItem Data

        GUILayout.BeginVertical ( "Staged | Player Item Data", "window" );

        GUIContent propertyLabel = new GUIContent ( "Player Items (Staged)" );
        EditorGUILayout.PropertyField ( stagedPlayerItems, propertyLabel, true );
        serializedObject.ApplyModifiedProperties ();

        GUILayout.EndVertical ();

        #endregion

        EditorGUILayout.Separator ();

        if ( GUILayout.Button ( "debug show data" ) )
        {
            foreach ( PlayerItemData playerItemData in m_stagedPlayerItems )
            {
                PrintData ( playerItemData );
            }
        }

        GUILayout.FlexibleSpace ();

        #endregion

        GUILayout.EndArea ();
    }

    private void PrintData ( PlayerItemData playerItemData )
    {
        if ( playerItemData.Source == null )
        {
            Debug.LogError ( $"PlayerItemData [{playerItemData.PlayerItemId}] missing Source Object reference." );
            return;
        }
        Debug.Log ( $"PlayerItem [{playerItemData.PlayerItemId}] - Bounds [{GetColliderBounds ( playerItemData.Source ):N2}]" );
    }

    private Bounds GetColliderBounds ( GameObject assetModel )
    {
        GameObject assetInstance = Instantiate ( assetModel, Vector3.zero, Quaternion.identity );

        // Need to clear out transforms while encapsulating bounds
        //assetInstance.transform.localScale = Vector3.one;

        // Start with root object's bounds
        Bounds bounds = new Bounds ( Vector3.zero, Vector3.zero );
        if ( assetInstance.transform.TryGetComponent<Renderer> ( out var mainRenderer ) )
        {
            // New Bounds() will include 0,0,0 which you may not want to Encapsulate
            // because the vertices of the mesh may be way off the model's origin,
            // so instead start with the first renderer bounds and Encapsulate from there
            bounds = mainRenderer.bounds;
        }

        Transform [] descendants = assetInstance.GetComponentsInChildren<Transform> ();
        foreach ( Transform desc in descendants )
        {
            if ( desc.TryGetComponent<Renderer> ( out var childRenderer ) )
            {
                if ( bounds.extents == Vector3.zero )
                    bounds = childRenderer.bounds;
                bounds.Encapsulate ( childRenderer.bounds );
            }
        }

        // DEBUG
        // Apply bounds to box collider
        //BoxCollider collider = assetInstance.AddComponent<BoxCollider> ();
        //collider.center = bounds.center - assetModel.transform.position;
        //collider.size = bounds.size;

        // Destroy instance
        DestroyImmediate ( assetInstance );

        return bounds;
    }
}
