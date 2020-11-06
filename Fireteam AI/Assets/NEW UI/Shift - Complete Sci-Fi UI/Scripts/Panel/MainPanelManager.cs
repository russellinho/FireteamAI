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
                        titleController.SaveAudioSettings();
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
                } else if (newPanel == "Versus") {
                    titleController.TogglePlayerTemplate(false);
                    titleController.ToggleWeaponPreview(false);
                    titleController.JoinMatchmaking();
                    campaignLobby.SetActive(false);
                    versusLobby.SetActive(true);
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
            Debug.Log("Reopning: " + panels[currentPanelIndex].panelName);
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
            topPanelGroup.alpha = (b ? 1f : 0f);
        }

        public void ToggleBottomBar(bool b) {
            bottomPanelGroup.alpha = (b ? 1f : 0f);
        }
    }
}