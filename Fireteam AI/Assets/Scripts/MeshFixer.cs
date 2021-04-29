using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshFixer : MonoBehaviour
{
    public Transform meshTransform;
    public GameObject target;
    public Transform rootBone;
    
    private Dictionary<string, Transform> rootBoneMap;
// Use this for initialization
    public void AdaptMesh (GameObject objectCarrying = null) {
        SkinnedMeshRenderer targetRenderer = target.GetComponent<SkinnedMeshRenderer>();
        Dictionary<string,Transform> boneMap = new Dictionary<string,Transform>();
        rootBoneMap = new Dictionary<string, Transform>();
        
        if (objectCarrying != null) {
            objectCarrying.SetActive(false);
        }
        Transform[] r = rootBone.GetComponentsInChildren<Transform>();
        for (int i = 0; i < r.Length; i++) {
            rootBoneMap[r[i].gameObject.name] = r[i];
        }
        if (objectCarrying != null) {
            objectCarrying.SetActive(true);
        }

        foreach(Transform bone in targetRenderer.bones) {
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
        RecalculateBounds();
    }

    void RecalculateBounds() {
        meshTransform.localPosition = Vector3.zero;
        meshTransform.localRotation = Quaternion.identity;
    }

}
