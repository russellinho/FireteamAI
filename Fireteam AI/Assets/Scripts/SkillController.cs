﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Koobando.AntiCheat;

public class SkillController : MonoBehaviour
{
    public static float BOOSTER_LVL1_EFFECT = 0.05f;
    public static float BOOSTER_LVL2_EFFECT = 0.1f;
    public static float BOOSTER_LVL3_EFFECT = 0.2f;
    private EncryptedFloat speedBoost;
    private EncryptedFloat damageBoost;
    public EncryptedFloat recoilBoost;
    public EncryptedFloat accuracyBoost;
    public EncryptedFloat throwForceBoost;
    public EncryptedFloat deploymentTimeBoost;
    private float munitionsEngineeringTimer;

    // Collective boosts
    private EncryptedInt hackerBoost;
    private EncryptedInt storedHackerBoost;

    void Update()
    {
        MunitionsEngineeringRefresh();
        if (Input.GetKeyDown(KeyCode.H)) {
            Debug.LogError("Boost: " + GetHackerBoost() + " | This stored: " + GetThisPlayerHackerBoost());
        }
    }

    public void InitializePassiveSkills(int weaponCategory)
    {
        // Assault Rifle Mastery (00)
        if (weaponCategory == 0) {
            deploymentTimeBoost = 0f;
            throwForceBoost = 0f;
            SkillData s = PlayerData.playerdata.skillList["0/0"];
            if (s.Level == 1) {
                accuracyBoost = 0.1f;
                recoilBoost = 0.05f;
            } else if (s.Level == 2) {
                accuracyBoost = 0.2f;
                recoilBoost = 0.1f;
            } else if (s.Level == 3) {
                accuracyBoost = 0.4f;
                recoilBoost = 0.2f;
            }
        } else if (weaponCategory == 1) {
            deploymentTimeBoost = 0f;
            throwForceBoost = 0f;
            // SMG Mastery (10)
            SkillData s = PlayerData.playerdata.skillList["1/0"];
            if (s.Level == 1) {
                accuracyBoost = 0.1f;
                recoilBoost = 0.05f;
            } else if (s.Level == 2) {
                accuracyBoost = 0.2f;
                recoilBoost = 0.1f;
            } else if (s.Level == 3) {
                accuracyBoost = 0.4f;
                recoilBoost = 0.2f;
            }
        } else if (weaponCategory == 2) {
            deploymentTimeBoost = 0f;
            throwForceBoost = 0f;
            // LMG Mastery (6/0)
            SkillData s = PlayerData.playerdata.skillList["6/0"];
            if (s.Level == 1) {
                accuracyBoost = 0.1f;
                recoilBoost = 0.05f;
            } else if (s.Level == 2) {
                accuracyBoost = 0.2f;
                recoilBoost = 0.1f;
            } else if (s.Level == 3) {
                accuracyBoost = 0.4f;
                recoilBoost = 0.2f;
            }
        } else if (weaponCategory == 3) {
            deploymentTimeBoost = 0f;
            throwForceBoost = 0f;
            // Shotgun Mastery (1/1)
            SkillData s = PlayerData.playerdata.skillList["1/1"];
            if (s.Level == 1) {
                accuracyBoost = 0.1f;
                recoilBoost = 0.05f;
            } else if (s.Level == 2) {
                accuracyBoost = 0.2f;
                recoilBoost = 0.1f;
            } else if (s.Level == 3) {
                accuracyBoost = 0.4f;
                recoilBoost = 0.2f;
            }
        } else if (weaponCategory == 4) {
            deploymentTimeBoost = 0f;
            throwForceBoost = 0f;
            // Sniper Rifle Mastery (5/0)
            SkillData s = PlayerData.playerdata.skillList["5/0"];
            if (s.Level == 1) {
                accuracyBoost = 0.05f;
                recoilBoost = 0.05f;
            } else if (s.Level == 2) {
                accuracyBoost = 0.1f;
                recoilBoost = 0.1f;
            } else if (s.Level == 3) {
                accuracyBoost = 0.2f;
                recoilBoost = 0.2f;
            }
        } else if (weaponCategory == 5) {
            deploymentTimeBoost = 0f;
            throwForceBoost = 0f;
            // Pistol (0/1)
            SkillData s = PlayerData.playerdata.skillList["0/1"];
            if (s.Level == 1) {
                accuracyBoost = 0.1f;
                recoilBoost = 0.05f;
            } else if (s.Level == 2) {
                accuracyBoost = 0.2f;
                recoilBoost = 0.1f;
            } else if (s.Level == 3) {
                accuracyBoost = 0.4f;
                recoilBoost = 0.2f;
            }
        } else if (weaponCategory == 6) {
            deploymentTimeBoost = 0f;
            throwForceBoost = 0f;
            // Launcher (6/1)
            SkillData s = PlayerData.playerdata.skillList["6/1"];
            if (s.Level == 1) {
                accuracyBoost = 0f;
                recoilBoost = 0.05f;
            } else if (s.Level == 2) {
                accuracyBoost = 0f;
                recoilBoost = 0.15f;
            } else if (s.Level == 3) {
                accuracyBoost = 0f;
                recoilBoost = 0.2f;
            }
        } else if (weaponCategory == 7) {
            deploymentTimeBoost = 0f;
            // Throwables Mastery (2/0)
            SkillData s = PlayerData.playerdata.skillList["2/0"];
            accuracyBoost = 0f;
            recoilBoost = 0f;
            if (s.Level == 0) {
                throwForceBoost = 0.4f;
            } else if (s.Level == 1) {
                throwForceBoost = 0.6f;
            } else if (s.Level == 2) {
                throwForceBoost = 1f;
            }
        } else if (weaponCategory == 8) {
            deploymentTimeBoost = 0f;
            throwForceBoost = 0f;
            // Booster Mastery (4/1)
            SkillData s = PlayerData.playerdata.skillList["4/1"];
            recoilBoost = 0f;
            accuracyBoost = 0f;
        } else if (weaponCategory == 9) {
            throwForceBoost = 0f;
            // Deployables Mastery (4/0)
            SkillData s = PlayerData.playerdata.skillList["4/0"];
            recoilBoost = 0f;
            accuracyBoost = 0f;
            if (s.Level == 1) {
                deploymentTimeBoost = 0.25f;
            } else if (s.Level == 2) {
                deploymentTimeBoost = 0.5f;
            } else if (s.Level == 3) {
                deploymentTimeBoost = 1f;
            }
        } else if (weaponCategory == 10) {
            deploymentTimeBoost = 0f;
            throwForceBoost = 0f;
            // Rifle Mastery (5/1)
            SkillData s = PlayerData.playerdata.skillList["5/1"];
            if (s.Level == 1) {
                accuracyBoost = 0.1f;
                recoilBoost = 0.05f;
            } else if (s.Level == 2) {
                accuracyBoost = 0.2f;
                recoilBoost = 0.1f;
            } else if (s.Level == 3) {
                accuracyBoost = 0.4f;
                recoilBoost = 0.2f;
            }
        }

        // Accuracy Mastery
        if (PlayerData.playerdata.skillList["5/6"].Level == 1) {
            accuracyBoost += 0.1f;
        } else if (PlayerData.playerdata.skillList["5/6"].Level == 2) {
            accuracyBoost += 0.2f;
        } else if (PlayerData.playerdata.skillList["5/6"].Level == 3) {
            accuracyBoost += 0.3f;
        } else if (PlayerData.playerdata.skillList["5/6"].Level == 4) {
            accuracyBoost += 0.4f;
        }

        accuracyBoost = Mathf.Clamp(accuracyBoost, 0f, 0.9f);
    }

    public int GetDeployableMasteryLevel()
    {
        return PlayerData.playerdata.skillList["4/0"].Level;
    }

    public float GetReloadSpeedBoostForCurrentWeapon(Weapon w)
    {
        if (w.category == "Assault Rifle") {
            if (PlayerData.playerdata.skillList["0/2"].Level == 1) {
                return 0.25f;
            } else if (PlayerData.playerdata.skillList["0/2"].Level == 2) {
                return 0.5f;
            } else if (PlayerData.playerdata.skillList["0/2"].Level == 3) {
                return 1f;
            }
        } else if (w.category == "SMG") {
            if (PlayerData.playerdata.skillList["1/2"].Level == 1) {
                return 0.25f;
            } else if (PlayerData.playerdata.skillList["1/2"].Level == 2) {
                return 0.5f;
            } else if (PlayerData.playerdata.skillList["1/2"].Level == 3) {
                return 1f;
            }
        } else if (w.category == "Pistol") {
            if (PlayerData.playerdata.skillList["0/3"].Level == 1) {
                return 0.25f;
            } else if (PlayerData.playerdata.skillList["0/3"].Level == 2) {
                return 0.5f;
            } else if (PlayerData.playerdata.skillList["0/3"].Level == 3) {
                return 1f;
            }
        } else if (w.category == "Shotgun") {
            if (PlayerData.playerdata.skillList["1/3"].Level == 1) {
                return 0.25f;
            } else if (PlayerData.playerdata.skillList["1/3"].Level == 2) {
                return 0.5f;
            } else if (PlayerData.playerdata.skillList["1/3"].Level == 3) {
                return 1f;
            }
        } else if (w.category == "LMG") {
            if (PlayerData.playerdata.skillList["6/2"].Level == 1) {
                return 0.25f;
            } else if (PlayerData.playerdata.skillList["6/2"].Level == 2) {
                return 0.5f;
            } else if (PlayerData.playerdata.skillList["6/2"].Level == 3) {
                return 1f;
            }
        } else if (w.category == "Sniper Rifle") {
            if (PlayerData.playerdata.skillList["5/2"].Level == 1) {
                return 0.25f;
            } else if (PlayerData.playerdata.skillList["5/2"].Level == 2) {
                return 0.5f;
            } else if (PlayerData.playerdata.skillList["5/2"].Level == 3) {
                return 1f;
            }
        } else if (w.category == "Rifle") {
            if (PlayerData.playerdata.skillList["5/3"].Level == 1) {
                return 0.25f;
            } else if (PlayerData.playerdata.skillList["5/3"].Level == 2) {
                return 0.5f;
            } else if (PlayerData.playerdata.skillList["5/3"].Level == 3) {
                return 1f;
            }
        } else if (w.category == "Launcher") {
            if (PlayerData.playerdata.skillList["6/3"].Level == 1) {
                return 0.25f;
            } else if (PlayerData.playerdata.skillList["6/3"].Level == 2) {
                return 0.5f;
            } else if (PlayerData.playerdata.skillList["6/3"].Level == 3) {
                return 1f;
            }
        }
        return 0f;
    }

    public float GetStaminaBoost()
    {
        // Cardio Conditioning (1/5)
        if (PlayerData.playerdata.skillList["1/5"].Level == 1) {
            return 0.25f;
        } else if (PlayerData.playerdata.skillList["1/5"].Level == 2) {
            return 0.5f;
        } else if (PlayerData.playerdata.skillList["1/5"].Level == 3) {
            return 0.8f;
        }
        
        return 0f;
    }

    public float GetNinjaSpeedBoost()
    {
        int equippedSuppressorCount = 0;
        if (PlayerData.playerdata.primaryModInfo.SuppressorId != null && PlayerData.playerdata.primaryModInfo.SuppressorId != "") {
            equippedSuppressorCount++;
        }
        if (PlayerData.playerdata.secondaryModInfo.SuppressorId != null && PlayerData.playerdata.secondaryModInfo.SuppressorId != "") {
            equippedSuppressorCount++;
        }
        float totalSpeedBoost = 0f;
        // Ninja (1/8)
        if (PlayerData.playerdata.skillList["1/8"].Level == 1) {
            for (int i = 0; i < equippedSuppressorCount; i++) {
                totalSpeedBoost += 0.02f;
            }
        } else if (PlayerData.playerdata.skillList["1/8"].Level == 2) {
            for (int i = 0; i < equippedSuppressorCount; i++) {
                totalSpeedBoost += 0.04f;
            }
        } else if (PlayerData.playerdata.skillList["1/8"].Level == 3) {
            for (int i = 0; i < equippedSuppressorCount; i++) {
                totalSpeedBoost += 0.06f;
            }
        }
        
        return totalSpeedBoost;
    }

    public int GetBoosterEffectLevel()
    {
        return PlayerData.playerdata.skillList["4/1"].Level;
    }

    public float GetFiringSpeedBoostForCurrentWeapon(Weapon w)
    {
        if (w.category == "Booster") {
            if (PlayerData.playerdata.skillList["4/1"].Level == 1) {
                return 0.25f;
            } else if (PlayerData.playerdata.skillList["4/1"].Level == 2) {
                return 0.5f;
            } else if (PlayerData.playerdata.skillList["4/1"].Level == 3) {
                return 0.75f;
            }
        }
        return 0f;
    }

    public void HandleHealthChangeEvent(int health)
    {
        float newDamageBoost = 0f;
        // Death Wish
        if (PlayerData.playerdata.skillList["0/7"].Level == 1) {
            if (health < 20) {
                newDamageBoost = Mathf.Clamp(Mathf.Pow(1.2f, 20 - health), 0f, 15f);
            }
        } else if (PlayerData.playerdata.skillList["0/7"].Level == 2) {
            if (health < 40) {
                newDamageBoost = Mathf.Clamp(Mathf.Pow(1.1f, 40 - health), 0f, 30f);
            }
        } else if (PlayerData.playerdata.skillList["0/7"].Level == 3) {
            if (health < 60) {
                newDamageBoost = Mathf.Clamp(Mathf.Pow(1.07f, 60 - health), 0f, 50f);
            }
        }

        float newSpeedBoost = 0f;
        // Running on E (1/9)
        if (PlayerData.playerdata.skillList["1/9"].Level == 1) {
            if (health < 20) {
                newSpeedBoost = Mathf.Clamp(Mathf.Pow(1.1f, 20 - health), 0f, 5f);
            }
        } else if (PlayerData.playerdata.skillList["1/9"].Level == 2) {
            if (health < 40) {
                newSpeedBoost = Mathf.Clamp(Mathf.Pow(1.09f, 40 - health), 0f, 15f);
            }
        } else if (PlayerData.playerdata.skillList["1/9"].Level == 3) {
            if (health < 60) {
                newSpeedBoost = Mathf.Clamp(Mathf.Pow(1.07f, 60 - health), 0f, 30f);
            }
        }

        newDamageBoost /= 100f;
        damageBoost = newDamageBoost;

        newSpeedBoost /= 100f;
        speedBoost = newSpeedBoost;
    }

    public float GetDamageBoost()
    {
        return 1f + damageBoost;
    }

    public float GetSpeedBoost()
    {
        return 1f + speedBoost;
    }

    public float GetArmorBoost()
    {
        if (PlayerData.playerdata.skillList["6/4"].Level == 1) {
            return 0.1f;
        } else if (PlayerData.playerdata.skillList["6/4"].Level == 2) {
            return 0.2f;
        } else if (PlayerData.playerdata.skillList["6/4"].Level == 3) {
            return 0.3f;
        } else if (PlayerData.playerdata.skillList["6/4"].Level == 4) {
            return 0.4f;
        }
        return 0f;
    }

    public bool WasCriticalHit()
    {
        if (PlayerData.playerdata.skillList["0/8"].Level == 1) {
            int r = Random.Range(0, 100);
            if (r == 0) {
                return true;
            }
        } else if (PlayerData.playerdata.skillList["0/8"].Level == 2) {
            int r = Random.Range(0, 100);
            if (r == 0 || r == 1) {
                return true;
            }
        } else if (PlayerData.playerdata.skillList["0/8"].Level == 3) {
            int r = Random.Range(0, 100);
            if (r >= 0 && r <= 3) {
                return true;
            }
        }
        return false;
    }

    public float GetMeleeDamageBoost()
    {
        if (PlayerData.playerdata.skillList["0/4"].Level == 1) {
            return 0.15f;
        } else if (PlayerData.playerdata.skillList["0/4"].Level == 2) {
            return 0.3f;
        } else if (PlayerData.playerdata.skillList["0/4"].Level == 3) {
            return 0.5f;
        }
        return 0f;
    }

    public float GetMeleeLungeBoost()
    {
        if (PlayerData.playerdata.skillList["0/5"].Level == 1) {
            return 0.15f;
        } else if (PlayerData.playerdata.skillList["0/5"].Level == 2) {
            return 0.3f;
        } else if (PlayerData.playerdata.skillList["0/5"].Level == 3) {
            return 0.5f;
        }
        return 0f;
    }

    public float GetJumpBoost()
    {
        if (PlayerData.playerdata.skillList["1/6"].Level == 1) {
            return 0.05f;
        } else if (PlayerData.playerdata.skillList["1/6"].Level == 2) {
            return 0.1f;
        } else if (PlayerData.playerdata.skillList["1/6"].Level == 3) {
            return 0.2f;
        }
        return 0f;
    }

    public float GetFallDamageReduction()
    {
        if (PlayerData.playerdata.skillList["1/4"].Level == 1) {
            return 0.25f;
        } else if (PlayerData.playerdata.skillList["1/4"].Level == 2) {
            return 0.5f;
        }
        return 0f;
    }

    public float HandleAllyDeath()
    {
        float totalSpeedBoost = 0f;
        // Contingency Time (0/11)
        if (PlayerData.playerdata.skillList["0/11"].Level == 1) {
            int deadPlayerCount = GetComponent<PlayerActionScript>().gameController.GetDeadPlayerCount();
            for (int i = 0; i < deadPlayerCount; i++) {
                totalSpeedBoost += 0.02f;
            }
        } else if (PlayerData.playerdata.skillList["0/11"].Level == 2) {
            int deadPlayerCount = GetComponent<PlayerActionScript>().gameController.GetDeadPlayerCount();
            for (int i = 0; i < deadPlayerCount; i++) {
                totalSpeedBoost += 0.03f;
            }
        } else if (PlayerData.playerdata.skillList["0/11"].Level == 3) {
            int deadPlayerCount = GetComponent<PlayerActionScript>().gameController.GetDeadPlayerCount();
            for (int i = 0; i < deadPlayerCount; i++) {
                totalSpeedBoost += 0.05f;
            }
        }
        return totalSpeedBoost;
    }

    public int GetHealthCaddyBoost()
    {
        int boost = 0;
        if (PlayerData.playerdata.skillList["4/4"].Level == 1) {
            boost = 1;
        } else if (PlayerData.playerdata.skillList["4/4"].Level == 2) {
            boost = 2;
        } else if (PlayerData.playerdata.skillList["4/4"].Level == 3) {
            boost = 3;
        }
        return boost;
    }

    public int GetAmmoCaddyBoost()
    {
        int boost = 0;
        if (PlayerData.playerdata.skillList["6/9"].Level == 1) {
            boost = 1;
        } else if (PlayerData.playerdata.skillList["6/9"].Level == 2) {
            boost = 2;
        } else if (PlayerData.playerdata.skillList["6/9"].Level == 3) {
            boost = 3;
        }
        return boost;
    }

    public int GetDigitalNomadBoost()
    {
        int boost = 0;
        if (PlayerData.playerdata.skillList["2/8"].Level == 1) {
            boost = 1;
        } else if (PlayerData.playerdata.skillList["2/8"].Level == 2) {
            boost = 2;
        } else if (PlayerData.playerdata.skillList["2/8"].Level == 3) {
            boost = 3;
        }
        return boost;
    }

    public float GetDexterityBoost()
    {
        if (PlayerData.playerdata.skillList["2/2"].Level == 1) {
            return 0.25f;
        } else if (PlayerData.playerdata.skillList["2/2"].Level == 2) {
            return 0.5f;
        } else if (PlayerData.playerdata.skillList["2/2"].Level == 3) {
            return 0.75f;
        }
        return 0f;
    }

    public float GetTechMasteryBoost()
    {
        float boost = 0f;
        if (PlayerData.playerdata.skillList["2/1"].Level == 1) {
            return 0.1f;
        } else if (PlayerData.playerdata.skillList["2/1"].Level == 2) {
            return 0.2f;
        } else if (PlayerData.playerdata.skillList["2/1"].Level == 3) {
            return 0.3f;
        }
        return boost;
    }

    public float GetTechDeploySpeedBoost()
    {
        float boost = 0f;
        if (PlayerData.playerdata.skillList["2/1"].Level == 1) {
            return 0.25f;
        } else if (PlayerData.playerdata.skillList["2/1"].Level == 2) {
            return 0.5f;
        } else if (PlayerData.playerdata.skillList["2/1"].Level == 3) {
            return 1f;
        }
        return boost;
    }

    public float GetMeleeResistance()
    {
        if (PlayerData.playerdata.skillList["6/6"].Level == 1) {
            return 0.1f;
        } else if (PlayerData.playerdata.skillList["6/6"].Level == 2) {
            return 0.25f;
        } else if (PlayerData.playerdata.skillList["6/6"].Level == 3) {
            return 0.5f;
        }
        return 0f;
    }

    public int GetKeenEyesMultiplier()
    {
        if (PlayerData.playerdata.skillList["5/9"].Level == 1) {
            return 2;
        } else if (PlayerData.playerdata.skillList["5/9"].Level == 2) {
            return 3;
        } else if (PlayerData.playerdata.skillList["5/9"].Level == 3) {
            return 5;
        } else if (PlayerData.playerdata.skillList["5/9"].Level == 4) {
            return 8;
        }
        return 1;
    }

    public float GetSniperAmplification()
    {
        if (PlayerData.playerdata.skillList["5/5"].Level == 1) {
            return 0.2f;
        } else if (PlayerData.playerdata.skillList["5/5"].Level == 2) {
            return 0.3f;
        } else if (PlayerData.playerdata.skillList["5/5"].Level == 3) {
            return 0.5f;
        }
        return 0f;
    }

    public float GetShootToKillBoost()
    {
        if (PlayerData.playerdata.skillList["5/11"].Level == 1) {
            return 0.02f;
        } else if (PlayerData.playerdata.skillList["5/11"].Level == 2) {
            return 0.05f;
        } else if (PlayerData.playerdata.skillList["5/11"].Level == 3) {
            return 0.1f;
        }
        return 0f;
    }

    public bool BulletSpongeAbsorbed()
    {
        if (PlayerData.playerdata.skillList["6/10"].Level == 1) {
            int r = Random.Range(0, 100);
            if (r == 0) {
                return true;
            }
            return false;
        } else if (PlayerData.playerdata.skillList["6/10"].Level == 1) {
            int r = Random.Range(0, 100);
            if (r >= 0 && r <= 3) {
                return true;
            }
            return false;
        } else if (PlayerData.playerdata.skillList["6/10"].Level == 1) {
            int r = Random.Range(0, 100);
            if (r >= 0 && r <= 7) {
                return true;
            }
            return false;
        }
        return false;
    }

    public int GetMunitionsEngineeringLevel()
    {
        return PlayerData.playerdata.skillList["2/3"].Level;
    }

    public bool MunitionsEngineeringFlag()
    {
        if (PlayerData.playerdata.skillList["2/3"].Level == 1) {
            if (munitionsEngineeringTimer > 20f) {
                return true;
            }
            return false;
        } else if (PlayerData.playerdata.skillList["2/3"].Level == 2) {
            if (munitionsEngineeringTimer > 18f) {
                return true;
            }
            return false;
        } else if (PlayerData.playerdata.skillList["2/3"].Level == 3) {
            if (munitionsEngineeringTimer > 15f) {
                return true;
            }
            return false;
        }
        return false;
    }

    public void MunitionsEngineeringRefresh()
    {
        munitionsEngineeringTimer += Time.deltaTime;
    }

    public void MunitionsEngineeringReset()
    {
        munitionsEngineeringTimer = 0f;
    }

    // Collective boosts

    public void AddHackerBoost(int val)
    {
        hackerBoost += val;
    }

    public void RemoveHackerBoost(int val)
    {
        hackerBoost -= val;
    }

    public int GetHackerBoost()
    {
        return hackerBoost;
    }

    public int GetMyHackerBoost()
    {
        if (PlayerData.playerdata.skillList["2/5"].Level == 1) {
            return 5;
        } else if (PlayerData.playerdata.skillList["2/5"].Level == 1) {
            return 8;
        } else if (PlayerData.playerdata.skillList["2/5"].Level == 1) {
            return 12;
        }
        return 0;
    }

    public void SetThisPlayerHackerBoost(int val)
    {
        storedHackerBoost = val;
    }

    public int GetThisPlayerHackerBoost()
    {
        return storedHackerBoost;
    }

}
