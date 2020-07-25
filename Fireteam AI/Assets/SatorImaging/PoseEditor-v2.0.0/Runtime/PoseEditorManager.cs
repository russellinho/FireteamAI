#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



namespace SatorImaging.PoseEditor
{
    public class PoseEditorManager : MonoBehaviour
    {
        [Range(0f, 1000f)] public float worldSize = 1.0f;
        [Range(0f, 0.1f)] public float handleSize = 0.0125f;

        [Space()]
        public bool drawSkeleton = true;
        public Color handleColor = Color.white;
        public Color inactiveHandleColor = new Color(0.0f, 0.32f, 0.0f, 1.0f); //new Color(0.125f, 0.125f, 0.125f, 0.5f);
        public bool multiColor = true;
        [Range(1f, 36f)] public int multiColorVariation = 6;
        [Range(0f, 1f)] public float multiColorSaturation = 1.0f;
        [Range(0f, 1f)] public float multiColorBrightness = 1.0f;

        [Header("Joint/Skeleton Name Filter")]
        public bool caseSensitive;
        public string nameFilter;

        [Space]
        public bool drawLabel = true;
        [Range(0f, 256f)] public int labelFontSize = 16;
        public Color labelColor = new Color(0.0f, 0.64f, 0.0f, 1.0f);
        public Color inactiveLabelColor = new Color(0.0f, 0.32f, 0.0f, 1.0f);
        public bool inheritHandleColor = true;

        //[Space]
        public GUIStyle labelStyle;


        Transform[] bones;
        Transform[][] childBones;





        [MenuItem("GameObject/Add Pose Editor", priority = 39)]
        [MenuItem("Component/Add Pose Editor", priority = 9999)]
        static void AddPoseEditor()
        {
            foreach (var s in Selection.GetTransforms(SelectionMode.Unfiltered))
            {
                if (s.GetComponent<PoseEditorManager>())
                {
                    s.GetComponent<PoseEditorManager>().OnValidate();
                    //s.GetComponent<PoseEditorManager>().FindBones();
                    continue;
                }

                s.gameObject.AddComponent<PoseEditorManager>();
            }

        }






        public void RemovePoseEditor()
        {
            foreach (var b in bones)
            {
                var handles = b.GetComponentsInChildren<SelectionHandle>();
                foreach (var h in handles)
                {
                    DestroyImmediate(h);
                }
            }
            DestroyImmediate(this);
        }


        public void FindBones()
        {
            var meshes = GetComponentsInChildren<SkinnedMeshRenderer>();

            System.StringComparison compType =
                caseSensitive ? System.StringComparison.CurrentCulture : System.StringComparison.CurrentCultureIgnoreCase;

            var result = new List<Transform>();
            // first, find objects already has handle
            foreach (var o in GetComponentsInChildren<SelectionHandle>())
            {
                var t = o.transform;
                // ignore duplicate
                if (result.Contains(t))
                {
                    continue;
                }
                // TODO: remove duplicates, naming filter, skip if not match
                if (!string.IsNullOrEmpty(nameFilter) && -1 == t.name.IndexOf(nameFilter, compType))
                {
                    continue;
                }
                result.Add(t);
            }
            // second, add skinned mesh bones.
            foreach (var m in meshes)
            {
                foreach (var b in m.bones)
                {
                    // ignore duplicate
                    if (result.Contains(b))
                    {
                        continue;
                    }
                    // TODO: remove duplicates, naming filter, skip if not match
                    if (!string.IsNullOrEmpty(nameFilter) && -1 == b.name.IndexOf(nameFilter, compType))
                    {
                        continue;
                    }

                    result.Add(b);
                }
            }
            // add handle to bones
            var hiddenBones = new List<Transform>();
            foreach (var b in result)
            {
                var handle = b.GetComponent<SelectionHandle>();
                if (null == handle)
                {
                    handle = b.gameObject.AddComponent<SelectionHandle>();
                }
                handle.manager = this;

                // hide children
                if (handle.hideChildren)
                {
                    var children = handle.GetComponentsInChildren<SelectionHandle>(true);
                    // 0 is itself
                    Debug.Log("I'm " + handle.name + " and index 0 is: " + children[0]);
                    for (var cidx = 1; cidx < children.Length; cidx++)
                    {
                        if (hiddenBones.Contains(children[cidx].transform))
                        {
                            continue;
                        }
                        hiddenBones.Add(children[cidx].transform);
                    }
                }
            }
            result.RemoveAll(b => hiddenBones.Contains(b));
            bones = result.ToArray();



            // find child bones
            childBones = new Transform[bones.Length][];
            for (var i = 0; i < bones.Length; i++)
            {
                // skip if hide children
                if (bones[i].GetComponent<SelectionHandle>().hideChildren)
                {
                    continue;
                }

                var children = bones[i].GetComponentsInChildren<SelectionHandle>(true);
                var found = new List<Transform>();
                foreach (var c in children)
                {
                    if (c.transform == bones[i])
                    {
                        continue;
                    }

                    if (c.transform.parent == bones[i])
                    {
                        found.Add(c.transform);
                    }
                }
                if (0 == found.Count)
                {
                    childBones[i] = null;
                }
                else
                {
                    childBones[i] = found.ToArray();
                }

            }



        }





        void OnValidate()
        {
            FindBones();

        }


        //void Reset()
        //{
        //    FindBones();
        //}









        void OnDrawGizmos()
        {
            DrawHandle(false);
        }



        public void DrawHandle(bool colorize = true)
        {
            // build gui style first time.
            if (null == labelStyle || string.IsNullOrEmpty(labelStyle.name))
            {
                labelStyle = new GUIStyle(EditorStyles.largeLabel)
                {
                    alignment = TextAnchor.LowerCenter,
                    clipping = TextClipping.Overflow,
                    fixedWidth = 1,
                    fixedHeight = 1,
                    contentOffset = new Vector2(5.5f, 0),
                    fontSize = 10, // must be initialize with non-zero value.
                };

            }
            labelStyle.fontSize = labelFontSize;
            labelStyle.normal.textColor = colorize ? labelColor : inactiveLabelColor;




            Handles.color = colorize ? handleColor : inactiveHandleColor;

            var colorIndex = 0;
            for (var i = 0; i < bones.Length; i++)
            {
                if (null == bones[i] || !bones[i].gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (colorize && multiColor && (0 == i || 1 < bones[i].parent.childCount))
                {
                    Handles.color = Color.HSVToRGB(colorIndex * (1f / multiColorVariation) % 1f, multiColorSaturation, multiColorBrightness);
                    colorIndex++;
                }



                var bonePosition = bones[i].position;

                // draw skeleton
                if (null != childBones[i])
                {
                    for (var c = 0; c < childBones[i].Length; c++)
                    {
                        if (null == childBones[i][c])
                        {
                            continue;
                        }

                        if (drawSkeleton)
                        {
                            Handles.DrawLine(bonePosition, childBones[i][c].position);
                        }

                        //Handles.Slider(bones[i].position, childBones[i][c].position - bones[i].position);
                    }
                }

                // draw label
                if (drawLabel)
                {
                    var h = bones[i].GetComponent<SelectionHandle>();
                    if (null != h && !string.IsNullOrEmpty(h.label))
                    {
                        if (colorize && inheritHandleColor)
                        {
                            labelStyle.normal.textColor = Handles.color;
                        }
                        Handles.Label(bonePosition, h.label, labelStyle);
                    }
                }




                // ignore while navigating scene view.
                if (Event.current.alt && Event.current.isMouse)
                {
                    continue;
                }


                EditorGUI.BeginChangeCheck();
                Handles.FreeMoveHandle(bones[i].position, bones[i].rotation, handleSize * worldSize, Vector3.zero, Handles.SphereHandleCap);
                //Handles.FreeRotateHandle(bones[i].rotation, bones[i].position, handleSize);
                //Handles.RotationHandle(bones[i].rotation, bones[i].position);


                if (EditorGUI.EndChangeCheck())
                {
                    if (Event.current.shift)
                    {
                        var sel = new Object[Selection.objects.Length + 1];
                        sel[0] = bones[i].gameObject;
                        for (var s = 1; s < sel.Length; s++)
                        {
                            sel[s] = Selection.objects[s - 1];
                        }
                        Selection.activeTransform = bones[i];
                        Selection.objects = sel;
                    }
                    else
                    {
                        Selection.activeTransform = bones[i];
                        //EditorGUIUtility.PingObject(b);
                    }
                }

            }

        }



    }//class
}//namespace
#endif
