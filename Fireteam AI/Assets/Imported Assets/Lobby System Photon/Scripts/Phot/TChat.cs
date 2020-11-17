using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Realtime;
using TMPro;

namespace Photon.Pun.LobbySystemPhoton
{
	public class TChat : MonoBehaviour
	{

		public ScrollRect myScrollRect;
		public TMP_InputField TextSend;
		public TMP_Text TextChat;
		public GameObject TextSendObj;
		// public GameObject FrameSmiley;
		[SerializeField]
		public string[] emojis;
		public bool isSelect = false;
		public PhotonView photonView;

		public void Awake()
		{
			photonView = GetComponent<PhotonView>();
		}

		// Update is called once per frame
		void LateUpdate()
		{
			if (Input.GetKeyDown(KeyCode.Return) && isSelect && TextSend.text.Length > 0)
			{
				string msg = TextSend.text;
				sendChatOfMaster(msg);
				TextSend.text = "";
				ToggleInputFieldSelected(true);
			}
			else if (Input.GetKeyDown(KeyCode.Return) && isSelect && TextSend.text.Length == 0)
			{
				//TextSendObj.SetActive (false);
				ToggleInputFieldSelected(false);
				TextSend.text = "";
			}
			// else if (!isSelect && TextSend.text.Length > 0)
			// {
			// 	isSelect = true;
			// 	EventSystem.current.SetSelectedGameObject(TextSend.gameObject, null);
			// }
		}

		public void sendChatOfMaster(string msg)
		{
			if (msg != "")
			{
				bool isMaster = false;
				if (PhotonNetwork.IsMasterClient)
				{
					isMaster = true;
				}
				// photonView.RPC("sendChatMaster", RpcTarget.MasterClient, isMaster, msg, PhotonNetwork.LocalPlayer.NickName);
				photonView.RPC("SendMsg", RpcTarget.All, isMaster, msg, PhotonNetwork.LocalPlayer.NickName);
				TextSend.text = "";
			}
		}

		public void sendChatOfMasterViaBtn()
		{
			string msg = TextSend.text;
			sendChatOfMaster(msg);
		}

		void ToggleInputFieldSelected(bool b) {
			if (b) {
				EventSystem.current.SetSelectedGameObject(TextSend.gameObject, null);
				TextSend.ActivateInputField();
			} else {
				EventSystem.current.SetSelectedGameObject(null);
			}
			isSelect = b;
		}

		// public void ShowSmileys()
		// {
		// 	if (FrameSmiley.activeSelf)
		// 	{
		// 		FrameSmiley.SetActive(false);
		// 	}
		// 	else
		// 	{
		// 		FrameSmiley.SetActive(true);
		// 	}
		// }

		// public void AddSmiley(int idSmiley)
		// {
		// 	TextSend.text += " "+emojis[idSmiley];
		// 	FrameSmiley.SetActive(false);
		// }


		[PunRPC]
		public void sendChatMaster(bool master, string msg, string pse)
		{
			if (PhotonNetwork.IsMasterClient)
			{
				photonView.RPC("SendMsg", RpcTarget.All, master, msg, pse);
			}
		}

		[PunRPC]
		public void SendMsg(bool master, string msg, string pse)
		{
			// for (int i = 0; i < emojis.Length; i++)
			// {
			// 	msg = msg.Replace(emojis[i], " <size=150%><sprite="+i+"><size=100%>");
			// }

			if (master)
			{
				TextChat.text += "<color=#63c6ffff>" + pse + " : </color><color=#ffffffff>" + msg + "</color>\n";
			}
			else
			{
				TextChat.text += "<color=#f2f2f2ff>" + pse + " : </color><color=#ffffffff>" + msg + "</color>\n";
			}
			myScrollRect.verticalNormalizedPosition = 0f;
		}

		public void OnPhotonPlayerConnected(Player player)
		{
			SendMsgConnectionMaster(player.NickName);
		}

		public void SendServerMessage(string message)
		{
			// photonView.RPC("SendMsgConnectionMaster", RpcTarget.MasterClient, message);
			photonView.RPC("SendMsgConnectionAll", RpcTarget.All, message);
		}

		[PunRPC]
		public void SendMsgConnectionMaster(string message)
		{
			if (PhotonNetwork.IsMasterClient)
			{
				photonView.RPC("SendMsgConnectionAll", RpcTarget.All, message);
			}
		}

		[PunRPC]
		public void SendMsgConnectionAll(string message)
		{
			TextChat.text += "<color=#ffa500ff><b>SERVER: </b>" + message + "</color>\n";
		}

		public void SelectInputByClick()
		{
			if (!isSelect)
			{
				isSelect = true;
			}
		}

		public void DeselectInputByClick() {
			if (isSelect) {
				isSelect = false;
			}
		}
	}
}