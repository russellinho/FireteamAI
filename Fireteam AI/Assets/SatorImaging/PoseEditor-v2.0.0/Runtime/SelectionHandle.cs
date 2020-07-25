#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



namespace SatorImaging.PoseEditor
{
    public class SelectionHandle : MonoBehaviour
    {
        public PoseEditorManager manager;
        public bool hideChildren = false;
        //[Space]
        [Multiline]
        public string label = "";





        [MenuItem("GameObject/Add Annotation", priority = 39)]
        [MenuItem("Component/Add Annotation", priority = 9999)]
        static void AddSelectionHandle()
        {
            foreach (var s in Selection.GetTransforms(SelectionMode.Unfiltered))
            {
                if (s.GetComponent<SelectionHandle>())
                {
                    s.GetComponent<SelectionHandle>().OnValidate();
                    continue;
                }

                var c = s.gameObject.AddComponent<SelectionHandle>();
                c.label = "Annotation";
                c.OnValidate();

                // create manager
                if (null == c.manager)
                {
                    c.manager = c.transform.root.GetComponent<PoseEditorManager>();
                    if (null == c.manager)
                    {
                        c.manager = c.transform.root.gameObject.AddComponent<PoseEditorManager>();
                    }

                }
            }

        }






        void OnValidate()
        {
            label = label.Trim();
            if (!string.IsNullOrEmpty(label))
            {
                label = label.TrimEnd(new char[] { '\n', ' ', '|' }) + "\n|";
            }

            if (manager)
            {
                manager.FindBones();
            }
        }





    }//class
}//namespace
#endif
