using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Koobando.AntiCheat;

public class Stats
{
    public EncryptedFloat speed;
    public EncryptedFloat stamina;
    public EncryptedFloat armor;
    public EncryptedFloat avoidability;
    public EncryptedInt detection;
    public EncryptedInt health;

    public Stats() {
        SetDefaults();
    }

    public void SetDefaults() {
        this.speed = 1f;
        this.stamina = 1f;
        this.armor = 1f;
        this.avoidability = 1f;
        this.detection = 1;
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

    public void setDetection(int value)
    {
        this.detection = value;
    }

    public void setAvoidability(float value)
    {
        this.avoidability = 1f + value;
    }

    public void setStats(float speed, float stamina, float armor, float avoidability, int detection, int health)
    {
        setSpeed(speed);
        setStamina(stamina);
        setArmor(armor);
        setAvoidability(avoidability);
        setDetection(detection);
        setHealth(health);
    }
}
