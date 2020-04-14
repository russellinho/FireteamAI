using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(WeaponReloadCreatorScript))]
public class WeaponReloadCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WeaponReloadCreatorScript myScript = (WeaponReloadCreatorScript)target;
        if(GUILayout.Button("Record Frame"))
        {
            myScript.RecordPosition();
        }
    }
}
