using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerActionScript : MonoBehaviourPunCallbacks
{
    // Object references
    public GameControllerScript gameController;
    public AudioControllerScript audioController;
    public CharacterController charController;
    public GameObject fpsHands;
    public WeaponActionScript wepActionScript;
    public AudioSource aud;
    public Camera viewCam;
    public Transform bodyTrans;
    public GameObject spectatorCam;
    public GameObject thisSpectatorCam;
    public PlayerHUDScript hud;
    public EquipmentScript equipmentScript;
    public CameraShakeScript camShakeScript;
    public InGameMessengerHUD inGameMessengerHud;
    public Animator animator;

    // Player variables
    public int health;
    public float sprintTime;
    public bool godMode;
    public bool canShoot;
    private float charHeightOriginal;
    private float charCenterYOriginal;
    public bool escapeValueSent;
    private bool assaultModeChangedIndicator;
    public int kills;
    private int deaths;
    public bool isRespawning;
    public float respawnTimer;
    private bool escapeAvailablePopup;
    private bool isDefusing;
    private float enterSpectatorModeTimer;

    // Game logic helper variables
    public GameObject[] subComponents;
    public FirstPersonController fpc;

    private float crouchPosY;
    private float crouchBodyPosY;
    private float crouchBodyScaleY;

    private Vector3 alivePosition;
    private Vector3 deadPosition;
    private float fraction;
    private float deathCameraLerpVar;
    private Vector3 deathCameraLerpPos;
    private bool rotationSaved;

    public float hitTimer;
    public float healTimer;
    public Vector3 hitLocation;

    // Mission references
    private GameObject currentBomb;
    private int currentBombIndex;
    private int bombIterator;
    private float bombDefuseCounter = 0f;

    void Start()
    {
        // Else, load the in-game necessities
        //AddMyselfToPlayerList();

        // Setting original positions for returning from crouching
        charHeightOriginal = charController.height;
        charCenterYOriginal = charController.center.y;
        // escapeValueSent = false;
        // assaultModeChangedIndicator = false;
        // isDefusing = false;

        health = 100;
        // kills = 0;
        // deaths = 0;
        // sprintTime = 3f;

        // currentBombIndex = 0;
        // bombIterator = 0;

        // // If this isn't the local player's prefab, then he/she shouldn't be controlled by the local player
        // if (!GetComponent<PhotonView>().IsMine)
        // {
        //     subComponents[2].SetActive(false);
        //     subComponents[3].SetActive(false);
        //     Destroy(GetComponentInChildren<AudioListener>());
        //     viewCam.enabled = false;
        //     //enabled = false;
        //     return;
        // }

        // gameController = GameObject.FindWithTag("GameController").GetComponent<GameControllerScript>();

        // Initialize variables
        canShoot = true;

        crouchPosY = 0.3f;
        crouchBodyPosY = 0.25f;
        crouchBodyScaleY = 0.66f;

        fraction = 0f;
        deathCameraLerpVar = 0f;
        rotationSaved = false;

        hitTimer = 1f;
        healTimer = 1f;
        isRespawning = false;
        respawnTimer = 0f;
        escapeAvailablePopup = false;
        enterSpectatorModeTimer = 0f;
    }

    void Update()
    {
        /** if (gameController == null)
        {
            GameObject gc = GameObject.FindWithTag("GameController");
            if (gc == null)
            {
                return;
            }
            gameController = gc.GetComponent<GameControllerScript>();
        }

        if (!photonView.IsMine)
        {
            return;
        }*/

        // Instant respawn hack
        /**if (Input.GetKeyDown (KeyCode.P)) {
            BeginRespawn ();
        }*/

        // if (enterSpectatorModeTimer > 0f)
        // {
        //     enterSpectatorModeTimer -= Time.deltaTime;
        //     if (enterSpectatorModeTimer <= 0f)
        //     {
        //         EnterSpectatorMode();
        //     }
        // }

        // if (gameController.sectorsCleared == 0 && gameController.bombsRemaining == 2)
        // {
        //     gameController.sectorsCleared++;
        //     hud.OnScreenEffect("SECTOR CLEARED!", false);
        //     BeginRespawn();
        // }

        // if (gameController.bombsRemaining == 0 && !escapeAvailablePopup)
        // {
        //     escapeAvailablePopup = true;
        //     hud.MessagePopup("Escape available! Head to the waypoint!");
        //     hud.ComBoxPopup(2f, "Well done. There's an extraction waiting for you on the top of the construction site. Democko signing out.");
        // }

        // // Update assault mode
        // hud.UpdateAssaultModeIndHud(gameController.assaultMode);

        // // On assault mode changed
        // bool h = gameController.assaultMode;
        // if (h != assaultModeChangedIndicator)
        // {
        //     assaultModeChangedIndicator = h;
        //     hud.MessagePopup("Your cover is blown!");
        //     hud.ComBoxPopup(2f, "They know you're here! Slot the bastards!");
        //     hud.ComBoxPopup(20f, "Cicadas on the rooftops! Watch the rooftops!");
        // }

        if (health > 0 && fpc.enabled && fpc.m_IsRunning)
        {
            //audioController.PlaySprintSound(true);
            canShoot = false;
            animator.SetBool("isSprinting", true);
            if (sprintTime > 0f)
            {
                sprintTime -= Time.deltaTime;
            }
            if (fpc.m_IsRunning && sprintTime <= 0f)
            {
                fpc.sprintLock = true;
            }
        }
        else
        {
            //audioController.PlaySprintSound(false);
            animator.SetBool("isSprinting", false);
            if (sprintTime < 3f)
            {
                sprintTime += Time.deltaTime;
            }
            if (!isDefusing)
            {
                canShoot = true;
            }
            else
            {
                canShoot = false;
            }
        }

        if (!Input.GetKey(KeyCode.LeftShift) && fpc.sprintLock)
        {
            fpc.sprintLock = false;
        }

        // DeathCheck();
        // if (health <= 0)
        // {
        //     if (!escapeValueSent)
        //     {
        //         escapeValueSent = true;
        //         gameController.IncrementDeathCount();
        //     }
        // }
        // else
        // {
        //     BombCheck();
        // }

        if (fpc.enabled && fpc.canMove)
        {
            Crouch();
        }
        //DetermineEscaped();
        //RespawnRoutine();
    }

    void AddMyselfToPlayerList()
    {
        GameControllerScript.playerList.Add(photonView.OwnerActorNr, gameObject);
        GameControllerScript.totalKills.Add(photonView.Owner.NickName, kills);
        GameControllerScript.totalDeaths.Add(photonView.Owner.NickName, deaths);
    }

    public void TakeDamage(int d)
    {
        photonView.RPC("RpcTakeDamage", RpcTarget.All, d);
    }

    [PunRPC]
    void RpcTakeDamage(int d)
    {
        audioController.PlayGruntSound();
        if (photonView.IsMine)
        {
            audioController.PlayHitSound();
        }
        if (!godMode)
        {
            health -= d;
        }
    }

    public void Crouch()
    {
        bool originalCrouch = fpc.m_IsCrouching;
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (charController.isGrounded) {
                fpc.m_IsCrouching = !fpc.m_IsCrouching;
                animator.SetBool("Crouching", fpc.m_IsCrouching);
            }
        }

        // Collect the original y position of the FPS controller since we're going to move it downwards to crouch
        if (fpc.m_IsCrouching) {
            charController.height = 1f;
            charController.center = new Vector3(0f, 0.54f, 0f);
        } else {
            charController.height = charHeightOriginal;
            charController.center = new Vector3(0f, charCenterYOriginal, 0f);
        }
        
        // Set the animation to crouching
        animator.SetBool("Crouching", fpc.m_IsCrouching);

        // Network it
        // if (fpc.m_IsCrouching != originalCrouch)
        // {
        //     photonView.RPC("RpcCrouch", RpcTarget.Others, fpc.m_IsCrouching);
        // }
    }

    [PunRPC]
    public void RpcCrouch(bool crouch)
    {
        fpc.m_IsCrouching = crouch;
        float h = charHeightOriginal;
        //float viewH = fpcPositionYOriginal;
        //float speed = charController.velocity;
        float bodyScale = bodyTrans.lossyScale.y;

        // if (fpc.m_IsCrouching)
        // {
        //     h = charHeightOriginal * .65f;
        //     viewH = .55f;
        //     bodyScale = .7f;
        // }
        // else
        // {
        //     viewH = .8f;
        //     bodyScale = bodyScaleOriginal;
        // }

        float lastHeight = charController.height;
        // float lastCameraHeight = fpcPosition.position.y;
        // charController.height = Mathf.Lerp(charController.height, h, 10 * Time.deltaTime);
        // fpcPosition.localPosition = new Vector3(fpcPosition.localPosition.x, viewH, fpcPosition.localPosition.z);
        bodyTrans.localScale = new Vector3(bodyTrans.localScale.x, bodyScale, bodyTrans.localScale.z);
        transform.position = new Vector3(transform.position.x, transform.position.y + ((charController.height - lastHeight) / 2), transform.position.z);
    }

    void DeathCheck()
    {
        if (health <= 0)
        {
            /**if (fpsHands.activeInHierarchy) {
                //fpsHands.SetActive (false);
            }*/
            fpc.enabled = false;
            if (!rotationSaved)
            {
                if (escapeValueSent)
                {
                    gameController.ConvertCounts(1, -1);
                }
                photonView.RPC("RpcToggleFPSHands", RpcTarget.All, false);
                hud.ToggleHUD(false);
                hud.ToggleSpectatorMessage(true);
                deathCameraLerpPos = new Vector3(viewCam.transform.localPosition.x, viewCam.transform.localPosition.y, viewCam.transform.localPosition.z - 4.5f);
                alivePosition = new Vector3(0f, bodyTrans.localRotation.eulerAngles.y, 0f);
                deadPosition = new Vector3(-90f, bodyTrans.localRotation.eulerAngles.y, 0f);
                enterSpectatorModeTimer = 6f;
                rotationSaved = true;
                photonView.RPC("RpcAddToTotalDeaths", RpcTarget.All);
            }
            if (bodyTrans.rotation.x > -90f)
            {
                fraction += Time.deltaTime * 8f;
                bodyTrans.localRotation = Quaternion.Euler(Vector3.Lerp(alivePosition, deadPosition, fraction));
            }
            DeathCameraEffect();
        }
    }

    [PunRPC]
    void RpcAddToTotalDeaths()
    {
        deaths++;
        GameControllerScript.totalDeaths[photonView.Owner.NickName]++;
    }

    // If map objective is defusing bombs, this method checks if the player is near any bombs
    void BombCheck()
    {
        if (gameController.bombs == null)
        {
            return;
        }

        /**if (!currentBomb) {
            bool found = false;
            int count = 0;
            foreach (GameObject i in gameController.bombs) {
                BombScript b = i.GetComponent<BombScript> ();
                if (!b.defused) {
                    if (Vector3.Distance (gameObject.transform.position, i.transform.position) <= 4.5f) {
                        currentBomb = i;
                        currentBombIndex = count;
                        found = true;
                        break;
                    }
                }
                count++;
            }
            if (!found) {
                currentBomb = null;
            }
        }*/

        if (!currentBomb)
        {
            // Enable movement again
            if (!fpc.canMove && !hud.container.inGameMessenger.inputText.enabled)
            {
                fpc.canMove = true;
                isDefusing = false;
                hud.ToggleActionBar(false);
                hud.container.defusingText.enabled = false;
                hud.container.hintText.enabled = false;
                bombDefuseCounter = 0f;
            }

            if (bombIterator >= gameController.bombs.Length)
            {
                currentBomb = null;
                bombIterator = 0;
            }
            BombScript b = gameController.bombs[bombIterator].GetComponent<BombScript>();
            if (!b.defused)
            {
                if (Vector3.Distance(gameObject.transform.position, gameController.bombs[bombIterator].transform.position) <= 4.5f)
                {
                    currentBombIndex = bombIterator;
                    currentBomb = gameController.bombs[currentBombIndex];
                    bombIterator = -1;
                }
            }
            bombIterator++;
        }

        if (currentBomb != null)
        {
            // Check if the player is still near the bomb
            if (Vector3.Distance(gameObject.transform.position, currentBomb.transform.position) > 4.5f || currentBomb.GetComponent<BombScript>().defused)
            {
                currentBomb = null;
                hud.container.hintText.enabled = false;
                return;
            }

            if (Input.GetKey(KeyCode.E) && health > 0)
            {
                fpc.canMove = false;
                isDefusing = true;
                hud.container.hintText.enabled = false;
                hud.ToggleActionBar(true);
                hud.container.defusingText.enabled = true;
                bombDefuseCounter += (Time.deltaTime / 8f);
                hud.SetActionBarSlider(bombDefuseCounter);
                if (bombDefuseCounter >= 1f)
                {
                    bombDefuseCounter = 0f;

                    photonView.RPC("RpcDefuseBomb", RpcTarget.All, currentBombIndex);
                    gameController.DecrementBombsRemaining();
                    currentBomb = null;

                    hud.ToggleActionBar(false);
                    hud.container.defusingText.enabled = false;
                    hud.container.hintText.enabled = false;
                    // Enable movement again
                    fpc.canMove = true;
                    isDefusing = false;
                }
            }
            else
            {
                // Enable movement again
                if (!fpc.canMove)
                {
                    fpc.canMove = true;
                    isDefusing = false;
                    hud.ToggleActionBar(false);
                    hud.container.defusingText.enabled = false;
                    hud.container.hintText.enabled = true;
                    bombDefuseCounter = 0f;
                }
            }
        }
    }

    public void ResetHitTimer()
    {
        photonView.RPC("RpcResetHitTimer", RpcTarget.All);
        hitTimer = 0f;
    }

    [PunRPC]
    void RpcResetHitTimer()
    {
        hitTimer = 0f;
    }

    public void ResetHealTimer()
    {
        healTimer = 0f;
    }

    [PunRPC]
    void RpcSetHealth(int h)
    {
        health = h;
    }

    public void SetHitLocation(Vector3 pos)
    {
        hitLocation = pos;
    }

    void DetermineEscaped()
    {
        if (gameController.escapeAvailable)
        {
            if (!escapeValueSent)
            {
                if (health > 0 && Vector3.Distance(gameController.exitPoint.transform.position, transform.position) <= 10f && transform.position.y >= (gameController.exitPoint.transform.position.y - 1f))
                {
                    gameController.IncrementEscapeCount();
                    escapeValueSent = true;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (photonView.IsMine)
        {
            if (other.gameObject.tag.Equals("AmmoBox"))
            {
                wepActionScript.totalBulletsLeft = 120 + (wepActionScript.bulletsPerMag - wepActionScript.currentBullets);
                other.gameObject.GetComponent<PickupScript>().PlayPickupSound();
                other.gameObject.GetComponent<PickupScript>().DestroyPickup();
            }
            else if (other.gameObject.tag.Equals("HealthBox"))
            {
                ResetHealTimer();
                photonView.RPC("RpcSetHealth", RpcTarget.All, 100);
                other.gameObject.GetComponent<PickupScript>().PlayPickupSound();
                other.gameObject.GetComponent<PickupScript>().DestroyPickup();
            }
        }
        else
        {
            if (other.gameObject.tag.Equals("AmmoBox"))
            {
                other.gameObject.GetComponent<PickupScript>().DestroyPickup();
            }
            else if (other.gameObject.tag.Equals("HealthBox"))
            {
                other.gameObject.GetComponent<PickupScript>().DestroyPickup();
            }
        }
    }

    void DeathCameraEffect()
    {
        deathCameraLerpVar += (Time.deltaTime / 4f);
        viewCam.transform.localPosition = Vector3.Lerp(viewCam.transform.localPosition, deathCameraLerpPos, deathCameraLerpVar);
    }

    [PunRPC]
    void RpcToggleFPSHands(bool b)
    {
        viewCam.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().enabled = b;
        viewCam.gameObject.GetComponentInChildren<MeshRenderer>().enabled = b;
    }

    void EnterSpectatorMode()
    {
        photonView.RPC("RpcChangePlayerDisableStatus", RpcTarget.All, false);
        thisSpectatorCam = Instantiate(spectatorCam, Vector3.zero, Quaternion.Euler(Vector3.zero));
        thisSpectatorCam.transform.SetParent(gameObject.transform);
    }

    void LeaveSpectatorMode()
    {
        Destroy(thisSpectatorCam);
        thisSpectatorCam = null;
        photonView.RPC("RpcChangePlayerDisableStatus", RpcTarget.All, true);
    }

    [PunRPC]
    void RpcChangePlayerDisableStatus(bool status)
    {
        subComponents[0].SetActive(status);
        subComponents[1].SetActive(status);
        subComponents[4].SetActive(status);
        if (photonView.IsMine)
        {
            subComponents[2].SetActive(status);
            subComponents[3].SetActive(status);
            charController.enabled = status;
            fpc.enabled = status;
            viewCam.enabled = status;
            wepActionScript.enabled = status;
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        escapeValueSent = false;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Destroy(gameObject);
    }

    [PunRPC]
    void SyncPlayerColor(Vector3 c)
    {
        bodyTrans.gameObject.GetComponent<MeshRenderer>().material.color = new Color(c.x / 255f, c.y / 255f, c.z / 255f, 1f);
    }

    void BeginRespawn()
    {
        enterSpectatorModeTimer = 0f;
        if (health <= 0)
        {
            gameController.ConvertCounts(-1, 0);
            gameController.gameOver = false;
            // Flash the respawn time bar on the screen
            hud.RespawnBar();
            // Then, actually start the respawn process
            respawnTimer = 5f;
            isRespawning = true;
        }
    }

    void RespawnRoutine()
    {
        if (isRespawning)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0f)
            {
                isRespawning = false;
                Respawn();
            }
        }
    }

    // Reset character health, scale, rotation, position, ammo, re-enable FPS hands, disabled HUD components, disabled scripts, death variables, etc.
    void Respawn()
    {
        photonView.RPC("RpcSetHealth", RpcTarget.All, 100);
        photonView.RPC("RpcToggleFPSHands", RpcTarget.All, true);
        hud.ToggleHUD(true);
        hud.ToggleSpectatorMessage(false);
        fpc.m_IsCrouching = false;
        fpc.m_IsWalking = true;
        escapeValueSent = false;
        canShoot = true;
        fpc.canMove = true;
        fraction = 0f;
        deathCameraLerpVar = 0f;
        rotationSaved = false;
        hitTimer = 1f;
        healTimer = 1f;
        currentBomb = null;
        bombDefuseCounter = 0f;
        wepActionScript.totalBulletsLeft = 120;
        wepActionScript.currentBullets = wepActionScript.bulletsPerMag;

        // Send player back to spawn position, reset rotation, leave spectator mode
        bodyTrans.localRotation = Quaternion.Euler(alivePosition);
        transform.rotation = Quaternion.Euler(Vector3.zero);
        transform.position = new Vector3(gameController.spawnLocation.position.x, gameController.spawnLocation.position.y, gameController.spawnLocation.position.z);
        LeaveSpectatorMode();
        wepActionScript.CockingAction();
    }

    [PunRPC]
    void RpcDefuseBomb(int index)
    {
        gameController.bombs[index].GetComponent<BombScript>().Defuse();
    }

    public void HandleGameOverBanner()
    {
        if (fpc.enabled)
        {
            EnterSpectatorMode();
        }
        thisSpectatorCam.GetComponent<SpectatorScript>().GameOverCam();
    }
}
