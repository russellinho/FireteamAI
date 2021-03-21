using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoosterScript : MonoBehaviour
{
    private PlayerActionScript playerActionScript;

    void Start()
    {
      playerActionScript = transform.GetComponentInParent<PlayerActionScript>();
    }

    public void UseBoosterItem(string boosterName) {
      if (boosterName.Equals("Medkit")) {
              UseMedKit();
          }
      else if (boosterName.Equals("Adrenaphine")) {
              UseAdrenaphine();
          }
      else {
        Debug.Log("Hello" + boosterName);
      }
    }


    public void UseMedKit() {
      playerActionScript.ResetHealTimer();
      playerActionScript.PlayHealParticleEffect();
      playerActionScript.audioController.PlayGruntSound();
      playerActionScript.InjectMedkit();
    }

    public void UseAdrenaphine() {
      playerActionScript.ResetBoostTimer();
      playerActionScript.PlayBoostParticleEffect(true);
      playerActionScript.audioController.PlayGruntSound();
      playerActionScript.InjectAdrenaphine();
    }



}
