using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(DatabaseUpdater))]
public class DatabaseUpdaterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DatabaseUpdater myScript = (DatabaseUpdater)target;
        if(GUILayout.Button("Add Item To All Accounts"))
        {
            myScript.AddItemToAllAccounts();
        }
        if (GUILayout.Button("Add Melee Category To All Accounts")) {
            myScript.AddEquippedMeleeFieldToAllAccounts();
        }
    }
}
