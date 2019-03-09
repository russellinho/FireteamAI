using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPlayerScript : MonoBehaviour
{

    // Movement speed of player
    public int speed;
    // Stamina allows you to run longer
    public int stamina;
    // Armor allows you to take less damage all around
    public int armor;

    void Start() {
        speed = 0;
        stamina = 0;
        armor = 0;
    }

}
