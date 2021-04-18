using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using SpawnMode = GameControllerScript.SpawnMode;
using ActionStates = BetaEnemyScript.ActionStates;

public class EnemyModelCreator : MonoBehaviourPunCallbacks
{
    public string enemyName;
    public int modelNumber;

    private string equippedHeadgear;
    private string equippedFacewear;
    private string equippedEyewear;
    private string equippedTop;
    private string equippedBottom;
    private string equippedFootwear;
    private int equippedSkin;
    private bool renderBeard;
    private bool renderHair;
    
    public GameObject equippedSkinRef;
    public GameObject equippedHeadgearRef;
    public GameObject equippedFacewearRef;
    public GameObject equippedEyewearRef;
    public GameObject equippedTopRef;
    public GameObject equippedBottomRef;
    public GameObject equippedFootwearRef;

    public GameObject myHeadgearRenderer;
    public GameObject myFacewearRenderer;
    public GameObject myTopRenderer;
    public GameObject myBottomRenderer;
    public GameObject myFootwearRenderer;
    public GameObject mySkinRenderer;
    public GameObject myEyewearRenderer;
    public GameObject myHairRenderer;
    public GameObject myBeardRenderer;
    public GameObject myEyesRenderer;
    public GameObject myEyelashRenderer;

    public GameObject myBones;
    public PhotonView pView;
    public Material detectionOutline;
    private bool modelCreated;
    private static bool createModelSemaphore;
    public BetaEnemyScript enemyScript;
    public GameObject[] topsSelection;
    public GameObject[] bottomsSelection;
    public GameObject[] eyewearSelection;
    public GameObject[] facewearSelection;
    public GameObject[] footwearSelection;
    public GameObject[] headgearSelection;
    public GameObject[] skinSelection;

    public override void OnEnable() {
        // if (PhotonNetwork.IsMasterClient) {
        //     EquipRandomOutfitForEnemy();
        //     modelCreated = true;
        // } else {
        //     modelCreated = false;
        // }
        if (PhotonNetwork.IsMasterClient) {
            if (!modelCreated) {
                EquipRandomOutfitForEnemy();
                // pView.RPC("RpcSetModelCreated", RpcTarget.All);
                SendEquippedItemsToClients();
            }
        } else {
            if (!modelCreated) {
                PingServerForEquipment();
            }
        }
        if (enemyScript.gameControllerScript.spawnMode == SpawnMode.Paused && enemyScript.actionState == ActionStates.Dead) {
            DespawnPlayer();
        }
    }

    // Only used for testing purposes - do not uncomment
    // void Update() {
    //     if (Input.GetKeyDown(KeyCode.K)) {
    //         EquipRandomOutfitForEnemy();
    //     }
    // }

    // void Update() {
        // if (!modelCreated && !PhotonNetwork.IsMasterClient) {
        //     if (!createModelSemaphore) {
        //         createModelSemaphore = true;
        //         PingServerForEquipment();
        //         createModelSemaphore = false;
        //         modelCreated = true;
        //     }
        // }
    // }

    // [PunRPC]
    // void RpcSetModelCreated() {
    //     modelCreated = true;
    // }

    void EquipRandomOutfitForEnemy() {
        if (enemyName.Equals("Cicadas")) {
            GenerateRandomOutfitForCicadas();
            EquipGeneratedOutfitForCicadas();
        }
    }

    void GenerateRandomOutfitForCicadas() {
        // Choose a shirt first as it will determine which skin to equip
        int r = Random.Range(0, 3);
        if (r == 0) {
            // Camo tank
            equippedTop = "Camo Tank";
            equippedSkin = 1;
        } else if (r == 1) {
            // Camo top/short sleeve shirt
            equippedTop = "Camo Top";
            equippedSkin = 0;
        } else {
            // Camo shirt/long sleeve shirt
            equippedTop = "Camo Shirt";
            equippedSkin = 2;
        }

        r = Random.Range(0, 3);
        // Now that we have the skin and shirt, we can pick out pants, which will determine shoes as well
        if (r == 0) {
            // Cargo pants
            equippedBottom = "Cargo Pants";
            equippedFootwear = "Combat Boots";
        } else if (r == 1) {
            // Cargo shorts
            equippedBottom = "Cargo Shorts";
            equippedFootwear = "Combat Shoes";
        } else {
            // Cargo jeans
            equippedBottom = "Cargo Jeans";
            equippedFootwear = "Combat Shoes";
        }

        // 17% chance of wearing a ski mask
        r = Random.Range(0, 7);
        if (r == 0) {
            equippedFacewear = "Ski Mask";
            equippedHeadgear = null;
            equippedEyewear = null;
        } else {
            equippedFacewear = null;
        }
        
        // If the ski mask wasn't equipped, maybe wear glasses and/or a hat
        if (r != 0) {
            // Maybe wear a hat - 20% chance
            r = Random.Range(0, 6);
            if (r == 0) {
                // Baseball cap
                equippedHeadgear = "Baseball Hat";
            } else {
                equippedHeadgear = null;
            }

            // Maybe wear glasses - 30% chance
            r = Random.Range(0, 4);
            if (r == 0) {
                equippedEyewear = "Sport Glasses";
            } else {
                equippedEyewear = null;
            }
        }
    }

    void EquipGeneratedOutfitForCicadas() {
        if (modelCreated) return;
        modelCreated = true;
        // Render beard by default
        if (myBeardRenderer != null) {
            myBeardRenderer.GetComponent<SkinnedMeshRenderer>().enabled = true;
            renderBeard = true;
        }
        if (myHairRenderer != null) {
            myHairRenderer.GetComponent<SkinnedMeshRenderer>().enabled = true;
            renderHair = true;
        }
        // First equip the correct skin
        UnequipSkin();
        EquipSkin(skinSelection[equippedSkin]);

        // Second, equip the correct top
        UnequipTop();
        if (equippedTop != null) {
            if (equippedTop.Equals("Camo Tank")) {
                EquipTop(topsSelection[0]);
            } else if (equippedTop.Equals("Camo Shirt")) {
                EquipTop(topsSelection[2]);
            } else if (equippedTop.Equals("Camo Top")) {
                EquipTop(topsSelection[1]);
            }
        }

        // Third, equip the correct bottoms
        UnequipBottom();
        if (equippedBottom != null) {
            if (equippedBottom.Equals("Cargo Pants")) {
                EquipBottom(bottomsSelection[0]);
            } else if (equippedBottom.Equals("Cargo Shorts")) {
                EquipBottom(bottomsSelection[1]);
            } else if (equippedBottom.Equals("Cargo Jeans")) {
                EquipBottom(bottomsSelection[2]);
            }
        }

        // Fourth, equip the correct shoes
        UnequipFootwear();
        if (equippedFootwear != null) {
            if (equippedFootwear.Equals("Combat Boots")) {
                EquipFootwear(footwearSelection[0]);
            } else if (equippedFootwear.Equals("Combat Shoes")) {
                EquipFootwear(footwearSelection[1]);
            }
        }

        // Equip facewear
        UnequipFacewear();
        if (equippedFacewear != null) {
            if (equippedFacewear.Equals("Ski Mask")) {
                if (myBeardRenderer != null) {
                    myBeardRenderer.GetComponent<SkinnedMeshRenderer>().enabled = false;
                    renderBeard = false;
                }
                if (myHairRenderer != null) {
                    myHairRenderer.GetComponent<SkinnedMeshRenderer>().enabled = false;
                    renderHair = false;
                }
                EquipFacewear(facewearSelection[0]);
            }
        }
        
        // Equip eyewear
        UnequipEyewear();
        if (equippedEyewear != null) {
            if (equippedEyewear.Equals("Sport Glasses")) {
                EquipEyewear(eyewearSelection[0]);
            }
        }

        // Equip hats
        UnequipHeadgear();
        if (equippedHeadgear != null) {
            if (equippedHeadgear.Equals("Baseball Hat")) {
                if (myHairRenderer != null) {
                    myHairRenderer.GetComponent<SkinnedMeshRenderer>().enabled = false;
                    renderHair = false;
                }
                EquipHeadgear(headgearSelection[0]);
            }
        }
    }

    void EquipTop(GameObject o) {
        equippedTopRef = (GameObject)Instantiate(o);
        equippedTopRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedTopRef.GetComponentInChildren<MeshFixer>();
        m.target = myTopRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    void EquipBottom(GameObject o) {
        equippedBottomRef = (GameObject)Instantiate(o);
        equippedBottomRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedBottomRef.GetComponentInChildren<MeshFixer>();
        m.target = myBottomRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    void EquipEyewear(GameObject o) {
        equippedEyewearRef = (GameObject)Instantiate(o);
        equippedEyewearRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedEyewearRef.GetComponentInChildren<MeshFixer>();
        m.target = myEyewearRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    void EquipFacewear(GameObject o) {
        equippedFacewearRef = (GameObject)Instantiate(o);
        equippedFacewearRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedFacewearRef.GetComponentInChildren<MeshFixer>();
        m.target = myFacewearRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    void EquipFootwear(GameObject o) {
        equippedFootwearRef = (GameObject)Instantiate(o);
        equippedFootwearRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedFootwearRef.GetComponentInChildren<MeshFixer>();
        m.target = myFootwearRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    void EquipSkin(GameObject o) {
        equippedSkinRef = (GameObject)Instantiate(o);
        equippedSkinRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedSkinRef.GetComponentInChildren<MeshFixer>();
        m.target = mySkinRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    void EquipHeadgear(GameObject o) {
        equippedHeadgearRef = (GameObject)Instantiate(o);
        equippedHeadgearRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedHeadgearRef.GetComponentInChildren<MeshFixer>();
        m.target = myHeadgearRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    void UnequipTop() {
        if (equippedTopRef != null) {
            Destroy(equippedTopRef);
            equippedTopRef = null;
        }
    }

    void UnequipBottom() {
        if (equippedBottomRef != null) {
            Destroy(equippedBottomRef);
            equippedBottomRef = null;
        }
    }

    void UnequipEyewear() {
        if (equippedEyewearRef != null) {
            Destroy(equippedEyewearRef);
            equippedEyewearRef = null;
        }
    }

    void UnequipFacewear() {
        if (equippedFacewearRef != null) {
            Destroy(equippedFacewearRef);
            equippedFacewearRef = null;
        }
    }

    void UnequipFootwear() {
        if (equippedFootwearRef != null) {
            Destroy(equippedFootwearRef);
            equippedFootwearRef = null;
        }
    }

    void UnequipSkin() {
        if (equippedSkinRef != null) {
            Destroy(equippedSkinRef);
            equippedSkinRef = null;
        }
    }

    void UnequipHeadgear() {
        if (equippedHeadgearRef != null) {
            Destroy(equippedHeadgearRef);
            equippedHeadgearRef = null;
        }
    }

    [PunRPC]
    void RpcSetEquippedItems(string equippedHeadgear, string equippedFacewear, string equippedEyewear, string equippedTop, string equippedBottom, string equippedFootwear, int equippedSkin) {
        this.equippedFacewear = equippedFacewear;
        this.equippedEyewear = equippedEyewear;
        this.equippedTop = equippedTop;
        this.equippedHeadgear = equippedHeadgear;
        this.equippedBottom = equippedBottom;
        this.equippedFootwear = equippedFootwear;
        this.equippedSkin = equippedSkin;
        EquipGeneratedOutfitForCicadas();
    }

    void SendEquippedItemsToClients() {
        if (pView != null) {
            pView.RPC("RpcSetEquippedItems", RpcTarget.Others, this.equippedHeadgear, this.equippedFacewear, this.equippedEyewear, this.equippedTop, this.equippedBottom, this.equippedFootwear, this.equippedSkin);
        }
    }

    // public override void OnPlayerEnteredRoom(Player newPlayer) {
    //     if (PhotonNetwork.IsMasterClient) {
    //         SendEquippedItemsToClients();
    //     }
    // }

    [PunRPC]
    void RpcPingServerForEquipment() {
        SendEquippedItemsToClients();
    }

    public void PingServerForEquipment() {
        if (pView != null) {
            pView.RPC("RpcPingServerForEquipment", RpcTarget.MasterClient);
        }
    }

    // public bool PlayerIsDespawned() {
    //     return equippedSkinRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled;
    // }

    public void ToggleUpdateWhenOffscreen(bool b)
    {
        if (equippedSkinRef != null) {
            equippedSkinRef.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (equippedHeadgearRef != null) {
            equippedHeadgearRef.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (equippedFacewearRef != null) {
            equippedFacewearRef.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (equippedEyewearRef != null) {
            equippedEyewearRef.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (equippedTopRef != null) {
            equippedTopRef.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (equippedBottomRef != null) {
            equippedBottomRef.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (equippedFootwearRef != null) {
            equippedFootwearRef.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (myHairRenderer != null) {
            myHairRenderer.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (myBeardRenderer != null) {
            myBeardRenderer.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (myEyesRenderer != null) {
            myEyesRenderer.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
        if (myEyelashRenderer != null) {
            myEyelashRenderer.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = b;
        }
    }

    public bool BodyIsDespawned()
    {
        return !equippedSkinRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled;
    }

    public void DespawnPlayer() {
        if (equippedSkinRef != null) {
            equippedSkinRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (equippedHeadgearRef != null) {
            equippedHeadgearRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (equippedFacewearRef != null) {
            equippedFacewearRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (equippedEyewearRef != null) {
            equippedEyewearRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (equippedTopRef != null) {
            equippedTopRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (equippedBottomRef != null) {
            equippedBottomRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (equippedFootwearRef != null) {
            equippedFootwearRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (myHairRenderer != null) {
            myHairRenderer.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (myBeardRenderer != null) {
            myBeardRenderer.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (myEyesRenderer != null) {
            myEyesRenderer.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (myEyelashRenderer != null) {
            myEyelashRenderer.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
    }

    public void RespawnPlayer() {
        if (equippedSkinRef != null) {
            equippedSkinRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
        if (equippedHeadgearRef != null) {
            equippedHeadgearRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
        if (equippedFacewearRef != null) {
            equippedFacewearRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
        if (equippedEyewearRef != null) {
            equippedEyewearRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
        if (equippedTopRef != null) {
            equippedTopRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
        if (equippedBottomRef != null) {
            equippedBottomRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
        if (equippedFootwearRef != null) {
            equippedFootwearRef.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
        if (myEyesRenderer != null) {
            myEyesRenderer.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
        if (myEyelashRenderer != null) {
            myEyelashRenderer.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }

        if (myHairRenderer != null && renderHair) {
            myHairRenderer.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
        if (myBeardRenderer != null && renderBeard) {
            myBeardRenderer.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        }
    }
    
    public void ToggleDetectionOutline(bool b) {
        if (!modelCreated) {
            return;
        }
        SkinnedMeshRenderer equippedSkinRenderer = equippedSkinRef.GetComponentInChildren<SkinnedMeshRenderer>();
        SkinnedMeshRenderer equippedHeadgearRenderer = null;
        SkinnedMeshRenderer equippedTopRenderer = equippedTopRef.GetComponentInChildren<SkinnedMeshRenderer>();
        SkinnedMeshRenderer equippedBottomRenderer = equippedBottomRef.GetComponentInChildren<SkinnedMeshRenderer>();
        SkinnedMeshRenderer equippedFootwearRenderer = equippedFootwearRef.GetComponentInChildren<SkinnedMeshRenderer>();
        if (equippedHeadgearRef != null) {
            equippedHeadgearRenderer = equippedHeadgearRef.GetComponentInChildren<SkinnedMeshRenderer>();
        }

        Material[] skinRendererMatsRef = equippedSkinRenderer.materials;
        Material[] headgearRendererMatsRef = null;
        if (equippedHeadgearRef != null) {
            headgearRendererMatsRef = equippedHeadgearRenderer.materials;
        }
        Material[] topRendererMatsRef = equippedTopRenderer.materials;
        Material[] bottomRendererMatsRef = equippedBottomRenderer.materials;
        Material[] footwearRendererMatsRef = equippedFootwearRenderer.materials;

        if (b) {
            skinRendererMatsRef[1] = detectionOutline;
            if (equippedHeadgearRef != null) {
                headgearRendererMatsRef[1] = detectionOutline;
            }
            topRendererMatsRef[1] = detectionOutline;
            bottomRendererMatsRef[1] = detectionOutline;
            footwearRendererMatsRef[1] = detectionOutline;
        } else {
            skinRendererMatsRef[1] = null;
            if (equippedHeadgearRef != null) {
                headgearRendererMatsRef[1] = null;
            }
            topRendererMatsRef[1] = null;
            bottomRendererMatsRef[1] = null;
            footwearRendererMatsRef[1] = null;
        }

        equippedSkinRenderer.materials = skinRendererMatsRef;
        equippedTopRenderer.materials = topRendererMatsRef;
        equippedBottomRenderer.materials = bottomRendererMatsRef;
        equippedFootwearRenderer.materials = footwearRendererMatsRef;
        if (equippedHeadgearRef != null) {
            equippedHeadgearRenderer.materials = headgearRendererMatsRef;
        }
	}

}
