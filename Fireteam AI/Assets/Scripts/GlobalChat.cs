using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GlobalChat : MonoBehaviour
{

    public ScrollRect myScrollRect;
    public TMP_InputField TextSend;
    public TMP_Text TextChat;
    public GameObject TextSendObj;
    public bool isSelect = false;

    // Update is called once per frame
    // void LateUpdate()
    // {
    //     if (Input.GetKeyDown(KeyCode.Return) && isSelect && TextSend.text.Length > 0)
    //     {
    //         string msg = TextSend.text;
    //         sendChatOfMaster(msg);
    //         TextSend.text = "";
    //         ToggleInputFieldSelected(true);
    //     }
    //     else if (Input.GetKeyDown(KeyCode.Return) && isSelect && TextSend.text.Length == 0)
    //     {
    //         //TextSendObj.SetActive (false);
    //         ToggleInputFieldSelected(false);
    //         TextSend.text = "";
    //     }
    // }

    // public void sendChatOfMaster(string msg)
    // {
    //     if (msg != "")
    //     {
    //         bool isMaster = false;
    //         if (PhotonNetwork.IsMasterClient)
    //         {
    //             isMaster = true;
    //         }
    //         // photonView.RPC("sendChatMaster", RpcTarget.MasterClient, isMaster, msg, PhotonNetwork.LocalPlayer.NickName);
    //         photonView.RPC("SendMsg", RpcTarget.All, isMaster, msg, PhotonNetwork.LocalPlayer.NickName);
    //         TextSend.text = "";
    //     }
    // }

    // public void sendChatOfMasterViaBtn()
    // {
    //     string msg = TextSend.text;
    //     sendChatOfMaster(msg);
    // }

    // void ToggleInputFieldSelected(bool b) {
    //     if (b) {
    //         EventSystem.current.SetSelectedGameObject(TextSend.gameObject, null);
    //         TextSend.ActivateInputField();
    //     } else {
    //         EventSystem.current.SetSelectedGameObject(null);
    //     }
    //     isSelect = b;
    // }

    // public void sendChatMaster(bool master, string msg, string pse)
    // {
    //     if (PhotonNetwork.IsMasterClient)
    //     {
    //         photonView.RPC("SendMsg", RpcTarget.All, master, msg, pse);
    //     }
    // }

    // public void SendMsg(bool master, string msg, string pse)
    // {
    //     if (master)
    //     {
    //         TextChat.text += "<color=#63c6ffff>" + pse + " : </color><color=#ffffffff>" + msg + "</color>\n";
    //     }
    //     else
    //     {
    //         TextChat.text += "<color=#f2f2f2ff>" + pse + " : </color><color=#ffffffff>" + msg + "</color>\n";
    //     }
    //     myScrollRect.verticalNormalizedPosition = 0f;
    // }

    // // public void OnPhotonPlayerConnected(Player player)
    // // {
    // //     SendMsgConnectionMaster(player.NickName);
    // // }

    // public void SendServerMessage(string message)
    // {
    //     // photonView.RPC("SendMsgConnectionMaster", RpcTarget.MasterClient, message);
    //     photonView.RPC("SendMsgConnectionAll", RpcTarget.All, message);
    // }

    // public void SendMsgConnectionMaster(string message)
    // {
    //     if (PhotonNetwork.IsMasterClient)
    //     {
    //         photonView.RPC("SendMsgConnectionAll", RpcTarget.All, message);
    //     }
    // }

    // public void SendMsgConnectionAll(string message)
    // {
    //     TextChat.text += "<color=#ffa500ff><b>SERVER: </b>" + message + "</color>\n";
    // }

    // public void SelectInputByClick()
    // {
    //     if (!isSelect)
    //     {
    //         isSelect = true;
    //     }
    // }

    // public void DeselectInputByClick() {
    //     if (isSelect) {
    //         isSelect = false;
    //     }
    // }
}
