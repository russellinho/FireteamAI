using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Chat;
using Photon.Pun;

public class GlobalChatClient : MonoBehaviour, IChatClientListener
{
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
		Debug.LogWarning("status: " + string.Format("{0} is {1}. Msg:{2}", user, status, message));
	}

    public void OnPrivateMessage(string sender, object message, string channelName)
	{
		// TODO: Fill out later
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
