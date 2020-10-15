using UnityEngine;
using TMPro;

namespace Michsky.UI.Shift
{
    public class ModalWindowManager : MonoBehaviour
    {
        public TitleControllerScript titleController;
        [Header("RESOURCES")]
        public TextMeshProUGUI windowTitle;
        public TextMeshProUGUI windowDescription;

        [Header("SETTINGS")]
        public bool sharpAnimations = false;
        public bool useCustomTexts = false;
        public string titleText = "Title";
        [TextArea] public string descriptionText = "Description here";

        Animator mWindowAnimator;
        bool isOn = false;
        bool hidPlayer;

        void Start()
        {
            mWindowAnimator = gameObject.GetComponent<Animator>();

            if (useCustomTexts == false)
            {
                windowTitle.text = titleText;
                windowDescription.text = descriptionText;
            }
        }

        public void ModalWindowIn()
        {
            if (titleController != null) {
                if (PlayerData.playerdata.bodyReference != null && PlayerData.playerdata.bodyReference.activeInHierarchy) {
                    hidPlayer = true;
                    titleController.TogglePlayerBody(false);
                }
            }
            if (isOn == false)
            {
                if (sharpAnimations == false)
                    mWindowAnimator.CrossFade("Window In", 0.1f);
                else
                    mWindowAnimator.Play("Window In");

                isOn = true;
            }
        }

        public void ModalWindowOut()
        {
            if (titleController != null) {
                if (PlayerData.playerdata.bodyReference != null && hidPlayer) {
                    hidPlayer = false;
                    titleController.TogglePlayerBody(true);
                }
            }
            if (isOn == true)
            {
                if (sharpAnimations == false)
                    mWindowAnimator.CrossFade("Window Out", 0.1f);
                else
                    mWindowAnimator.Play("Window Out");

                isOn = false;
            }
        }

        public bool GetIsOn() {
            return isOn;
        }

        public void SetKeyBindingDescriptionText(string s) {
            windowDescription.text = "PRESS A KEY TO RE-MAP [" + s + "]";
        }
    }
}