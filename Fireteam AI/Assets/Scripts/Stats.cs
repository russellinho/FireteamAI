using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats
{
    public float speed;
    public float stamina;
    public float armor;
    public int health;

    public Stats() {
        SetDefaults();
    }

    public void SetDefaults() {
        this.speed = 1f;
        this.stamina = 1f;
        this.armor = 1f;
        this.health = 0;
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

    public void updateHealth(int value)
    {
        this.health += value;
    }

    public void updateStats(float speed, float stamina, float armor, int health)
    {
        updateSpeed(speed);
        updateStamina(stamina);
        updateArmor(armor);
        updateHealth(health);

    }
}
