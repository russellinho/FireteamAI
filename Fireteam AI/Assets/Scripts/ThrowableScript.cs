using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableScript : MonoBehaviour
{

    public Rigidbody rBody;
    public SphereCollider col;

    // Start is called before the first frame update
    void Awake()
    {
        col.enabled = false;
        rBody.useGravity = false;
        rBody.isKinematic = true;
    }

    public void Launch() {
        // TODO: Fill out
    }
}
