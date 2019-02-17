﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshFixer : MonoBehaviour
{

    public GameObject target;
    public Transform rootBone;
    
    private Dictionary<string, Transform> rootBoneMap;
// Use this for initialization
    public void AdaptMesh () {
        SkinnedMeshRenderer targetRenderer = target.GetComponent<SkinnedMeshRenderer>();
        Dictionary<string,Transform> boneMap = new Dictionary<string,Transform>();
        rootBoneMap = new Dictionary<string, Transform>();

        Transform[] r = rootBone.GetComponentsInChildren<Transform>();
        for (int i = 0; i < r.Length; i++) {
            rootBoneMap[r[i].gameObject.name] = r[i];
        }

        foreach(Transform bone in targetRenderer.bones) {
//            Debug.Log(bone.gameObject.name);
            boneMap[bone.gameObject.name] = bone;
        }

        SkinnedMeshRenderer myRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

        Transform[] newBones = new Transform[myRenderer.bones.Length];
        
        for(int i = 0; i < myRenderer.bones.Length; i++) {
            GameObject bone = myRenderer.bones[i].gameObject;
            //Debug.Log(bone.name);
            if(!boneMap.TryGetValue(bone.name, out newBones[i])) {
                //Debug.Log("Unable to map bone \"" + bone.name + "\" to target skeleton.");
                newBones[i] = rootBoneMap[bone.name];
            }
        }
        myRenderer.bones = newBones;
    }

}
