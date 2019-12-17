using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

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

    public override void OnEnable() {
        if (PhotonNetwork.IsMasterClient) {
            EquipRandomOutfitForEnemy();
            modelCreated = true;
        } else {
            modelCreated = false;
        }
    }

    // Only used for testing purposes - do not uncomment
    // void Update() {
    //     if (Input.GetKeyDown(KeyCode.K)) {
    //         EquipRandomOutfitForEnemy();
    //     }
    // }

    void Update() {
        if (!modelCreated && !PhotonNetwork.IsMasterClient) {
            if (!createModelSemaphore) {
                createModelSemaphore = true;
                PingServerForEquipment();
                createModelSemaphore = false;
                modelCreated = true;
            }
        }
    }

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
            equippedSkin = 2;
        } else if (r == 1) {
            // Camo shirt/short sleeve shirt
            equippedTop = "Camo Shirt";
            equippedSkin = 1;
        } else {
            // Camo top/long sleeve shirt
            equippedTop = "Camo Top";
            equippedSkin = 3;
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
        EquipSkin("Models/Enemies/Cicadas/" + modelNumber + "/Skin" + equippedSkin + "/cicada" + modelNumber + "skin" + equippedSkin);

        // Second, equip the correct top
        UnequipTop();
        if (equippedTop != null) {
            if (equippedTop.Equals("Camo Tank")) {
                EquipTop("Models/Enemies/Cicadas/Clothing/Tops/1/camotank");
            } else if (equippedTop.Equals("Camo Shirt")) {
                EquipTop("Models/Enemies/Cicadas/Clothing/Tops/2/camotop");
            } else if (equippedTop.Equals("Camo Top")) {
                EquipTop("Models/Enemies/Cicadas/Clothing/Tops/3/camoshirt");
            }
        }

        // Third, equip the correct bottoms
        UnequipBottom();
        if (equippedBottom != null) {
            if (equippedBottom.Equals("Cargo Pants")) {
                EquipBottom("Models/Enemies/Cicadas/Clothing/Bottoms/1/cargos");
            } else if (equippedBottom.Equals("Cargo Shorts")) {
                EquipBottom("Models/Enemies/Cicadas/Clothing/Bottoms/3/cargoshorts");
            } else if (equippedBottom.Equals("Cargo Jeans")) {
                EquipBottom("Models/Enemies/Cicadas/Clothing/Bottoms/2/cargojeans");
            }
        }

        // Fourth, equip the correct shoes
        UnequipFootwear();
        if (equippedFootwear != null) {
            if (equippedFootwear.Equals("Combat Boots")) {
                EquipFootwear("Models/Enemies/Cicadas/Clothing/Shoes/1/combatboots");
            } else if (equippedFootwear.Equals("Combat Shoes")) {
                EquipFootwear("Models/Enemies/Cicadas/Clothing/Shoes/2/combatshoes");
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
                EquipFacewear("Models/Enemies/Cicadas/Clothing/Face/1/shroud");
            }
        }
        
        // Equip eyewear
        UnequipEyewear();
        if (equippedEyewear != null) {
            if (equippedEyewear.Equals("Sport Glasses")) {
                EquipEyewear("Models/Enemies/Cicadas/Clothing/Eyes/1/sportsglasses");
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
                EquipHeadgear("Models/Enemies/Cicadas/Clothing/Hats/1/baseballhat");
            }
        }
    }

    void EquipTop(string prefabPath) {
        equippedTopRef = (GameObject)Instantiate((GameObject)Resources.Load(prefabPath));
        equippedTopRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedTopRef.GetComponentInChildren<MeshFixer>();
        m.target = myTopRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    void EquipBottom(string prefabPath) {
        equippedBottomRef = (GameObject)Instantiate((GameObject)Resources.Load(prefabPath));
        equippedBottomRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedBottomRef.GetComponentInChildren<MeshFixer>();
        m.target = myBottomRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    void EquipEyewear(string prefabPath) {
        equippedEyewearRef = (GameObject)Instantiate((GameObject)Resources.Load(prefabPath));
        equippedEyewearRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedEyewearRef.GetComponentInChildren<MeshFixer>();
        m.target = myEyewearRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    void EquipFacewear(string prefabPath) {
        equippedFacewearRef = (GameObject)Instantiate((GameObject)Resources.Load(prefabPath));
        equippedFacewearRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedFacewearRef.GetComponentInChildren<MeshFixer>();
        m.target = myFacewearRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    void EquipFootwear(string prefabPath) {
        equippedFootwearRef = (GameObject)Instantiate((GameObject)Resources.Load(prefabPath));
        equippedFootwearRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedFootwearRef.GetComponentInChildren<MeshFixer>();
        m.target = myFootwearRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    void EquipSkin(string prefabPath) {
        equippedSkinRef = (GameObject)Instantiate((GameObject)Resources.Load(prefabPath));
        equippedSkinRef.transform.SetParent(gameObject.transform);
        MeshFixer m = equippedSkinRef.GetComponentInChildren<MeshFixer>();
        m.target = mySkinRenderer.gameObject;
        m.rootBone = myBones.transform;
        m.AdaptMesh();
    }

    void EquipHeadgear(string prefabPath) {
        equippedHeadgearRef = (GameObject)Instantiate((GameObject)Resources.Load(prefabPath));
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
        if (PhotonNetwork.IsMasterClient) {
            SendEquippedItemsToClients();
        }
    }

    void PingServerForEquipment() {
        if (pView != null) {
            pView.RPC("RpcPingServerForEquipment", RpcTarget.MasterClient);
        }
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
