using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats
{
    public float speed;
    public float stamina;
    public float armor;

    public Stats() {
        this.speed = 1f;
        this.stamina = 1f;
        this.armor = 1f;
    }

    public void updateSpeed(float value)
    {
        this.speed += value;
    }

    public void updateStamina(float value)
    {
        this.stamina += value;
    }

    public void updateArmor(float value)
    {
        this.armor += value;
    }

    public void updateStats(float speed, float stamina, float armor)
    {
        updateSpeed(speed);
        updateStamina(stamina);
        updateArmor(armor);
    }
}
