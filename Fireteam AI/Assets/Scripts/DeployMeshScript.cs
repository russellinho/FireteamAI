using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeployMeshScript : MonoBehaviour
{
    public GameObject collidingWithObject;
    public GameObject invalidDeployIndicator;

    void OnCollisionEnter(Collision collision) {
        collidingWithObject = collision.gameObject;
    }

    void OnCollisionExit(Collision collision) {
        collidingWithObject = null;
    }

    public void IndicateIsInvalid(bool b) {
        if (b) {
            invalidDeployIndicator.SetActive(true);
        } else {
            invalidDeployIndicator.SetActive(false);
        }
    }
}
