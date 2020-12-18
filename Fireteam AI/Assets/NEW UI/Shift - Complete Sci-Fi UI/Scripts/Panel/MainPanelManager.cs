using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Michsky.UI.Shift
{
    public class MainPanelManager : MonoBehaviour
    {
        public const int SETTINGS_INDEX = 4;
        public const int MOD_SHOP_INDEX = 3;
        public const int CAMPAIGN_INDEX = 5;
        public const int VERSUS_INDEX = 6;
        public const int MARKET_INDEX = 1;
        private const int LOADOUT_INDEX = 2;
        private const int HOME_INDEX = 0;
        public TitleControllerScript titleController;
        public string panelManagerType;
        [Header("PANEL LIST")]
        public List<PanelItem> panels = new List<PanelItem>();

        [Header("SETTINGS")]
        public int currentPanelIndex = 0;
        private int currentButtonIndex = 0;
        private bool refresh;
        public int newPanelIndex;

        private GameObject currentPanel;
        private GameObject nextPanel;
        private GameObject currentButton;
        private GameObject nextButton;

        private Animator currentPanelAnimator;
        private Animator nextPanelAnimator;
        private Animator currentButtonAnimator;
        private Animator nextButtonAnimator;
        public CanvasGroup topPanelGroup;
        public CanvasGroup bottomPanelGroup;
        public GameObject campaignLobby;
        public GameObject versusLobby;

        string panelFadeIn = "Panel In";
        string panelFadeOut = "Panel Out";
        string buttonFadeIn = "Normal to Pressed";
        string buttonFadeOut = "Pressed to Dissolve";
        string buttonFadeNormal = "Pressed to Normal";

        [System.Serializable]
        public class PanelItem
        {
            public string panelName;
            public GameObject panelObject;
            public GameObject buttonObject;
        }

        void Start()
        {
            currentButton = panels[currentPanelIndex].buttonObject;
            currentButtonAnimator = currentButton.GetComponent<Animator>();
            currentButtonAnimator.Play(buttonFadeIn);

            currentPanel = panels[currentPanelIndex].panelObject;
            currentPanelAnimator = currentPanel.GetComponent<Animator>();
            currentPanelAnimator.Play(panelFadeIn);
        }

        public void OpenFirstTab()
        {
            if (currentPanelIndex != 0)
            {
                currentPanel = panels[currentPanelIndex].panelObject;
                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                currentPanelAnimator.Play(panelFadeOut);

                currentButton = panels[currentPanelIndex].buttonObject;
                currentButtonAnimator = currentButton.GetComponent<Animator>();
                currentButtonAnimator.Play(buttonFadeNormal);

                currentPanelIndex = 0;
                currentButtonIndex = 0;

                currentPanel = panels[currentPanelIndex].panelObject;
                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                currentPanelAnimator.Play(panelFadeIn);

                currentButton = panels[currentButtonIndex].buttonObject;
                currentButtonAnimator = currentButton.GetComponent<Animator>();
                currentButtonAnimator.Play(buttonFadeIn);
            }

            else if (currentPanelIndex == 0)
            {
                currentPanel = panels[currentPanelIndex].panelObject;
                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                currentPanelAnimator.Play(panelFadeIn);

                currentButton = panels[currentButtonIndex].buttonObject;
                currentButtonAnimator = currentButton.GetComponent<Animator>();
                currentButtonAnimator.Play(buttonFadeIn);
            }
        }

        public void OpenPanel(string newPanel)
        {
            for (int i = 0; i < panels.Count; i++)
            {
                if (panels[i].panelName == newPanel)
                    newPanelIndex = i;
            }

            if (panelManagerType == "Top") {
                if (!refresh) {
                    if (currentPanelIndex == SETTINGS_INDEX) {
                        titleController.SaveSettings();
                        titleController.SaveKeyBindings();
                    } else if (currentPanelIndex == MOD_SHOP_INDEX) {
                        titleController.SaveModsForCurrentWeapon();
                    } else if (currentPanelIndex == CAMPAIGN_INDEX) {
                        titleController.ExitMatchmaking();
                    } else if (currentPanelIndex == VERSUS_INDEX) {
                        titleController.ExitMatchmaking();
                    } else if (currentPanelIndex == MARKET_INDEX) {
                        titleController.ClearPreview();
                    }
                }
                if (newPanel == "Settings") {
                    titleController.TogglePlayerTemplate(false);
                    titleController.ToggleWeaponPreview(false);
                    titleController.DestroyOldWeaponTemplate();
                    titleController.RefreshSavedAudioDevice();
                } else if (newPanel == "Mod Shop") {
                    titleController.TogglePlayerTemplate(false);
                    titleController.ToggleWeaponPreview(true);
                    titleController.HandleLeftSideButtonPress(titleController.modShopPrimaryWepBtn);
                    titleController.OnModShopPrimaryWepBtnClicked(true);
                    titleController.OpenModShopPrimaryWeaponTabs();
                    titleController.HandleRightSideButtonPress(titleController.modShopSuppressorsBtn);
                    titleController.OnSuppressorsBtnClicked();
                } else if (newPanel == "Market") {
                    titleController.TogglePlayerTemplate(true);
                    titleController.ToggleWeaponPreview(false);
                    titleController.DestroyOldWeaponTemplate();
                    titleController.HandleLeftSideButtonPress(titleController.shopPrimaryWepBtn);
                    titleController.OnMarketplacePrimaryWepBtnClicked();
                    titleController.OpenMarketplacePrimaryWeaponTabs();
                    titleController.HandleRightSideButtonPress(titleController.shopCharacterBtn);
                    titleController.OnMarketplaceCharacterBtnClicked();
                } else if (newPanel == "Loadout") {
                    titleController.TogglePlayerTemplate(true);
                    titleController.ToggleWeaponPreview(false);
                    titleController.DestroyOldWeaponTemplate();
                    titleController.HandleLeftSideButtonPress(titleController.primaryWepBtn);
                    titleController.OnPrimaryWepBtnClicked();
                    titleController.OpenPrimaryWeaponTabs();
                    titleController.HandleRightSideButtonPress(titleController.characterBtn);
                    titleController.OnCharacterBtnClicked();
                } else if (newPanel == "Campaign") {
                    titleController.TogglePlayerTemplate(false);
                    titleController.ToggleWeaponPreview(false);
                    titleController.JoinMatchmaking();
                    campaignLobby.SetActive(true);
                    versusLobby.SetActive(false);
                    VivoxVoiceManager.Instance.SetAudioInput(PlayerPreferences.playerPreferences.preferenceData.audioInputName);
                } else if (newPanel == "Versus") {
                    titleController.TogglePlayerTemplate(false);
                    titleController.ToggleWeaponPreview(false);
                    titleController.JoinMatchmaking();
                    campaignLobby.SetActive(false);
                    versusLobby.SetActive(true);
                    VivoxVoiceManager.Instance.SetAudioInput(PlayerPreferences.playerPreferences.preferenceData.audioInputName);
                } else {
                    titleController.TogglePlayerTemplate(true);
                    titleController.ToggleWeaponPreview(false);
                    titleController.DestroyOldWeaponTemplate();
                }
            }

            if (refresh || newPanelIndex != currentPanelIndex)
            {
                currentPanel = panels[currentPanelIndex].panelObject;
                currentPanelIndex = newPanelIndex;
                nextPanel = panels[currentPanelIndex].panelObject;

                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                nextPanelAnimator = nextPanel.GetComponent<Animator>();

                currentPanelAnimator.Play(panelFadeOut);
                nextPanelAnimator.Play(panelFadeIn);

                currentButton = panels[currentButtonIndex].buttonObject;
                currentButtonIndex = newPanelIndex;
                nextButton = panels[currentButtonIndex].buttonObject;

                currentButtonAnimator = currentButton.GetComponent<Animator>();
                nextButtonAnimator = nextButton.GetComponent<Animator>();

                currentButtonAnimator.Play(buttonFadeOut);
                nextButtonAnimator.Play(buttonFadeIn);
            }

            refresh = false;
        }

        public void ReopenCurrentPanel() {
            refresh = true;
            OpenPanel(panels[currentPanelIndex].panelName);
        }

        public void NextPage()
        {
            if (currentPanelIndex <= panels.Count - 2)
            {
                currentPanel = panels[currentPanelIndex].panelObject;
                currentButton = panels[currentButtonIndex].buttonObject;
                nextButton = panels[currentButtonIndex + 1].buttonObject;

                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                currentButtonAnimator = currentButton.GetComponent<Animator>();

                currentButtonAnimator.Play(buttonFadeNormal);
                currentPanelAnimator.Play(panelFadeOut);

                currentPanelIndex += 1;
                currentButtonIndex += 1;
                nextPanel = panels[currentPanelIndex].panelObject;

                nextPanelAnimator = nextPanel.GetComponent<Animator>();
                nextButtonAnimator = nextButton.GetComponent<Animator>();
                nextPanelAnimator.Play(panelFadeIn);
                nextButtonAnimator.Play(buttonFadeIn);
            }
        }

        public void PrevPage()
        {
            if (currentPanelIndex >= 1)
            {
                currentPanel = panels[currentPanelIndex].panelObject;
                currentButton = panels[currentButtonIndex].buttonObject;
                nextButton = panels[currentButtonIndex - 1].buttonObject;

                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                currentButtonAnimator = currentButton.GetComponent<Animator>();

                currentButtonAnimator.Play(buttonFadeNormal);
                currentPanelAnimator.Play(panelFadeOut);

                currentPanelIndex -= 1;
                currentButtonIndex -= 1;
                nextPanel = panels[currentPanelIndex].panelObject;

                nextPanelAnimator = nextPanel.GetComponent<Animator>();
                nextButtonAnimator = nextButton.GetComponent<Animator>();
                nextPanelAnimator.Play(panelFadeIn);
                nextButtonAnimator.Play(buttonFadeIn);
            }
        }

        public void ToggleTopBar(bool b) {
            if (b) {
                topPanelGroup.alpha = 1f;
                topPanelGroup.interactable = true;
                topPanelGroup.blocksRaycasts = true;
            } else {
                topPanelGroup.alpha = 0f;
                topPanelGroup.interactable = false;
                topPanelGroup.blocksRaycasts = false;
            }
        }

        public void ToggleBottomBar(bool b) {
            if (b) {
                bottomPanelGroup.alpha = 1f;
                bottomPanelGroup.interactable = true;
                bottomPanelGroup.blocksRaycasts = true;
            } else {
                bottomPanelGroup.alpha = 0f;
                bottomPanelGroup.interactable = false;
                bottomPanelGroup.blocksRaycasts = false;
            }
        }

        public bool CurrentPanelAllowsPreviews()
        {
            return currentPanelIndex == HOME_INDEX || currentPanelIndex == MARKET_INDEX || currentPanelIndex == LOADOUT_INDEX || currentPanelIndex == MOD_SHOP_INDEX;
        }
    }
}