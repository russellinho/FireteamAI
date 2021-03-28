using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeployableScript : MonoBehaviour
{
    public int deployableId;
    private const short MAX_FIRST_AID_KIT_USES = 6;
    private const short MAX_AMMO_BAG_USES = 8;
    public string deployableName;
    public string refString;
    public short usesRemaining;
    public GameObject initializeRef;
    public AudioSource audioSource;
    public AudioClip deploySound;
    public AudioClip breakSound;
    // Determines if deployable can be stuck to any surface

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) {
            Debug.LogError("USES REMAINING: " + usesRemaining);
        }
    }

    public int InstantiateDeployable(int skillBoost = 0) {
        if (deployableName.Equals("First Aid Kit")) {
            usesRemaining = (short)(MAX_FIRST_AID_KIT_USES + (short)skillBoost);
        } else if (deployableName.Equals("Ammo Bag")) {
            usesRemaining = (short)(MAX_AMMO_BAG_USES + (short)skillBoost);
        } else if (deployableName.Equals("Bubble Shield (Skill)")) {
            PlayDeploySound();
            initializeRef.SetActive(true);
            initializeRef.GetComponent<BubbleShieldScript>().Initialize(skillBoost);
        }
        deployableId = gameObject.GetInstanceID();
        return deployableId;
    }

    public void UseDeployableItem() {
        usesRemaining--;
    }

    public bool CheckOutOfUses() {
        return (usesRemaining == 0 ? true : false);
    }

    public void PlayDeploySound()
    {
        audioSource.clip = deploySound;
        audioSource.Play();
    }

    public void PlayBreakSound()
    {
        audioSource.clip = breakSound;
        audioSource.Play();
    }

    public void BeginDestroyItem()
    {
        StartCoroutine("DestroyItem");
    }

    IEnumerator DestroyItem()
    {
        yield return new WaitForSeconds(10);
        Destroy(gameObject);
    }

}
