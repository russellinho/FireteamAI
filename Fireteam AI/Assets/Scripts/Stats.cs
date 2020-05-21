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

    public void setSpeed(float value)
    {
        this.speed = 1f + value;
    }

    public void setStamina(float value)
    {
        this.stamina = 1f + value;
    }

    public void setArmor(float value)
    {
        this.armor = 1f + value;
    }

    public void setHealth(int value)
    {
        this.health = value;
    }

    public void setStats(float speed, float stamina, float armor, int health)
    {
        setSpeed(speed);
        setStamina(stamina);
        setArmor(armor);
        setHealth(health);

    }
}
