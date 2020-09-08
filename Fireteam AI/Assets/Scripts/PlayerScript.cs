using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Koobando.AntiCheat;

public class PlayerScript : MonoBehaviour
{

    // Stats
    // Movement speed of player
    public EncryptedFloat speed;
    // Stamina allows you to run longer
    public EncryptedFloat stamina;
    // Armor allows you to take less damage all around
    public EncryptedFloat armor;
    // Health modifier
    public EncryptedInt health;
    // Stat multipliers
    public Stats stats;

    public const float baseSpeed = 6f;
    public const float baseStamina = 4f;
    public const float baseArmor = 1f;
    public const int baseHealth = 100;


    void Awake() {
        stats = new Stats();
        speed = baseSpeed;
        stamina = baseStamina;
        armor = baseArmor;
        health = baseHealth;
    }

    public void setSpeed()
    {
        //Debug.Log(stats.speed);
        this.speed = baseSpeed * stats.speed;
    }

    public void setStamina()
    {
        //Debug.Log(stats.stamina);
        this.stamina = baseStamina * stats.stamina;
    }

    public void setArmor()
    {
        this.armor = baseArmor * stats.armor;
    }

    public void setHealth()
    {
        this.health = baseHealth + stats.health;
    }

    public void updateStats()
    {
        setSpeed();
        setStamina();
        setArmor();
        setHealth();
    }

}
