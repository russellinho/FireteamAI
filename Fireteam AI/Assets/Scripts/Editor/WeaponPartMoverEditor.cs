using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(WeaponPartMover))]
public class WeaponPartMoverEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WeaponPartMover myScript = (WeaponPartMover)target;
        if(GUILayout.Button("Translate Parts"))
        {
            myScript.TranslateParts();
        }
    }
}
