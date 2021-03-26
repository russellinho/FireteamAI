using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Koobando.AntiCheat;
using Random = UnityEngine.Random;

public class SkillController : MonoBehaviour
{
    private const float ONE_SHOT_ONE_KILL_TIME = 10f;
    public static float BOOSTER_LVL1_EFFECT = 0.05f;
    public static float BOOSTER_LVL2_EFFECT = 0.1f;
    public static float BOOSTER_LVL3_EFFECT = 0.2f;
    public static float MARTIAL_ARTS_ATTACK_CAP = 0.15f;
    public static float MARTIAL_ARTS_DEFENSE_CAP = 0.3f;
    private const float FIRETEAM_BOOST_CAP = 0.1f;
    private const float FIRM_GRIP_TIME_1 = 10f;
    private const float FIRM_GRIP_TIME_2 = 20f;
    private const float FIRM_GRIP_TIME_3 = 30f;
    private const float FIRM_GRIP_COOLDOWN_1 = 720f;
    private const float FIRM_GRIP_COOLDOWN_2 = 600f;
    private const float FIRM_GRIP_COOLDOWN_3 = 480f;
    private const float SNIPERS_DEL_TIME_1 = 10f;
    private const float SNIPERS_DEL_TIME_2 = 20f;
    private const float SNIPERS_DEL_TIME_3 = 30f;
    private const float SNIPERS_DEL_COOLDOWN_1 = 300f;
    private const float SNIPERS_DEL_COOLDOWN_2 = 240f;
    private const float SNIPERS_DEL_COOLDOWN_3 = 180f;
    private const float BULLET_STREAM_TIME_1 = 10f;
    private const float BULLET_STREAM_TIME_2 = 15f;
    private const float BULLET_STREAM_TIME_3 = 20f;
    private const float BULLET_STREAM_TIME_4 = 30f;
    private const float BULLET_STREAM_COOLDOWN_1 = 480f;
    private const float BULLET_STREAM_COOLDOWN_2 = 420f;
    private const float BULLET_STREAM_COOLDOWN_3 = 360f;
    private const float BULLET_STREAM_COOLDOWN_4 = 300f;
    private const float RAMPAGE_TIME_1 = 10f;
    private const float RAMPAGE_TIME_2 = 20f;
    private const float RAMPAGE_TIME_3 = 30f;
    private const float RAMPAGE_COOLDOWN_1 = 600f;
    private const float RAMPAGE_COOLDOWN_2 = 480f;
    private const float RAMPAGE_COOLDOWN_3 = 360f;
    public static float REGENERATOR_MAX_DISTANCE = 15f;
    private const float REGENERATOR_TIMER_1 = 30f;
    private const float REGENERATOR_TIMER_2 = 28f;
    private const float REGENERATOR_TIMER_3 = 26f;
    private const float REGENERATOR_TIMER_4 = 24f;
    private const float REGENERATOR_TIMER_5 = 22f;
    private const float PAINKILLERS_TIMER_1 = 30f;
    private const float PAINKILLERS_TIMER_2 = 27f;
    private const float PAINKILLERS_TIMER_3 = 24f;
    private const float PAINKILLERS_TIMER_4 = 20f;
    private const int REGENERATOR_RECOVER_1 = 2;
    private const int REGENERATOR_RECOVER_2 = 3;
    private const int REGENERATOR_RECOVER_3 = 4;
    private const int REGENERATOR_RECOVER_4 = 5;
    private const int REGENERATOR_RECOVER_5 = 6;
    private const float MOTIVATE_BOOST_CAP = 0.2f;
    private const float INTIMIDATION_BOOST_CAP = 0.2f;
    private EncryptedFloat speedBoost;
    private EncryptedFloat damageBoost;
    private EncryptedFloat armorBoost;
    public EncryptedFloat recoilBoost;
    public EncryptedFloat accuracyBoost;
    public EncryptedFloat throwForceBoost;
    public EncryptedFloat deploymentTimeBoost;
    private EncryptedInt silhouetteBoost;
    private float munitionsEngineeringTimer;
    private float regenerationTimer;
    private float oneShotOneKillTimer;
    public LinkedList<int> regeneratorPlayerIds = new LinkedList<int>();
    public LinkedList<int> painkillerPlayerIds = new LinkedList<int>();
    private float thisRegeneratorTimer;
    private bool thisRegeneratorActive;
    private EncryptedInt thisRegeneratorLevel;
    private float thisPainkillerTimer;
    private bool thisPainkillerActive;
    private EncryptedInt thisPainkillerLevel;

    // Active boosts
    private float firmGripTimer;
    private float firmGripCooldown;
    private float snipersDelTimer;
    private float snipersDelCooldown;
    private float bulletStreamTimer;
    private float bulletStreamCooldown;
    private float rampageTimer;
    private float rampageCooldown;

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
    private EncryptedFloat intimidationBoost;
    private EncryptedFloat storedIntimidationBoost;
    private EncryptedFloat avoidabilityBoost;
    private int[] providerBoost = new int[4];
    private EncryptedInt storedProviderBoost;
    private int[] ddosBoost = new int[4];
    private EncryptedInt storedDdosBoost;
    private EncryptedFloat fireteamBoost;
    private EncryptedFloat storedFireteamBoost;
    private EncryptedFloat martialArtsAttackBoost;
    private EncryptedFloat martialArtsDefenseBoost;
    private EncryptedFloat storedMartialArtsAttackBoost;
    private EncryptedFloat storedMartialArtsDefenseBoost;
    private ArrayList motivateBoosts;
    private EncryptedFloat motivateDamageBoost;
    private EncryptedFloat storedAvoidabilityBoost;
    private EncryptedBool runNGun;
    private EncryptedBool jetpackBoost;
    private EncryptedBool rusticCowboy;

    void Update()
    {
        MunitionsEngineeringRefresh();
        RegenerationRefresh();
        UpdateOneShotOneKillTimer();
        UpdateBloodLustActivation();
        UpdateBloodLustActive();
        UpdateFirmGrip();
        UpdateSnipersDel();
        UpdateBulletStream();
        UpdateRampage();
        UpdateRegenerator();
        UpdatePainkiller();
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
        if (motivateBoosts == null) {
            motivateBoosts = new ArrayList();
        }
        InitializeShadowSightBoost();
        InitializeRunNGun();
        InitializeJetpackBoost();
        InitializeRusticCowboy();
        SetThisPlayerHackerBoost(GetMyHackerBoost());
        AddHackerBoost(GetMyHackerBoost());

        SetThisPlayerHeadstrongBoost(GetMyHeadstrongBoost());
        AddHeadstrongBoost(GetMyHeadstrongBoost());

        SetThisPlayerResourcefulBoost(GetMyResourcefulBoost());
        AddResourcefulBoost(GetMyResourcefulBoost());

        SetThisPlayerInspireBoost(GetMyInspireBoost());
        AddInspireBoost(GetMyInspireBoost());

        SetThisPlayerIntimidationBoost(GetMyIntimidationBoost());
        AddIntimidationBoost(GetMyIntimidationBoost());

        SetThisPlayerProviderBoost(GetMyProviderBoost());
        AddProviderBoost(GetMyProviderBoost());

        SetThisPlayerDdosLevel(GetMyDdosLevel());
        AddDdosBoost(GetMyDdosLevel());

        SetThisPlayerFireteamBoost(GetMyFireteamBoost());
        AddFireteamBoost(GetMyFireteamBoost());

        SetThisPlayerMartialArtsAttackBoost(GetMyMartialArtsAttackBoost());
        SetThisPlayerMartialArtsDefenseBoost(GetMyMartialArtsDefenseBoost());
        AddMartialArtsBoost(GetMyMartialArtsAttackBoost(), GetMyMartialArtsDefenseBoost());

        SetThisSilhouetteBoost(GetSilhouetteBoost());
        SetThisRegeneratorLevel(GetRegeneratorLevel());
        SetThisPainkillerLevel(GetPainkillerLevel());

        SetThisPlayerAvoidabilityBoost(GetMyAvoidabilityBoost());
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

    public int GetSilhouetteBoost()
    {
        if (PlayerData.playerdata.skillList["1/11"].Level == 1) {
            return 25;
        } else if (PlayerData.playerdata.skillList["1/11"].Level == 2) {
            return 50;
        } else if (PlayerData.playerdata.skillList["1/11"].Level == 3) {
            return 75;
        }
        return 0;
    }

    public void SetThisSilhouetteBoost(int val)
    {
        silhouetteBoost = val;
    }

    public int GetThisSilhouetteBoost()
    {
        return silhouetteBoost;
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

    public void AddIntimidationBoost(float val)
    {
        intimidationBoost += val;
    }

    public void RemoveIntimidationBoost(float val)
    {
        intimidationBoost -= val;
    }

    public float GetIntimidationBoost()
    {
        return Math.Min(intimidationBoost, INTIMIDATION_BOOST_CAP);
    }

    public float GetMyIntimidationBoost()
    {
        if (PlayerData.playerdata.skillList["3/6"].Level == 1) {
            return 0.01f;
        } else if (PlayerData.playerdata.skillList["3/6"].Level == 2) {
            return 0.02f;
        } else if (PlayerData.playerdata.skillList["3/6"].Level == 3) {
            return 0.04f;
        } else if (PlayerData.playerdata.skillList["3/6"].Level == 4) {
            return 0.06f;
        }
        return 0f;
    }

    public void SetThisPlayerIntimidationBoost(float val)
    {
        storedIntimidationBoost = val;
    }

    public float GetThisPlayerIntimidationBoost()
    {
        return storedIntimidationBoost;
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

    public void AddDdosBoost(int val)
    {
        ddosBoost[val]++;
    }

    public void RemoveDdosBoost(int val)
    {
        ddosBoost[val]--;
    }

    private int GetDdosLevel()
    {
        for (int i = 3; i >= 0; i--) {
            if (ddosBoost[i] > 0) {
                return i;
            }
        }
        return 0;
    }

    public float GetDdosAccuracyReduction()
    {
        int level = GetDdosLevel();
        if (level == 1) {
            return 0.03f;
        } else if (level == 2) {
            return 0.05f;
        } else if (level == 3) {
            return 0.1f;
        }
        return 0f;
    }

    public float GetDdosDelayTime()
    {
        int level = GetDdosLevel();
        if (level == 1) {
            return 0.5f;
        } else if (level == 2) {
            return 1f;
        } else if (level == 3) {
            return 2f;
        }
        return 0;
    }

    public int GetDdosDetectionBoost()
    {
        int level = GetDdosLevel();
        if (level == 1) {
            return 2;
        } else if (level == 2) {
            return 4;
        } else if (level == 3) {
            return 6;
        }
        return 0;
    }

    public int GetMyDdosLevel()
    {
        return PlayerData.playerdata.skillList["4/7"].Level;
    }

    public void SetThisPlayerDdosLevel(int val)
    {
        storedDdosBoost = val;
    }

    public int GetThisPlayerDdosLevel()
    {
        return storedDdosBoost;
    }

    public void AddFireteamBoost(float val)
    {
        fireteamBoost += val;
    }

    public void RemoveFireteamBoost(float val)
    {
        fireteamBoost -= val;
    }

    public float GetFireteamBoost(float avgDistanceBetweenTeam)
    {
        if (avgDistanceBetweenTeam > 0f) {
            float distMultiplier = Mathf.Min(1f, Mathf.Pow((8f / avgDistanceBetweenTeam), 1.5f));
            if (distMultiplier < 0.1f) {
                distMultiplier = 0f;
            }
            return Mathf.Min(fireteamBoost, FIRETEAM_BOOST_CAP) * distMultiplier;
        }
        return 0f;
    }

    public float GetMyFireteamBoost()
    {
        if (PlayerData.playerdata.skillList["3/10"].Level == 1) {
            return 0.01f;
        } else if (PlayerData.playerdata.skillList["3/10"].Level == 2) {
            return 0.02f;
        } else if (PlayerData.playerdata.skillList["3/10"].Level == 3) {
            return 0.03f;
        } else if (PlayerData.playerdata.skillList["3/10"].Level == 4) {
            return 0.04f;
        }
        return 0f;
    }

    public void SetThisPlayerFireteamBoost(float val)
    {
        storedFireteamBoost = val;
    }

    public float GetThisPlayerFireteamBoost()
    {
        return storedFireteamBoost;
    }

    public void AddMartialArtsBoost(float attackVal, float defenseVal)
    {
        martialArtsAttackBoost += attackVal;
        martialArtsDefenseBoost += defenseVal;
    }

    public void RemoveMartialArtsBoost(float attackVal, float defenseVal)
    {
        martialArtsAttackBoost -= attackVal;
        martialArtsDefenseBoost -= defenseVal;
    }

    public float GetMartialArtsAttackBoost()
    {
        return Mathf.Min(martialArtsAttackBoost, MARTIAL_ARTS_ATTACK_CAP);
    }

    public float GetMartialArtsDefenseBoost()
    {
        return Mathf.Min(martialArtsDefenseBoost, MARTIAL_ARTS_DEFENSE_CAP);
    }

    public float GetMyMartialArtsAttackBoost()
    {
        if (PlayerData.playerdata.skillList["3/7"].Level == 1) {
            return 0.01f;
        } else if (PlayerData.playerdata.skillList["3/7"].Level == 2) {
            return 0.02f;
        } else if (PlayerData.playerdata.skillList["3/7"].Level == 3) {
            return 0.05f;
        }
        return 0f;
    }

    public float GetMyMartialArtsDefenseBoost()
    {
        if (PlayerData.playerdata.skillList["3/7"].Level == 1) {
            return 0.05f;
        } else if (PlayerData.playerdata.skillList["3/7"].Level == 2) {
            return 0.1f;
        } else if (PlayerData.playerdata.skillList["3/7"].Level == 3) {
            return 0.15f;
        }
        return 0f;
    }

    public void SetThisPlayerMartialArtsAttackBoost(float val)
    {
        storedMartialArtsAttackBoost = val;
    }

    public void SetThisPlayerMartialArtsDefenseBoost(float val)
    {
        storedMartialArtsDefenseBoost = val;
    }

    public float GetThisPlayerMartialArtsAttackBoost()
    {
        return storedMartialArtsAttackBoost;
    }

    public float GetThisPlayerMartialArtsDefenseBoost()
    {
        return storedMartialArtsDefenseBoost;
    }

    // End collective boosts

    public int GetNanoparticulatesChanceBoost()
    {
        if (PlayerData.playerdata.skillList["2/6"].Level == 1) {
            return 10;
        } else if (PlayerData.playerdata.skillList["2/6"].Level == 2) {
            return 20;
        } else if (PlayerData.playerdata.skillList["2/6"].Level == 3) {
            return 30;
        }
        return 0;
    }

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

    // ACTIVE SKILLS

    public bool SkillIsAvailable(int skill)
    {
        if (skill == 1) {
            return firmGripCooldown <= 0f;
        } else if (skill == 2) {
            return rampageCooldown <= 0f;
        } else if (skill == 4) {
            return snipersDelCooldown <= 0f;
        } else if (skill == 5) {
            return bulletStreamCooldown <= 0f;
        } else if (skill == 9) {
            return CanCallGuardianAngel();
        }
        return false;
    }

    public bool HasSkill(int skill)
    {
        if (skill == 1) {
            return PlayerData.playerdata.skillList["0/9"].Level > 0;
        } else if (skill == 2) {
            return PlayerData.playerdata.skillList["0/10"].Level > 0;
        } else if (skill == 4) {
            return PlayerData.playerdata.skillList["5/10"].Level > 0;
        } else if (skill == 5) {
            return PlayerData.playerdata.skillList["6/11"].Level > 0;
        } else if (skill == 9) {
            return PlayerData.playerdata.skillList["4/9"].Level > 0;
        }
        return false;
    }

    public bool ActivateFirmGrip()
    {
        if (firmGripCooldown <= 0f) {
            if (PlayerData.playerdata.skillList["0/9"].Level == 1) {
                firmGripCooldown = FIRM_GRIP_COOLDOWN_1;
                firmGripTimer = FIRM_GRIP_TIME_1;
                return true;
            } else if (PlayerData.playerdata.skillList["0/9"].Level == 2) {
                firmGripCooldown = FIRM_GRIP_COOLDOWN_2;
                firmGripTimer = FIRM_GRIP_TIME_2;
                return true;
            } else if (PlayerData.playerdata.skillList["0/9"].Level == 3) {
                firmGripCooldown = FIRM_GRIP_COOLDOWN_3;
                firmGripTimer = FIRM_GRIP_TIME_3;
                return true;
            }
        }
        return false;
    }

    public bool ActivateSnipersDel()
    {
        if (snipersDelCooldown <= 0f) {
            if (PlayerData.playerdata.skillList["5/10"].Level == 1) {
                snipersDelCooldown = SNIPERS_DEL_COOLDOWN_1;
                snipersDelTimer = SNIPERS_DEL_TIME_1;
                return true;
            } else if (PlayerData.playerdata.skillList["5/10"].Level == 2) {
                snipersDelCooldown = SNIPERS_DEL_COOLDOWN_2;
                snipersDelTimer = SNIPERS_DEL_TIME_2;
                return true;
            } else if (PlayerData.playerdata.skillList["5/10"].Level == 3) {
                snipersDelCooldown = SNIPERS_DEL_COOLDOWN_3;
                snipersDelTimer = SNIPERS_DEL_TIME_3;
                return true;
            }
        }
        return false;
    }

    public bool ActivateBulletStream()
    {
        if (bulletStreamCooldown <= 0f) {
            if (PlayerData.playerdata.skillList["6/11"].Level == 1) {
                bulletStreamCooldown = BULLET_STREAM_COOLDOWN_1;
                bulletStreamTimer = BULLET_STREAM_TIME_1;
                return true;
            } else if (PlayerData.playerdata.skillList["6/11"].Level == 2) {
                bulletStreamCooldown = BULLET_STREAM_COOLDOWN_2;
                bulletStreamTimer = BULLET_STREAM_TIME_2;
                return true;
            } else if (PlayerData.playerdata.skillList["6/11"].Level == 3) {
                bulletStreamCooldown = BULLET_STREAM_COOLDOWN_3;
                bulletStreamTimer = BULLET_STREAM_TIME_3;
                return true;
            } else if (PlayerData.playerdata.skillList["6/11"].Level == 4) {
                bulletStreamCooldown = BULLET_STREAM_COOLDOWN_4;
                bulletStreamTimer = BULLET_STREAM_TIME_4;
                return true;
            }
        }
        return false;
    }

    public bool ActivateRampage()
    {
        if (rampageCooldown <= 0f) {
            if (PlayerData.playerdata.skillList["0/10"].Level == 1) {
                rampageCooldown = RAMPAGE_COOLDOWN_1;
                rampageTimer = RAMPAGE_TIME_1;
                return true;
            } else if (PlayerData.playerdata.skillList["0/10"].Level == 2) {
                rampageCooldown = RAMPAGE_COOLDOWN_2;
                rampageTimer = RAMPAGE_TIME_2;
                return true;
            } else if (PlayerData.playerdata.skillList["0/10"].Level == 3) {
                rampageCooldown = RAMPAGE_COOLDOWN_3;
                rampageTimer = RAMPAGE_TIME_3;
                return true;
            }
        }
        return false;
    }

    void UpdateFirmGrip()
    {
        if (firmGripTimer > 0f) {
            firmGripTimer -= Time.deltaTime;
        }
        if (firmGripCooldown > 0f && firmGripTimer <= 0f) {
            firmGripCooldown -= Time.deltaTime;
        }
    }

    void UpdateSnipersDel()
    {
        if (snipersDelTimer > 0f) {
            snipersDelTimer -= Time.deltaTime;
        }
        if (snipersDelCooldown > 0f && snipersDelTimer <= 0f) {
            snipersDelCooldown -= Time.deltaTime;
        }
    }

    void UpdateBulletStream()
    {
        if (bulletStreamTimer > 0f) {
            bulletStreamTimer -= Time.deltaTime;
        }
        if (bulletStreamCooldown > 0f && bulletStreamTimer <= 0f) {
            bulletStreamCooldown -= Time.deltaTime;
        }
    }

    void UpdateRampage()
    {
        if (rampageTimer > 0f) {
            rampageTimer -= Time.deltaTime;
        }
        if (rampageCooldown > 0f && rampageTimer <= 0f) {
            rampageCooldown -= Time.deltaTime;
        }
    }

    public float GetFirmGripBoost()
    {
        if (firmGripTimer > 0f) {
            if (PlayerData.playerdata.skillList["0/9"].Level == 1) {
                return 0.5f;
            } else if (PlayerData.playerdata.skillList["0/9"].Level == 2) {
                return 0.7f;
            } else if (PlayerData.playerdata.skillList["0/9"].Level == 3) {
                return 0.95f;
            }
        }
        return 0f;
    }

    public bool GetSnipersDelBoost()
    {
        if (snipersDelTimer > 0f) {
            if (PlayerData.playerdata.skillList["5/10"].Level > 0) {
                return true;
            }
        }
        return false;
    }

    public bool GetBulletStreamBoost()
    {
        if (bulletStreamTimer > 0f) {
            if (PlayerData.playerdata.skillList["6/11"].Level > 0) {
                return true;
            }
        }
        return false;
    }

    public bool GetRampageBoost()
    {
        if (rampageTimer > 0f) {
            if (PlayerData.playerdata.skillList["0/10"].Level > 0) {
                return true;
            }
        }
        return false;
    }

    // END ACTIVE SKILLS

    public int GetRegeneratorLevel()
    {
        return PlayerData.playerdata.skillList["4/5"].Level;
    }

    public int GetThisRegeneratorLevel()
    {
        return thisRegeneratorLevel;
    }

    public void SetThisRegeneratorLevel(int val)
    {
        thisRegeneratorLevel = val;
    }

    public void ActivateRegenerator(bool b)
    {
        if (b) {
            if (!thisRegeneratorActive) {
                if (GetThisRegeneratorLevel() == 1) {
                    thisRegeneratorTimer = REGENERATOR_TIMER_1;
                } else if (GetThisRegeneratorLevel() == 2) {
                    thisRegeneratorTimer = REGENERATOR_TIMER_2;
                } else if (GetThisRegeneratorLevel() == 3) {
                    thisRegeneratorTimer = REGENERATOR_TIMER_3;
                } else if (GetThisRegeneratorLevel() == 4) {
                    thisRegeneratorTimer = REGENERATOR_TIMER_4;
                } else if (GetThisRegeneratorLevel() == 5) {
                    thisRegeneratorTimer = REGENERATOR_TIMER_5;
                }
                thisRegeneratorActive = true;
            }
        } else {
            thisRegeneratorTimer = 0f;
            thisRegeneratorActive = false;
        }
    }

    public int GetRegeneratorRecoveryAmount()
    {
        int retAmt = 0;
        if (thisRegeneratorTimer <= 0f) {
            if (GetThisRegeneratorLevel() == 1) {
                retAmt = REGENERATOR_RECOVER_1;
                thisRegeneratorTimer = REGENERATOR_TIMER_1;
            } else if (GetThisRegeneratorLevel() == 2) {
                retAmt = REGENERATOR_RECOVER_2;
                thisRegeneratorTimer = REGENERATOR_TIMER_2;
            } else if (GetThisRegeneratorLevel() == 3) {
                retAmt = REGENERATOR_RECOVER_3;
                thisRegeneratorTimer = REGENERATOR_TIMER_3;
            } else if (GetThisRegeneratorLevel() == 4) {
                retAmt = REGENERATOR_RECOVER_4;
                thisRegeneratorTimer = REGENERATOR_TIMER_4;
            } else if (GetThisRegeneratorLevel() == 5) {
                retAmt = REGENERATOR_RECOVER_5;
                thisRegeneratorTimer = REGENERATOR_TIMER_5;
            }
        }
        return retAmt;
    }

    public void AddRegenerator(int playerId)
    {
        regeneratorPlayerIds.AddLast(playerId);
    }

    public void RemoveRegenerator(int playerId)
    {
        regeneratorPlayerIds.Remove(playerId);
    }

    void UpdateRegenerator()
    {
        if (thisRegeneratorActive) {
            thisRegeneratorTimer -= Time.deltaTime;
        }
    }

    public int GetPainkillerLevel()
    {
        return PlayerData.playerdata.skillList["4/6"].Level;
    }

    private float GetPainkillerAmount()
    {
        if (thisPainkillerActive && thisPainkillerTimer <= 0f) {
            if (GetThisPainkillerLevel() == 1) {
                return 0.01f;
            } else if (GetThisPainkillerLevel() == 2) {
                return 0.02f;
            } else if (GetThisPainkillerLevel() == 3) {
                return 0.04f;
            } else if (GetThisPainkillerLevel() == 4) {
                return 0.05f;
            }
        }
        return 0f;
    }

    public int GetThisPainkillerLevel()
    {
        return thisPainkillerLevel;
    }

    public void SetThisPainkillerLevel(int val)
    {
        thisPainkillerLevel = val;
    }

    public void ActivatePainkiller(bool b)
    {
        if (b) {
            if (!thisPainkillerActive) {
                if (GetThisPainkillerLevel() == 1) {
                    thisPainkillerTimer = PAINKILLERS_TIMER_1;
                } else if (GetThisPainkillerLevel() == 2) {
                    thisPainkillerTimer = PAINKILLERS_TIMER_2;
                } else if (GetThisPainkillerLevel() == 3) {
                    thisPainkillerTimer = PAINKILLERS_TIMER_3;
                } else if (GetThisPainkillerLevel() == 4) {
                    thisPainkillerTimer = PAINKILLERS_TIMER_4;
                }
                thisPainkillerActive = true;
            }
        } else {
            thisPainkillerTimer = 0f;
            thisPainkillerActive = false;
        }
    }

    public float GetPainkillerTotalAmount()
    {
        float totalAmt = 0f;
        LinkedList<int>.Enumerator ids = painkillerPlayerIds.GetEnumerator();
        try {
            while (ids.MoveNext()) {
                SkillController painkillerSkillController = GameControllerScript.playerList[ids.Current].objRef.GetComponent<SkillController>();
                totalAmt += painkillerSkillController.GetPainkillerAmount();
            }
        } catch (Exception e) {
            Debug.LogError("Exception occurred in [GetPainkillerTotalAmount]: " + e.Message);
        }
        return Mathf.Min(0.95f, totalAmt);
    }

    public void AddPainkiller(int playerId)
    {
        painkillerPlayerIds.AddLast(playerId);
    }

    public void RemovePainkiller(int playerId)
    {
        painkillerPlayerIds.Remove(playerId);
    }

    void UpdatePainkiller()
    {
        if (thisPainkillerActive) {
            if (thisPainkillerTimer > 0f) {
                thisPainkillerTimer -= Time.deltaTime;
            }
        }
    }

    public bool CanCallGuardianAngel()
    {
        return PlayerData.playerdata.skillList["4/9"].Level > 0 && GetComponent<PlayerActionScript>().GetGuardianAngelsRemaining() > 0;
    }

    public int GetMaxGuardianAngels()
    {
        if (PlayerData.playerdata.skillList["4/9"].Level == 1) {
            return 1;
        } else if (PlayerData.playerdata.skillList["4/9"].Level == 2) {
            return 2;
        } else if (PlayerData.playerdata.skillList["4/9"].Level == 3) {
            return 3;
        } else if (PlayerData.playerdata.skillList["4/9"].Level == 4) {
            return 4;
        }
        return 0;
    }

    public bool CanHealPlayers()
    {
        return PlayerData.playerdata.skillList["4/8"].Level > 0;
    }

    public int GetFlatlineHealAmount()
    {
        if (PlayerData.playerdata.skillList["4/8"].Level == 1) {
            return 5;
        } else if (PlayerData.playerdata.skillList["4/8"].Level == 2) {
            return 6;
        } else if (PlayerData.playerdata.skillList["4/8"].Level == 3) {
            return 7;
        } else if (PlayerData.playerdata.skillList["4/8"].Level == 4) {
            return 8;
        }
        return 0;
    }

    public int GetFlatlineSacrificeAmount()
    {
        if (PlayerData.playerdata.skillList["4/8"].Level == 1) {
            return 14;
        } else if (PlayerData.playerdata.skillList["4/8"].Level == 2) {
            return 12;
        } else if (PlayerData.playerdata.skillList["4/8"].Level == 3) {
            return 10;
        } else if (PlayerData.playerdata.skillList["4/8"].Level == 4) {
            return 8;
        }
        return 0;
    }

    public int GetMotivateHealthTrigger()
    {
        if (PlayerData.playerdata.skillList["3/3"].Level == 1) {
            return 20;
        } else if (PlayerData.playerdata.skillList["3/3"].Level == 2) {
            return 30;
        } else if (PlayerData.playerdata.skillList["3/3"].Level == 3) {
            return 50;
        } else if (PlayerData.playerdata.skillList["3/3"].Level == 4) {
            return 60;
        }
        return 0;
    }

    public float GetMotivateDamageBoost()
    {
        if (PlayerData.playerdata.skillList["3/3"].Level == 1) {
            return 0.01f;
        } else if (PlayerData.playerdata.skillList["3/3"].Level == 2) {
            return 0.02f;
        } else if (PlayerData.playerdata.skillList["3/3"].Level == 3) {
            return 0.03f;
        } else if (PlayerData.playerdata.skillList["3/3"].Level == 4) {
            return 0.04f;
        }
        return 0f;
    }

    public void AddMotivateBoost(int fromActorNo, float damageBoost)
    {
        MotivateNode n = new MotivateNode();
        n.actorNo = fromActorNo;
        n.damageBoost = damageBoost;
        motivateBoosts.Add(n);
    }

    public float RemoveMotivateBoost(int actorNo)
    {
        int removeIndex = -1;
        float retVal = 0f;
        for (int i = 0; i < motivateBoosts.Count; i++) {
            MotivateNode n = (MotivateNode)motivateBoosts[i];
            if (n.actorNo == actorNo) {
                removeIndex = i;
                retVal = n.damageBoost;
                break;
            }
        }
        if (removeIndex != -1) {
            motivateBoosts.RemoveAt(removeIndex);
        }
        return retVal;
    }

    public void AddToMotivateDamageBoost(float dmg)
    {
        motivateDamageBoost += dmg;
    }

    public void RemoveFromMotivateDamageBoost(float dmg)
    {
        motivateDamageBoost -= dmg;
    }

    public float GetMyMotivateDamageBoost()
    {
        return Mathf.Clamp(motivateDamageBoost, 0f, MOTIVATE_BOOST_CAP);
    }

    public string SerializeMotivateBoosts()
    {
        string s = "";
        for (int i = 0; i < motivateBoosts.Count; i++) {
            if (i != 0) {
                s += ",";
            }
            MotivateNode n = (MotivateNode)motivateBoosts[i];
            s += (n.actorNo + "|" + n.damageBoost);
        }
        return s;
    }

    public void SyncMotivateBoost(ArrayList motivateNodes, float motivateDamageBoost)
    {
        motivateBoosts = motivateNodes;
        this.motivateDamageBoost = motivateDamageBoost;
    }

    public float GetFightingSpiritTime()
    {
        if (PlayerData.playerdata.skillList["3/8"].Level == 1) {
            return 5f;
        } else if (PlayerData.playerdata.skillList["3/8"].Level == 2) {
            return 10f;
        } else if (PlayerData.playerdata.skillList["3/8"].Level == 3) {
            return 20f;
        } else if (PlayerData.playerdata.skillList["3/8"].Level == 4) {
            return 30f;
        }
        return 0f;
    }

    public float GetAvoidabilityBoost()
    {
        return avoidabilityBoost;
    }

    void InitializeShadowSightBoost()
    {
        if (PlayerData.playerdata.skillList["1/12"].Level == 1) {
            avoidabilityBoost = 0.1f;
        } else if (PlayerData.playerdata.skillList["1/12"].Level == 2) {
            avoidabilityBoost = 0.2f;
        } else if (PlayerData.playerdata.skillList["1/12"].Level == 3) {
            avoidabilityBoost = 0.3f;
        } else {
            avoidabilityBoost = 0f;
        }
    }

    public float GetMyAvoidabilityBoost()
    {
        if (PlayerData.playerdata.skillList["1/7"].Level == 1) {
            return 0.1f;
        } else if (PlayerData.playerdata.skillList["1/7"].Level == 2) {
            return 0.25f;
        } else if (PlayerData.playerdata.skillList["1/7"].Level == 3) {
            return 0.5f;
        }
        return 0f;
    }

    public void SetThisPlayerAvoidabilityBoost(float val)
    {
        storedAvoidabilityBoost = val;
    }

    public float GetThisPlayerAvoidabilityBoost()
    {
        return storedAvoidabilityBoost;
    }

    private void InitializeRunNGun()
    {
        runNGun = (PlayerData.playerdata.skillList["1/13"].Level > 0);
    }

    public bool HasRunNGun()
    {
        return runNGun;
    }

    private void InitializeJetpackBoost()
    {
        jetpackBoost = (PlayerData.playerdata.skillList["2/13"].Level > 0);
    }

    public bool HasJetpackBoost()
    {
        return jetpackBoost;
    }

    private void InitializeRusticCowboy()
    {
        rusticCowboy = (PlayerData.playerdata.skillList["0/12"].Level > 0);
    }

    public bool HasRusticCowboy()
    {
        return rusticCowboy;
    }

    public struct MotivateNode {
        public int actorNo;
        public float damageBoost;
    }

}
