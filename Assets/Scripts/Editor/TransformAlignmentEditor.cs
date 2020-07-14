using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TransformAlignmentUtil))]
public class TransformAlignmentEditor : Editor
{
    public override void OnInspectorGUI ()
    {
        base.OnInspectorGUI ();

        var myScript = target as TransformAlignmentUtil;

        if ( GUILayout.Button ( "Align Position" ) )
        {
            myScript.AlignPosition ();
        }
        if ( GUILayout.Button ( "Align Rotation" ) )
        {
            myScript.AlignRotation ();
        }
        if ( GUILayout.Button ( "Align Scale" ) )
        {
            myScript.AlignScale ();
        }
    }
}
