using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GlobalChat : MonoBehaviour
{
    public char lobbyType;
    public ScrollRect myScrollRect;
    public TMP_InputField TextSend;
    public TMP_Text TextChat;
    public GameObject TextSendObj;
    public bool isSelect = false;

    public void ClearChat()
    {
        TextChat.text = "";
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Return) && isSelect && TextSend.text.Length > 0)
        {
            string msg = TextSend.text;
            SendGlobalMessage(msg);
            TextSend.text = "";
            ToggleInputFieldSelected(true);
        }
        else if (Input.GetKeyDown(KeyCode.Return) && isSelect && TextSend.text.Length == 0)
        {
            //TextSendObj.SetActive (false);
            ToggleInputFieldSelected(false);
            TextSend.text = "";
        }
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

    public void PostMessage(bool master, string sender, string msg)
    {
        if (master)
        {
            TextChat.text += "<color=#63c6ffff>" + sender + " : </color><color=#ffffffff>" + msg + "</color>\n";
        }
        else
        {
            TextChat.text += "<color=#9c4141ff>" + sender + " : </color><color=#ffffffff>" + msg + "</color>\n";
        }
        myScrollRect.verticalNormalizedPosition = 0f;
    }

    void SendGlobalMessage(string msg) {
        PlayerData.playerdata.globalChatClient.SendGlobalMessage(lobbyType, msg);
    }

    public void OnPostMessageClicked()
    {
        string msg = TextSend.text;
        SendGlobalMessage(msg);
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
