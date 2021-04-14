using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneDeviceScript : MonoBehaviour
{
    const float BASE_TIME = 15f;
    public void UseDevice(PlayerActionScript playerActionScript)
    {
        WeaponMeta w = GetComponent<WeaponMeta>();
        if (w.weaponName.EndsWith("(Skill)")) {
            if (playerActionScript.weaponScript.currentlyEquippedType == -2) {
                CallEcmFeedback(playerActionScript, true);
            } else if (playerActionScript.weaponScript.currentlyEquippedType == -3) {
                CallInfraredScan(playerActionScript, true);
            }
        }
    }

    void CallEcmFeedback(PlayerActionScript playerActionScript, bool fromSkill)
    {
        float duration = playerActionScript.skillController.GetEcmFeedbackTime();
        playerActionScript.TriggerEcmFeedback(PlayerData.playerdata.skillList["2/10"].Level, duration);
    }

    void CallInfraredScan(PlayerActionScript playerActionScript, bool fromSkill)
    {
        float duration = playerActionScript.skillController.GetInfraredScanTime();
        playerActionScript.TriggerInfraredScan(duration);
    }
}
