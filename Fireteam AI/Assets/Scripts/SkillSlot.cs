using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSlot : MonoBehaviour
{
    public ItemPopupScript skillDescriptionPopupRef;
    public int treeId;
    public int skillId;
    public SkillManager skillManager;
    public RawImage skillThumb;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI skillName;
    public GameObject disableOverlay;
    public string[] skillDescription;
    private bool delayInitFlag;
    private int initLevel;
    private bool delayToggleSkillEnabledFlag;
    private bool delayToggleSkillEnabledVal;

    public void DelayInit(int level)
    {
        delayInitFlag = true;
        initLevel = level;
    }

    public void Init(int level)
    {
        SetLevel(level);
    }

    void Update()
    {
        if (delayInitFlag) {
            Init(initLevel);
            initLevel = 0;
            delayInitFlag = false;
        }
        if (delayToggleSkillEnabledFlag) {
            ToggleSkillEnabled(delayToggleSkillEnabledVal);
            delayToggleSkillEnabledFlag = false;
        }
    }

    public void OnAddSkillPointClicked()
    {
        skillManager.AddSkillPoint(treeId, skillId);
    }

    public void SetLevel(int level)
    {
        this.levelText.text = ""+level;
        if (level == GetMaxLevelForThisSkill()) {
            // Disable button if maxed
            GetComponentInChildren<Button>().interactable = false;
        }
    }

    public void DelayToggleSkillEnabled(bool b)
    {
        delayToggleSkillEnabledFlag = true;
        delayToggleSkillEnabledVal = b;
    }

    public void ToggleSkillEnabled(bool b)
    {
        disableOverlay.SetActive(b);
    }

    public string GetPrerequisitesStringForThisSkill()
    {
        string s = "";
        int[] prereqs = skillManager.GetPrerequisitesForSkill(treeId, skillId);
        if (prereqs.Length > 0) {
            s = skillManager.GetSkillSlot(treeId, prereqs[0]).skillName.text;
            for (int i = 1; i < prereqs.Length; i++) {
                s += "\n" + skillManager.GetSkillSlot(treeId, prereqs[i]).skillName.text;
            }
        }
        return s;
    }

    public int[] GetPrerequisitesForThisSkill()
    {
        return skillManager.GetPrerequisitesForSkill(treeId, skillId);
    }

    public int GetCurrentLevelForThisSkill()
    {
        return PlayerData.playerdata.skillList[treeId + "/" + skillId].Level;
    }

    public int GetMaxLevelForThisSkill()
    {
        return skillManager.GetMaxLevelForSkill(treeId, skillId);
    }

    public string GetCurrentSkillDescription()
    {
        int d = GetCurrentLevelForThisSkill() - 1;
        return skillDescription[d < 0 ? 0 : d];
    }
}
