using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(WeaponCreatorScript))]
public class WeaponCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WeaponCreatorScript myScript = (WeaponCreatorScript)target;
        if(GUILayout.Button("Assign Weapon Stats"))
        {
            myScript.AssignWeaponStats();
        }
    }
}
