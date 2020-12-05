using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainSounds : MonoBehaviour
{
    [SerializeField]
    public AudioClip[] jumpSounds;
    public AudioClip[] landSounds;

    public AudioClip[] concreteTerrain;
    public AudioClip[] grassTerrain;
    public AudioClip[] woodTerrain;
    public AudioClip[] waterTerrain;
    public AudioClip[] stoneTerrain;
    public AudioClip[] weakiceTerrain;
    public AudioClip[] snowTerrain;
    public AudioClip[] glassTerrain;
    public AudioClip[] sandTerrain;
    public AudioClip[] brushTerrain;
    public AudioClip[] dirtTerrain;
    public AudioClip[] deckwoodTerrain;
    public AudioClip[] bluntwoodTerrain;
    public AudioClip[] gravelTerrain;
    public AudioClip[] leavesTerrain;
    public AudioClip[] linoTerrain;
    public AudioClip[] marbleTerrain;
    public AudioClip[] metalboxTerrain;
    public AudioClip[] metalbarTerrain;
    public AudioClip[] mudTerrain;
    public AudioClip[] mufflediceTerrain;
    public AudioClip[] quicksandTerrain;
    public AudioClip[] rugTerrain;
    public AudioClip[] squeakywoodTerrain;

    public AudioClip[] GetTerrainFootsteps(int i)
    {
        switch (i) {
            case 0:
                return concreteTerrain;
            case 1:
                return grassTerrain;
            case 2:
                return woodTerrain;
            case 3:
                return waterTerrain;
            case 4:
                return stoneTerrain;
            case 5:
                return weakiceTerrain;
            case 6:
                return snowTerrain;
            case 7:
                return glassTerrain;
            case 8:
                return sandTerrain;
            case 9:
                return brushTerrain;
            case 10:
                return dirtTerrain;
            case 11:
                return deckwoodTerrain;
            case 12:
                return bluntwoodTerrain;
            case 13:
                return gravelTerrain;
            case 14:
                return leavesTerrain;
            case 15:
                return linoTerrain;
            case 16:
                return marbleTerrain;
            case 17:
                return metalboxTerrain;
            case 18:
                return metalbarTerrain;
            case 19:
                return mudTerrain;
            case 20:
                return mufflediceTerrain;
            case 21:
                return quicksandTerrain;
            case 22:
                return rugTerrain;
            case 23:
                return squeakywoodTerrain;
        }
        return null;
    }
}
