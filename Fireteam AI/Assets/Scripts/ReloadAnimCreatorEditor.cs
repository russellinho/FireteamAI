using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ReloadAnimCreator))]
public class ReloadAnimCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ReloadAnimCreator myScript = (ReloadAnimCreator)target;
        if(GUILayout.Button("Record Diff"))
        {
            myScript.DetermineDifference();
        }
    }
}