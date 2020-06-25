using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ActionStates = BetaEnemyScript.ActionStates;
using Photon.Pun;
using Random = UnityEngine.Random;

public class AIControllerScript : MonoBehaviour
{
    // Respawn all in queue after this amount of secs
    private const float RESPAWN_JOB_DELAY = 10f;
    // Respawn the enemy after this amount of seconds
    public float enemyRespawnSecs;
    // spreadConstant determines how spread out enemies spawn from each other. The closer to 0, the more grouped together AI will be when they respawn.
    // The further from 0, the more spread out enemies will spawn from each other.
    [SerializeField]
    private int spreadConstant;
    public int[] maxGroupSizePerSpawn;
    public GameControllerScript gameController;
    public Transform[] spawnPoints;
    // Indices align with each other on each array (ex: spawnPoint at index 0 has the priority of spawnPriorities at index 0)
    // The lower the number, the more preferred of a spawn place this is 
    private Queue aiReadyToRespawn;
    
    void Awake() {
        aiReadyToRespawn = new Queue();
        if (spawnPoints != null) {
            if (spawnPoints.Length > 0 && spreadConstant >= spawnPoints.Length) {
                spreadConstant = spawnPoints.Length - 1;
            }
        }
    }

    void Start() {
        if (gameController.matchType == 'C') {
            StartCoroutine("RespawnJobCampaign");
        } else if (gameController.matchType == 'V') {
            StartCoroutine("RespawnJobVersus");
        }
    }

    IEnumerator RespawnJobCampaign() {
        yield return new WaitForSeconds(RESPAWN_JOB_DELAY);
        
        if (PhotonNetwork.IsMasterClient) {
            SpawnNextWaveOfEnemies();
        }

        StartCoroutine("RespawnJobCampaign");
    }

    IEnumerator RespawnJobVersus() {
        yield return new WaitForSeconds(RESPAWN_JOB_DELAY);
        
        if (gameController.isVersusHostForThisTeam()) {
            SpawnNextWaveOfEnemies();
        }

        StartCoroutine("RespawnJobVersus");
    }

    public void AddToRespawnQueue(int pViewId) {
        aiReadyToRespawn.Enqueue(pViewId);
    }

    public void ClearRespawnQueue() {
        aiReadyToRespawn.Clear();
    }

    void SpawnNextWaveOfEnemies() {
        // Remove everything from the spawn queue and spawn them in
        if (aiReadyToRespawn.Count == 0) return;

        SpawnOrganizer[] farthestSpawnPoints = GetFarthestSpawnPoints();
        int[] spawnCounts = new int[spawnPoints.Length];
        int spawnPointIterator = 0;
        int altSpawnPointIterator = 0;
        int altSpawnTracker = 0;

        while (true) {
            try {
                int nextAiId = (int)aiReadyToRespawn.Dequeue();
                GameObject nextAi = gameController.enemyList[nextAiId];
                BetaEnemyScript b = nextAi.GetComponent<BetaEnemyScript>();
                if (b.actionState != ActionStates.Dead) {
                    // If for some reason the enemy is already alive, just skip it
                    continue;
                }
                // Handle spawning routine with spread constant
                if (spreadConstant > 0) {
                    // Spawn the enemy at the current spawn point
                    int i = farthestSpawnPoints[altSpawnPointIterator].index;
                    Vector3 newSpawnPos = new Vector3(spawnPoints[i].position.x + Random.Range(0f, 5f), spawnPoints[i].position.y + Random.Range(0f, 5f), spawnPoints[i].position.z + Random.Range(0f, 5f));
                    b.RespawnAtPosition(newSpawnPos, true);
                    spawnCounts[i]++;
                    altSpawnTracker++;
                    // Round-robin style spawn tracker - if we've evenly distribute the spawn for spreadConstant, then reset
                    if (altSpawnTracker >= spreadConstant) {
                        altSpawnTracker = 0;
                        altSpawnPointIterator = spawnPointIterator;
                    } else {
                        // Find the next available spawn point
                        altSpawnPointIterator++;
                        while (altSpawnPointIterator < farthestSpawnPoints.Length && spawnCounts[altSpawnPointIterator] >= maxGroupSizePerSpawn[altSpawnPointIterator]) {
                            altSpawnPointIterator++;
                        }
                    }
                    // If reached the max spawn group size for this point, go to the next available spawn point
                    if (spawnCounts[i] >= maxGroupSizePerSpawn[i]) {
                        for (int j = 0; j < farthestSpawnPoints.Length; j++) {
                            if (spawnCounts[j] < maxGroupSizePerSpawn[j]) {
                                spawnPointIterator = j;
                                altSpawnPointIterator = spawnPointIterator;
                                altSpawnTracker = 0;
                                break;
                            }
                        }
                    }
                } else {
                    // Handle spawning routine with no spread constant
                    // Spawn the enemy at the current spawn point
                    int i = farthestSpawnPoints[spawnPointIterator].index;
                    Vector3 newSpawnPos = new Vector3(spawnPoints[i].position.x + Random.Range(0f, 5f), spawnPoints[i].position.y + Random.Range(0f, 5f), spawnPoints[i].position.z + Random.Range(0f, 5f));
                    b.RespawnAtPosition(newSpawnPos, true);
                    spawnCounts[i]++;
                    // If reached the max spawn group size for this point, go to the next one
                    if (spawnCounts[i] >= maxGroupSizePerSpawn[i]) {
                        spawnPointIterator++;
                    }
                }
                // Protect against out of bounds
                if (spawnPointIterator >= farthestSpawnPoints.Length) {
                    spawnPointIterator = 0;
                }
                if (altSpawnPointIterator >= farthestSpawnPoints.Length) {
                    altSpawnPointIterator = spawnPointIterator;
                }
            } catch (InvalidOperationException e) {
                // Exits when the queue is empty
                break;
            }
        }

        // Clear the queue afterwards
        gameController.ClearAIRespawns();
    }

    // Gets x number of farthest spawn points from all players
    private SpawnOrganizer[] GetFarthestSpawnPoints() {
        SpawnOrganizer[] dists = new SpawnOrganizer[spawnPoints.Length];
        int i = 0;

        // First, for each spawn point, determine the distance from the closest player
        foreach (Transform t in spawnPoints) {
            bool first = true;
            float minD = 0f;
            foreach (KeyValuePair<int, PlayerStat> p in GameControllerScript.playerList) {
                float d = Vector3.Distance(t.position, p.Value.objRef.transform.position);
                if (first) {
                    minD = d;
                    first = false;
                } else {
                    minD = Mathf.Min(d, minD);
                }
            }
            dists[i] = new SpawnOrganizer(i, minD);
            i++;
        }

        // Once you have all the farthest distances, sort by top farthest
        Array.Sort(dists, 0, dists.Length, new SpawnOrganizerComparer());

        return dists;
    }

    private class SpawnOrganizerComparer : IComparer
    {
        // Call CaseInsensitiveComparer.Compare with the parameters reversed.
        public int Compare(object x, object y)
        {
            SpawnOrganizer a = (SpawnOrganizer)x;
            SpawnOrganizer b = (SpawnOrganizer)y;
            if (a.dist < b.dist)
                return 1;
            if (a.dist > b.dist)
                return -1;
            else
                return 0;
        }
    }

    private class SpawnOrganizer {
        public SpawnOrganizer(int i, float d) {
            index = i;
            dist = d;
        }
        public int index {get;}
        public float dist {get;}
    }

}
