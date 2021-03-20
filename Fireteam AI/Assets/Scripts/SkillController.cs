using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Koobando.AntiCheat;

public class SkillController : MonoBehaviour
{
    private const float ONE_SHOT_ONE_KILL_TIME = 10f;
    public static float BOOSTER_LVL1_EFFECT = 0.05f;
    public static float BOOSTER_LVL2_EFFECT = 0.1f;
    public static float BOOSTER_LVL3_EFFECT = 0.2f;
    private EncryptedFloat speedBoost;
    private EncryptedFloat damageBoost;
    private EncryptedFloat armorBoost;
    public EncryptedFloat recoilBoost;
    public EncryptedFloat accuracyBoost;
    public EncryptedFloat throwForceBoost;
    public EncryptedFloat deploymentTimeBoost;
    private float munitionsEngineeringTimer;
    private float regenerationTimer;
    private float oneShotOneKillTimer;

    // Timed boosts
    private float bloodLustActiveTimer;
    private float bloodLustActivateTimer;
    private short bloodLustActivateCount;

    // Collective boosts
    private EncryptedInt hackerBoost;
    private EncryptedInt storedHackerBoost;
    private EncryptedFloat headstrongBoost;
    private EncryptedFloat storedHeadstrongBoost;
    private EncryptedFloat resourcefulBoost;
    private EncryptedFloat storedResourcefulBoost;
    private EncryptedFloat inspireBoost;
    private EncryptedFloat storedInspireBoost;
    private int[] providerBoost = new int[4];
    private EncryptedInt storedProviderBoost;

    void Update()
    {
        MunitionsEngineeringRefresh();
        RegenerationRefresh();
        UpdateOneShotOneKillTimer();
        UpdateBloodLustActivation();
        UpdateBloodLustActive();
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

    public void InitializeCollectiveBoosts()
    {
        SetThisPlayerHackerBoost(GetMyHackerBoost());
        AddHackerBoost(GetMyHackerBoost());

        SetThisPlayerHeadstrongBoost(GetMyHeadstrongBoost());
        AddHeadstrongBoost(GetMyHeadstrongBoost());

        SetThisPlayerResourcefulBoost(GetMyResourcefulBoost());
        AddResourcefulBoost(GetMyResourcefulBoost());

        SetThisPlayerInspireBoost(GetMyInspireBoost());
        AddInspireBoost(GetMyInspireBoost());

        SetThisPlayerProviderBoost(GetMyProviderBoost());
        AddProviderBoost(GetMyProviderBoost());
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

        float newArmorBoost = 0f;
        // Crunch Time (6/7)
        if (PlayerData.playerdata.skillList["6/7"].Level == 1) {
            if (health < 20) {
                newArmorBoost = Mathf.Clamp(Mathf.Pow(1.15f, 20 - health), 0f, 10f);
            }
        } else if (PlayerData.playerdata.skillList["6/7"].Level == 2) {
            if (health < 40) {
                newArmorBoost = Mathf.Clamp(Mathf.Pow(1.09f, 40 - health), 0f, 20f);
            }
        } else if (PlayerData.playerdata.skillList["6/7"].Level == 3) {
            if (health < 60) {
                newArmorBoost = Mathf.Clamp(Mathf.Pow(1.065f, 60 - health), 0f, 30f);
            }
        }

        newDamageBoost /= 100f;
        damageBoost = newDamageBoost;

        newSpeedBoost /= 100f;
        speedBoost = newSpeedBoost;

        newArmorBoost /= 100f;
        armorBoost = newArmorBoost;
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
        return 1f + armorBoost;
    }

    public float GetOverallArmorBoost()
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

    public float GetArmorAmplificationBoost()
    {
        if (PlayerData.playerdata.skillList["6/5"].Level == 1) {
            return 0.5f;
        } else if (PlayerData.playerdata.skillList["6/5"].Level == 2) {
            return 1f;
        } else if (PlayerData.playerdata.skillList["6/5"].Level == 3) {
            return 2f;
        }
        return 0f;
    }

    public float GetSilentKillerBoost()
    {
        if (PlayerData.playerdata.skillList["5/4"].Level == 1) {
            return 0.1f;
        } else if (PlayerData.playerdata.skillList["5/4"].Level == 2) {
            return 0.2f;
        } else if (PlayerData.playerdata.skillList["5/4"].Level == 3) {
            return 0.3f;
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

    public int GetRegenerationLevel()
    {
        return PlayerData.playerdata.skillList["4/2"].Level;
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

    public bool RegenerationFlag()
    {
        if (PlayerData.playerdata.skillList["4/2"].Level == 1) {
            if (regenerationTimer > 20f) {
                return true;
            }
            return false;
        } else if (PlayerData.playerdata.skillList["4/2"].Level == 2) {
            if (regenerationTimer > 20f) {
                return true;
            }
            return false;
        } else if (PlayerData.playerdata.skillList["4/2"].Level == 3) {
            if (regenerationTimer > 18f) {
                return true;
            }
            return false;
        } else if (PlayerData.playerdata.skillList["4/2"].Level == 4) {
            if (regenerationTimer > 17f) {
                return true;
            }
            return false;
        } else if (PlayerData.playerdata.skillList["4/2"].Level == 5) {
            if (regenerationTimer > 15f) {
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

    public void RegenerationRefresh()
    {
        regenerationTimer += Time.deltaTime;
    }

    public void MunitionsEngineeringReset()
    {
        munitionsEngineeringTimer = 0f;
    }

    public void RegenerationReset()
    {
        regenerationTimer = 0f;
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
        } else if (PlayerData.playerdata.skillList["2/5"].Level == 2) {
            return 8;
        } else if (PlayerData.playerdata.skillList["2/5"].Level == 3) {
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

    public void AddHeadstrongBoost(float val)
    {
        headstrongBoost += val;
    }

    public void RemoveHeadstrongBoost(float val)
    {
        headstrongBoost -= val;
    }

    public float GetHeadstrongBoost()
    {
        return headstrongBoost;
    }

    public float GetMyHeadstrongBoost()
    {
        if (PlayerData.playerdata.skillList["3/1"].Level == 1) {
            return 0.01f;
        } else if (PlayerData.playerdata.skillList["3/1"].Level == 2) {
            return 0.02f;
        } else if (PlayerData.playerdata.skillList["3/1"].Level == 3) {
            return 0.04f;
        } else if (PlayerData.playerdata.skillList["3/1"].Level == 4) {
            return 0.06f;
        }
        return 0f;
    }

    public void SetThisPlayerHeadstrongBoost(float val)
    {
        storedHeadstrongBoost = val;
    }

    public float GetThisPlayerHeadstrongBoost()
    {
        return storedHeadstrongBoost;
    }

    public void AddResourcefulBoost(float val)
    {
        resourcefulBoost += val;
    }

    public void RemoveResourcefulBoost(float val)
    {
        resourcefulBoost -= val;
    }

    public float GetResourcefulBoost()
    {
        return resourcefulBoost;
    }

    public float GetMyResourcefulBoost()
    {
        if (PlayerData.playerdata.skillList["3/4"].Level == 1) {
            return 0.5f;
        } else if (PlayerData.playerdata.skillList["3/4"].Level == 2) {
            return 1f;
        } else if (PlayerData.playerdata.skillList["3/4"].Level == 3) {
            return 2f;
        }
        return 0f;
    }

    public void SetThisPlayerResourcefulBoost(float val)
    {
        storedResourcefulBoost = val;
    }

    public float GetThisPlayerResourcefulBoost()
    {
        return storedResourcefulBoost;
    }

    public void AddInspireBoost(float val)
    {
        inspireBoost += val;
    }

    public void RemoveInspireBoost(float val)
    {
        inspireBoost -= val;
    }

    public float GetInspireBoost()
    {
        return inspireBoost;
    }

    public float GetMyInspireBoost()
    {
        if (PlayerData.playerdata.skillList["3/2"].Level == 1) {
            return 0.01f;
        } else if (PlayerData.playerdata.skillList["3/2"].Level == 2) {
            return 0.02f;
        } else if (PlayerData.playerdata.skillList["3/2"].Level == 3) {
            return 0.04f;
        } else if (PlayerData.playerdata.skillList["3/2"].Level == 4) {
            return 0.06f;
        }
        return 0f;
    }

    public void SetThisPlayerInspireBoost(float val)
    {
        storedInspireBoost = val;
    }

    public float GetThisPlayerInspireBoost()
    {
        return storedInspireBoost;
    }

    public void AddProviderBoost(int val)
    {
        providerBoost[val]++;
    }

    public void RemoveProviderBoost(int val)
    {
        providerBoost[val]--;
    }

    public int GetProviderBoost()
    {
        for (int i = 3; i >= 0; i--) {
            if (providerBoost[i] > 0) {
                return i;
            }
        }
        return 0;
    }

    public int GetMyProviderBoost()
    {
        return PlayerData.playerdata.skillList["3/5"].Level;
    }

    public void SetThisPlayerProviderBoost(int val)
    {
        storedProviderBoost = val;
    }

    public int GetThisPlayerProviderBoost()
    {
        return storedProviderBoost;
    }

    // End collective boosts

    public int GetHealthDropChanceBoost()
    {
        if (PlayerData.playerdata.skillList["4/3"].Level == 1) {
            return 2;
        } else if (PlayerData.playerdata.skillList["4/3"].Level == 2) {
            return 5;
        } else if (PlayerData.playerdata.skillList["4/3"].Level == 3) {
            return 10;
        }
        return 0;
    }

    public int GetAmmoDropChanceBoost()
    {
        if (PlayerData.playerdata.skillList["6/8"].Level == 1) {
            return 4;
        } else if (PlayerData.playerdata.skillList["6/8"].Level == 2) {
            return 8;
        } else if (PlayerData.playerdata.skillList["6/8"].Level == 3) {
            return 15;
        }
        return 0;
    }

    public float GetHitmanDamageBoost()
    {
        if (PlayerData.playerdata.skillList["5/7"].Level == 1) {
            return 0.15f;
        } else if (PlayerData.playerdata.skillList["5/7"].Level == 2) {
            return 0.3f;
        } else if (PlayerData.playerdata.skillList["5/7"].Level == 3) {
            return 0.45f;
        }
        return 0f;
    }

    public float GetOneShotOneKillDamageBoost()
    {
        if (PlayerData.playerdata.skillList["5/8"].Level == 1) {
            return 0.25f;
        } else if (PlayerData.playerdata.skillList["5/8"].Level == 2) {
            return 0.5f;
        } else if (PlayerData.playerdata.skillList["5/8"].Level == 3) {
            return 0.8f;
        } else if (PlayerData.playerdata.skillList["5/8"].Level == 4) {
            return 1f;
        }
        return 0f;
    }

    public void UpdateOneShotOneKillTimer()
    {
        if (oneShotOneKillTimer < ONE_SHOT_ONE_KILL_TIME) {
            oneShotOneKillTimer += Time.deltaTime;
        }
    }

    public void ResetOneShotOneKillTimer()
    {
        oneShotOneKillTimer = 0f;
    }

    public bool OneShotOneKillReady()
    {
        return oneShotOneKillTimer >= ONE_SHOT_ONE_KILL_TIME;
    }

    public int GetBloodLeechLevel()
    {
        return PlayerData.playerdata.skillList["4/7"].Level;
    }

    public void RegisterKillForKillstreak()
    {
        // Blood Lust (0/6)
        int bloodLustLvl = PlayerData.playerdata.skillList["0/6"].Level;
        if (bloodLustLvl > 0) {
            if (bloodLustActiveTimer <= 0f) {
                if (bloodLustActivateCount == 0) {
                    if (bloodLustLvl == 1) {
                        bloodLustActivateTimer = 4f;
                    } else if (bloodLustLvl == 2) {
                        bloodLustActivateTimer = 4f;
                    } else if (bloodLustLvl == 3) {
                        bloodLustActivateTimer = 5f;
                    } else if (bloodLustLvl == 4) {
                        bloodLustActivateTimer = 5f;
                    }
                }
                bloodLustActivateCount++;
                if (bloodLustLvl == 1) {
                    if (bloodLustActivateCount >= 10) {
                        bloodLustActivateCount = 0;
                        bloodLustActivateTimer = 0f;
                        bloodLustActiveTimer = 10f;
                    }
                } else if (bloodLustLvl == 2) {
                    if (bloodLustActivateCount >= 8) {
                        bloodLustActivateCount = 0;
                        bloodLustActivateTimer = 0f;
                        bloodLustActiveTimer = 15f;
                    }
                } else if (bloodLustLvl == 3) {
                    if (bloodLustActivateCount >= 6) {
                        bloodLustActivateCount = 0;
                        bloodLustActivateTimer = 0f;
                        bloodLustActiveTimer = 20f;
                    }
                } else if (bloodLustLvl == 4) {
                    if (bloodLustActivateCount >= 4) {
                        bloodLustActivateCount = 0;
                        bloodLustActivateTimer = 0f;
                        bloodLustActiveTimer = 25f;
                    }
                }
            }
        }
    }

    private void UpdateBloodLustActive()
    {
        if (bloodLustActiveTimer > 0f) {
            bloodLustActiveTimer -= Time.deltaTime;
        }
    }

    private void UpdateBloodLustActivation()
    {
        if (bloodLustActivateTimer > 0f) {
            bloodLustActivateTimer -= Time.deltaTime;
            if (bloodLustActivateTimer <= 0f) {
                bloodLustActivateTimer = 0f;
                bloodLustActivateCount = 0;
            }
        }
    }

    public float GetBloodLustDamageBoost()
    {
        if (bloodLustActiveTimer > 0f) {
            if (PlayerData.playerdata.skillList["0/6"].Level == 1) {
                return 0.04f;
            } else if (PlayerData.playerdata.skillList["0/6"].Level == 2) {
                return 0.06f;
            } else if (PlayerData.playerdata.skillList["0/6"].Level == 3) {
                return 0.08f;
            } else if (PlayerData.playerdata.skillList["0/6"].Level == 4) {
                return 0.1f;
            }
        }
        return 0f;
    }

}
