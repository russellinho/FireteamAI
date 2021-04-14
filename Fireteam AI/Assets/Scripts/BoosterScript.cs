using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoosterScript : MonoBehaviour
{

    public void UseBoosterItem(string boosterName, PlayerActionScript playerActionScript) {
      if (boosterName.Equals("Medkit")) {
              UseMedKit(playerActionScript);
          }
      else if (boosterName.Equals("Adrenaphine")) {
              UseAdrenaphine(playerActionScript);
          }
      else {
        Debug.Log("Hello" + boosterName);
      }
    }


    public void UseMedKit(PlayerActionScript playerActionScript) {
      playerActionScript.ResetHealTimer();
      playerActionScript.PlayHealParticleEffect();
      playerActionScript.audioController.PlayGruntSound();
      playerActionScript.InjectMedkit();
    }

    public void UseAdrenaphine(PlayerActionScript playerActionScript) {
      playerActionScript.ResetBoostTimer();
      playerActionScript.PlayBoostParticleEffect(true);
      playerActionScript.audioController.PlayGruntSound();
      playerActionScript.InjectAdrenaphine();
    }



}
