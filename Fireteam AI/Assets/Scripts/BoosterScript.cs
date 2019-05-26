using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoosterScript : MonoBehaviour
{
    private PlayerActionScript playerActionScript;
    private WeaponStats weaponStats;




    void Start()
    {
      playerActionScript = transform.GetComponentInParent<PlayerActionScript>();

    }

    void Update()
    {

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
      StartCoroutine(playerActionScript.addHealth());
    }

    public void UseAdrenaphine() {
      playerActionScript.ResetBoostTimer();
      playerActionScript.PlayBoostParticleEffect();
      playerActionScript.audioController.PlayGruntSound();
      StartCoroutine(playerActionScript.useStaminaBoost(10f, 2f));
    }



}
