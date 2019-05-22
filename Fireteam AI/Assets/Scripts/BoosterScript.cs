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

    void Update()
    {

    }



    public void UseBoosterItem(string boosterName) {
      if (boosterName.Equals("Medkit")) {
              UseMedKit();
          }
      else if (boosterName.Equals("Adrenaphine")) {

          }
      else {
        Debug.Log("Hello" + boosterName);
      }
    }


    public void UseMedKit() {
      StartCoroutine(addHealth());
    }

    IEnumerator addHealth(){
        Debug.Log("medkit used");
        // use below to test on self
        // playerActionScript.health = 60;
        if (playerActionScript.health < 100 && playerActionScript.health > 0){ 
          for (int i = 0; i < 5; i++) {
            if (playerActionScript.health+12 > 100){
              playerActionScript.health = 100;
            } else {
              playerActionScript.health += 12;
            }
            yield return new WaitForSeconds(2);

          }

         } else {
           yield return null;
         }
    }
}
