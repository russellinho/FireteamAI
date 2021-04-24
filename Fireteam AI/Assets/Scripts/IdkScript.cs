using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdkScript : MonoBehaviour
{
    void OnCollisionEnter(Collision collision) {
        Debug.LogError(gameObject.name + " collide w/ " + collision.gameObject.name + " | layer: " + collision.gameObject.layer);
    }
}
