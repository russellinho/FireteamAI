using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GameControllerScript))]
public class GameControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // if(GUILayout.Button("Assign All"))
        // {
        //     GameControllerScript myScript = (GameControllerScript) target;
        //     Terrain[] lights = (Terrain[]) GameObject.FindObjectsOfType (typeof(Terrain));
        //     myScript.terrainMetaData = new Terrain[lights.Length];
        //     for (int i = 0; i < lights.Length; i++) {
        //         lights[i].index = i;
        //         myScript.terrainMetaData[i] = lights[i];
        //     }
        // }

    }
}
