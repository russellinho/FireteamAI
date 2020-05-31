using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(BetaEnemyScript))]
public class AIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BetaEnemyScript myScript = (BetaEnemyScript)target;
        if(GUILayout.Button("Kill AI"))
        {
            myScript.EditorKillAi();
        }
        // if(GUILayout.Button("Assign all nav points"))
        // {
        //     myScript.EditorAssignNavPts();
        // }
    }
}
