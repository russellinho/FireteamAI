using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terrain : MonoBehaviour
{
    public AudioClip footstepSounds;
    public GameObject[] bulletHolePrefab;
    public int index;

    public GameObject GetRandomBulletHole()
    {
        return bulletHolePrefab[Random.Range(0, bulletHolePrefab.Length)];
    }
}
