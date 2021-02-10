using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Chat;
using Photon.Pun;

public class GlobalChatClient : MonoBehaviour, IChatClientListener
{
    private const string ROOM_REQUEST_MSG = "f@AC?3CSWGRvnv@J";
    private const string ROOM_JOIN_MSG = "JOIN|";
    private const short MESSAGES_PER_MIN_LIMIT = 25; // This is the number of messages a user may send per minute. Prevents spam.
    public ChatClient chatClient;
    protected internal ChatAppSettings chatAppSettings;
    private float messageLimitTimer;
    private short messagesSentThisMin;

    public void Initialize(string username)
    {
        if (this.chatClient != null) {
            return;
        }

        #if PHOTON_UNITY_NETWORKING
        this.chatAppSettings = PhotonNetwork.PhotonServerSettings.AppSettings.GetChatSettings();
        #endif

        Connect(username);
    }

    // Update is called once per frame
    void Update()
    {
        if (this.chatClient != null)
		{
			this.chatClient.Service();
            messageLimitTimer += Time.deltaTime;
            if (messageLimitTimer >= 60f) {
                messageLimitTimer = 0f;
                messagesSentThisMin = 0;
            }
		}
    }

    void Connect(string username)
	{
		this.chatClient = new ChatClient(this);
        #if !UNITY_WEBGL
        this.chatClient.UseBackgroundWorkerForSending = true;
        #endif
        this.chatClient.AuthValues = new AuthenticationValues(username);
		this.chatClient.ConnectUsingSettings(this.chatAppSettings);

		Debug.Log("Connecting to Photon Chat as: " + username);
	}

    public void OnConnected()
	{
        Debug.Log("Successfully connected to Photon Chat.");
        // Set online status
        UpdateStatus("ONLINE");
	}

    // Call this whenever you enter the campaign or versus matchmaking lobby
    public void SubscribeToGlobalChat(char modeLobby) {
        if (this.chatClient.CanChat) {
            if (modeLobby == 'C') {
                this.chatClient.Subscribe("Campaign");
            } else if (modeLobby == 'V') {
                this.chatClient.Subscribe("Versus");
            }
        }
    }

    public void UpdateStatus(string status)
    {
        chatClient.SetOnlineStatus(ChatUserStatus.Online, status);
    }

    public void AddStatusListenersToFriends(List<string> usernames)
    {
        chatClient.AddFriends(usernames.ToArray());
    }

    public void AskToJoinGame(string username)
    {
        SendPrivateMessageToUser(username, ROOM_REQUEST_MSG);
    }

    public void SendPrivateMessageToUser(string username, string message)
    {
        chatClient.SendPrivateMessage(username, message);
    }

    // Call this when you return to the home screen or leave the title scene
    public void UnsubscribeFromGlobalChat() {
        this.chatClient.Unsubscribe(new string[] {"Campaign", "Versus"});
    }

    public void OnUserSubscribed(string channel, string user)
    {
        Debug.LogFormat("OnUserSubscribed: channel=\"{0}\" userId=\"{1}\"", channel, user);
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        Debug.LogFormat("OnUserUnsubscribed: channel=\"{0}\" userId=\"{1}\"", channel, user);
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
	{
		Debug.Log("status: " + string.Format("{0} is {1}. Msg:{2}", user, status, message));
        // Update messenger entry scripts here
        if (PlayerData.playerdata.titleRef != null) {
            if (status == ChatUserStatus.Online) {
                PlayerData.playerdata.titleRef.friendsMessenger.UpdateStatusForUsername(user, true, message.ToString());
            } else if (status == ChatUserStatus.Offline) {
                PlayerData.playerdata.titleRef.friendsMessenger.UpdateStatusForUsername(user, false, "OFFLINE");
            }
        }
	}

    public void OnPrivateMessage(string sender, object message, string channelName)
	{
        Debug.LogFormat( "OnPrivateMessage: {0} ({1}) > {2}", channelName, sender, message );

        string sMessage = message.ToString();
        // Only proceed with requests if on title and sender is a verified friend
        if (PlayerData.playerdata.titleRef != null) {
            if (PlayerData.playerdata.titleRef.friendsMessenger.CheckIsVerifiedFriendByUsername(sender)) {
                // If it was a request to join my game, send back the room name to join if I'm in one
                if (PhotonNetwork.InRoom && sMessage == ROOM_REQUEST_MSG) {
                    chatClient.SendPrivateMessage(sender, ROOM_JOIN_MSG + PhotonNetwork.CurrentRoom.Name);
                } else {
                    // If it was a join code, then join that room
                    if (sMessage.Substring(0, 5) == ROOM_JOIN_MSG) {
                        PlayerData.playerdata.titleRef.WarpJoinGame(sMessage.Substring(5, sMessage.Length - 5));
                    } else {
                        string chattingWithUsername = PlayerData.playerdata.friendsList[PlayerData.playerdata.titleRef.friendsMessenger.GetChattingWithFriendRequestId()].FriendUsername;
                        if (chattingWithUsername == sender) {
                            PlayerData.playerdata.titleRef.friendsMessenger.SendMsg(false, message.ToString(), sender);
                        } else {
                            string thisFriendRequestId = PlayerData.playerdata.titleRef.friendsMessenger.GetFriendRequestIdByUsername(sender);
                            PlayerData.playerdata.titleRef.friendsMessenger.GetMessengerEntry(thisFriendRequestId).ToggleNotification(true);
                            PlayerData.playerdata.titleRef.friendsMessenger.ToggleNotification(true);
                        }
                    }
                }
            }
        }
	}

    public List<object> GetCachedMessagesForUser(string username)
    {
        string channelName = chatClient.GetPrivateChannelNameByUser(username);
        return chatClient.PrivateChannels[channelName].Messages;
    }

    public int GetMessageCountForUser(string username)
    {
        string channelName = chatClient.GetPrivateChannelNameByUser(username);
        return chatClient.PrivateChannels[channelName].MessageCount;
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
	{
        if (PlayerData.playerdata.titleRef != null) {
            if (channelName.Equals("Campaign")) {
                for (int i = 0; i < senders.Length; i++) {
                    PlayerData.playerdata.titleRef.chatManagerCamp.PostMessage(true, senders[i], messages[i].ToString());
                }
            } else if (channelName.Equals("Versus")) {
                for (int i = 0; i < senders.Length; i++) {
                    PlayerData.playerdata.titleRef.chatManagerVersus.PostMessage(true, senders[i], messages[i].ToString());
                }
            }
        }
	}

    public void OnSubscribed(string[] channels, bool[] results)
	{
		Debug.Log("OnSubscribed: " + string.Join(", ", channels));
	}

    public void OnUnsubscribed(string[] channels)
	{
		Debug.Log("OnUnsubscribed: " + string.Join(", ", channels));
        if (PlayerData.playerdata.titleRef != null) {
            PlayerData.playerdata.titleRef.chatManagerCamp.ClearChat();
            PlayerData.playerdata.titleRef.chatManagerVersus.ClearChat();
        }
	}

    public void OnDisconnected()
	{
	    Debug.Log("Successfully disconnected from Photon Chat.");
	}

    public void OnChatStateChange(ChatState state)
	{
		// TODO: Fill out later
	}

    public void DebugReturn(ExitGames.Client.Photon.DebugLevel level, string message)
	{
		if (level == ExitGames.Client.Photon.DebugLevel.ERROR)
		{
			Debug.LogError(message);
		}
		else if (level == ExitGames.Client.Photon.DebugLevel.WARNING)
		{
			Debug.LogWarning(message);
		}
		else
		{
			Debug.Log(message);
		}
    }

    public bool SendGlobalMessage(char modeLobby, string message) {
        if (messagesSentThisMin < MESSAGES_PER_MIN_LIMIT) {
            messagesSentThisMin++;
        } else {
            return false;
        }
        if (modeLobby == 'C') {
            return this.chatClient.PublishMessage("Campaign", message);
        } else if (modeLobby == 'V') {
            return this.chatClient.PublishMessage("Versus", message);
        }
        return false;
    }

    /// <summary>To avoid that the Editor becomes unresponsive, disconnect all Photon connections in OnDestroy.</summary>
    public void OnDestroy()
    {
        if (this.chatClient != null)
        {
            this.chatClient.Disconnect();
        }
    }

    /// <summary>To avoid that the Editor becomes unresponsive, disconnect all Photon connections in OnApplicationQuit.</summary>
    public void OnApplicationQuit()
	{
		if (this.chatClient != null)
		{
			this.chatClient.Disconnect();
		}
	}
}
