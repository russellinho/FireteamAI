using UnityEngine;
using TMPro;

namespace Michsky.UI.Shift
{
    public class ModalWindowManager : MonoBehaviour
    {
        public TitleControllerScript titleController;
        public MainPanelManager mainPanelManager;
        [Header("RESOURCES")]
        public TextMeshProUGUI windowTitle;
        public TextMeshProUGUI windowDescription;

        [Header("SETTINGS")]
        public bool sharpAnimations = false;
        public bool useCustomTexts = false;
        public string titleText = "Title";
        [TextArea] public string descriptionText = "Description here";

        public Animator mWindowAnimator;
        public bool isOn = false;

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
                if (mainPanelManager.currentPanelIndex == MainPanelManager.MOD_SHOP_INDEX) {
                    titleController.ToggleWeaponPreview(false);
                } else {
                    if (PlayerData.playerdata.bodyReference != null && PlayerData.playerdata.bodyReference.activeInHierarchy) {
                        titleController.HideAll(false);
                    }
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
                if (mainPanelManager.currentPanelIndex == MainPanelManager.MOD_SHOP_INDEX) {
                    titleController.ToggleWeaponPreview(true);
                } else {
                    if (PlayerData.playerdata.bodyReference != null && !titleController.confirmingSale && (this == titleController.addFriendPopup ? true : !titleController.addFriendPopup.isOn) && !titleController.confirmingTransaction && !titleController.confirmingGift && mainPanelManager.CurrentPanelAllowsPreviews()) {
                        titleController.HideAll(true);
                    }
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
            SetText("PRESS A KEY TO RE-MAP [" + s + "]");
        }

        public void SetText(string s) {
            windowDescription.text = s;
        }
    }
}