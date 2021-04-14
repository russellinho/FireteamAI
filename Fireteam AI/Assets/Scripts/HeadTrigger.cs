using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadTrigger : MonoBehaviour
{
    private const float WATER_LAYER = 4;
    public BetaEnemyScript enemyScript;
    public NpcScript npcScript;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == WATER_LAYER) {
            if (npcScript != null) {
                npcScript.TakeDamage(100, transform.position, 2, 0);
            } else if (enemyScript != null) {
                enemyScript.TakeDamage(100, transform.position, 2, 0, 0, 0);
            }
        }
    }
}
