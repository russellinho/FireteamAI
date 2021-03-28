using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleShieldScript : MonoBehaviour
{
    private const float BASE_DURATION = 30f;
    private const short MAX_SIZE = 10;
    public GameObject deviceRef;
    private float timer;
    private float duration;
    private bool initializing;
    public void Initialize(int level = 0)
    {
        initializing = true;
        if (level == 0) {
            duration = BASE_DURATION;
        } else if (level == 1) {
            duration = 30;
        } else if (level == 2) {
            duration = 50;
        } else if (level == 3) {
            duration = 100;
        }
    }

    void Update() {
        if (initializing) {
            timer += Time.deltaTime * 1.5f;
            transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(MAX_SIZE, MAX_SIZE, MAX_SIZE), timer);
            if (timer >= 1f) {
                initializing = false;
            }
        } else {
            duration -= Time.deltaTime;
            if (duration <= 0f) {
                DeployableScript d = deviceRef.GetComponent<DeployableScript>();
                d.PlayBreakSound();
                d.BeginDestroyItem();
                gameObject.SetActive(false);
            }
        }
    }
}
