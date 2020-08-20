using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTest : MonoBehaviour
{
    // Start is called before the first frame update
    public Animator anim;
    

    // Update is called once per frame
    void Update()
    {
     if (Input.GetKeyDown(KeyCode.A)){ // do once 
         anim.Play("male_reload");
     }
     if (Input.GetKeyDown(KeyCode.S)){ // press down 
         anim.Play("male_sprint"); // what ever state animation 
     }   
     if (Input.GetKeyDown(KeyCode.D)){ // release key 
        anim.Play("male_idleStand");
     }
    }
}
