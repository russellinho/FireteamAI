using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MeshFixer))]
public class MeshFixerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if(GUILayout.Button("Exact"))
        {
            MeshFixer myScript = (MeshFixer) target;
            myScript.ExactThis();
        }

        if(GUILayout.Button("Encapsulate"))
        {
            MeshFixer myScript = (MeshFixer) target;
            myScript.EncapsulateThis();
        }

    }
}
