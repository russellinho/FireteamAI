using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshFixer : MonoBehaviour
{

    public GameObject target;
    public Transform rootBone;
// Use this for initialization
    void Start () {
        SkinnedMeshRenderer targetRenderer = target.GetComponent<SkinnedMeshRenderer>();
        Dictionary<string,Transform> boneMap = new Dictionary<string,Transform>();

        foreach(Transform bone in targetRenderer.bones) {
           // Debug.Log(bone.gameObject.name);
            boneMap[bone.gameObject.name] = bone;
        }

        SkinnedMeshRenderer myRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

        // foreach(Transform bone in myRenderer.bones) {
        //     Debug.Log(bone.gameObject.name);
        // }
        Transform[] newBones = new Transform[myRenderer.bones.Length];

        for(int i = 0; i < myRenderer.bones.Length; ++i) {
            GameObject bone = myRenderer.bones[i].gameObject;
            if(!boneMap.TryGetValue(bone.name, out newBones[i])) {
                Debug.Log("Unable to map bone \"" + bone.name + "\" to target skeleton.");
                newBones[i] = bone.transform;
            }
        }
        myRenderer.bones = newBones;
    }
}
