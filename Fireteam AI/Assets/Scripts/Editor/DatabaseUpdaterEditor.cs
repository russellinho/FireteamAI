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
        // if (GUILayout.Button("Add Melee Category To All Accounts")) {
        //     myScript.AddEquippedMeleeFieldToAllAccounts();
        // }
        // if (GUILayout.Button("Add Default Wep Category To All Accounts")) {
        //     myScript.AddDefaultWeaponFieldToAllAccounts();
        // }
        // if (GUILayout.Button("Add Logged In Field to all accounts")) {
        //     myScript.AddLoggedInFieldToAllAccounts();
        // }
        // if (GUILayout.Button("Add KASH")) {
        //     myScript.AddKashFieldToAllAccounts();
        // }
    }
}
