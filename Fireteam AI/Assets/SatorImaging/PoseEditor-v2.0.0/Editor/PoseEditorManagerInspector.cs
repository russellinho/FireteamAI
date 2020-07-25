#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;



namespace SatorImaging.PoseEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PoseEditorManager))]
    public class PoseEditorManagerInspector : Editor
    {
        static bool isParameterShown = true;


        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.HelpBox("Mouse drag on sphere is required to select manipulator. Shift+Click to add selection.", MessageType.Info);
                if(GUILayout.Button("Remove Pose Editor\n& Handles Associated", GUILayout.MinWidth(144f), GUILayout.MinHeight(38f)))
                {
                    (target as PoseEditorManager).RemovePoseEditor();
                    // need to return here, to avoid error in the following code.
                    return;
                }
            }
            EditorGUILayout.EndHorizontal();

            //isParameterShown = EditorGUILayout.Foldout(isParameterShown, "Show Properties");
            isParameterShown = EditorGUILayout.ToggleLeft("Show Properties", isParameterShown);


            if (isParameterShown)
            {
                base.OnInspectorGUI();
            }
        }


        // draw handle while inactive
        void OnSceneGUI()
        {
            (target as PoseEditorManager).DrawHandle();

        }




    }//class
}//namespace
#endif
