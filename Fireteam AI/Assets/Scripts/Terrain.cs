using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terrain : MonoBehaviour
{
    public enum TerrainType {Concrete, Grass, Wood, Water, Stone, Weakice, Snow, Glass, Sand, Brush, Dirt, Deckwood, Bluntwood, Gravel, Leaves, Lino, Marble, Metalbox, Metalbar, Mud, Muffledice, Quicksand, Rug, Squeakywood};
    public TerrainType terrainType;
    public GameObject[] bulletHolePrefab;
    public int index;

    public GameObject GetRandomBulletHole()
    {
        return bulletHolePrefab[Random.Range(0, bulletHolePrefab.Length)];
    }
}
