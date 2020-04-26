using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeployMeshScript : MonoBehaviour
{
    public GameObject collidingWithObject;
    public MeshRenderer[] rends;
    public Material validMat;
    public Material invalidMat;
    private bool isValid;

    void OnCollisionEnter(Collision collision) {
        collidingWithObject = collision.gameObject;
    }

    void OnCollisionExit(Collision collision) {
        collidingWithObject = null;
    }

    public void IndicateIsInvalid(bool b) {
        if (b) {
            if (!isValid) {
                isValid = true;
                for (int i = 0; i < rends.Length; i++) {
                    rends[i].material = validMat;
                }
            }
        } else {
            if (isValid) {
                isValid = false;
                for (int i = 0; i < rends.Length; i++) {
                    rends[i].material = invalidMat;
                }
            }
        }
    }
}
