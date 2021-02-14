using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;
using Firebase.Database;
using HttpsCallableReference = Firebase.Functions.HttpsCallableReference;
using Koobando.UI.Console;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using VivoxUnity;
using VivoxUnity.Common;
using VivoxUnity.Private;

public class PlayerData : MonoBehaviour, IOnEventCallback
{
    private const byte SPAWN_CODE = 123;
    private const byte SPAWN_INIT_CODE = 124;
    private const byte LEAVE_CODE = 125;
    private const byte ASK_OTHERS_FOR_THEM = 111;
    private const float TITLE_POS_X = 0f;
    private const float TITLE_POS_Y = -1.11f;
    private const float TITLE_POS_Z = 2.1f;
    private const float TITLE_ROT_X = 0f;
    private const float TITLE_ROT_Y = 180f;
    private const float TITLE_ROT_Z = 0f;

    public const string DEFAULT_SECONDARY = "I32";
    public const string DEFAULT_SUPPORT = "N76 Fragmentation";
    public const string DEFAULT_MELEE = "Recon Knife";
    public const string DEFAULT_FOOTWEAR_MALE = "Standard Boots (M)";
    public const string DEFAULT_FOOTWEAR_FEMALE = "Standard Boots (F)";
    public const uint MAX_EXP = 50000000;
    public const uint MAX_GP = uint.MaxValue;
    public const uint MAX_KASH = uint.MaxValue;

    public static PlayerData playerdata;
    public bool disconnectedFromServer;
    public string disconnectReason;
    private bool dataLoadedFlag;
    private bool triggerEmergencyExitFlag;
    private string emergencyExitMessage;
    private bool playerDataModifyLegalFlag;
    public bool inventoryDataModifyLegalFlag;
    public PlayerInfo info;
    public PlayerInventory inventory;
    public ObservableDict<string, FriendData> friendsList;
    public Dictionary<string, string> cachedSocialStatus;
    public ObservableDict<string, GiftData> giftList;
    public Dictionary<string, CachedMessage> cachedConversations;
    public ModInfo primaryModInfo;
    public ModInfo secondaryModInfo;
    public ModInfo supportModInfo;

    public GameObject bodyReference;
    public GameObject inGamePlayerReference;
    public TitleControllerScript titleRef;
    public GameOverController gameOverControllerRef;
    public GlobalChatClient globalChatClient;
    public Texture[] rankInsignias;

    void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
        if (playerdata == null)
        {
            DontDestroyOnLoad(gameObject);
            this.info = new PlayerInfo();
            this.inventory = new PlayerInventory();
            this.primaryModInfo = new ModInfo();
            this.secondaryModInfo = new ModInfo();
            this.supportModInfo = new ModInfo();
            friendsList = new ObservableDict<string, FriendData>();
            giftList = new ObservableDict<string, GiftData>();
            cachedConversations = new Dictionary<string, CachedMessage>();
            cachedSocialStatus = new Dictionary<string, string>();
            playerdata = this;

            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/loggedIn").ValueChanged += HandleForceLogoutEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/gp").ValueChanged += HandleGpChangeEvent;
            DAOScript.dao.dbRef.Child("users/" + AuthScript.authHandler.user.UserId + "/kash").ValueChanged += HandleKashChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedArmor").ValueChanged += HandleArmorChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedBottom").ValueChanged += HandleBottomChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedCharacter").ValueChanged += HandleCharacterChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedFacewear").ValueChanged += HandleFacewearChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedFootwear").ValueChanged += HandleFootwearChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedHeadgear").ValueChanged += HandleHeadgearChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedMelee").ValueChanged += HandleMeleeChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedPrimary").ValueChanged += HandlePrimaryChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedSecondary").ValueChanged += HandleSecondaryChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedSupport").ValueChanged += HandleSupportChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/equipment/equippedTop").ValueChanged += HandleTopChangeEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/ban").ChildAdded += HandleBanEvent;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/ban").ChildChanged += HandleBanEvent;

            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/friends").ChildAdded += HandleFriendAdded;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/friends").ChildRemoved += HandleFriendRemoved;

            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/gifts").ChildAdded += HandleGiftAdded;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_users/" + AuthScript.authHandler.user.UserId + "/gifts").ChildRemoved += HandleGiftRemoved;

            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/facewear").ChildAdded += HandleInventoryAdded;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/headgear").ChildAdded += HandleInventoryAdded;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/footwear").ChildAdded += HandleInventoryAdded;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/tops").ChildAdded += HandleInventoryAdded;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/bottoms").ChildAdded += HandleInventoryAdded;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/characters").ChildAdded += HandleInventoryAdded;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/weapons").ChildAdded += HandleInventoryAdded;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/mods").ChildAdded += HandleInventoryAdded;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/armor").ChildAdded += HandleInventoryAdded;

            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/facewear").ChildRemoved += HandleInventoryRemoved;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/headgear").ChildRemoved += HandleInventoryRemoved;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/footwear").ChildRemoved += HandleInventoryRemoved;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/tops").ChildRemoved += HandleInventoryRemoved;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/bottoms").ChildRemoved += HandleInventoryRemoved;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/characters").ChildRemoved += HandleInventoryRemoved;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/weapons").ChildRemoved += HandleInventoryRemoved;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/mods").ChildRemoved += HandleInventoryRemoved;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/armor").ChildRemoved += HandleInventoryRemoved;

            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/facewear").ChildChanged += HandleInventoryChanged;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/headgear").ChildChanged += HandleInventoryChanged;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/footwear").ChildChanged += HandleInventoryChanged;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/tops").ChildChanged += HandleInventoryChanged;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/bottoms").ChildChanged += HandleInventoryChanged;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/characters").ChildChanged += HandleInventoryChanged;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/weapons").ChildChanged += HandleInventoryChanged;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/mods").ChildChanged += HandleInventoryChanged;
            DAOScript.dao.dbRef.Child("fteam_ai/fteam_ai_inventory/" + AuthScript.authHandler.user.UserId + "/armor").ChildChanged += HandleInventoryChanged;

            SceneManager.sceneLoaded += OnSceneFinishedLoading;
            PlayerData.playerdata.info.PropertyChanged += OnPlayerInfoChange;
            friendsList.CollectionChanged += OnPlayerInfoChange;
            friendsList.PropertyChanged += OnPlayerInfoChange;
            giftList.CollectionChanged += OnPlayerInfoChange;
            giftList.PropertyChanged += OnPlayerInfoChange;
        }
        else if (playerdata != this)
        {
            Destroy(gameObject);
        }
    }

    void Update() {
        if (dataLoadedFlag) {
            globalChatClient.Initialize(PlayerData.playerdata.info.Playername);
            InstantiatePlayer();
            titleRef.SetPlayerStatsForTitle();
            titleRef.ToggleLoadingScreen(false);
            if (PhotonNetwork.InRoom) {
                string gameModeWas = (string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"];
                if (gameModeWas == "versus") {
                    titleRef.mainPanelManager.OpenPanel("Versus");
                } else if (gameModeWas == "camp") {
                    titleRef.mainPanelManager.OpenPanel("Campaign");
                }
                titleRef.connexion.listPlayer.rejoinedRoomFlag = true;
                titleRef.connexion.listPlayer.OnJoinedRoom();
            } else {
			    titleRef.mainPanelManager.OpenFirstTab();
                try {
                    ChannelId leavingChannelId = VivoxVoiceManager.Instance.TransmittingSession.Channel;
                    VivoxVoiceManager.Instance.TransmittingSession.Disconnect();
                    VivoxVoiceManager.Instance.LoginSession.DeleteChannelSession(leavingChannelId);
                } catch (Exception e) {
                    Debug.Log("Tried to leave Vivox voice channel, but encountered an error: " + e.Message);
                }
            }
            titleRef.friendsMessenger.RefreshNotifications();
            dataLoadedFlag = false;
        }
        if (triggerEmergencyExitFlag) {
            DoEmergencyExit();
            triggerEmergencyExitFlag = false;
        }
    }

    Vector3 GetTitlePos() {
        return new Vector3(TITLE_POS_X, TITLE_POS_Y, TITLE_POS_Z);
    }

    Quaternion GetTitleRot() {
        return Quaternion.Euler(TITLE_ROT_X, TITLE_ROT_Y, TITLE_ROT_Z);
    }

    string GetCharacterPrefabName() {
        string characterPrefabName = "";
        if (PlayerData.playerdata.info.EquippedCharacter.Equals("Lucas")) {
            characterPrefabName = "LucasGamePrefab";
        } else if (PlayerData.playerdata.info.EquippedCharacter.Equals("Daryl")) {
            characterPrefabName = "DarylGamePrefab";
        } else if (PlayerData.playerdata.info.EquippedCharacter.Equals("Yongjin")) {
            characterPrefabName = "YongjinGamePrefab";
        } else if (PlayerData.playerdata.info.EquippedCharacter.Equals("Rocko")) {
            characterPrefabName = "RockoGamePrefab";
        } else if (PlayerData.playerdata.info.EquippedCharacter.Equals("Codename Sayre")) {
            characterPrefabName = "SayreGamePrefab";
        } else if (PlayerData.playerdata.info.EquippedCharacter.Equals("Hana")) {
            characterPrefabName = "HanaGamePrefab";
        } else if (PlayerData.playerdata.info.EquippedCharacter.Equals("Jade")) {
            characterPrefabName = "JadeGamePrefab";
        } else if (PlayerData.playerdata.info.EquippedCharacter.Equals("Dani")) {
            characterPrefabName = "DaniGamePrefab";
        }
        return characterPrefabName;
    }

    protected virtual void OnPlayerInfoChange(object sender, PropertyChangedEventArgs e) {
        // This should never be triggered unless called from the listeners. Therefore if it is, we need to ban the player
        if (!playerDataModifyLegalFlag) {
            // Ban player here
            Dictionary<string, object> inputData = new Dictionary<string, object>();
            inputData["callHash"] = DAOScript.functionsCallHash;
            inputData["uid"] = AuthScript.authHandler.user.UserId;
            inputData["duration"] = "-1";
            inputData["reason"] = "Illegal modification of user data.";

            HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("banPlayer");
            func.CallAsync(inputData).ContinueWith((task) => {
                TriggerEmergencyExit("You've been banned for the following reason:\nIllegal modification of user data.\nIf you feel this was done in error, you can dispute it by opening a ticket at \"www.koobando.com/support\".");
            });
        }
    }

    protected virtual void OnPlayerInfoChange(object sender, NotifyCollectionChangedEventArgs e) {
        if (!playerDataModifyLegalFlag) {
            // Ban player here
            Dictionary<string, object> inputData = new Dictionary<string, object>();
            inputData["callHash"] = DAOScript.functionsCallHash;
            inputData["uid"] = AuthScript.authHandler.user.UserId;
            inputData["duration"] = "-1";
            inputData["reason"] = "Illegal modification of user data.";

            HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("banPlayer");
            func.CallAsync(inputData).ContinueWith((task) => {
                TriggerEmergencyExit("You've been banned for the following reason:\nIllegal modification of user data.\nIf you feel this was done in error, you can dispute it by opening a ticket at \"www.koobando.com/support\".");
            });
        }
    }

    public void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        string levelName = SceneManager.GetActiveScene().name;
        if (levelName.Equals("Badlands1") || levelName.Equals("Badlands1_Red") || levelName.Equals("Badlands1_Blue"))
        {
            globalChatClient.UnsubscribeFromGlobalChat();
            string characterPrefabName = GetCharacterPrefabName();
            SpawnPlayer(characterPrefabName, Photon.Pun.LobbySystemPhoton.ListPlayer.mapSpawnPoints[0]);
            AskOthersForThemselves();
            if (PlayerData.playerdata.globalChatClient != null) {
                PlayerData.playerdata.globalChatClient.UpdateStatus("IN GAME");
            }
            // PlayerData.playerdata.inGamePlayerReference = PhotonNetwork.Instantiate(
            //     characterPrefabName,
            //     Photon.Pun.LobbySystemPhoton.ListPlayer.mapSpawnPoints[0],
            //     Quaternion.Euler(Vector3.zero));
        } else if (levelName.Equals("Badlands2") || levelName.Equals("Badlands2_Red") || levelName.Equals("Badlands2_Blue")) {
            globalChatClient.UnsubscribeFromGlobalChat();
            string characterPrefabName = GetCharacterPrefabName();
            SpawnPlayer(characterPrefabName, Photon.Pun.LobbySystemPhoton.ListPlayer.mapSpawnPoints[1]);
            AskOthersForThemselves();
            if (PlayerData.playerdata.globalChatClient != null) {
                PlayerData.playerdata.globalChatClient.UpdateStatus("IN GAME");
            }
            // PlayerData.playerdata.inGamePlayerReference = PhotonNetwork.Instantiate(
            //     characterPrefabName,
            //     Photon.Pun.LobbySystemPhoton.ListPlayer.mapSpawnPoints[1],
            //     Quaternion.Euler(0f, 180f, 0f));
        }
        // else if (levelName.Equals("Test")) {
        //     string characterPrefabName = GetCharacterPrefabName();
        //     PlayerData.playerdata.inGamePlayerReference = PhotonNetwork.Instantiate(
        //         characterPrefabName,
        //         Photon.Pun.LobbySystemPhoton.ListPlayer.mapSpawnPoints[1],
        //         Quaternion.Euler(Vector3.zero));
        // }
        else
        {
            // if (PlayerData.playerdata.inGamePlayerReference != null)
            // {
                // PhotonNetwork.Destroy(PlayerData.playerdata.inGamePlayerReference);
                // foreach (PlayerStat entry in GameControllerScript.playerList.Values)
                // {
                //     Destroy(entry.objRef);
                // }

                // GameControllerScript.playerList.Clear();
            // }
            if (levelName.Equals("Title"))
            {
                if (PlayerData.playerdata.bodyReference == null)
                {
                    LoadPlayerData();
                    // LoadInventory();
                }
                titleRef.SetPlayerStatsForTitle();
                if (PlayerData.playerdata.globalChatClient != null) {
                    PlayerData.playerdata.globalChatClient.UpdateStatus("ONLINE");
                }
            }
        }

    }

    void SpawnPlayer(string playerPrefab, Vector3 spawnPoints)
    {
        GameObject player = Instantiate((GameObject)Resources.Load(playerPrefab), spawnPoints, Quaternion.Euler(Vector3.zero));
        PlayerData.playerdata.inGamePlayerReference = player;
        PhotonView photonView = player.GetComponent<PhotonView>();
        // photonView.ViewID = PhotonNetwork.LocalPlayer.ActorNumber;
        photonView.SetOwnerInternal(PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer.ActorNumber);
        VivoxVoiceManager.Instance.AudioInputDevices.Muted = true;

        if (PhotonNetwork.AllocateViewID(photonView))
        {
            InitPlayerInGame(player);
            AddMyselfToPlayerList(photonView, player);
            SpawnMyselfOnOthers(true);
        }
        else
        {
            Debug.Log("Failed to allocate a ViewId.");
            Destroy(player);
        }
    }

    void SpawnMyselfOnOthers(bool initial) {
        if (IsNotInGame()) return;
        GameObject player = PlayerData.playerdata.inGamePlayerReference;
        PhotonView photonView = player.GetComponent<PhotonView>();
        object[] data = new object[]
        {
            GetCharacterPrefabName(), player.transform.position, player.transform.rotation, photonView.ViewID, photonView.OwnerActorNr
        };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others,
            // CachingOption = EventCaching.AddToRoomCache
            CachingOption = EventCaching.DoNotCache
        };

        SendOptions sendOptions = new SendOptions
        {
            Reliability = true
        };

        if (initial) {
            // Spawning myself locally
            PhotonNetwork.RaiseEvent(SPAWN_INIT_CODE, data, raiseEventOptions, sendOptions);
        } else {
            // Spawning myself on other machines
            PhotonNetwork.RaiseEvent(SPAWN_CODE, data, raiseEventOptions, sendOptions);
        }
    }

    void AskOthersForThemselves() {
        object[] data = new object[]{};

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others,
            CachingOption = EventCaching.DoNotCache
        };

        SendOptions sendOptions = new SendOptions
        {
            Reliability = true
        };

        PhotonNetwork.RaiseEvent(ASK_OTHERS_FOR_THEM, data, raiseEventOptions, sendOptions);
    }
    
    public void DestroyMyself() {
        object[] data = new object[]
        {
            PhotonNetwork.LocalPlayer.ActorNumber
        };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others,
            CachingOption = EventCaching.DoNotCache
        };

        SendOptions sendOptions = new SendOptions
        {
            Reliability = true
        };

        PhotonNetwork.RaiseEvent(LEAVE_CODE, data, raiseEventOptions, sendOptions);
        PhotonNetwork.LoadLevel("Title");
    }

    void AddMyselfToPlayerList(PhotonView pView, GameObject playerRef)
    {
        // Debug.Log("Actor no: " + pView.Owner.ActorNumber);
        char team = 'N';
        if ((string)pView.Owner.CustomProperties["team"] == "red") {
            team = 'R';
            Debug.Log(pView.Owner.NickName + " joined red team.");
        } else if ((string)pView.Owner.CustomProperties["team"] == "blue") {
            team = 'B';
            Debug.Log(pView.Owner.NickName + " joined blue team.");
        }
        PlayerStat p = new PlayerStat(playerRef, pView.Owner.ActorNumber, pView.Owner.NickName, team, Convert.ToUInt32(pView.Owner.CustomProperties["exp"]));
        if (GameControllerScript.playerList == null) {
            GameControllerScript.playerList = new Dictionary<int, PlayerStat>();
        }
        GameControllerScript.playerList.Add(pView.Owner.ActorNumber, p);
    }

    void AddMyselfToPlayerList(int actorNo)
    {
        Debug.Log("Actor no: " + actorNo);
        Player playerBeingAdded = PhotonNetwork.CurrentRoom.GetPlayer(actorNo);
        char team = 'N';
        if ((string)playerBeingAdded.CustomProperties["team"] == "red") {
            team = 'R';
            Debug.Log(playerBeingAdded.NickName + " joined red team.");
        } else if ((string)playerBeingAdded.CustomProperties["team"] == "blue") {
            team = 'B';
            Debug.Log(playerBeingAdded.NickName + " joined blue team.");
        }
        PlayerStat p = new PlayerStat(null, actorNo, playerBeingAdded.NickName, team, Convert.ToUInt32(playerBeingAdded.CustomProperties["exp"]));
        if (GameControllerScript.playerList == null) {
            GameControllerScript.playerList = new Dictionary<int, PlayerStat>();
        }
        GameControllerScript.playerList.Add(actorNo, p);
    }

    public void OnEvent(EventData photonEvent)
    {
        if (IsNotInGame()) return;
        if (photonEvent.Code == SPAWN_INIT_CODE)
        {
            object[] data = (object[]) photonEvent.CustomData;
            int ownerActorNr = (int) data[4];
            if (GameControllerScript.playerList.ContainsKey(ownerActorNr)) {
                return;
            }
            string gameMode = (string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"];

            if (gameMode == "camp") {
                GameObject player = (GameObject) Instantiate((GameObject)Resources.Load(((string)data[0])), (Vector3) data[1], (Quaternion) data[2]);
                PhotonView photonView = player.GetComponent<PhotonView>();
                photonView.SetOwnerInternal(PhotonNetwork.CurrentRoom.GetPlayer(ownerActorNr), ownerActorNr);
                photonView.ViewID = (int) data[3];
                Debug.Log("Spawned character " + player.gameObject.name + " with owner " + ownerActorNr + " and view ID " + photonView.ViewID);
                InitPlayerInGame(player);
                player.GetComponent<EquipmentScript>().SyncDataOnJoin();
                player.GetComponent<WeaponScript>().SyncDataOnJoin();
                player.GetComponent<PlayerActionScript>().SyncDataOnJoin(true);
                AddMyselfToPlayerList(photonView, player);
            } else if (gameMode == "versus") {
                string currentMapName = SceneManager.GetActiveScene().name;
                string team = (string)PhotonNetwork.CurrentRoom.GetPlayer(ownerActorNr).CustomProperties["team"];
                // Only spawn players on the same team, but add ALL players to the list
                if ((currentMapName.EndsWith("_Red") && team == "red") || (currentMapName.EndsWith("_Blue") && team == "blue")) {
                    GameObject player = (GameObject) Instantiate((GameObject)Resources.Load(((string)data[0])), (Vector3) data[1], (Quaternion) data[2]);
                    PhotonView photonView = player.GetComponent<PhotonView>();
                    photonView.SetOwnerInternal(PhotonNetwork.CurrentRoom.GetPlayer(ownerActorNr), ownerActorNr);
                    photonView.ViewID = (int) data[3];
                    Debug.Log("Spawned character " + player.gameObject.name + " with owner " + ownerActorNr + " and view ID " + photonView.ViewID + " on team [" + team + "]");
                    InitPlayerInGame(player);
                    player.GetComponent<EquipmentScript>().SyncDataOnJoin();
                    player.GetComponent<WeaponScript>().SyncDataOnJoin();
                    player.GetComponent<PlayerActionScript>().SyncDataOnJoin(true);
                    AddMyselfToPlayerList(photonView, player);
                } else {
                    AddMyselfToPlayerList(ownerActorNr);
                }
            }
        } else if (photonEvent.Code == SPAWN_CODE)
        {
            object[] data = (object[]) photonEvent.CustomData;
            int ownerActorNr = (int) data[4];
            if (GameControllerScript.playerList.ContainsKey(ownerActorNr)) {
                return;
            }
            string gameMode = (string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"];

            if (gameMode == "camp") {
                GameObject player = (GameObject) Instantiate((GameObject)Resources.Load(((string)data[0])), (Vector3) data[1], (Quaternion) data[2]);
                PhotonView photonView = player.GetComponent<PhotonView>();
                photonView.SetOwnerInternal(PhotonNetwork.CurrentRoom.GetPlayer(ownerActorNr), ownerActorNr);
                photonView.ViewID = (int) data[3];
                Debug.Log("Spawned character " + player.gameObject.name + " with owner " + ownerActorNr + " and view ID " + photonView.ViewID);
                InitPlayerInGame(player);
                player.GetComponent<EquipmentScript>().SyncDataOnJoin();
                player.GetComponent<WeaponScript>().SyncDataOnJoin();
                player.GetComponent<PlayerActionScript>().SyncDataOnJoin(false);
                AddMyselfToPlayerList(photonView, player);
            } else if (gameMode == "versus") {
                string currentMapName = SceneManager.GetActiveScene().name;
                string team = (string)PhotonNetwork.CurrentRoom.GetPlayer(ownerActorNr).CustomProperties["team"];
                // Only spawn players on the same team, but add ALL players to the list
                if ((currentMapName.EndsWith("_Red") && team == "red") || (currentMapName.EndsWith("_Blue") && team == "blue")) {
                    GameObject player = (GameObject) Instantiate((GameObject)Resources.Load(((string)data[0])), (Vector3) data[1], (Quaternion) data[2]);
                    PhotonView photonView = player.GetComponent<PhotonView>();
                    photonView.SetOwnerInternal(PhotonNetwork.CurrentRoom.GetPlayer(ownerActorNr), ownerActorNr);
                    photonView.ViewID = (int) data[3];
                    Debug.Log("Spawned character " + player.gameObject.name + " with owner " + ownerActorNr + " and view ID " + photonView.ViewID + " on team [" + team + "]");
                    InitPlayerInGame(player);
                    player.GetComponent<EquipmentScript>().SyncDataOnJoin();
                    player.GetComponent<WeaponScript>().SyncDataOnJoin();
                    player.GetComponent<PlayerActionScript>().SyncDataOnJoin(false);
                    AddMyselfToPlayerList(photonView, player);
                } else {
                    AddMyselfToPlayerList(ownerActorNr);
                }
            }
        } else if (photonEvent.Code == LEAVE_CODE)
        {
            if (IsNotInGame()) return;
            object[] data = (object[]) photonEvent.CustomData;
            int actorNo = (int) data[0];
            if (!GameControllerScript.playerList.ContainsKey(actorNo)) {
                return;
            }
            GameObject playerToDestroy = GameControllerScript.playerList[actorNo].objRef;
            PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerHUDScript>().RemovePlayerMarker(actorNo);
            PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerActionScript>().gameController.TogglePlayerSpeaking(false, actorNo, null);
            GameControllerScript.playerList.Remove(actorNo);
            foreach (PlayerStat entry in GameControllerScript.playerList.Values)
            {
                if (entry.objRef == null) continue;
                entry.objRef.GetComponent<PlayerActionScript> ().escapeValueSent = false;
            }
            PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerActionScript>().gameController.ResetEscapeValues ();
            PlayerData.playerdata.inGamePlayerReference.GetComponent<PlayerActionScript>().OnPlayerLeftRoom(PhotonNetwork.CurrentRoom.GetPlayer(actorNo));
            if (playerToDestroy != null) {
                Destroy(playerToDestroy);
            }
        } else if (photonEvent.Code == ASK_OTHERS_FOR_THEM)
        {
            SpawnMyselfOnOthers(false);
        }
    }

    void InitPlayerInGame(GameObject player) {
        player.GetComponent<EquipmentScript>().PreInitialize();
        player.GetComponent<EquipmentScript>().Initialize();
        player.GetComponent<PlayerHUDScript>().Initialize();
        player.GetComponent<WeaponScript>().PreInitialize();
        player.GetComponent<WeaponScript>().Initialize();
        player.GetComponent<WeaponActionScript>().Initialize();
        player.GetComponent<PlayerActionScript>().PreInitialize();
        player.GetComponent<PlayerActionScript>().Initialize();
        player.GetComponent<CameraShakeScript>().PreInitialize();
        player.GetComponent<AudioControllerScript>().Initialize();
        player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().Initialize();
    }

    public void LoadPlayerData()
    {
        if (titleRef == null) {
            titleRef = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
        }
        if (globalChatClient == null) {
            globalChatClient = GameObject.Find("GlobalChatClient").GetComponent<GlobalChatClient>();
        }
        playerDataModifyLegalFlag = true;
        // Check if the DB has equipped data for the player. If not, then set default char and equips.
        // If error occurs, show error message on splash and quit the application
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;

		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("loadPlayerDataAndInventory");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                TriggerEmergencyExit("Your data could not be loaded. Either your data is corrupted, or the service is unavailable. Please check the website for further details. If this issue persists, please create a ticket at koobando.com/support.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() == "200") {
                    Dictionary<object, object> playerDataSnap = (Dictionary<object, object>)results["playerData"];
                    Dictionary<object, object> inventorySnap = (Dictionary<object, object>)results["inventory"];
                    Dictionary<object, object> friendsUsernameMap = (Dictionary<object, object>)results["friendUsernameMap"];
                    Dictionary<object, object> expMap = (Dictionary<object, object>)results["expMap"];
                    List<object> friendData = (List<object>)results["friendData"];
                    info.DefaultChar = playerDataSnap["defaultChar"].ToString();
                    info.DefaultWeapon = playerDataSnap["defaultWeapon"].ToString();
                    info.Playername = playerDataSnap["username"].ToString();
                    info.Exp = uint.Parse(playerDataSnap["exp"].ToString());
                    info.Gp = uint.Parse(playerDataSnap["gp"].ToString());
                    info.Kash = Convert.ToUInt32(results["kash"]);
                    info.PrivilegeLevel = results["privilegeLevel"].ToString();
                    
                    if (playerDataSnap.ContainsKey("equipment")) {
                        Dictionary<object, object> equipmentSnap = (Dictionary<object, object>)playerDataSnap["equipment"];
                        info.EquippedCharacter = equipmentSnap["equippedCharacter"].ToString();
                        info.EquippedPrimary = equipmentSnap["equippedPrimary"].ToString();
                        info.EquippedSecondary = equipmentSnap["equippedSecondary"].ToString();
                        info.EquippedSupport = equipmentSnap["equippedSupport"].ToString();
                        info.EquippedMelee = equipmentSnap["equippedMelee"].ToString();
                        info.EquippedTop = equipmentSnap["equippedTop"].ToString();
                        info.EquippedBottom = equipmentSnap["equippedBottom"].ToString();
                        info.EquippedFootwear = equipmentSnap["equippedFootwear"].ToString();
                        info.EquippedFacewear = equipmentSnap["equippedFacewear"].ToString();
                        info.EquippedHeadgear = equipmentSnap["equippedHeadgear"].ToString();
                        info.EquippedArmor = equipmentSnap["equippedArmor"].ToString();
                        Dictionary<object, object> weaponsInventorySnap = (Dictionary<object, object>)inventorySnap["weapons"];
                        Dictionary<object, object> thisWeaponInventorySnap = (Dictionary<object, object>)weaponsInventorySnap[info.EquippedPrimary];
                        if (inventorySnap.ContainsKey("mods")) {
                            Dictionary<object, object> modsInventorySnap = (Dictionary<object, object>)inventorySnap["mods"];
                            Dictionary<object, object> suppressorModSnap = null;
                            Dictionary<object, object> sightModSnap = null;
                            string suppressorModId = thisWeaponInventorySnap["equippedSuppressor"].ToString();
                            string sightModId = thisWeaponInventorySnap["equippedSight"].ToString();
                            primaryModInfo.WeaponName = info.EquippedPrimary;
                            primaryModInfo.SuppressorId = suppressorModId;
                            primaryModInfo.SightId = sightModId;
                            if (!"".Equals(suppressorModId)) {
                                suppressorModSnap = (Dictionary<object, object>)modsInventorySnap[suppressorModId];
                                primaryModInfo.EquippedSuppressor = suppressorModSnap["name"].ToString();
                            }
                            if (!"".Equals(sightModId)) {
                                sightModSnap = (Dictionary<object, object>)modsInventorySnap[sightModId];
                                primaryModInfo.EquippedSight = sightModSnap["name"].ToString();
                            }
                            thisWeaponInventorySnap = (Dictionary<object, object>)weaponsInventorySnap[info.EquippedSecondary];
                            suppressorModId = thisWeaponInventorySnap["equippedSuppressor"].ToString();
                            sightModId = thisWeaponInventorySnap["equippedSight"].ToString();
                            secondaryModInfo.WeaponName = info.EquippedSecondary;
                            secondaryModInfo.SuppressorId = suppressorModId;
                            secondaryModInfo.SightId = sightModId;
                            if (!"".Equals(suppressorModId)) {
                                suppressorModSnap = (Dictionary<object, object>)modsInventorySnap[suppressorModId];
                                secondaryModInfo.EquippedSuppressor = suppressorModSnap["name"].ToString();
                            }
                            if (!"".Equals(sightModId)) {
                                sightModSnap = (Dictionary<object, object>)modsInventorySnap[sightModId];
                                secondaryModInfo.EquippedSight = sightModSnap["name"].ToString();
                            }
                            thisWeaponInventorySnap = (Dictionary<object, object>)weaponsInventorySnap[info.EquippedSupport];
                            suppressorModId = thisWeaponInventorySnap["equippedSuppressor"].ToString();
                            sightModId = thisWeaponInventorySnap["equippedSight"].ToString();
                            supportModInfo.WeaponName = info.EquippedSupport;
                            supportModInfo.SuppressorId = suppressorModId;
                            supportModInfo.SightId = sightModId;
                            if (!"".Equals(suppressorModId)) {
                                suppressorModSnap = (Dictionary<object, object>)modsInventorySnap[suppressorModId];
                                supportModInfo.EquippedSuppressor = suppressorModSnap["name"].ToString();
                            }
                            if (!"".Equals(sightModId)) {
                                sightModSnap = (Dictionary<object, object>)modsInventorySnap[sightModId];
                                supportModInfo.EquippedSuppressor = sightModSnap["name"].ToString();
                            }
                        }
                    } else {
                        info.EquippedCharacter = playerDataSnap["defaultChar"].ToString();
                        char g = InventoryScript.itemData.characterCatalog[info.EquippedCharacter].gender;
                        info.EquippedPrimary = playerDataSnap["defaultWeapon"].ToString();
                        info.EquippedSecondary = DEFAULT_SECONDARY;
                        info.EquippedSupport = DEFAULT_SUPPORT;
                        info.EquippedMelee = DEFAULT_MELEE;
                        info.EquippedTop = InventoryScript.itemData.characterCatalog[info.EquippedCharacter].defaultTop;
                        info.EquippedBottom = InventoryScript.itemData.characterCatalog[info.EquippedCharacter].defaultBottom;
                        info.EquippedFootwear = (g == 'M' ? DEFAULT_FOOTWEAR_MALE : DEFAULT_FOOTWEAR_FEMALE);
                        info.EquippedFacewear = "";
                        info.EquippedHeadgear = "";
                        info.EquippedArmor = "";
        
                        primaryModInfo.EquippedSuppressor = "";
                        primaryModInfo.EquippedSight = "";
                        primaryModInfo.WeaponName = "";
                        primaryModInfo.SuppressorId = "";
                        primaryModInfo.SightId = "";
        
                        secondaryModInfo.EquippedSuppressor = "";
                        secondaryModInfo.EquippedSight = "";
                        secondaryModInfo.WeaponName = "";
                        secondaryModInfo.SuppressorId = "";
                        secondaryModInfo.SightId = "";
        
                        supportModInfo.EquippedSuppressor = "";
                        supportModInfo.EquippedSight = "";
                        supportModInfo.WeaponName = "";
                        supportModInfo.SuppressorId = "";
                        supportModInfo.SightId = "";
                    }
                    LoadInventory(inventorySnap);
                    if (playerDataSnap.ContainsKey("gifts")) {
                        LoadGifts((Dictionary<object, object>)playerDataSnap["gifts"]);
                    }
                    LoadFriends(friendsUsernameMap, expMap, friendData);
                    List<object> itemsExpired = (List<object>)results["itemsExpired"];
                    if (itemsExpired.Count > 0) {
                        titleRef.TriggerExpirationPopup(itemsExpired);
                    }
                    playerDataModifyLegalFlag = false;
                    dataLoadedFlag = true;
                } else {
                    TriggerEmergencyExit("Your data could not be loaded. Either your data is corrupted, or the service is unavailable. Please check the website for further details. If this issue persists, please create a ticket at koobando.com/support.");
                }
            }
        });
    }

    public void InstantiatePlayer() {
        FindBodyRef(info.EquippedCharacter);
        EquipmentScript characterEquips = bodyReference.GetComponent<EquipmentScript>();
        characterEquips.equippedSkin = -1;
        WeaponScript characterWeps = bodyReference.GetComponent<WeaponScript>();
        this.primaryModInfo = LoadModDataForWeapon(info.EquippedPrimary);
        this.secondaryModInfo = LoadModDataForWeapon(info.EquippedSecondary);
        this.supportModInfo = LoadModDataForWeapon(info.EquippedSupport);
        playerDataModifyLegalFlag = true;
        characterEquips.ts = titleRef;
        characterWeps.ts = titleRef;
        OnCharacterChange(info.EquippedCharacter);
        OnHeadgearChange(info.EquippedHeadgear);
        OnFacewearChange(info.EquippedFacewear);
        OnTopChange(info.EquippedTop);
        OnBottomChange(info.EquippedBottom);
        OnFootwearChange(info.EquippedFootwear);
        OnArmorChange(info.EquippedArmor);
        OnPrimaryChange(info.EquippedPrimary);
        OnSecondaryChange(info.EquippedSecondary);
        OnSupportChange(info.EquippedSupport);
        OnMeleeChange(info.EquippedMelee);
        PhotonNetwork.NickName = info.Playername;
        playerDataModifyLegalFlag = false;
    }

    void RefreshInventory(DataSnapshot snapshot, char transactionType) {
        if (transactionType == 'a') {
            string itemName = snapshot.Key;
            if (InventoryScript.itemData.characterCatalog.ContainsKey(itemName)) {
                CharacterData c = new CharacterData();
                c.Duration = snapshot.Child("duration").Value.ToString();
                c.AcquireDate = snapshot.Child("acquireDate").Value.ToString();
                c.PropertyChanged += OnPlayerInfoChange;
                inventory.myCharacters.Add(itemName, c);
            } else if (InventoryScript.itemData.equipmentCatalog.ContainsKey(itemName)) {
                string category = InventoryScript.itemData.equipmentCatalog[itemName].category;
                EquipmentData e = new EquipmentData();
                e.PropertyChanged += OnPlayerInfoChange;
                e.Duration = snapshot.Child("duration").Value.ToString();
                e.AcquireDate = snapshot.Child("acquireDate").Value.ToString();
                if (category == "Top") {
                    inventory.myTops.Add(itemName, e);
                } else if (category == "Bottom") {
                    inventory.myBottoms.Add(itemName, e);
                } else if (category == "Footwear") {
                    inventory.myFootwear.Add(itemName, e);
                } else if (category == "Facewear") {
                    inventory.myFacewear.Add(itemName, e);
                } else if (category == "Headgear") {
                    inventory.myHeadgear.Add(itemName, e);
                }
            } else if (InventoryScript.itemData.armorCatalog.ContainsKey(itemName)) {
                ArmorData a = new ArmorData();
                a.PropertyChanged += OnPlayerInfoChange;
                a.Duration = snapshot.Child("duration").Value.ToString();
                a.AcquireDate = snapshot.Child("acquireDate").Value.ToString();
                inventory.myArmor.Add(itemName, a);
            } else if (InventoryScript.itemData.weaponCatalog.ContainsKey(itemName)) {
                WeaponData w = new WeaponData();
                w.PropertyChanged += OnPlayerInfoChange;
                w.Duration = snapshot.Child("duration").Value.ToString();
                w.AcquireDate = snapshot.Child("acquireDate").Value.ToString();
                w.EquippedSuppressor = "";
                w.EquippedSight = "";
                w.EquippedClip = "";
                inventory.myWeapons.Add(itemName, w);
            } else if (InventoryScript.itemData.modCatalog.ContainsKey(snapshot.Child("name").Value.ToString())) {
                ModData m = new ModData();
                m.PropertyChanged += OnPlayerInfoChange;
                m.Name = snapshot.Child("name").Value.ToString();
                m.Duration = snapshot.Child("duration").Value.ToString();
                m.AcquireDate = snapshot.Child("acquireDate").Value.ToString();
                m.EquippedOn = snapshot.Child("equippedOn").Value.ToString();
                inventory.myMods.Add(itemName, m);
            }
        } else if (transactionType == 'd') {
            string itemName = snapshot.Key;
            if (inventory.myTops.ContainsKey(itemName)) {
                inventory.myTops[itemName].PropertyChanged -= OnPlayerInfoChange;
                inventory.myTops.Remove(itemName);
                titleRef.RemoveItemFromShopContent('e', itemName);
            } else if (inventory.myBottoms.ContainsKey(itemName)) {
                inventory.myBottoms[itemName].PropertyChanged -= OnPlayerInfoChange;
                inventory.myBottoms.Remove(itemName);
                titleRef.RemoveItemFromShopContent('e', itemName);
            } else if (inventory.myArmor.ContainsKey(itemName)) {
                inventory.myArmor[itemName].PropertyChanged -= OnPlayerInfoChange;
                inventory.myArmor.Remove(itemName);
                titleRef.RemoveItemFromShopContent('e', itemName);
            } else if (inventory.myCharacters.ContainsKey(itemName)) {
                inventory.myCharacters[itemName].PropertyChanged -= OnPlayerInfoChange;
                inventory.myCharacters.Remove(itemName);
                titleRef.RemoveItemFromShopContent('c', itemName);
            } else if (inventory.myFacewear.ContainsKey(itemName)) {
                inventory.myFacewear[itemName].PropertyChanged -= OnPlayerInfoChange;
                inventory.myFacewear.Remove(itemName);
                titleRef.RemoveItemFromShopContent('e', itemName);
            } else if (inventory.myFootwear.ContainsKey(itemName)) {
                inventory.myFootwear[itemName].PropertyChanged -= OnPlayerInfoChange;
                inventory.myFootwear.Remove(itemName);
                titleRef.RemoveItemFromShopContent('e', itemName);
            } else if (inventory.myHeadgear.ContainsKey(itemName)) {
                inventory.myHeadgear[itemName].PropertyChanged -= OnPlayerInfoChange;
                inventory.myHeadgear.Remove(itemName);
                titleRef.RemoveItemFromShopContent('e', itemName);
            } else if (inventory.myWeapons.ContainsKey(itemName)) {
                inventory.myWeapons[itemName].PropertyChanged -= OnPlayerInfoChange;
                inventory.myWeapons.Remove(itemName);
                titleRef.RemoveItemFromShopContent('w', itemName);
            } else if (inventory.myMods.ContainsKey(itemName)) {
                inventory.myMods[itemName].PropertyChanged -= OnPlayerInfoChange;
                inventory.myMods.Remove(itemName);
                titleRef.RemoveItemFromShopContent('m', itemName);
                if (titleRef != null) {
                    playerDataModifyLegalFlag = true;
                    
                    // Update player template weapon
                    OnPrimaryChange(PlayerData.playerdata.info.EquippedPrimary);
                    OnSecondaryChange(PlayerData.playerdata.info.EquippedSecondary);
                    OnSupportChange(PlayerData.playerdata.info.EquippedSupport);

                    // Update weapon mod template if active and refresh weapon stats
                    if (titleRef.mainPanelManager.currentPanelIndex == titleRef.mainPanelManager.GetModShopIndex()) {
                        titleRef.LoadWeaponForModding(titleRef.weaponPreviewShopSlot);
                    }
                    
                    playerDataModifyLegalFlag = false;
                }
            }
        } else if (transactionType == 'm') {
            string itemName = snapshot.Key;
            if (inventory.myTops.ContainsKey(itemName)) {
                EquipmentData e = null;
                e = inventory.myTops[itemName];
                e.Duration = snapshot.Child("duration").Value.ToString();
                e.AcquireDate = snapshot.Child("acquireDate").Value.ToString();
            } else if (inventory.myBottoms.ContainsKey(itemName)) {
                EquipmentData e = null;
                e = inventory.myBottoms[itemName];
                e.Duration = snapshot.Child("duration").Value.ToString();
                e.AcquireDate = snapshot.Child("acquireDate").Value.ToString();
            } else if (inventory.myArmor.ContainsKey(itemName)) {
                ArmorData e = null;
                e = inventory.myArmor[itemName];
                e.Duration = snapshot.Child("duration").Value.ToString();
                e.AcquireDate = snapshot.Child("acquireDate").Value.ToString();
            } else if (inventory.myCharacters.ContainsKey(itemName)) {
                CharacterData e = null;
                e = inventory.myCharacters[itemName];
                e.Duration = snapshot.Child("duration").Value.ToString();
                e.AcquireDate = snapshot.Child("acquireDate").Value.ToString();
            } else if (inventory.myFacewear.ContainsKey(itemName)) {
                EquipmentData e = null;
                e = inventory.myFacewear[itemName];
                e.Duration = snapshot.Child("duration").Value.ToString();
                e.AcquireDate = snapshot.Child("acquireDate").Value.ToString();
            } else if (inventory.myFootwear.ContainsKey(itemName)) {
                EquipmentData e = null;
                e = inventory.myFootwear[itemName];
                e.Duration = snapshot.Child("duration").Value.ToString();
                e.AcquireDate = snapshot.Child("acquireDate").Value.ToString();
            } else if (inventory.myHeadgear.ContainsKey(itemName)) {
                EquipmentData e = null;
                e = inventory.myHeadgear[itemName];
                e.Duration = snapshot.Child("duration").Value.ToString();
                e.AcquireDate = snapshot.Child("acquireDate").Value.ToString();
            } else if (inventory.myWeapons.ContainsKey(itemName)) {
                WeaponScript wepScript = bodyReference.GetComponent<WeaponScript>();
                WeaponData e = null;
                e = inventory.myWeapons[itemName];
                e.Duration = snapshot.Child("duration").Value.ToString();
                e.AcquireDate = snapshot.Child("acquireDate").Value.ToString();
                e.EquippedSuppressor = snapshot.Child("equippedSuppressor").Value.ToString();
                e.EquippedSight = snapshot.Child("equippedSight").Value.ToString();
                e.EquippedClip = snapshot.Child("equippedClip").Value.ToString();
            } else if (inventory.myMods.ContainsKey(itemName)) {
                // If on menu, update player template weapon, weapon mod template if active, mod shop entries, refresh weapon stats
                ModData e = inventory.myMods[itemName];
                e.Duration = snapshot.Child("duration").Value.ToString();
                e.AcquireDate = snapshot.Child("acquireDate").Value.ToString();
                e.EquippedOn = snapshot.Child("equippedOn").Value.ToString();
                if (titleRef != null) {
                    playerDataModifyLegalFlag = true;
                    
                    // Update player template weapon
                    OnPrimaryChange(PlayerData.playerdata.info.EquippedPrimary);
                    OnSecondaryChange(PlayerData.playerdata.info.EquippedSecondary);
                    OnSupportChange(PlayerData.playerdata.info.EquippedSupport);

                    // Update weapon mod template if active and refresh weapon stats
                    if (titleRef.mainPanelManager.currentPanelIndex == titleRef.mainPanelManager.GetModShopIndex()) {
                        titleRef.LoadWeaponForModding(titleRef.weaponPreviewShopSlot);
                        // Update all active shop slots
                        titleRef.RefreshModShopContent();
                    }
                    
                    playerDataModifyLegalFlag = false;
                }
            }
        }
    }

    void LoadFriends(Dictionary<object, object> friendsUsernamesMap, Dictionary<object, object> expMap, List<object> friendsDetails) {
        playerDataModifyLegalFlag = true;
        List<string> usernameList = new List<string>();
        foreach (object f in friendsDetails) {
            Dictionary<object, object> d = (Dictionary<object, object>)f;
            // Extract the friend request ID
            string friendRequestId = d["friendRequestId"].ToString();
            if (PlayerData.playerdata.friendsList.ContainsKey(friendRequestId)) {
                // If already loaded from DB, just add to UI
                FriendData fd = PlayerData.playerdata.friendsList[friendRequestId];
                if (titleRef != null) {
                    if (fd.Status != 2 || (fd.Status == 2 && fd.Blocker == AuthScript.authHandler.user.UserId)) {
                        titleRef.friendsMessenger.EnqueueMessengerEntryCreation(friendRequestId, fd.FriendUsername, fd.Exp);
                    }
                }
            } else {
                // Get details
                Dictionary<object, object> friendRequest = (Dictionary<object, object>)d["details"];
                string requestor = friendRequest["requestor"].ToString();
                string requestee = friendRequest["requestee"].ToString();
                string friendId = requestor == AuthScript.authHandler.user.UserId ? requestee : requestor;
                int status = Convert.ToInt32(friendRequest["status"]);
                string blocker = null;
                if (friendRequest.ContainsKey("blocker")) {
                    blocker = friendRequest["blocker"].ToString();
                }
                // Create friend data
                FriendData fd = new FriendData();
                fd.PropertyChanged += OnPlayerInfoChange;
                fd.FriendRequestId = friendRequestId;
                fd.FriendId = friendId;
                fd.FriendUsername = friendsUsernamesMap[friendId].ToString();
                fd.Exp = Convert.ToUInt32(expMap[friendId].ToString());
                fd.Status = status;
                fd.Requestee = requestee;
                fd.Requestor = requestor;
                fd.Blocker = blocker;

                // Add update callback
                DAOScript.dao.dbRef.Child("fteam_ai/friends/" + friendRequestId).ChildChanged += HandleFriendUpdate;
                DAOScript.dao.dbRef.Child("fteam_ai/friends/" + friendRequestId).ChildAdded += HandleFriendUpdate;

                // Add to friend list
                usernameList.Add(fd.FriendUsername);
                PlayerData.playerdata.friendsList.Add(friendRequestId, fd);

                // Add to UI
                if (titleRef != null) {
                    if (status != 2 || (status == 2 && blocker == AuthScript.authHandler.user.UserId)) {
                        titleRef.friendsMessenger.EnqueueMessengerEntryCreation(friendRequestId, fd.FriendUsername, fd.Exp);
                    }
                }
            }
        }
        // globalChatClient.AddStatusListenersToFriends(usernameList);
        playerDataModifyLegalFlag = false;
    }

    public void LoadGifts(Dictionary<object, object> snapshot)
    {
        playerDataModifyLegalFlag = true;

        foreach(KeyValuePair<object, object> entry in snapshot) {
            string giftId = entry.Key.ToString();
            GiftData g = null;
            if (PlayerData.playerdata.giftList.ContainsKey(giftId)) {
                g = PlayerData.playerdata.giftList[giftId];
            } else {
                Dictionary<object, object> gift = (Dictionary<object, object>)entry.Value;
                g = new GiftData();
                g.PropertyChanged += OnPlayerInfoChange;
                g.GiftId = giftId;
                g.Category = gift["category"].ToString();
                g.Sender = gift["from"].ToString();
                g.ItemName = gift["itemName"].ToString();
                g.Duration = Convert.ToSingle(gift["duration"]);
                g.Message = gift["message"].ToString();

                PlayerData.playerdata.giftList.Add(giftId, g);
            }

            // Add gift entry in gift inbox if on title screen
            if (PlayerData.playerdata.titleRef != null) {
                PlayerData.playerdata.titleRef.giftInbox.EnqueueGiftEntryCreation(giftId, g.Category, g.Sender, g.ItemName, g.Duration, g.Message);
            }
        }

        playerDataModifyLegalFlag = false;
    }

    public void LoadInventory(Dictionary<object, object> snapshot) {
        inventoryDataModifyLegalFlag = true;
        playerDataModifyLegalFlag = true;
        Dictionary<object, object> subSnapshot = (Dictionary<object, object>)snapshot["weapons"];
        IEnumerator dataLoaded = subSnapshot.GetEnumerator();
        // Load weapons
        foreach(KeyValuePair<object, object> entry in subSnapshot) {
            if (inventory.myWeapons.ContainsKey(entry.Key.ToString())) {
                continue;
            }
            WeaponData w = new WeaponData();
            Dictionary<object, object> thisSnapshot = (Dictionary<object, object>)entry.Value;
            string key = entry.Key.ToString();
            w.AcquireDate = thisSnapshot["acquireDate"].ToString();
            w.Duration = thisSnapshot["duration"].ToString();
            object equippedSuppressor = null;
            if (thisSnapshot.ContainsKey("equippedSuppressor")) {
                equippedSuppressor = thisSnapshot["equippedSuppressor"];
            }
            object equippedClip = null;
            if (thisSnapshot.ContainsKey("equippedClip")) {
                equippedClip = thisSnapshot["equippedClip"];
            }
            object equippedSight = null;
            if (thisSnapshot.ContainsKey("equippedSight")) {
                equippedSight = thisSnapshot["equippedSight"];
            }
            w.EquippedSuppressor = (equippedSuppressor == null ? "" : equippedSuppressor.ToString());
            w.EquippedClip = (equippedClip == null ? "" : equippedClip.ToString());
            w.EquippedSight = (equippedSight == null ? "" : equippedSight.ToString());
            w.PropertyChanged += OnPlayerInfoChange;
            inventory.myWeapons.Add(key, w);
        }
        subSnapshot = (Dictionary<object, object>)snapshot["characters"];
        dataLoaded = subSnapshot.GetEnumerator();
        // Load characters
        foreach(KeyValuePair<object, object> entry in subSnapshot) {
            if (inventory.myCharacters.ContainsKey(entry.Key.ToString())) {
                continue;
            }
            CharacterData c = new CharacterData();
            Dictionary<object, object> thisSnapshot = (Dictionary<object, object>)entry.Value;
            string key = entry.Key.ToString();
            c.AcquireDate = thisSnapshot["acquireDate"].ToString();
            c.Duration = thisSnapshot["duration"].ToString();
            c.PropertyChanged += OnPlayerInfoChange;
            // If item is expired, delete from database. Else, add it to inventory
            c.PropertyChanged += OnPlayerInfoChange;
            inventory.myCharacters.Add(key, c);
        }
        if (snapshot.ContainsKey("armor")) {
            subSnapshot = (Dictionary<object, object>)snapshot["armor"];
            dataLoaded = subSnapshot.GetEnumerator();
            // Load armor
            foreach(KeyValuePair<object, object> entry in subSnapshot) {
                if (inventory.myArmor.ContainsKey(entry.Key.ToString())) {
                    continue;
                }
                ArmorData a = new ArmorData();
                Dictionary<object, object> thisSnapshot = (Dictionary<object, object>)entry.Value;
                string key = entry.Key.ToString();
                a.AcquireDate = thisSnapshot["acquireDate"].ToString();
                a.Duration = thisSnapshot["duration"].ToString();
                a.PropertyChanged += OnPlayerInfoChange;
                inventory.myArmor.Add(key, a);
            }
        }
        if (snapshot.ContainsKey("tops")) {
            subSnapshot = (Dictionary<object, object>)snapshot["tops"];
            dataLoaded = subSnapshot.GetEnumerator();
            // Load tops
            foreach(KeyValuePair<object, object> entry in subSnapshot) {
                if (inventory.myTops.ContainsKey(entry.Key.ToString())) {
                    continue;
                }
                EquipmentData d = new EquipmentData();
                Dictionary<object, object> thisSnapshot = (Dictionary<object, object>)entry.Value;
                string key = entry.Key.ToString();
                d.AcquireDate = thisSnapshot["acquireDate"].ToString();
                d.Duration = thisSnapshot["duration"].ToString();
                d.PropertyChanged += OnPlayerInfoChange;
                inventory.myTops.Add(key, d);
            }
        }
        if (snapshot.ContainsKey("bottoms")) {
            subSnapshot = (Dictionary<object, object>)snapshot["bottoms"];
            dataLoaded = subSnapshot.GetEnumerator();
            // Load bottoms
            foreach(KeyValuePair<object, object> entry in subSnapshot) {
                if (inventory.myBottoms.ContainsKey(entry.Key.ToString())) {
                    continue;
                }
                EquipmentData d = new EquipmentData();
                Dictionary<object, object> thisSnapshot = (Dictionary<object, object>)entry.Value;
                string key = entry.Key.ToString();
                d.AcquireDate = thisSnapshot["acquireDate"].ToString();
                d.Duration = thisSnapshot["duration"].ToString();
                d.PropertyChanged += OnPlayerInfoChange;
                inventory.myBottoms.Add(key, d);
            }
        }
        if (snapshot.ContainsKey("footwear")) {
            subSnapshot = (Dictionary<object, object>)snapshot["footwear"];
            dataLoaded = subSnapshot.GetEnumerator();
            // Load footwear
            foreach(KeyValuePair<object, object> entry in subSnapshot) {
                if (inventory.myFootwear.ContainsKey(entry.Key.ToString())) {
                    continue;
                }
                EquipmentData d = new EquipmentData();
                Dictionary<object, object> thisSnapshot = (Dictionary<object, object>)entry.Value;
                string key = entry.Key.ToString();
                d.AcquireDate = thisSnapshot["acquireDate"].ToString();
                d.Duration = thisSnapshot["duration"].ToString();
                d.PropertyChanged += OnPlayerInfoChange;
                inventory.myFootwear.Add(key, d);
            }
        }
        if (snapshot.ContainsKey("headgear")) {
            subSnapshot = (Dictionary<object, object>)snapshot["headgear"];
            dataLoaded = subSnapshot.GetEnumerator();
            // Load headgear
            foreach(KeyValuePair<object, object> entry in subSnapshot) {
                if (inventory.myHeadgear.ContainsKey(entry.Key.ToString())) {
                    continue;
                }
                EquipmentData d = new EquipmentData();
                Dictionary<object, object> thisSnapshot = (Dictionary<object, object>)entry.Value;
                string key = entry.Key.ToString();
                d.AcquireDate = thisSnapshot["acquireDate"].ToString();
                d.Duration = thisSnapshot["duration"].ToString();
                d.PropertyChanged += OnPlayerInfoChange;
                inventory.myHeadgear.Add(key, d);
            }
        }
        if (snapshot.ContainsKey("facewear")) {
            subSnapshot = (Dictionary<object, object>)snapshot["facewear"];
            dataLoaded = subSnapshot.GetEnumerator();
            // Load facewear
            foreach(KeyValuePair<object, object> entry in subSnapshot) {
                if (inventory.myFacewear.ContainsKey(entry.Key.ToString())) {
                    continue;
                }
                EquipmentData d = new EquipmentData();
                Dictionary<object, object> thisSnapshot = (Dictionary<object, object>)entry.Value;
                string key = entry.Key.ToString();
                d.AcquireDate = thisSnapshot["acquireDate"].ToString();
                d.Duration = thisSnapshot["duration"].ToString();
                d.PropertyChanged += OnPlayerInfoChange;
                inventory.myFacewear.Add(key, d);
            }
        }
        if (snapshot.ContainsKey("mods")) {
            subSnapshot = (Dictionary<object, object>)snapshot["mods"];
            dataLoaded = subSnapshot.GetEnumerator();
            // Load mods
            foreach(KeyValuePair<object, object> entry in subSnapshot) {
                if (inventory.myMods.ContainsKey(entry.Key.ToString())) {
                    continue;
                }
                ModData m = new ModData();
                Dictionary<object, object> thisSnapshot = (Dictionary<object, object>)entry.Value;
                string key = entry.Key.ToString();
                m.Name = thisSnapshot["name"].ToString();
                m.AcquireDate = thisSnapshot["acquireDate"].ToString();
                m.Duration = thisSnapshot["duration"].ToString();
                m.EquippedOn = thisSnapshot["equippedOn"].ToString();
                m.PropertyChanged += OnPlayerInfoChange;
                inventory.myMods.Add(key, m);
            }
        }
        inventoryDataModifyLegalFlag = false;
        playerDataModifyLegalFlag = false;
    }

    public void FindBodyRef(string character)
    {
        if (bodyReference == null)
        {
            bodyReference = Instantiate(titleRef.characterRefs[titleRef.charactersRefsIndices[character]], GetTitlePos(), GetTitleRot());
        }
        // else
        // {
        //     bodyReference = GameObject.FindGameObjectWithTag("Player");
        // }
    }

    // Ensure that character changed listener re-equips weapons and sets equipment to defautl too
    public void UpdateBodyRef()
    {
        if (titleRef == null) {
            titleRef = GameObject.Find("TitleController").GetComponent<TitleControllerScript>();
        }
        if (bodyReference == null) return;
        if (bodyReference.GetComponent<EquipmentScript>().equippedCharacter == PlayerData.playerdata.info.EquippedCharacter)
        {
            return;
        }
        WeaponScript weaponScrpt = bodyReference.GetComponent<WeaponScript>();
        Destroy(bodyReference);
        bodyReference = null;
        bodyReference = Instantiate(titleRef.characterRefs[titleRef.charactersRefsIndices[PlayerData.playerdata.info.EquippedCharacter]], GetTitlePos(), GetTitleRot());
        EquipmentScript characterEquips = bodyReference.GetComponent<EquipmentScript>();
        WeaponScript characterWeps = bodyReference.GetComponent<WeaponScript>();
        characterEquips.ts = titleRef;
        characterWeps.ts = titleRef;
    }

    // Saves mod data for given weapon. If ID is null, then don't mess with that mod. If ID is empty, remove mod. Else, equip that mod.
    public void SaveModDataForWeapon(string weaponName, string suppressorId, string sightId) {
        if (titleRef != null) {
            titleRef.TriggerBlockScreen(true);
        }
        WeaponScript myWeps = bodyReference.GetComponent<WeaponScript>();
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["weaponName"] = weaponName;
        inputData["suppressorId"] = suppressorId;
        inputData["sightId"] = sightId;

		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("saveModDataForWeapon");
        func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() == "200") {
                    Debug.Log("Mod saves successful.");
                } else {
                    TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                }
            }
        });
    }

    public ModInfo LoadModDataForWeapon(string weaponName) {
        playerDataModifyLegalFlag = true;
        ModInfo modInfo = new ModInfo();
        modInfo.PropertyChanged += OnPlayerInfoChange;
        modInfo.WeaponName = weaponName;

        foreach (KeyValuePair<string, ModData> entry in PlayerData.playerdata.inventory.myMods)
        {
            // If the mod is equipped on the given weapon, load it into the requested mod info
            ModData m = entry.Value;
            if (m.EquippedOn.Equals(weaponName)) {
                Mod modDetails = InventoryScript.itemData.modCatalog[m.Name];
                if (modDetails.category.Equals("Suppressor")) {
                    modInfo.SuppressorId = entry.Key;
                    modInfo.EquippedSuppressor = m.Name;
                } else if (modDetails.category.Equals("Sight")) {
                    modInfo.SightId = entry.Key;
                    modInfo.EquippedSight = m.Name;
                } else if (modDetails.category.Equals("Clip")) {
                    // modInfo.clipId = m.id;
                    modInfo.EquippedClip = m.Name;
                }
            }
        }
        
        playerDataModifyLegalFlag = false;
        return modInfo;
    }

    public void AddItemToInventory(string itemName, string type, float duration, bool purchased) {
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
        inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["itemName"] = itemName;
        inputData["duration"] = duration;
        inputData["category"] = ConvertTypeToFirebaseType(type);
        if (purchased) {
            HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("transactItem");
            func.CallAsync(inputData).ContinueWith((taskA) => {
                if (taskA.IsFaulted) {
                    TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                    titleRef.TriggerBlockScreen(false);
                    titleRef.confirmingTransaction = false;
                } else {
                    Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                    if (results["status"].ToString() == "200") {
                        titleRef.TriggerAlertPopup("Purchase successful! The item has been added to your inventory.");
                        titleRef.TriggerBlockScreen(false);
                        titleRef.confirmingTransaction = false;
                    } else {
                        TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                        titleRef.TriggerBlockScreen(false);
                        titleRef.confirmingTransaction = false;
                    }
                }
            });
        } else {
            HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("giveItemToUser");
            func.CallAsync(inputData).ContinueWith((taskA) => {
                if (taskA.IsFaulted) {
                    TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                    titleRef?.TriggerBlockScreen(false);
                } else {
                    Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                    if (results["status"].ToString() == "200") {
                        titleRef?.TriggerAlertPopup("The item has been added to your inventory!");
                    } else {
                        TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                    }
                    titleRef?.TriggerBlockScreen(false);
                }
            });
        }
    }

    bool ItemIsDeletable(string itemName, string type) {
        if (type == "Armor") {
            if (!InventoryScript.itemData.armorCatalog[itemName].deleteable) {
                return false;
            }
        } else if (type == "Character") {
            if (!InventoryScript.itemData.characterCatalog[itemName].deleteable) {
                return false;
            }
        } else if (type == "Weapon") {
            if (!InventoryScript.itemData.weaponCatalog[itemName].deleteable) {
                return false;
            }
        } else if (type == "Mod") {
            if (!InventoryScript.itemData.modCatalog[PlayerData.playerdata.inventory.myMods[itemName].Name].deleteable) {
                return false;
            }
        } else {
            if (!InventoryScript.itemData.equipmentCatalog[itemName].deleteable) {
                return false;
            }
        }
        return true;
    }

    // Removes item from inventory in DB
    public void DeleteItemFromInventory(string itemName, string type, string modId)
    {
        // If item cannot be deleted, then skip
        if (!ItemIsDeletable(itemName, type))
        {
            return;
        }
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
        inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["itemId"] = itemName;
        inputData["category"] = ConvertTypeToFirebaseType(type);
        HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("deleteItemFromUser");
        func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() != "200") {
                    TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                }
            }
        });
    }

    public void SellItemFromInventory(string itemId, string type)
    {
        // If item cannot be deleted, then skip
        if (!ItemIsDeletable(itemId, type))
        {
            return;
        }
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
        inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["itemName"] = itemId;
        inputData["category"] = ConvertTypeToFirebaseType(type);
        HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("sellItemFromUser");
        func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsCompleted) {
                inputData.Remove("itemName");
                inputData["itemId"] = itemId;
                func = DAOScript.dao.functions.GetHttpsCallable("deleteItemFromUser");
                func.CallAsync(inputData).ContinueWith((taskB) => {
                    if (taskB.IsCompleted) {
                        titleRef.TriggerAlertPopup("Sale successful! The GP has been refunded to you.");
                        titleRef.TriggerBlockScreen(false);
                        titleRef.confirmingSale = false;
                    } else if (taskB.IsFaulted) {
                        TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                        titleRef.TriggerBlockScreen(false);
                        titleRef.confirmingSale = false;
                    } else {
                        Dictionary<object, object> results = (Dictionary<object, object>)taskB.Result.Data;
                        if (results["status"].ToString() != "200") {
                            TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                            titleRef.TriggerBlockScreen(false);
                            titleRef.confirmingSale = false;
                        }
                    }
                });
            } else if (taskA.IsFaulted) {
                TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                titleRef.TriggerBlockScreen(false);
                titleRef.confirmingSale = false;
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() != "200") {
                    TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                    titleRef.TriggerBlockScreen(false);
                    titleRef.confirmingSale = false;
                }
            }
        });
    }

    public void AddExpAndGpToPlayer(uint aExp, uint aGp) {
        // Save locally
        uint newExp = (uint)Mathf.Min(PlayerData.playerdata.info.Exp + aExp, PlayerData.MAX_EXP);
        uint newGp = (uint)Mathf.Min(PlayerData.playerdata.info.Gp + aGp, PlayerData.MAX_GP);
        // Save to DB
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
        inputData["exp"] = newExp;
        inputData["gp"] = newGp;

		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("savePlayerData");
		func.CallAsync(inputData).ContinueWith((taskA) => {
            if (taskA.IsFaulted) {
                TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
            } else {
                Dictionary<object, object> results = (Dictionary<object, object>)taskA.Result.Data;
                if (results["status"].ToString() == "200") {
                    Debug.Log("Save successful.");
                } else {
                    TriggerEmergencyExit("Database is currently unavailable. Please try again later.");
                }
            }
        });
    }

    public Texture GetRankInsigniaForRank(string rank) {
        switch (rank) {
            case "Trainee":
                return rankInsignias[0];
            case "Recruit":
                return rankInsignias[1];
            case "Private":
                return rankInsignias[2];
            case "Private First Class":
                return rankInsignias[3];
            case "Corporal":
                return rankInsignias[4];
            case "Sergeant":
                return rankInsignias[5];
            case "Staff Sergeant I":
                return rankInsignias[6];
            case "Staff Sergeant II":
                return rankInsignias[7];
            case "Staff Sergeant III":
                return rankInsignias[8];
            case "Sergeant First Class I":
                return rankInsignias[9];
            case "Sergeant First Class II":
                return rankInsignias[10];
            case "Sergeant First Class III":
                return rankInsignias[11];
            case "Master Sergeant I":
                return rankInsignias[12];
            case "Master Sergeant II":
                return rankInsignias[13];
            case "Master Sergeant III":
                return rankInsignias[14];
            case "Master Sergeant IV":
                return rankInsignias[15];
            case "Command Sergeant Major I":
                return rankInsignias[16];
            case "Command Sergeant Major II":
                return rankInsignias[17];
            case "Command Sergeant Major III":
                return rankInsignias[18];
            case "Command Sergeant Major IV":
                return rankInsignias[19];
            case "Command Sergeant Major V":
                return rankInsignias[20];
            case "Second Lieutenant I":
                return rankInsignias[21];
            case "Second Lieutenant II":
                return rankInsignias[22];
            case "Second Lieutenant III":
                return rankInsignias[23];
            case "Second Lieutenant IV":
                return rankInsignias[24];
            case "Second Lieutenant V":
                return rankInsignias[25];
            case "First Lieutenant I":
                return rankInsignias[26];
            case "First Lieutenant II":
                return rankInsignias[27];
            case "First Lieutenant III":
                return rankInsignias[28];
            case "First Lieutenant IV":
                return rankInsignias[29];
            case "First Lieutenant V":
                return rankInsignias[30];
            case "Captain I":
                return rankInsignias[31];
            case "Captain II":
                return rankInsignias[32];
            case "Captain III":
                return rankInsignias[33];
            case "Captain IV":
                return rankInsignias[34];
            case "Captain V":
                return rankInsignias[35];
            case "Major I":
                return rankInsignias[36];
            case "Major II":
                return rankInsignias[37];
            case "Major III":
                return rankInsignias[38];
            case "Major IV":
                return rankInsignias[39];
            case "Major V":
                return rankInsignias[40];
            case "Lieutenant Colonel I":
                return rankInsignias[41];
            case "Lieutenant Colonel II":
                return rankInsignias[42];
            case "Lieutenant Colonel III":
                return rankInsignias[43];
            case "Lieutenant Colonel IV":
                return rankInsignias[44];
            case "Lieutenant Colonel V":
                return rankInsignias[45];
            case "Colonel I":
                return rankInsignias[46];
            case "Colonel II":
                return rankInsignias[47];
            case "Colonel III":
                return rankInsignias[48];
            case "Colonel IV":
                return rankInsignias[49];
            case "Colonel V":
                return rankInsignias[50];
            case "Brigadier General":
                return rankInsignias[51];
            case "Major General":
                return rankInsignias[52];
            case "Lieutenant General":
                return rankInsignias[53];
            case "General":
                return rankInsignias[54];
            case "General of the Army":
                return rankInsignias[55];
            case "Commander in Chief I":
                return rankInsignias[56];
            case "Commander in Chief II":
                return rankInsignias[57];
            case "Commander in Chief III":
                return rankInsignias[58];
            case "Commander in Chief IV":
                return rankInsignias[59];
            case "Commander in Chief V":
                return rankInsignias[60];
            default:
                return rankInsignias[0];
        }
    }

    public Rank GetRankFromExp(uint exp) {
        if (exp >= 0 && exp <= 1999) {
            return new Rank("Trainee", 0, 1999);
        } else if (exp >= 2000 && exp <= 4499) {
            return new Rank("Recruit", 2000, 4499);
        } else if (exp >= 4500 && exp <= 5999) {
            return new Rank("Private", 4500, 5999);
        } else if (exp >= 6000 && exp <= 17999) {
            return new Rank("Private First Class", 6000, 17999);
        } else if (exp >= 18000 && exp <= 31999) {
            return new Rank("Corporal", 18000, 31999);
        } else if (exp >= 32000 && exp <= 53999) {
            return new Rank("Sergeant", 32000, 53999);
        } else if (exp >= 54000 && exp <= 78999) {
            return new Rank("Staff Sergeant I", 54000, 78999);
        } else if (exp >= 79000 && exp <= 108999) {
            return new Rank("Staff Sergeant II", 79000, 108999);
        } else if (exp >= 109000 && exp <= 144999) {
            return new Rank("Staff Sergeant III", 109000, 144999);
        } else if (exp >= 145000 && exp <= 185499) {
            return new Rank("Sergeant First Class I", 145000, 185499);
        } else if (exp >= 185500 && exp <= 232999) {
            return new Rank("Sergeant First Class II", 185500, 232999);
        } else if (exp >= 233000 && exp <= 291499) {
            return new Rank("Sergeant First Class III", 233000, 291499);
        } else if (exp >= 291500 && exp <= 353999) {
            return new Rank("Master Sergeant I", 291500, 353999);
        } else if (exp >= 354000 && exp <= 424999) {
            return new Rank("Master Sergeant II", 354000, 424999);
        } else if (exp >= 425000 && exp <= 503499) {
            return new Rank("Master Sergeant III", 425000, 503499);
        } else if (exp >= 503500 && exp <= 592999) {
            return new Rank("Master Sergeant IV", 503500, 592999);
        } else if (exp >= 593000 && exp <= 692999) {
            return new Rank("Command Sergeant Major I", 593000, 692999);
        } else if (exp >= 693000 && exp <= 803499) {
            return new Rank("Command Sergeant Major II", 693000, 803499);
        } else if (exp >= 803500 && exp <= 924999) {
            return new Rank("Command Sergeant Major III", 803500, 924999);
        } else if (exp >= 925000 && exp <= 1059999) {
            return new Rank("Command Sergeant Major IV", 925000, 1059999);
        } else if (exp >= 1060000 && exp <= 1199999) {
            return new Rank("Command Sergeant Major V", 1060000, 1199999);
        } else if (exp >= 1200000 && exp <= 1353499) {
            return new Rank("Second Lieutenant I", 1200000, 1353499);
        } else if (exp >= 1353500 && exp <= 1517999) {
            return new Rank("Second Lieutenant II", 1353500, 1517999);
        } else if (exp >= 1518000 && exp <= 1692999) {
            return new Rank("Second Lieutenant III", 1518000, 1692999);
        } else if (exp >= 1693000 && exp <= 1878499) {
            return new Rank("Second Lieutenant IV", 1693000, 1878499);
        } else if (exp >= 1878500 && exp <= 2071499) {
            return new Rank("Second Lieutenant V", 1878500, 2071499);
        } else if (exp >= 2071500 && exp <= 2278499) {
            return new Rank("First Lieutenant I", 2071500, 2278499);
        } else if (exp >= 2278500 && exp <= 2496999) {
            return new Rank("First Lieutenant II", 2278500, 2496999);
        } else if (exp >= 2497000 && exp <= 2724999) {
            return new Rank("First Lieutenant III", 2497000, 2724999);
        } else if (exp >= 2725000 && exp <= 2964999) {
            return new Rank("First Lieutenant IV", 2725000, 2964999);
        } else if (exp >= 2965000 && exp <= 3214499) {
            return new Rank("First Lieutenant V", 2965000, 3214499);
        } else if (exp >= 3214500 && exp <= 3510999) {
            return new Rank("Captain I", 3214500, 3510999);
        } else if (exp >= 3511000 && exp <= 3835999) {
            return new Rank("Captain II", 3511000, 3835999);
        } else if (exp >= 3836000 && exp <= 4189999) {
            return new Rank("Captain III", 3836000, 4189999);
        } else if (exp >= 4190000 && exp <= 4571499) {
            return new Rank("Captain IV", 4190000, 4571499);
        } else if (exp >= 4571500 && exp <= 4999999) {
            return new Rank("Captain V", 4571500, 4999999);
        } else if (exp >= 5000000 && exp <= 5474999) {
            return new Rank("Major I", 5000000, 5474999);
        } else if (exp >= 5475000 && exp <= 5996499) {
            return new Rank("Major II", 5475000, 5996499);
        } else if (exp >= 5996500 && exp <= 6564499) {
            return new Rank("Major III", 5996500, 6564499);
        } else if (exp >= 6564500 && exp <= 7178999) {
            return new Rank("Major IV", 6564500, 7178999);
        } else if (exp >= 7179000 && exp <= 7857999) {
            return new Rank("Major V", 7179000, 7857999);
        } else if (exp >= 7858000 && exp <= 8599999) {
            return new Rank("Lieutenant Colonel I", 7858000, 8599999);
        } else if (exp >= 8600000 && exp <= 9407499) {
            return new Rank("Lieutenant Colonel II", 8600000, 9407499);
        } else if (exp >= 9407500 && exp <= 10278999) {
            return new Rank("Lieutenant Colonel III", 9407500, 10278999);
        } else if (exp >= 10279000 && exp <= 11214999) {
            return new Rank("Lieutenant Colonel IV", 10279000, 11214999);
        } else if (exp >= 11215000 && exp <= 12214199) {
            return new Rank("Lieutenant Colonel V", 11215000, 12214199);
        } else if (exp >= 12215000 && exp <= 13278999) {
            return new Rank("Colonel I", 12215000, 13278999);
        } else if (exp >= 13279000 && exp <= 14406999) {
            return new Rank("Colonel II", 13279000, 14406999);
        } else if (exp >= 14407000 && exp <= 15599999) {
            return new Rank("Colonel III", 14407000, 15599999);
        } else if (exp >= 15600000 && exp <= 16857499) {
            return new Rank("Colonel IV", 15600000, 16857499);
        } else if (exp >= 16857500 && exp <= 18214999) {
            return new Rank("Colonel V", 16857500, 18214999);
        } else if (exp >= 18215000 && exp <= 19642999) {
            return new Rank("Brigadier General", 18215000, 19642999);
        } else if (exp >= 19643000 && exp <= 21429999) {
            return new Rank("Major General", 19643000, 21429999);
        } else if (exp >= 21430000 && exp <= 24285999) {
            return new Rank("Lieutenant General", 21430000, 24285999);
        } else if (exp >= 24286000 && exp <= 28571999) {
            return new Rank("General", 24286000, 28571999);
        } else if (exp >= 28572000 && exp <= 32856999) {
            return new Rank("General of the Army", 28572000, 32856999);
        } else if (exp >= 32857000 && exp <= 37142999) {
            return new Rank("Commander in Chief I", 32857000, 37142999);
        } else if (exp >= 37143000 && exp <= 41499999) {
            return new Rank("Commander in Chief II", 37143000, 41499999);
        } else if (exp >= 41500000 && exp <= 45719999) {
            return new Rank("Commander in Chief III", 41500000, 45719999);
        } else if (exp >= 45720000 && exp <= 49999999) {
            return new Rank("Commander in Chief IV", 45720000, 49999999);
        } else if (exp >= 50000000) {
            return new Rank("Commander in Chief V", 50000000, uint.MaxValue);
        }
        return new Rank("Trainee", 0, 1999);
    }

    public void TriggerEmergencyExit(string message) {
        emergencyExitMessage = message;
        triggerEmergencyExitFlag = true;
    }

    // Only called in an emergency situation when the game needs to exit immediately (ex: database failure or user gets banned).
    public void DoEmergencyExit() {
        // Freeze user mouse input
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Display emergency popup depending on which screen you're on
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene.Equals("GameOverSuccess") || currentScene.Equals("GameOverFail")) {
            gameOverControllerRef.TriggerAlertPopup("A fatal error has occurred:\n" + emergencyExitMessage + "\nThe game will now close.");
        } else if (currentScene.Equals("Title")) {
            titleRef.TriggerEmergencyPopup("A fatal error has occurred:\n" + emergencyExitMessage + "\nThe game will now close.");
        }
        StartCoroutine("EmergencyExitGame");
    }

    void HandleForceLogoutEvent(object sender, ValueChangedEventArgs args) {
        if (PlayerData.playerdata.bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }
        if (args.Snapshot.Key.ToString().Equals("loggedIn")) {
            if (args.Snapshot.Value != null) {
                if (args.Snapshot.Value.ToString() == "0") {
                    Application.Quit();
                }
            }
        }
    }

    void HandleGpChangeEvent(object sender, ValueChangedEventArgs args) {
        if (bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }
        if (args.Snapshot.Value != null) {
            playerDataModifyLegalFlag = true;
            PlayerData.playerdata.info.Gp = uint.Parse(args.Snapshot.Value.ToString());
            if (titleRef != null) {
                titleRef.myGpTxt.text = ""+PlayerData.playerdata.info.Gp;
            }
            playerDataModifyLegalFlag = false;
        }
    }

    void HandleKashChangeEvent(object sender, ValueChangedEventArgs args) {
        if (bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }
        if (args.Snapshot.Value != null) {
            playerDataModifyLegalFlag = true;
            PlayerData.playerdata.info.Kash = uint.Parse(args.Snapshot.Value.ToString());
            if (titleRef != null) {
                titleRef.myKashTxt.text = ""+PlayerData.playerdata.info.Kash;
            }
            playerDataModifyLegalFlag = false;
        }
    }

    void HandleArmorChangeEvent(object sender, ValueChangedEventArgs args) {
        if (bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        if (PlayerData.playerdata.info.EquippedArmor == args.Snapshot.Value.ToString()) {
            return;
        }
        playerDataModifyLegalFlag = true;
        // When the armor is changed, equip it
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.EquippedArmor = itemEquipped;
        OnArmorChange(itemEquipped);
        playerDataModifyLegalFlag = false;
        if (titleRef != null) {
            titleRef.TriggerBlockScreen(false);
        }
    }

    void OnArmorChange(string itemEquipped) {
        if (bodyReference == null) return;
        EquipmentScript thisEquipScript = bodyReference.GetComponent<EquipmentScript>();
        
        if (thisEquipScript != null) {
            thisEquipScript.equippedArmor = PlayerData.playerdata.info.EquippedArmor;
            if (thisEquipScript.equippedArmorTopRef != null) {
                Destroy(thisEquipScript.equippedArmorTopRef);
                thisEquipScript.equippedArmorTopRef = null;
            }
            if (thisEquipScript.equippedArmorBottomRef != null) {
                Destroy(thisEquipScript.equippedArmorBottomRef);
                thisEquipScript.equippedArmorBottomRef = null;
            }
        }

        if (itemEquipped != "") {
            Armor a = InventoryScript.itemData.armorCatalog[itemEquipped];
            GameObject p = (InventoryScript.itemData.characterCatalog[info.EquippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[a.malePrefabPathTop] : InventoryScript.itemData.itemReferences[a.femalePrefabPathTop]);
            thisEquipScript.equippedArmorTopRef = (GameObject)Instantiate(p);
            thisEquipScript.equippedArmorTopRef.transform.SetParent(bodyReference.transform);

            p = (InventoryScript.itemData.characterCatalog[info.EquippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[a.malePrefabPathBottom] : InventoryScript.itemData.itemReferences[a.femalePrefabPathBottom]);
            thisEquipScript.equippedArmorBottomRef = (GameObject)Instantiate(p);
            thisEquipScript.equippedArmorBottomRef.transform.SetParent(bodyReference.transform);
            
            MeshFixer m = thisEquipScript.equippedArmorTopRef.GetComponentInChildren<MeshFixer>();
            m.target = thisEquipScript.myArmorTopRenderer.gameObject;
            m.rootBone = thisEquipScript.myBones.transform;
            m.AdaptMesh();

            m = thisEquipScript.equippedArmorBottomRef.GetComponentInChildren<MeshFixer>();
            m.target = thisEquipScript.myArmorBottomRenderer.gameObject;
            m.rootBone = thisEquipScript.myBones.transform;
            m.AdaptMesh();

            titleRef.equippedArmorSlot.GetComponent<SlotScript>().ToggleThumbnail(true, a.thumbnailPath);
        } else {
            titleRef.equippedArmorSlot.GetComponent<SlotScript>().ToggleThumbnail(false, null);
        }

        thisEquipScript.UpdateStats();
    }

    void HandleTopChangeEvent(object sender, ValueChangedEventArgs args) {
        if (bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        if (PlayerData.playerdata.info.EquippedTop == args.Snapshot.Value.ToString()) {
            return;
        }
        playerDataModifyLegalFlag = true;
        // When the top is changed, equip it
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.EquippedTop = itemEquipped;
        OnTopChange(itemEquipped);
        playerDataModifyLegalFlag = false;
        if (titleRef != null) {
            titleRef.TriggerBlockScreen(false);
        }
    }

    void OnTopChange(string itemEquipped) {
        if (bodyReference == null) return;
        EquipmentScript thisEquipScript = bodyReference.GetComponent<EquipmentScript>();
        
        if (thisEquipScript != null) {
            thisEquipScript.equippedTop = PlayerData.playerdata.info.EquippedTop;
            if (thisEquipScript.equippedTopRef != null) {
                Destroy(thisEquipScript.equippedTopRef);
                thisEquipScript.equippedTopRef = null;
            }
        }
        
        Equipment e = InventoryScript.itemData.equipmentCatalog[itemEquipped];
        GameObject p = (InventoryScript.itemData.characterCatalog[info.EquippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        thisEquipScript.equippedTopRef = (GameObject)Instantiate(p);
        thisEquipScript.equippedTopRef.transform.SetParent(bodyReference.transform);
        MeshFixer m = thisEquipScript.equippedTopRef.GetComponentInChildren<MeshFixer>();
        m.target = thisEquipScript.myTopRenderer.gameObject;
        m.rootBone = thisEquipScript.myBones.transform;
        m.AdaptMesh();

        if (titleRef != null) {
            titleRef.equippedTopSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        }

        thisEquipScript.EquipSkin(e.skinType);
    }

    void HandleBottomChangeEvent(object sender, ValueChangedEventArgs args) {
        if (bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        if (PlayerData.playerdata.info.EquippedBottom == args.Snapshot.Value.ToString()) {
            return;
        }
        playerDataModifyLegalFlag = true;
        // When the bottom is changed, equip it
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.EquippedBottom = itemEquipped;
        OnBottomChange(itemEquipped);
        playerDataModifyLegalFlag = false;
        if (titleRef != null) {
            titleRef.TriggerBlockScreen(false);
        }
    }

    void OnBottomChange(string itemEquipped) {
        if (bodyReference == null) return;
        EquipmentScript thisEquipScript = bodyReference.GetComponent<EquipmentScript>();
        
        if (thisEquipScript != null) {
            thisEquipScript.equippedBottom = PlayerData.playerdata.info.EquippedBottom;
            if (thisEquipScript.equippedBottomRef != null) {
                Destroy(thisEquipScript.equippedBottomRef);
                thisEquipScript.equippedBottomRef = null;
            }
        }
        
        Equipment e = InventoryScript.itemData.equipmentCatalog[itemEquipped];
        GameObject p = (InventoryScript.itemData.characterCatalog[info.EquippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        thisEquipScript.equippedBottomRef = (GameObject)Instantiate(p);
        thisEquipScript.equippedBottomRef.transform.SetParent(bodyReference.transform);
        MeshFixer m = thisEquipScript.equippedBottomRef.GetComponentInChildren<MeshFixer>();
        m.target = thisEquipScript.myBottomRenderer.gameObject;
        m.rootBone = thisEquipScript.myBones.transform;
        m.AdaptMesh();

        if (titleRef != null) {
            titleRef.equippedBottomSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        }
    }

    void HandleCharacterChangeEvent(object sender, ValueChangedEventArgs args) {
        if (bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        if (PlayerData.playerdata.info.EquippedCharacter == args.Snapshot.Value.ToString()) {
            return;
        }
        playerDataModifyLegalFlag = true;
        // When the character is changed, equip it
        string itemEquipped = args.Snapshot.Value.ToString();
        Character c = InventoryScript.itemData.characterCatalog[itemEquipped];
        PlayerData.playerdata.info.EquippedCharacter = itemEquipped;
        UpdateBodyRef();
        OnCharacterChange(itemEquipped);
        playerDataModifyLegalFlag = false;
        if (titleRef != null) {
            titleRef.TriggerBlockScreen(false);
        }
    }

    void OnCharacterChange(string itemEquipped) {
        if (bodyReference == null) return;
        EquipmentScript thisEquipScript = bodyReference.GetComponent<EquipmentScript>();

        if (thisEquipScript != null) {
            thisEquipScript.equippedCharacter = PlayerData.playerdata.info.EquippedCharacter;
            if (thisEquipScript.equippedSkinRef != null) {
                Destroy(thisEquipScript.equippedSkinRef);
                thisEquipScript.equippedSkinRef = null;
            }
            if (thisEquipScript.equippedTopRef != null) {
                Destroy(thisEquipScript.equippedTopRef);
                thisEquipScript.equippedTopRef = null;
            }
            if (thisEquipScript.equippedBottomRef != null) {
                Destroy(thisEquipScript.equippedBottomRef);
                thisEquipScript.equippedBottomRef = null;
            }
            if (thisEquipScript.equippedFootwearRef != null) {
                Destroy(thisEquipScript.equippedFootwearRef);
                thisEquipScript.equippedFootwearRef = null;
            }
        }

        Character c = InventoryScript.itemData.characterCatalog[itemEquipped];
        if (titleRef != null) {
            titleRef.equippedCharacterSlot.GetComponent<SlotScript>().ToggleThumbnail(true, c.thumbnailPath);
            titleRef.currentCharGender = c.gender;
            thisEquipScript.ResetStats();
        }

        // thisEquipScript.EquipTop(c.defaultTop, null);        
        Equipment e = InventoryScript.itemData.equipmentCatalog[PlayerData.playerdata.info.EquippedTop];
        GameObject p = (c.gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        thisEquipScript.equippedTopRef = (GameObject)Instantiate(p);
        thisEquipScript.equippedTopRef.transform.SetParent(bodyReference.transform);
        MeshFixer m = thisEquipScript.equippedTopRef.GetComponentInChildren<MeshFixer>();
        m.target = thisEquipScript.myTopRenderer.gameObject;
        m.rootBone = thisEquipScript.myBones.transform;
        m.AdaptMesh();
        thisEquipScript.EquipSkin(e.skinType);
        if (titleRef != null) {
            titleRef.equippedTopSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        }
        // thisEquipScript.EquipBottom(c.defaultBottom, null);
        e = InventoryScript.itemData.equipmentCatalog[PlayerData.playerdata.info.EquippedBottom];
        p = (c.gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        thisEquipScript.equippedBottomRef = (GameObject)Instantiate(p);
        thisEquipScript.equippedBottomRef.transform.SetParent(bodyReference.transform);
        m = thisEquipScript.equippedBottomRef.GetComponentInChildren<MeshFixer>();
        m.target = thisEquipScript.myBottomRenderer.gameObject;
        m.rootBone = thisEquipScript.myBones.transform;
        m.AdaptMesh();
        if (titleRef != null) {
            titleRef.equippedBottomSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        }
        // thisEquipScript.EquipFootwear((c.gender == 'M' ? "Standard Boots (M)" : "Standard Boots (F)"), null);
        e = InventoryScript.itemData.equipmentCatalog[PlayerData.playerdata.info.EquippedFootwear];
        p = (c.gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        thisEquipScript.equippedFootwearRef = (GameObject)Instantiate(p);
        thisEquipScript.equippedFootwearRef.transform.SetParent(bodyReference.transform);
        m = thisEquipScript.equippedFootwearRef.GetComponentInChildren<MeshFixer>();
        m.target = thisEquipScript.myFootwearRenderer.gameObject;
        m.rootBone = thisEquipScript.myBones.transform;
        m.AdaptMesh();
        if (titleRef != null) {
            titleRef.equippedFootSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        }

        WeaponScript thisWepScript = bodyReference.GetComponent<WeaponScript>();
        // Reequip primary
        Weapon w = InventoryScript.itemData.weaponCatalog[info.EquippedPrimary];
        string weaponType = w.category;
        GameObject wepEquipped = thisWepScript.weaponHolder.LoadWeapon(w.prefabPath);
        thisWepScript.equippedPrimaryWeapon = info.EquippedPrimary;
        thisWepScript.equippedSecondaryWeapon = info.EquippedSecondary;
        thisWepScript.equippedSupportWeapon = info.EquippedSupport;
        thisWepScript.equippedMeleeWeapon = info.EquippedMelee;
        
        if (w.suppressorCompatible) {
            thisWepScript.EquipMod("Suppressor", primaryModInfo.EquippedSuppressor, info.EquippedPrimary, null);
        }
        if (w.sightCompatible) {
            thisWepScript.EquipMod("Sight", primaryModInfo.EquippedSight, info.EquippedPrimary, null);
        }

        if (titleRef.currentCharGender == 'M') {
            thisWepScript.SetTitleWeaponPositions(wepEquipped.GetComponent<WeaponMeta>().titleHandPositionsMale);
        } else {
            thisWepScript.SetTitleWeaponPositions(wepEquipped.GetComponent<WeaponMeta>().titleHandPositionsFemale);
        }
    }

    void HandleFacewearChangeEvent(object sender, ValueChangedEventArgs args) {
        if (bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        if (PlayerData.playerdata.info.EquippedFacewear == args.Snapshot.Value.ToString()) {
            return;
        }
        playerDataModifyLegalFlag = true;
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.EquippedFacewear = itemEquipped;
        OnFacewearChange(itemEquipped);
        playerDataModifyLegalFlag = false;
        if (titleRef != null) {
            titleRef.TriggerBlockScreen(false);
        }
    }

    void OnFacewearChange(string itemEquipped) {
        if (bodyReference == null) return;
        EquipmentScript thisEquipScript = bodyReference.GetComponent<EquipmentScript>();

        if (thisEquipScript != null) {
            thisEquipScript.equippedFacewear = itemEquipped;
            if (thisEquipScript.equippedFacewearRef != null) {
                Destroy(thisEquipScript.equippedFacewearRef);
                thisEquipScript.equippedFacewearRef = null;
            }
        }

        if (itemEquipped != "") {
            Equipment e = InventoryScript.itemData.equipmentCatalog[itemEquipped];
            GameObject p = (InventoryScript.itemData.characterCatalog[info.EquippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
            thisEquipScript.equippedFacewearRef = (GameObject)Instantiate(p);
            thisEquipScript.equippedFacewearRef.transform.SetParent(bodyReference.transform);
            MeshFixer m = thisEquipScript.equippedFacewearRef.GetComponentInChildren<MeshFixer>();
            m.target = thisEquipScript.myFacewearRenderer.gameObject;
            m.rootBone = thisEquipScript.myBones.transform;
            m.AdaptMesh();
            titleRef.equippedFaceSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        } else {
            titleRef.equippedFaceSlot.GetComponent<SlotScript>().ToggleThumbnail(false, null);
        }

        thisEquipScript.UpdateStats();
    }

    void HandleFootwearChangeEvent(object sender, ValueChangedEventArgs args) {
        if (bodyReference == null) return;
        if (PlayerData.playerdata.info.EquippedFootwear == args.Snapshot.Value.ToString()) {
            return;
        }
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }
        playerDataModifyLegalFlag = true;
        // When the footwear is changed, equip it
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.EquippedFootwear = itemEquipped;
        OnFootwearChange(itemEquipped);
        playerDataModifyLegalFlag = false;
        if (titleRef != null) {
            titleRef.TriggerBlockScreen(false);
        }
    }

    void OnFootwearChange(string itemEquipped) {
        if (bodyReference == null) return;
        EquipmentScript thisEquipScript = bodyReference.GetComponent<EquipmentScript>();
        
        if (thisEquipScript != null) {
            thisEquipScript.equippedFootwear = PlayerData.playerdata.info.EquippedFootwear;
            if (thisEquipScript.equippedFootwearRef != null) {
                Destroy(thisEquipScript.equippedFootwearRef);
                thisEquipScript.equippedFootwearRef = null;
            }
        }

        Equipment e = InventoryScript.itemData.equipmentCatalog[itemEquipped];
        GameObject p = (InventoryScript.itemData.characterCatalog[info.EquippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
        thisEquipScript.equippedFootwearRef = (GameObject)Instantiate(p);
        thisEquipScript.equippedFootwearRef.transform.SetParent(bodyReference.transform);
        MeshFixer m = thisEquipScript.equippedFootwearRef.GetComponentInChildren<MeshFixer>();
        m.target = thisEquipScript.myFootwearRenderer.gameObject;
        m.rootBone = thisEquipScript.myBones.transform;
        m.AdaptMesh();

        if (titleRef != null) {
            titleRef.equippedFootSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        }
    }

    void HandleHeadgearChangeEvent(object sender, ValueChangedEventArgs args) {
        if (bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        if (PlayerData.playerdata.info.EquippedHeadgear == args.Snapshot.Value.ToString()) {
            return;
        }
        playerDataModifyLegalFlag = true;
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.EquippedHeadgear = itemEquipped;
        OnHeadgearChange(itemEquipped);
        playerDataModifyLegalFlag = false;
        if (titleRef != null) {
            titleRef.TriggerBlockScreen(false);
        }
    }

    void OnHeadgearChange(string itemEquipped) {
        if (bodyReference == null) return;
        EquipmentScript thisEquipScript = bodyReference.GetComponent<EquipmentScript>();

        if (thisEquipScript != null) {
            thisEquipScript.equippedHeadgear = itemEquipped;
            if (thisEquipScript.equippedHeadgearRef != null) {
                Destroy(thisEquipScript.equippedHeadgearRef);
                thisEquipScript.equippedHeadgearRef = null;
            }
        }

        if (itemEquipped != "") {
            Equipment e = InventoryScript.itemData.equipmentCatalog[itemEquipped];
            GameObject p = (InventoryScript.itemData.characterCatalog[info.EquippedCharacter].gender == 'M' ? InventoryScript.itemData.itemReferences[e.malePrefabPath] : InventoryScript.itemData.itemReferences[e.femalePrefabPath]);
            thisEquipScript.equippedHeadgearRef = (GameObject)Instantiate(p);
            thisEquipScript.equippedHeadgearRef.transform.SetParent(bodyReference.transform);
            MeshFixer m = thisEquipScript.equippedHeadgearRef.GetComponentInChildren<MeshFixer>();
            m.target = thisEquipScript.myHeadgearRenderer.gameObject;
            m.rootBone = thisEquipScript.myBones.transform;
            m.AdaptMesh();
            titleRef.equippedHeadSlot.GetComponent<SlotScript>().ToggleThumbnail(true, e.thumbnailPath);
        } else {
            titleRef.equippedHeadSlot.GetComponent<SlotScript>().ToggleThumbnail(false, null);
        }

        thisEquipScript.UpdateStats();
    }

    void HandleMeleeChangeEvent(object sender, ValueChangedEventArgs args) {
        if (bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        if (PlayerData.playerdata.info.EquippedMelee == args.Snapshot.Value.ToString()) {
            return;
        }
        playerDataModifyLegalFlag = true;
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.EquippedMelee = itemEquipped;
        OnMeleeChange(itemEquipped);
        playerDataModifyLegalFlag = false;
        if (titleRef != null) {
            titleRef.TriggerBlockScreen(false);
        }
    }

    void OnMeleeChange(string itemEquipped) {
        if (bodyReference == null) return;
        WeaponScript thisWepScript = bodyReference.GetComponent<WeaponScript>();
        thisWepScript.equippedMeleeWeapon = itemEquipped;
        // Get the weapon from the weapon catalog for its properties
        Weapon w = InventoryScript.itemData.weaponCatalog[itemEquipped];
        titleRef.equippedMeleeSlot.GetComponent<SlotScript>().ToggleThumbnail(true, w.thumbnailPath);
    }

    void HandlePrimaryChangeEvent(object sender, ValueChangedEventArgs args) {
        if (bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        if (PlayerData.playerdata.info.EquippedPrimary == args.Snapshot.Value.ToString()) {
            return;
        }
        playerDataModifyLegalFlag = true;
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.EquippedPrimary = itemEquipped;
        OnPrimaryChange(itemEquipped);
        playerDataModifyLegalFlag = false;
        if (titleRef != null) {
            titleRef.TriggerBlockScreen(false);
        }
    }

    void OnPrimaryChange(string itemEquipped) {
        if (bodyReference == null) return;
        WeaponScript thisWepScript = bodyReference.GetComponent<WeaponScript>();
        thisWepScript.equippedPrimaryWeapon = itemEquipped;
        // Get the weapon from the weapon catalog for its properties
        Weapon w = InventoryScript.itemData.weaponCatalog[itemEquipped];
        string weaponType = w.category;
        ModInfo modInfo = PlayerData.playerdata.LoadModDataForWeapon(itemEquipped);
        PlayerData.playerdata.primaryModInfo = modInfo;
        GameObject wepEquipped = thisWepScript.weaponHolder.LoadWeapon(w.prefabPath);
        
        if (w.suppressorCompatible) {
            thisWepScript.EquipMod("Suppressor", modInfo.EquippedSuppressor, itemEquipped, null);
        }
        if (w.sightCompatible) {
            thisWepScript.EquipMod("Sight", modInfo.EquippedSight, itemEquipped, null);
        }

        if (titleRef.currentCharGender == 'M') {
            thisWepScript.SetTitleWeaponPositions(wepEquipped.GetComponent<WeaponMeta>().titleHandPositionsMale);
        } else {
            thisWepScript.SetTitleWeaponPositions(wepEquipped.GetComponent<WeaponMeta>().titleHandPositionsFemale);
        }

        // Puts the item that you just equipped in its proper slot
        titleRef.equippedPrimarySlot.GetComponent<SlotScript>().ToggleThumbnail(true, w.thumbnailPath);
    }

    void HandleSecondaryChangeEvent(object sender, ValueChangedEventArgs args) {
        if (bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        if (PlayerData.playerdata.info.EquippedSecondary == args.Snapshot.Value.ToString()) {
            return;
        }
        playerDataModifyLegalFlag = true;
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.EquippedSecondary = itemEquipped;
        OnSecondaryChange(itemEquipped);
        playerDataModifyLegalFlag = false;
        if (titleRef != null) {
            titleRef.TriggerBlockScreen(false);
        }
    }

    void OnSecondaryChange(string itemEquipped) {
        if (bodyReference == null) return;
        WeaponScript thisWepScript = bodyReference.GetComponent<WeaponScript>();
        thisWepScript.equippedSecondaryWeapon = itemEquipped;
        // Get the weapon from the weapon catalog for its properties
        Weapon w = InventoryScript.itemData.weaponCatalog[itemEquipped];
        string weaponType = w.category;
        ModInfo modInfo = PlayerData.playerdata.LoadModDataForWeapon(itemEquipped);
        PlayerData.playerdata.secondaryModInfo = modInfo;

        if (w.suppressorCompatible) {
            thisWepScript.EquipMod("Suppressor", modInfo.EquippedSuppressor, itemEquipped, null);
        }
        if (w.sightCompatible) {
            thisWepScript.EquipMod("Sight", modInfo.EquippedSight, itemEquipped, null);
        }

        titleRef.equippedSecondarySlot.GetComponent<SlotScript>().ToggleThumbnail(true, w.thumbnailPath);
    }

    void HandleSupportChangeEvent(object sender, ValueChangedEventArgs args) {
        if (bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        if (PlayerData.playerdata.info.EquippedSupport == args.Snapshot.Value.ToString()) {
            return;
        }
        playerDataModifyLegalFlag = true;
        string itemEquipped = args.Snapshot.Value.ToString();
        PlayerData.playerdata.info.EquippedSupport = itemEquipped;
        OnSupportChange(itemEquipped);
        playerDataModifyLegalFlag = false;
        if (titleRef != null) {
            titleRef.TriggerBlockScreen(false);
        }
    }

    void OnSupportChange(string itemEquipped) {
        if (bodyReference == null) return;
        WeaponScript thisWepScript = bodyReference.GetComponent<WeaponScript>();
        thisWepScript.equippedSupportWeapon = itemEquipped;
        // Get the weapon from the weapon catalog for its properties
        Weapon w = InventoryScript.itemData.weaponCatalog[itemEquipped];
        string weaponType = w.category;
        ModInfo modInfo = PlayerData.playerdata.LoadModDataForWeapon(itemEquipped);
        PlayerData.playerdata.supportModInfo = modInfo;

        titleRef.equippedSupportSlot.GetComponent<SlotScript>().ToggleThumbnail(true, w.thumbnailPath);
    }

    void HandleBanEvent(object sender, ChildChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }
        
        if (args.Snapshot.Value != null) {
            if (args.Snapshot.Key.ToString() != "reason") return;
            TriggerEmergencyExit("You've been banned for the following reason:\n" + args.Snapshot.Value.ToString() + "\nIf you feel this was done in error, you can dispute it by opening a ticket at \"www.koobando.com/support\".");
        }
    }

    void HandleInventoryChanged(object sender, ChildChangedEventArgs args) {
        if (bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        // When inventory item has been updated, find the item that has been updated and update it
        if (args.Snapshot.Value != null) {
            if (titleRef != null) {
                titleRef.TriggerBlockScreen(true);
            }
            playerDataModifyLegalFlag = true;
            inventoryDataModifyLegalFlag = true;
            RefreshInventory(args.Snapshot, 'm');
            inventoryDataModifyLegalFlag = false;
            playerDataModifyLegalFlag = false;
            if (titleRef != null) {
                titleRef.TriggerBlockScreen(false);
            }
        }
    }

    void HandleInventoryAdded(object sender, ChildChangedEventArgs args) {
        if (bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        // When inventory item has been added, also add that item to this game session
        if (args.Snapshot.Value != null) {
            if (titleRef != null) {
                titleRef.TriggerBlockScreen(true);
            }
            playerDataModifyLegalFlag = true;
            inventoryDataModifyLegalFlag = true;
            RefreshInventory(args.Snapshot, 'a');
            inventoryDataModifyLegalFlag = false;
            playerDataModifyLegalFlag = false;
            if (titleRef != null) {
                titleRef.TriggerBlockScreen(false);
            }
        }
    }

    void HandleInventoryRemoved(object sender, ChildChangedEventArgs args) {
        if (bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }
        
        // When inventory item has been removed, also remove that item from this game session
        if (args.Snapshot.Value != null) {
            if (titleRef != null) {
                titleRef.TriggerBlockScreen(true);
            }
            playerDataModifyLegalFlag = true;
            inventoryDataModifyLegalFlag = true;
            RefreshInventory(args.Snapshot, 'd');
            inventoryDataModifyLegalFlag = false;
            playerDataModifyLegalFlag = false;
            if (titleRef != null) {
                titleRef.TriggerBlockScreen(false);
            }
        }
    }

    void HandleGiftAdded(object sender, ChildChangedEventArgs args)
    {
        if (bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot.Value != null) {
            playerDataModifyLegalFlag = true;
            
            // Extract the gift ID
            string giftId = args.Snapshot.Key;
            // Get details

            GiftData g = new GiftData();
            g.PropertyChanged += OnPlayerInfoChange;
            g.GiftId = giftId;
            g.Category = args.Snapshot.Child("category").Value.ToString();
            g.Sender = args.Snapshot.Child("from").Value.ToString();
            g.ItemName = args.Snapshot.Child("itemName").Value.ToString();
            g.Duration = Convert.ToSingle(args.Snapshot.Child("duration").Value);
            g.Message = args.Snapshot.Child("message").Value.ToString();

            giftList.Add(giftId, g);

            // If on title, add gift to inbox
            if (PlayerData.playerdata.titleRef != null) {
                PlayerData.playerdata.titleRef.giftInbox.EnqueueGiftEntryCreation(giftId, g.Category, g.Sender, g.ItemName, g.Duration, g.Message);
            }

            playerDataModifyLegalFlag = false;
        }
    }

    void HandleGiftRemoved(object sender, ChildChangedEventArgs args)
    {
        if (bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot.Value != null) {
            playerDataModifyLegalFlag = true;

            string giftId = args.Snapshot.Key;
            GiftData g = PlayerData.playerdata.giftList[giftId];
            g.PropertyChanged -= OnPlayerInfoChange;
            PlayerData.playerdata.giftList.Remove(giftId);
            // If on title, remove entry from gift inbox
            if (PlayerData.playerdata.titleRef != null) {
                PlayerData.playerdata.titleRef.giftInbox.DeleteGiftEntry(giftId);
            }

            playerDataModifyLegalFlag = false;
        }
    }

    void HandleFriendAdded(object sender, ChildChangedEventArgs args)
    {
        if (bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        // When friend item has been added, also add that item to this game session
        if (args.Snapshot.Value != null) {
            playerDataModifyLegalFlag = true;
            
            // Extract the friend request ID
            string friendRequestId = args.Snapshot.Value.ToString();

            Dictionary<string, object> inputData = new Dictionary<string, object>();
            inputData["callHash"] = DAOScript.functionsCallHash;
            inputData["uid"] = AuthScript.authHandler.user.UserId;
            inputData["friendRequestId"] = friendRequestId;

            HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("getNewlyAddedFriend");
            func.CallAsync(inputData).ContinueWith((task) => {
                if (task.IsFaulted) {
                    Debug.LogError("Friend could not be added because no username was found.");
                } else {
                    Dictionary<object, object> results = (Dictionary<object, object>)task.Result.Data;
                    if (results["status"].ToString() == "200") {
                        // Get details
                        Dictionary<object, object> friendRequest = (Dictionary<object, object>)results["friendship"];
                        string requestor = friendRequest["requestor"].ToString();
                        string requestee = friendRequest["requestee"].ToString();
                        string friendId = requestor == AuthScript.authHandler.user.UserId ? requestee : requestor;
                        int status = Convert.ToInt32(friendRequest["status"]);
                        string blocker = null;
                        if (friendRequest.ContainsKey("blocker")) {
                            blocker = friendRequest["blocker"].ToString();
                        }
                        // Create friend data
                        FriendData fd = new FriendData();

                        fd.FriendUsername = results["username"].ToString();
                        fd.Exp = Convert.ToUInt32(results["exp"].ToString());
                        fd.PropertyChanged += OnPlayerInfoChange;
                        fd.FriendRequestId = friendRequestId;
                        fd.FriendId = friendId;
                        fd.Status = status;
                        fd.Requestor = requestor;
                        fd.Requestee = requestee;
                        fd.Blocker = blocker;

                        // Add update callback
                        DAOScript.dao.dbRef.Child("fteam_ai/friends/" + friendRequestId).ChildChanged += HandleFriendUpdate;
                        DAOScript.dao.dbRef.Child("fteam_ai/friends/" + friendRequestId).ChildAdded += HandleFriendUpdate;

                        // Add to friend list
                        globalChatClient.AddStatusListenersToFriends(new List<string>(){fd.FriendUsername});
                        PlayerData.playerdata.friendsList.Add(friendRequestId, fd);

                        // Add to UI
                        if (titleRef != null) {
                            if (status != 2 || (status == 2 && blocker == AuthScript.authHandler.user.UserId)) {
                                titleRef.friendsMessenger.EnqueueMessengerEntryCreation(friendRequestId, fd.FriendUsername, fd.Exp);
                            }
                        }
                    } else {
                        Debug.LogError("Friend could not be added because no username was found.");
                    }
                }
                playerDataModifyLegalFlag = false;
            });
        }
    }

    void HandleFriendRemoved(object sender, ChildChangedEventArgs args)
    {
        if (bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        // When friend item has been removed, also remove that item from this game session
        if (args.Snapshot.Value != null) {
            playerDataModifyLegalFlag = true;
            
            string key = args.Snapshot.Value.ToString();
            FriendData fd = PlayerData.playerdata.friendsList[key];
            fd.PropertyChanged -= OnPlayerInfoChange;

            // Remove friend from list
            globalChatClient.RemoveStatusListenersForFriends(new List<string>(){fd.FriendUsername});
            PlayerData.playerdata.friendsList.Remove(key);
            if (titleRef != null) {
                titleRef.friendsMessenger.DeleteMessengerEntry(key);
            }

            playerDataModifyLegalFlag = false;
        }
    }

    void HandleFriendUpdate(object sender, ChildChangedEventArgs args)
    {
        if (bodyReference == null) return;
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            TriggerEmergencyExit(args.DatabaseError.Message);
            return;
        }

        // When friend item has been added, also add that item to this game session
        if (args.Snapshot.Value != null) {
            playerDataModifyLegalFlag = true;
            string key = args.Snapshot.Reference.Parent.Key;
            FriendData fd = PlayerData.playerdata.friendsList[key];
            if (args.Snapshot.Key == "status") {
                fd.Status = Convert.ToInt32(args.Snapshot.Value);
                if (fd.Status == 1) {
                    PlayerData.playerdata.globalChatClient.AddStatusListenersToFriends(new List<string>(){fd.FriendUsername});
                } else {
                    PlayerData.playerdata.globalChatClient.RemoveStatusListenersForFriends(new List<string>(){fd.FriendUsername});
                }
            }
            if (args.Snapshot.Key == "blocker") {
                fd.Blocker = args.Snapshot.Value.ToString();
            }
            if (titleRef != null) {
                MessengerEntryScript m = titleRef.friendsMessenger.GetMessengerEntry(key);
                m?.UpdateFriendStatus();
            }

            playerDataModifyLegalFlag = false;
        }
    }

    IEnumerator EmergencyExitGame() {
        yield return new WaitForSeconds(5f);
        Dictionary<string, object> inputData = new Dictionary<string, object>();
		inputData["callHash"] = DAOScript.functionsCallHash;
		inputData["uid"] = AuthScript.authHandler.user.UserId;
		inputData["loggedIn"] = "0";
		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("setUserIsLoggedIn");
		func.CallAsync(inputData).ContinueWith((task) => {
            Application.Quit();
        });
    }

    public string ConvertTypeToFirebaseType(string type) {
        if (type.Equals("Weapon")) {
            return "weapons";
        } else if (type.Equals("Character")) {
            return "characters";
        } else if (type.Equals("Top")) {
            return "tops";
        } else if (type.Equals("Bottom")) {
            return "bottoms";
        } else if (type.Equals("Armor")) {
            return "armor";
        } else if (type.Equals("Footwear")) {
            return "footwear";
        } else if (type.Equals("Headgear")) {
            return "headgear";
        } else if (type.Equals("Facewear")) {
            return "facewear";
        } else if (type.Equals("Mod")) {
            return "mods";
        }
        return type;
    }

    bool IsNotInGame() {
        string thisSceneName = SceneManager.GetActiveScene().name;
        if (thisSceneName == "Title" || thisSceneName == "GameOverSuccess" || thisSceneName == "GameOverFail") {
            return true;
        }
        return false;
    }

    public bool CheckIsVerifiedFriendByUsername(string username)
    {
        foreach (KeyValuePair<string, FriendData> entry in PlayerData.playerdata.friendsList)
        {
            if (entry.Value.FriendUsername == username) {
                if (entry.Value.Status == 1) {
                    return true;
                } else {
                    return false;
                }
            }
        }
        
        return false;
    }

    public int GetCurrentDurationForItemAndType(string itemName, string category)
    {
        int currentDuration = 0;
        if (category == "Character") {
            if (PlayerData.playerdata.inventory.myCharacters.ContainsKey(itemName)) {
                currentDuration = int.Parse(PlayerData.playerdata.inventory.myCharacters[itemName].Duration);
            }
        } else if (category == "Armor") {
            if (PlayerData.playerdata.inventory.myArmor.ContainsKey(itemName)) {
                currentDuration = int.Parse(PlayerData.playerdata.inventory.myArmor[itemName].Duration);
            }
        } else if (category == "Weapon") {
            if (PlayerData.playerdata.inventory.myWeapons.ContainsKey(itemName)) {
                currentDuration = int.Parse(PlayerData.playerdata.inventory.myWeapons[itemName].Duration);
            }
        } else if (category == "Mod") {
            if (PlayerData.playerdata.inventory.myMods.ContainsKey(itemName)) {
                currentDuration = int.Parse(PlayerData.playerdata.inventory.myMods[itemName].Duration);
            }
        } else if (category == "Top") {
            if (PlayerData.playerdata.inventory.myTops.ContainsKey(itemName)) {
                currentDuration = int.Parse(PlayerData.playerdata.inventory.myTops[itemName].Duration);
            }
        } else if (category == "Bottom") {
            if (PlayerData.playerdata.inventory.myBottoms.ContainsKey(itemName)) {
                currentDuration = int.Parse(PlayerData.playerdata.inventory.myBottoms[itemName].Duration);
            }
        } else if (category == "Footwear") {
            if (PlayerData.playerdata.inventory.myFootwear.ContainsKey(itemName)) {
                currentDuration = int.Parse(PlayerData.playerdata.inventory.myFootwear[itemName].Duration);
            }
        } else if (category == "Facewear") {
            if (PlayerData.playerdata.inventory.myFacewear.ContainsKey(itemName)) {
                currentDuration = int.Parse(PlayerData.playerdata.inventory.myFacewear[itemName].Duration);
            }
        } else if (category == "Headgear") {
            if (PlayerData.playerdata.inventory.myHeadgear.ContainsKey(itemName)) {
                currentDuration = int.Parse(PlayerData.playerdata.inventory.myHeadgear[itemName].Duration);
            }
        }
        return currentDuration;
    }
}

public class PlayerInfo : INotifyPropertyChanged
{
    private string defaultChar;
    public string DefaultChar
    {
        get { return defaultChar; }
        set
        { 
            defaultChar = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("defaultChar"));
        }
    }

    private string defaultWeapon;
    public string DefaultWeapon
    {
        get { return defaultWeapon; }
        set
        { 
            defaultWeapon = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("defaultWeapon"));
        }
    }

	private string playername;
    public string Playername
    {
        get { return playername; }
        set
        { 
            playername = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("playername"));
        }
    }

    private string equippedCharacter;
    public string EquippedCharacter
    {
        get { return equippedCharacter; }
        set
        { 
            equippedCharacter = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("equippedCharacter"));
        }
    }

    private string equippedHeadgear;
    public string EquippedHeadgear
    {
        get { return equippedHeadgear; }
        set
        { 
            equippedHeadgear = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("equippedHeadgear"));
        }
    }

    private string equippedFacewear;
    public string EquippedFacewear
    {
        get { return equippedFacewear; }
        set
        { 
            equippedFacewear = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("equippedFacewear"));
        }
    }

    private string equippedTop;
    public string EquippedTop
    {
        get { return equippedTop; }
        set
        { 
            equippedTop = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("equippedTop"));
        }
    }

    private string equippedBottom;
    public string EquippedBottom
    {
        get { return equippedBottom; }
        set
        { 
            equippedBottom = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("equippedBottom"));
        }
    }

    private string equippedFootwear;
    public string EquippedFootwear
    {
        get { return equippedFootwear; }
        set
        { 
            equippedFootwear = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("equippedFootwear"));
        }
    }

    private string equippedArmor;
    public string EquippedArmor
    {
        get { return equippedArmor; }
        set
        { 
            equippedArmor = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("equippedArmor"));
        }
    }

    private string equippedPrimary;
    public string EquippedPrimary
    {
        get { return equippedPrimary; }
        set
        { 
            equippedPrimary = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("equippedPrimary"));
        }
    }

    private string equippedSecondary;
    public string EquippedSecondary
    {
        get { return equippedSecondary; }
        set
        { 
            equippedSecondary = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("equippedSecondary"));
        }
    }

    private string equippedSupport;
    public string EquippedSupport
    {
        get { return equippedSupport; }
        set
        { 
            equippedSupport = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("equippedSupport"));
        }
    }

    private string equippedMelee;
    public string EquippedMelee
    {
        get { return equippedMelee; }
        set
        { 
            equippedMelee = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("equippedMelee"));
        }
    }

    private uint exp;
    public uint Exp
    {
        get { return exp; }
        set
        { 
            exp = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("exp"));
        }
    }

    private uint gp;
    public uint Gp
    {
        get { return gp; }
        set
        { 
            gp = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("gp"));
        }
    }

    private uint kash;
    public uint Kash
    {
        get { return kash; }
        set
        {
            kash = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("kash"));
        }
    }

    private string privilegeLevel;
    public string PrivilegeLevel
    {
        get { return privilegeLevel; }
        set
        {
            privilegeLevel = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("privilegeLevel"));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged = (sender, args) => { };
}

public class PlayerInventory {
    public PlayerInventory() {
        this.myHeadgear = new ObservableDict<string, EquipmentData>();
        this.myTops = new ObservableDict<string, EquipmentData>();
        this.myBottoms = new ObservableDict<string, EquipmentData>();
        this.myFacewear = new ObservableDict<string, EquipmentData>();
        this.myFootwear = new ObservableDict<string, EquipmentData>();
        this.myArmor = new ObservableDict<string, ArmorData>();
        this.myWeapons = new ObservableDict<string, WeaponData>();
        this.myCharacters = new ObservableDict<string, CharacterData>();
        this.myMods = new ObservableDict<string, ModData>();
        myHeadgear.CollectionChanged += OnCollectionChanged;
        myHeadgear.PropertyChanged += OnPropertyChanged;
        myTops.CollectionChanged += OnCollectionChanged;
        myTops.PropertyChanged += OnPropertyChanged;
        myBottoms.CollectionChanged += OnCollectionChanged;
        myBottoms.PropertyChanged += OnPropertyChanged;
        myFacewear.CollectionChanged += OnCollectionChanged;
        myFacewear.PropertyChanged += OnPropertyChanged;
        myFootwear.CollectionChanged += OnCollectionChanged;
        myFootwear.PropertyChanged += OnPropertyChanged;
        myArmor.CollectionChanged += OnCollectionChanged;
        myArmor.PropertyChanged += OnPropertyChanged;
        myWeapons.CollectionChanged += OnCollectionChanged;
        myWeapons.PropertyChanged += OnPropertyChanged;
        myCharacters.CollectionChanged += OnCollectionChanged;
        myCharacters.PropertyChanged += OnPropertyChanged;
        myMods.CollectionChanged += OnCollectionChanged;
        myMods.PropertyChanged += OnPropertyChanged;
    }

    public ObservableDict<string, EquipmentData> myHeadgear;
    public ObservableDict<string, EquipmentData> myTops;
    public ObservableDict<string, EquipmentData> myBottoms;
    public ObservableDict<string, EquipmentData> myFacewear;
    public ObservableDict<string, EquipmentData> myFootwear;
    public ObservableDict<string, ArmorData> myArmor;
    public ObservableDict<string, WeaponData> myWeapons;
    public ObservableDict<string, CharacterData> myCharacters;
    public ObservableDict<string, ModData> myMods;

    protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        if (!PlayerData.playerdata.inventoryDataModifyLegalFlag) {
            if (PlayerData.playerdata == null) {
                Application.Quit();
            } else {
                // Ban player here for modifying item data
                Dictionary<string, object> inputData = new Dictionary<string, object>();
                inputData["callHash"] = DAOScript.functionsCallHash;
                inputData["uid"] = AuthScript.authHandler.user.UserId;
                inputData["duration"] = "-1";
                inputData["reason"] = "Illegal modification of user data.";

                HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("banPlayer");
                func.CallAsync(inputData).ContinueWith((task) => {
                    PlayerData.playerdata.TriggerEmergencyExit("You've been banned for the following reason:\nIllegal modification of game data.\nIf you feel this was done in error, you can dispute it by opening a ticket at \"www.koobando.com/support\".");
                });
            }
        }
    }

    protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if (!PlayerData.playerdata.inventoryDataModifyLegalFlag) {
            if (PlayerData.playerdata == null) {
                Application.Quit();
            } else {
                // Ban player here for modifying item data
                Dictionary<string, object> inputData = new Dictionary<string, object>();
                inputData["callHash"] = DAOScript.functionsCallHash;
                inputData["uid"] = AuthScript.authHandler.user.UserId;
                inputData["duration"] = "-1";
                inputData["reason"] = "Illegal modification of user data.";

                HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("banPlayer");
                func.CallAsync(inputData).ContinueWith((task) => {
                    PlayerData.playerdata.TriggerEmergencyExit("You've been banned for the following reason:\nIllegal modification of game data.\nIf you feel this was done in error, you can dispute it by opening a ticket at \"www.koobando.com/support\".");
                });
            }
        }
    }
}

public class ModInfo : INotifyPropertyChanged
{
    private string suppressorId;
    public string SuppressorId {
        get { return suppressorId; }
        set
        { 
            suppressorId = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("suppressorId"));
        }
    }
    private string sightId;
    public string SightId {
        get { return sightId; }
        set
        { 
            sightId = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("sightId"));
        }
    }

    private string weaponName;
    public string WeaponName {
        get { return weaponName; }
        set
        { 
            weaponName = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("weaponName"));
        }
    }

    private string equippedSuppressor;
    public string EquippedSuppressor {
        get { return equippedSuppressor; }
        set
        { 
            equippedSuppressor = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("equippedSuppressor"));
        }
    }

    private string equippedSight;
    public string EquippedSight {
        get { return equippedSight; }
        set
        { 
            equippedSight = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("equippedSight"));
        }
    }

    private string equippedClip;
    public string EquippedClip {
        get { return equippedClip; }
        set
        { 
            equippedClip = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("equippedClip"));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged = (sender, args) => { };
}

public class WeaponData : INotifyPropertyChanged {
    private string acquireDate;
    public string AcquireDate {
        get { return acquireDate; }
        set
        { 
            acquireDate = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("acquireDate"));
        }
    }

    private string duration;
    public string Duration {
        get { return duration; }
        set
        { 
            duration = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("duration"));
        }
    }

    private string equippedSuppressor;
    public string EquippedSuppressor {
        get { return equippedSuppressor; }
        set
        { 
            equippedSuppressor = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("equippedSuppressor"));
        }
    }

    private string equippedSight;
    public string EquippedSight {
        get { return equippedSight; }
        set
        { 
            equippedSight = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("equippedSight"));
        }
    }

    private string equippedClip;
    public string EquippedClip {
        get { return equippedClip; }
        set
        { 
            equippedClip = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("equippedClip"));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged = (sender, args) => { };
}

public class EquipmentData : INotifyPropertyChanged {
    private string acquireDate;
    public string AcquireDate {
        get { return acquireDate; }
        set
        { 
            acquireDate = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("acquireDate"));
        }
    }

    private string duration;
    public string Duration {
        get { return duration; }
        set
        { 
            duration = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("duration"));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged = (sender, args) => { };
}

public class ModData : INotifyPropertyChanged {
    private string name;
    public string Name {
        get { return name; }
        set
        { 
            name = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("name"));
        }
    }

    private string equippedOn;
    public string EquippedOn {
        get { return equippedOn; }
        set
        { 
            equippedOn = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("equippedOn"));
        }
    }

    private string acquireDate;
    public string AcquireDate {
        get { return acquireDate; }
        set
        { 
            acquireDate = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("acquireDate"));
        }
    }

    private string duration;
    public string Duration {
        get { return duration; }
        set
        { 
            duration = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("duration"));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged = (sender, args) => { };
}

public class ArmorData : INotifyPropertyChanged {
    private string acquireDate;
    public string AcquireDate {
        get { return acquireDate; }
        set
        { 
            acquireDate = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("acquireDate"));
        }
    }

    private string duration;
    public string Duration {
        get { return duration; }
        set
        { 
            duration = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("duration"));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged = (sender, args) => { };
}

public class CharacterData : INotifyPropertyChanged {
    private string acquireDate;
    public string AcquireDate {
        get { return acquireDate; }
        set
        { 
            acquireDate = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("acquireDate"));
        }
    }

    private string duration;
    public string Duration {
        get { return duration; }
        set
        { 
            duration = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("duration"));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged = (sender, args) => { };
}

public class FriendData : INotifyPropertyChanged {
    private string friendRequestId;
    public string FriendRequestId {
        get { return friendRequestId; }
        set
        {
            friendRequestId = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("friendRequestId"));
        }
    }

    private string friendId;
    public string FriendId {
        get { return friendId; }
        set
        {
            friendId = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("friendId"));
        }
    }

    private string friendUsername;
    public string FriendUsername {
        get { return friendUsername; }
        set
        {
            friendUsername = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("friendUsername"));
        }
    }

    private int status;
    public int Status {
        get { return status; }
        set
        {
            status = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("status"));
        }
    }

    private string requestor;
    public string Requestor {
        get { return requestor; }
        set
        {
            requestor = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("requestor"));
        }
    }

    private string requestee;
    public string Requestee {
        get { return requestee; }
        set
        {
            requestee = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("requestee"));
        }
    }

    private string blocker;
    public string Blocker {
        get { return blocker; }
        set
        {
            blocker = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("blocker"));
        }
    }

    private uint exp;
    public uint Exp
    {
        get { return exp; }
        set
        { 
            exp = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("exp"));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged = (sender, args) => { };
}

public class GiftData : INotifyPropertyChanged {
    private string giftId;
    public string GiftId {
        get { return giftId; }
        set
        {
            giftId = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("giftId"));
        }
    }

    private string category;
    public string Category {
        get { return category; }
        set
        {
            category = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("category"));
        }
    }

    private string sender;
    public string Sender {
        get { return sender; }
        set
        {
            sender = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("sender"));
        }
    }

    private string itemName;
    public string ItemName {
        get { return itemName; }
        set
        {
            itemName = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("itemName"));
        }
    }

    private float duration;
    public float Duration {
        get { return duration; }
        set
        {
            duration = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("duration"));
        }
    }

    private string message;
    public string Message {
        get { return message; }
        set
        {
            message = value;
            PropertyChanged(this, new PropertyChangedEventArgs ("message"));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged = (sender, args) => { };
}

public class Rank {
    public string name;
    public uint minExp;
    public uint maxExp;
    public Rank(string name, uint minExp, uint maxExp) {
        this.name = name;
        this.minExp = minExp;
        this.maxExp = maxExp;
    }
}

public class CachedMessage {
    public string cachedMessages;
    public int previousMessageCount;
    public CachedMessage()
    {
        cachedMessages = "";
        previousMessageCount = 0;
    }
}
