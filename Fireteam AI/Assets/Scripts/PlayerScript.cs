using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{

    // Stats
    // Movement speed of player
    public float speed;
    // Stamina allows you to run longer
    public float stamina;
    // Armor allows you to take less damage all around
    public float armor;
    // Health modifier
    public int health;
    // Stat multipliers
    public Stats stats;

    public const float baseSpeed = 6f;
    public const float baseStamina = 4f;
    public const float baseArmor = 1f;
    public const int baseHealth = 100;


    void Awake() {
        stats = new Stats();
        speed = 0;
        stamina = 0;
        armor = 0;
        health = 0;
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
