using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class WeaponScript : MonoBehaviour
{
    public EquipmentScript equipmentScript;
    public WeaponHandlerScript weaponHolder;
    public Animator animator;
    public TitleControllerScript ts;
    public string equippedPrimaryWeapon;
    public string equippedPrimaryType;
    public string equippedSecondaryWeapon;
    public string equippedSecondaryType;
    public int currentlyEquippedType;

    public bool weaponReady;
    public PhotonView pView;

    private bool onTitle;

    void Awake() {
        if (SceneManager.GetActiveScene().name.Equals("Title")) {
            onTitle = true;
            animator.SetBool("onTitle", true);
        } else {
            onTitle = false;
            animator.SetBool("onTitle", false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (pView != null && !pView.IsMine)
        {
            return;
        }
        if (onTitle)
        {
            ts = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
            equipmentScript = GetComponent<EquipmentScript>();
            weaponReady = false;
        } else {
            //EquipWeapon(PlayerData.playerdata.info.equippedPrimaryType, PlayerData.playerdata.info.equippedPrimary, null);
            //EquipWeapon(PlayerData.playerdata.info.equippedSecondaryType, PlayerData.playerdata.info.equippedSecondary, null);
            equippedPrimaryWeapon = PlayerData.playerdata.info.equippedPrimary;
            equippedPrimaryType = PlayerData.playerdata.info.equippedPrimaryType;
            equippedSecondaryWeapon = PlayerData.playerdata.info.equippedSecondary;
            equippedSecondaryType = PlayerData.playerdata.info.equippedSecondaryType;
            DrawWeapon(1);
        }
    }

    void Update() {
        if (pView != null && !pView.IsMine)
        {
            return;
        }
        if (!animator.GetBool("onTitle")) {
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                DrawWeapon(1);
            } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
                DrawWeapon(2);
            }
        }
    }

    void DrawWeapon(int weaponCat) {
        string equippedWep = "";
        string equippedType = "";
        if (weaponCat == 1)
        {
            if (currentlyEquippedType == 1) return;
            equippedWep = equippedPrimaryWeapon;
            equippedType = equippedPrimaryType;
        }
        else
        {
            if (currentlyEquippedType == 2) return;
            equippedWep = equippedSecondaryWeapon;
            equippedType = equippedSecondaryType;
        }
        pView.RPC("RpcDrawWeapon", RpcTarget.All, weaponCat, equippedWep, equippedType);
    }

    [PunRPC]
    private void RpcDrawWeapon(int weaponCat, string equippedWep, string equippedType) {
        weaponReady = false;
        animator.SetInteger("WeaponType", weaponCat);
        currentlyEquippedType = weaponCat;
        EquipWeapon(equippedType, equippedWep, null);
//            animator.CrossFadeInFixedTime("DrawWeapon", 0.1f, 0, 1f);
    }

    void EquipAssaultRifle(string weaponName) {
        // Set animation and hand positions
        equippedPrimaryType = "Assault Rifle";
        equippedPrimaryWeapon = weaponName;
        if (animator.GetBool("onTitle")) {
            SetTitleHandPositions();
        } else {
            weaponHolder.SetWeaponPosition();
            if (InventoryScript.rifleHandPositionsPerCharacter != null)
            {
                weaponHolder.SetSteadyHand(InventoryScript.rifleHandPositionsPerCharacter[PlayerData.playerdata.info.equippedCharacter][weaponName]);
            }
        }
    }

    void EquipShotgun(string weaponName) {
        equippedPrimaryType = "Shotgun";
        equippedPrimaryWeapon = weaponName;
        if (animator.GetBool("onTitle")) {
            SetTitleHandPositions();
        } else {
            weaponHolder.SetWeaponPosition();
            if (InventoryScript.shotgunHandPositionsPerCharacter != null) {
                weaponHolder.SetSteadyHand(InventoryScript.shotgunHandPositionsPerCharacter[PlayerData.playerdata.info.equippedCharacter][weaponName]);
            }
        }
    }

    public void EquipPistol(string weaponName) {
        // Set animation and hand positions
        equippedSecondaryType = "Pistol";
        equippedSecondaryWeapon = weaponName;
        if (!onTitle) {
            weaponHolder.SetWeaponPosition();
            weaponHolder.ResetSteadyHand();
        }
    }

    public void EquipSniperRifle(string weaponName) {
        equippedPrimaryType = "Sniper Rifle";
        equippedPrimaryWeapon = weaponName;
        if (animator.GetBool("onTitle")) {
            SetTitleHandPositions();
        } else {
            weaponHolder.SetWeaponPosition();
            if (InventoryScript.sniperRifleHandPositionsPerCharacter != null) {
                weaponHolder.SetSteadyHand(InventoryScript.sniperRifleHandPositionsPerCharacter[PlayerData.playerdata.info.equippedCharacter][weaponName]);
            }
        }
    }

    public void EquipWeapon(string weaponType, string weaponName, GameObject shopItemRef) {
        // Get the weapon from the weapon catalog for its properties
        Weapon w = InventoryScript.weaponCatalog[weaponName];
        switch (weaponType) {
            case "Assault Rifle":
                currentlyEquippedType = 1;
                weaponHolder.LoadWeapon(w.prefabPath);
                EquipAssaultRifle(weaponName);
                break;
            case "Pistol":
                if (!onTitle) {
                    currentlyEquippedType = 2;
                    weaponHolder.LoadWeapon(w.prefabPath);
                }
                EquipPistol(weaponName);
                break;
            case "Shotgun":
                currentlyEquippedType = 1;
                weaponHolder.LoadWeapon(w.prefabPath);
                EquipShotgun(weaponName);
                break;
            case "Sniper Rifle":
                currentlyEquippedType = 1;
                weaponHolder.LoadWeapon(w.prefabPath);
                EquipSniperRifle(weaponName);
                break;
        }

        if (onTitle) {
            // Shop GUI stuff
            if (shopItemRef != null) {
                // Sets item that you unequipped to white
                if (ts.currentlyEquippedItemPrefab != null) {
                    ts.currentlyEquippedItemPrefab.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
                    ts.currentlyEquippedItemPrefab.GetComponent<ShopItemScript>().equippedInd.enabled = false;
                }

                // Sets item that you just equipped to orange in the shop
                if (shopItemRef != null) {
                    shopItemRef.GetComponentsInChildren<Image>()[0].color = new Color(255f / 255f, 119f / 255f, 1f / 255f, 255f / 255f);
                    shopItemRef.GetComponent<ShopItemScript>().equippedInd.enabled = true;
                    ts.currentlyEquippedItemPrefab = shopItemRef;
                }
            }

            // Puts the item that you just equipped in its proper slot
            if (w.type.Equals("Primary")) {
                ts.equippedPrimarySlot.GetComponentInChildren<RawImage>().enabled = true;
                ts.equippedPrimarySlot.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(w.thumbnailPath);
            } else {
                ts.equippedSecondarySlot.GetComponentInChildren<RawImage>().enabled = true;
                ts.equippedSecondarySlot.GetComponentInChildren<RawImage>().texture = (Texture)Resources.Load(w.thumbnailPath);
            }
        }
    }

    public void SetTitleHandPositions() {
        if (ts != null) {
            if (ts.currentCharGender == 'M') {
                weaponHolder.SetWeaponPositionForTitle(new Vector3(-0.02f, 0.05f, 0.03f));
            } else {
                weaponHolder.SetWeaponPositionForTitle(new Vector3(-0.01f, 0.02f, 0.02f));
            }
        }
    }

    public void EquipDefaultWeapons() {
        equippedPrimaryWeapon = "AK-47";
        equippedPrimaryType = "Assault Rifle";
        equippedSecondaryWeapon = "Glock23";
        equippedSecondaryType = "Pistol";
        EquipWeapon(equippedPrimaryType, equippedPrimaryWeapon, null);
        EquipWeapon(equippedSecondaryType, equippedSecondaryWeapon, null);
    }

    [PunRPC]
    private void RpcSetLeftShoulderPos(Vector3 handPos) {
        weaponHolder.SetSteadyHand(handPos);
    }

    [PunRPC]
    private void RpcSetWeaponPos() {
        weaponHolder.SetWeaponPosition();
    }

}
