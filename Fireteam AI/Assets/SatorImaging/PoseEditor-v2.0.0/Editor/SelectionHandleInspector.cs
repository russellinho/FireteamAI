#if UNITY_EDITOR

using UnityEditor;



namespace SatorImaging.PoseEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SelectionHandle))]
    public class SelectionHandleInspector : Editor
    {
        static Editor cachedEditor;
        static bool isManagerShown = false;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            //EditorGUILayout.HelpBox("TEST", MessageType.Info, true);


            // draw manager inspector
            CreateCachedEditor((target as SelectionHandle).manager, null, ref cachedEditor);
            try
            {
                EditorGUILayout.Space();
                isManagerShown = EditorGUILayout.Foldout(isManagerShown, "Pose Editor Properties");
                //isManagerShown = EditorGUILayout.ToggleLeft("Pose Editor Properties", isManagerShown);
                if (isManagerShown)
                {
                    //cachedEditor.DrawHeader();
                    cachedEditor.OnInspectorGUI();
                    //cachedEditor.DrawDefaultInspector();
                }
            }
            catch
            {
            }

        }


        void OnSceneGUI()
        {
            var handle = target as SelectionHandle;
            if (null == handle.manager)
            {
                //DestroyImmediate(handle);
                return;
            }

            handle.manager.DrawHandle();

        }


    }//class
}//namespace
#endif
