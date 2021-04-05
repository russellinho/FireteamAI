using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using HttpsCallableReference = Firebase.Functions.HttpsCallableReference;

public class SkillManager : MonoBehaviour
{
    public TitleControllerScript titleController;
    public TextMeshProUGUI primaryTreeTxt;
    private bool setPrimaryTreeFlag;
    private int primaryTreeIndex;
    private bool refreshActiveSkillsFlag;
    [SerializeField]
    private SkillSlot[] commandoSkills;
    public GameObject commandoClassIndicator;
    [SerializeField]
    private SkillSlot[] reconSkills;
    public GameObject reconClassIndicator;
    [SerializeField]
    private SkillSlot[] engineerSkills;
    public GameObject engineerClassIndicator;
    [SerializeField]
    private SkillSlot[] mastermindSkills;
    public GameObject mastermindClassIndicator;
    [SerializeField]
    private SkillSlot[] medicSkills;
    public GameObject medicClassIndicator;
    [SerializeField]
    private SkillSlot[] marksmanSkills;
    public GameObject marksmanClassIndicator;
    [SerializeField]
    private SkillSlot[] heavySkills;
    public GameObject heavyClassIndicator;

    void Update()
    {
        if (setPrimaryTreeFlag) {
            SetPrimaryTree(primaryTreeIndex);
            primaryTreeIndex = -1;
            setPrimaryTreeFlag = false;
        }
        if (refreshActiveSkillsFlag) {
            RefreshActiveSkills();
            refreshActiveSkillsFlag = false;
        }
    }

    public SkillSlot GetSkillSlot(int tree, int skill)
    {
        switch (tree) {
            case 0:
                return commandoSkills[skill];
            case 1:
                return reconSkills[skill];
            case 2:
                return engineerSkills[skill];
            case 3:
                return mastermindSkills[skill];
            case 4:
                return medicSkills[skill];
            case 5:
                return marksmanSkills[skill];
            case 6:
                return heavySkills[skill];
            default:
                break;
        }
        return null;
    }

    public void AddSkillPoint(int treeId, int skillId)
    {
        if (PlayerData.playerdata.info.AvailableSkillPoints == 0 || PlayerData.playerdata.skillList[treeId + "/" + skillId].Level == GetMaxLevelForSkill(treeId, skillId)) {
            return;
        }
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["tree"] = treeId;
        inputData["skill"] = skillId;
        
        titleController.TriggerBlockScreen(true);
		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("addSkillPoint");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                PlayerData.playerdata.TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() == "200") {
                    int[] potentialUnlocks = GetSkillsThisIsPrereqFor(treeId, skillId);
                    for (int i = 0; i < potentialUnlocks.Length; i++) {
                        if (SkillCanBeUnlocked(treeId, potentialUnlocks[i])) {
                            GetSkillSlot(treeId, potentialUnlocks[i]).DelayToggleSkillEnabled(false);
                        }
                    }
                } else {
                    titleController.TriggerAlertPopup("SKILL POINT COULD NOT BE ADDED.");
                }
            }
            titleController.TriggerBlockScreen(false);
        });
    }

    public int GetMaxLevelForSkill(int treeId, int skillId)
    {
        if (treeId == 0) {
            if (skillId == 0) {
                return 3;
            } else if (skillId == 1) {
                return 3;
            } else if (skillId == 2) {
                return 3;
            } else if (skillId == 3) {
                return 3;
            } else if (skillId == 4) {
                return 3;
            } else if (skillId == 5) {
                return 3;
            } else if (skillId == 6) {
                return 4;
            } else if (skillId == 7) {
                return 3;
            } else if (skillId == 8) {
                return 3;
            } else if (skillId == 9) {
                return 3;
            } else if (skillId == 10) {
                return 3;
            } else if (skillId == 11) {
                return 3;
            } else if (skillId == 12) {
                return 1;
            }
        // Recon
        } else if (treeId == 1) {
            if (skillId == 0) {
                return 3;
            } else if (skillId == 1) {
                return 3;
            } else if (skillId == 2) {
                return 3;
            } else if (skillId == 3) {
                return 3;
            } else if (skillId == 4) {
                return 2;
            } else if (skillId == 5) {
                return 3;
            } else if (skillId == 6) {
                return 3;
            } else if (skillId == 7) {
                return 3;
            } else if (skillId == 8) {
                return 3;
            } else if (skillId == 9) {
                return 3;
            } else if (skillId == 10) {
                return 3;
            } else if (skillId == 11) {
                return 3;
            } else if (skillId == 12) {
                return 3;
            } else if (skillId == 13) {
                return 1;
            }
        // Engineer
        } else if (treeId == 2) {
            if (skillId == 0) {
                return 3;
            } else if (skillId == 1) {
                return 3;
            } else if (skillId == 2) {
                return 3;
            } else if (skillId == 3) {
                return 3;
            } else if (skillId == 4) {
                return 3;
            } else if (skillId == 5) {
                return 3;
            } else if (skillId == 6) {
                return 3;
            } else if (skillId == 7) {
                return 3;
            } else if (skillId == 8) {
                return 3;
            } else if (skillId == 9) {
                return 3;
            } else if (skillId == 10) {
                return 3;
            } else if (skillId == 11) {
                return 3;
            } else if (skillId == 12) {
                return 3;
            } else if (skillId == 13) {
                return 1;
            }
        // Mastermind
        } else if (treeId == 3) {
            if (skillId == 0) {
                return 3;
            } else if (skillId == 1) {
                return 4;
            } else if (skillId == 2) {
                return 4;
            } else if (skillId == 3) {
                return 4;
            } else if (skillId == 4) {
                return 3;
            } else if (skillId == 5) {
                return 3;
            } else if (skillId == 6) {
                return 4;
            } else if (skillId == 7) {
                return 3;
            } else if (skillId == 8) {
                return 4;
            } else if (skillId == 9) {
                return 3;
            } else if (skillId == 10) {
                return 4;
            }
        // Medic
        } else if (treeId == 4) {
            if (skillId == 0) {
                return 3;
            } else if (skillId == 1) {
                return 3;
            } else if (skillId == 2) {
                return 5;
            } else if (skillId == 3) {
                return 3;
            } else if (skillId == 4) {
                return 3;
            } else if (skillId == 5) {
                return 5;
            } else if (skillId == 6) {
                return 4;
            } else if (skillId == 7) {
                return 4;
            } else if (skillId == 8) {
                return 4;
            } else if (skillId == 9) {
                return 4;
            }
        // Marksman
        } else if (treeId == 5) {
            if (skillId == 0) {
                return 3;
            } else if (skillId == 1) {
                return 3;
            } else if (skillId == 2) {
                return 3;
            } else if (skillId == 3) {
                return 3;
            } else if (skillId == 4) {
                return 3;
            } else if (skillId == 5) {
                return 3;
            } else if (skillId == 6) {
                return 4;
            } else if (skillId == 7) {
                return 3;
            } else if (skillId == 8) {
                return 4;
            } else if (skillId == 9) {
                return 4;
            } else if (skillId == 10) {
                return 3;
            } else if (skillId == 11) {
                return 3;
            }
        // Heavy
        } else if (treeId == 6) {
            if (skillId == 0) {
                return 3;
            } else if (skillId == 1) {
                return 3;
            } else if (skillId == 2) {
                return 3;
            } else if (skillId == 3) {
                return 3;
            } else if (skillId == 4) {
                return 4;
            } else if (skillId == 5) {
                return 3;
            } else if (skillId == 6) {
                return 3;
            } else if (skillId == 7) {
                return 3;
            } else if (skillId == 8) {
                return 3;
            } else if (skillId == 9) {
                return 3;
            } else if (skillId == 10) {
                return 3;
            } else if (skillId == 11) {
                return 4;
            }
        }
        return 0;
    }

    public int[] GetPrerequisitesForSkill(int treeId, int skillId)
    {
        // Commando
        if (treeId == 0) {
            if (skillId == 0) {
                return new int[0];
            } else if (skillId == 1) {
                return new int[0];
            } else if (skillId == 2) {
                return new int[1]{0};
            } else if (skillId == 3) {
                return new int[1]{1};
            } else if (skillId == 4) {
                return new int[2]{2, 3};
            } else if (skillId == 5) {
                return new int[2]{2, 3};
            } else if (skillId == 6) {
                return new int[2]{2, 3};
            } else if (skillId == 7) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 8) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 9) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 10) {
                return new int[3]{7, 8, 9};
            } else if (skillId == 11) {
                return new int[3]{7, 8, 9};
            } else if (skillId == 12) {
            return new int[2]{10, 11};
            }
        // Recon
        } else if (treeId == 1) {
            if (skillId == 0) {
                return new int[0];
            } else if (skillId == 1) {
                return new int[0];
            } else if (skillId == 2) {
                return new int[1]{0};
            } else if (skillId == 3) {
                return new int[1]{1};
            } else if (skillId == 4) {
                return new int[2]{2, 3};
            } else if (skillId == 5) {
                return new int[2]{2, 3};
            } else if (skillId == 6) {
                return new int[2]{2, 3};
            } else if (skillId == 7) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 8) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 9) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 10) {
                return new int[3]{7, 8, 9};
            } else if (skillId == 11) {
                return new int[3]{7, 8, 9};
            } else if (skillId == 12) {
                return new int[3]{7, 8, 9};
            } else if (skillId == 13) {
                return new int[3]{10, 11, 12};
            }
        // Engineer
        } else if (treeId == 2) {
            if (skillId == 0) {
                return new int[0];
            } else if (skillId == 1) {
                return new int[0];
            } else if (skillId == 2) {
                return new int[2]{0, 1};
            } else if (skillId == 3) {
                return new int[2]{0, 1};
            } else if (skillId == 4) {
                return new int[2]{0, 1};
            } else if (skillId == 5) {
                return new int[3]{2, 3, 4};
            } else if (skillId == 6) {
                return new int[3]{2, 3, 4};
            } else if (skillId == 7) {
                return new int[3]{2, 3, 4};
            } else if (skillId == 8) {
                return new int[3]{5, 6, 7};
            } else if (skillId == 9) {
                return new int[3]{5, 6, 7};
            } else if (skillId == 10) {
                return new int[3]{5, 6, 7};
            } else if (skillId == 11) {
                return new int[3]{8, 9, 10};
            } else if (skillId == 12) {
                return new int[3]{8, 9, 10};
            } else if (skillId == 13) {
                return new int[3]{8, 9, 10};
            }
        // Mastermind
        } else if (treeId == 3) {
            if (skillId == 0) {
                return new int[0];
            } else if (skillId == 1) {
                return new int[1]{0};
            } else if (skillId == 2) {
                return new int[1]{0};
            } else if (skillId == 3) {
                return new int[1]{0};
            } else if (skillId == 4) {
                return new int[1]{0};
            } else if (skillId == 5) {
                return new int[4]{1, 2, 3, 4};
            } else if (skillId == 6) {
                return new int[4]{1, 2, 3, 4};
            } else if (skillId == 7) {
                return new int[4]{1, 2, 3, 4};
            } else if (skillId == 8) {
                return new int[3]{5, 6, 7};
            } else if (skillId == 9) {
                return new int[3]{5, 6, 7};
            } else if (skillId == 10) {
                return new int[2]{8, 9};
            }
        // Medic
        } else if (treeId == 4) {
            if (skillId == 0) {
                return new int[0];
            } else if (skillId == 1) {
                return new int[0];
            } else if (skillId == 2) {
                return new int[2]{0, 1};
            } else if (skillId == 3) {
                return new int[2]{0, 1};
            } else if (skillId == 4) {
                return new int[2]{0, 1};
            } else if (skillId == 5) {
                return new int[3]{2, 3, 4};
            } else if (skillId == 6) {
                return new int[3]{2, 3, 4};
            } else if (skillId == 7) {
                return new int[3]{2, 3, 4};
            } else if (skillId == 8) {
                return new int[3]{5, 6, 7};
            } else if (skillId == 9) {
                return new int[3]{5, 6, 7};
            }
        // Marksman
        } else if (treeId == 5) {
            if (skillId == 0) {
                return new int[0];
            } else if (skillId == 1) {
                return new int[0];
            } else if (skillId == 2) {
                return new int[1]{0};
            } else if (skillId == 3) {
                return new int[1]{1};
            } else if (skillId == 4) {
                return new int[2]{2, 3};
            } else if (skillId == 5) {
                return new int[2]{2, 3};
            } else if (skillId == 6) {
                return new int[2]{2, 3};
            } else if (skillId == 7) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 8) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 9) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 10) {
                return new int[3]{7, 8, 9};
            } else if (skillId == 11) {
                return new int[3]{7, 8, 9};
            }
        // Heavy
        } else if (treeId == 6) {
            if (skillId == 0) {
                return new int[0];
            } else if (skillId == 1) {
                return new int[0];
            } else if (skillId == 2) {
                return new int[1]{0};
            } else if (skillId == 3) {
                return new int[1]{1};
            } else if (skillId == 4) {
                return new int[2]{2, 3};
            } else if (skillId == 5) {
                return new int[2]{2, 3};
            } else if (skillId == 6) {
                return new int[2]{2, 3};
            } else if (skillId == 7) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 8) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 9) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 10) {
                return new int[3]{7, 8, 9};
            } else if (skillId == 11) {
                return new int[3]{7, 8, 9};
            }
        }
        return new int[0];
    }

    public int[] GetSkillsThisIsPrereqFor(int treeId, int skillId)
    {
        // Commando
        if (treeId == 0) {
            if (skillId == 0) {
                return new int[1]{2};
            } else if (skillId == 1) {
                return new int[1]{3};
            } else if (skillId == 2) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 3) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 4) {
                return new int[3]{7, 8, 9};
            } else if (skillId == 5) {
                return new int[3]{7, 8, 9};
            } else if (skillId == 6) {
                return new int[3]{7, 8, 9};
            } else if (skillId == 7) {
                return new int[2]{10, 11};
            } else if (skillId == 8) {
                return new int[2]{10, 11};
            } else if (skillId == 9) {
                return new int[2]{10, 11};
            } else if (skillId == 10) {
                return new int[1]{12};
            } else if (skillId == 11) {
                return new int[1]{12};
            } else if (skillId == 12) {
            return new int[0];
            }
        // Recon
        } else if (treeId == 1) {
            if (skillId == 0) {
                return new int[1]{2};
            } else if (skillId == 1) {
                return new int[1]{3};
            } else if (skillId == 2) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 3) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 4) {
                return new int[3]{7, 8, 9};
            } else if (skillId == 5) {
                return new int[3]{7, 8, 9};
            } else if (skillId == 6) {
                return new int[3]{7, 8, 9};
            } else if (skillId == 7) {
                return new int[3]{10, 11, 12};
            } else if (skillId == 8) {
                return new int[3]{10, 11, 12};
            } else if (skillId == 9) {
                return new int[3]{10, 11, 12};
            } else if (skillId == 10) {
                return new int[1]{13};
            } else if (skillId == 11) {
                return new int[1]{13};
            } else if (skillId == 12) {
                return new int[1]{13};
            } else if (skillId == 13) {
                return new int[0];
            }
        // Engineer
        } else if (treeId == 2) {
            if (skillId == 0) {
                return new int[3]{2, 3, 4};
            } else if (skillId == 1) {
                return new int[3]{2, 3, 4};
            } else if (skillId == 2) {
                return new int[3]{5, 6, 7};
            } else if (skillId == 3) {
                return new int[3]{5, 6, 7};
            } else if (skillId == 4) {
                return new int[3]{5, 6, 7};
            } else if (skillId == 5) {
                return new int[3]{8, 9, 10};
            } else if (skillId == 6) {
                return new int[3]{8, 9, 10};
            } else if (skillId == 7) {
                return new int[3]{8, 9, 10};
            } else if (skillId == 8) {
                return new int[3]{11, 12, 13};
            } else if (skillId == 9) {
                return new int[3]{11, 12, 13};
            } else if (skillId == 10) {
                return new int[3]{11, 12, 13};
            } else if (skillId == 11) {
                return new int[0];
            } else if (skillId == 12) {
                return new int[0];
            } else if (skillId == 13) {
                return new int[0];
            }
        // Mastermind
        } else if (treeId == 3) {
            if (skillId == 0) {
                return new int[4]{1, 2, 3, 4};
            } else if (skillId == 1) {
                return new int[3]{5, 6, 7};
            } else if (skillId == 2) {
                return new int[3]{5, 6, 7};
            } else if (skillId == 3) {
                return new int[3]{5, 6, 7};
            } else if (skillId == 4) {
                return new int[3]{5, 6, 7};
            } else if (skillId == 5) {
                return new int[2]{8, 9};
            } else if (skillId == 6) {
                return new int[2]{8, 9};
            } else if (skillId == 7) {
                return new int[2]{8, 9};
            } else if (skillId == 8) {
                return new int[1]{10};
            } else if (skillId == 9) {
                return new int[1]{10};
            } else if (skillId == 10) {
                return new int[0];
            }
        // Medic
        } else if (treeId == 4) {
            if (skillId == 0) {
                return new int[3]{2, 3, 4};
            } else if (skillId == 1) {
                return new int[3]{2, 3, 4};
            } else if (skillId == 2) {
                return new int[3]{5, 6, 7};
            } else if (skillId == 3) {
                return new int[3]{5, 6, 7};
            } else if (skillId == 4) {
                return new int[3]{5, 6, 7};
            } else if (skillId == 5) {
                return new int[2]{8, 9};
            } else if (skillId == 6) {
                return new int[2]{8, 9};
            } else if (skillId == 7) {
                return new int[2]{8, 9};
            } else if (skillId == 8) {
                return new int[0];
            } else if (skillId == 9) {
                return new int[0];
            }
        // Marksman
        } else if (treeId == 5) {
            if (skillId == 0) {
                return new int[1]{2};
            } else if (skillId == 1) {
                return new int[1]{3};
            } else if (skillId == 2) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 3) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 4) {
                return new int[3]{7, 8, 9};
            } else if (skillId == 5) {
                return new int[3]{7, 8, 9};
            } else if (skillId == 6) {
                return new int[3]{7, 8, 9};
            } else if (skillId == 7) {
                return new int[2]{10, 11};
            } else if (skillId == 8) {
                return new int[2]{10, 11};
            } else if (skillId == 9) {
                return new int[2]{10, 11};
            } else if (skillId == 10) {
                return new int[0];
            } else if (skillId == 11) {
                return new int[0];
            }
        // Heavy
        } else if (treeId == 6) {
            if (skillId == 0) {
                return new int[0];
            } else if (skillId == 1) {
                return new int[0];
            } else if (skillId == 2) {
                return new int[1]{0};
            } else if (skillId == 3) {
                return new int[1]{1};
            } else if (skillId == 4) {
                return new int[2]{2, 3};
            } else if (skillId == 5) {
                return new int[2]{2, 3};
            } else if (skillId == 6) {
                return new int[2]{2, 3};
            } else if (skillId == 7) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 8) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 9) {
                return new int[3]{4, 5, 6};
            } else if (skillId == 10) {
                return new int[3]{7, 8, 9};
            } else if (skillId == 11) {
                return new int[3]{7, 8, 9};
            }
        }
        return new int[0];
    }

    public bool SkillCanBeUnlocked(int treeId, int skillId)
    {
        int[] thisPrereqs = GetPrerequisitesForSkill(treeId, skillId);
        for (int i = 0; i < thisPrereqs.Length; i++) {
            int p = thisPrereqs[i];
            if (PlayerData.playerdata.skillList[treeId + "/" + p].Level != GetMaxLevelForSkill(treeId, p)) {
                return false;
            }
        }
        return true;
    }

    public void DelayRefreshActiveSkills()
    {
        refreshActiveSkillsFlag = true;
    }

    void RefreshActiveSkills()
    {
        foreach (SkillSlot s in commandoSkills) {
            int[] prereqs = s.GetPrerequisitesForThisSkill();
            bool met = true;
            for (int i = 0; i < prereqs.Length; i++) {
                if (PlayerData.playerdata.skillList["0/" + prereqs[i]].Level != GetMaxLevelForSkill(0, prereqs[i])) {
                    s.ToggleSkillEnabled(true);
                    met = false;
                    break;
                }
            }
            if (met) {
                s.ToggleSkillEnabled(false);
            }
        }

        foreach (SkillSlot s in reconSkills) {
            int[] prereqs = s.GetPrerequisitesForThisSkill();
            bool met = true;
            for (int i = 0; i < prereqs.Length; i++) {
                if (PlayerData.playerdata.skillList["1/" + prereqs[i]].Level != GetMaxLevelForSkill(1, prereqs[i])) {
                    s.ToggleSkillEnabled(true);
                    met = false;
                    break;
                }
            }
            if (met) {
                s.ToggleSkillEnabled(false);
            }
        }

        foreach (SkillSlot s in engineerSkills) {
            int[] prereqs = s.GetPrerequisitesForThisSkill();
            bool met = true;
            for (int i = 0; i < prereqs.Length; i++) {
                if (PlayerData.playerdata.skillList["2/" + prereqs[i]].Level != GetMaxLevelForSkill(2, prereqs[i])) {
                    s.ToggleSkillEnabled(true);
                    met = false;
                    break;
                }
            }
            if (met) {
                s.ToggleSkillEnabled(false);
            }
        }

        foreach (SkillSlot s in mastermindSkills) {
            int[] prereqs = s.GetPrerequisitesForThisSkill();
            bool met = true;
            for (int i = 0; i < prereqs.Length; i++) {
                if (PlayerData.playerdata.skillList["3/" + prereqs[i]].Level != GetMaxLevelForSkill(3, prereqs[i])) {
                    s.ToggleSkillEnabled(true);
                    met = false;
                    break;
                }
            }
            if (met) {
                s.ToggleSkillEnabled(false);
            }
        }

        foreach (SkillSlot s in medicSkills) {
            int[] prereqs = s.GetPrerequisitesForThisSkill();
            bool met = true;
            for (int i = 0; i < prereqs.Length; i++) {
                if (PlayerData.playerdata.skillList["4/" + prereqs[i]].Level != GetMaxLevelForSkill(4, prereqs[i])) {
                    s.ToggleSkillEnabled(true);
                    met = false;
                    break;
                }
            }
            if (met) {
                s.ToggleSkillEnabled(false);
            }
        }

        foreach (SkillSlot s in marksmanSkills) {
            int[] prereqs = s.GetPrerequisitesForThisSkill();
            bool met = true;
            for (int i = 0; i < prereqs.Length; i++) {
                if (PlayerData.playerdata.skillList["5/" + prereqs[i]].Level != GetMaxLevelForSkill(5, prereqs[i])) {
                    s.ToggleSkillEnabled(true);
                    met = false;
                    break;
                }
            }
            if (met) {
                s.ToggleSkillEnabled(false);
            }
        }

        foreach (SkillSlot s in heavySkills) {
            int[] prereqs = s.GetPrerequisitesForThisSkill();
            bool met = true;
            for (int i = 0; i < prereqs.Length; i++) {
                if (PlayerData.playerdata.skillList["6/" + prereqs[i]].Level != GetMaxLevelForSkill(6, prereqs[i])) {
                    s.ToggleSkillEnabled(true);
                    met = false;
                    break;
                }
            }
            if (met) {
                s.ToggleSkillEnabled(false);
            }
        }
    }

    public void DelaySetPrimaryTree(int treeId)
    {
        setPrimaryTreeFlag = true;
        primaryTreeIndex = treeId;
    }

    public int GetPrimaryTree()
    {
        return primaryTreeIndex;
    }

    public void SetPrimaryTree(int treeId)
    {
        commandoClassIndicator.SetActive(false);
        reconClassIndicator.SetActive(false);
        engineerClassIndicator.SetActive(false);
        mastermindClassIndicator.SetActive(false);
        medicClassIndicator.SetActive(false);
        marksmanClassIndicator.SetActive(false);
        heavyClassIndicator.SetActive(false);
        switch (treeId) {
            case 0:
                commandoClassIndicator.SetActive(true);
                primaryTreeTxt.text = "COMMANDO";
                break;
            case 1:
                reconClassIndicator.SetActive(true);
                primaryTreeTxt.text = "RECON";
                break;
            case 2:
                engineerClassIndicator.SetActive(true);
                primaryTreeTxt.text = "ENGINEER";
                break;
            case 3:
                mastermindClassIndicator.SetActive(true);
                primaryTreeTxt.text = "MASTERMIND";
                break;
            case 4:
                medicClassIndicator.SetActive(true);
                primaryTreeTxt.text = "MEDIC";
                break;
            case 5:
                marksmanClassIndicator.SetActive(true);
                primaryTreeTxt.text = "MARKSMAN";
                break;
            case 6:
                heavyClassIndicator.SetActive(true);
                primaryTreeTxt.text = "HEAVY";
                break;
            default:
                break;
        }
    }
}
