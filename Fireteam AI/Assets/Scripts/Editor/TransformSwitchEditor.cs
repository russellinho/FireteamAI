using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TransformSwitchScript))]
public class TransformSwitchEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TransformSwitchScript myScript = (TransformSwitchScript)target;
        if(GUILayout.Button("Switch Transform"))
        {
            myScript.SwitchTransform();
        }
    }
}