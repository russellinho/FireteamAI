using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Michsky.UI.Shift;

public class AchievementButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Color defaultColor = new Color(45f / 255f, 45f / 255f, 45f / 255f);
    private Color completedColor = new Color(45f / 255f, 128f / 255f, 45f / 255f);
    public TextMeshProUGUI achievementLabel;
    public Image achievementImageHolder;
    public Image defaultImageHolder;
    public Image defaultBackgroundHolder;
    public TextMeshProUGUI detailTitle;
    public TextMeshProUGUI detailDescription;
    public TextMeshProUGUI completionTxt;
    public Image backgroundColor;
    public Sprite achievementSprite;
    public string title;
    [TextArea] public string description;

    public void PopulateAchievement(string achievementId, string achievementName, params int[] quantities)
    {
        achievementLabel.text = achievementName;
        switch(achievementId) {
            case "00":
                if (quantities[0] == 1000) {
                    completionTxt.text = "COMPLETE";
                    backgroundColor.color = completedColor;
                } else {
                    completionTxt.text = quantities[0] + "/" + 1000;
                    backgroundColor.color = defaultColor;
                }
                break;
            case "01":
                if (quantities[0] == 1000) {
                    completionTxt.text = "COMPLETE";
                    backgroundColor.color = completedColor;
                } else {
                    completionTxt.text = quantities[0] + "/" + 1000;
                    backgroundColor.color = defaultColor;
                }
                break;
            case "02":
                if (quantities[0] == 1) {
                    completionTxt.text = "COMPLETE";
                    backgroundColor.color = completedColor;
                } else {
                    completionTxt.text = "INCOMPLETE";
                    backgroundColor.color = defaultColor;
                }
                break;
            case "03":
                if (quantities[0] == 1) {
                    completionTxt.text = "COMPLETE";
                    backgroundColor.color = completedColor;
                } else {
                    completionTxt.text = "INCOMPLETE";
                    backgroundColor.color = defaultColor;
                }
                break;
            case "04":
                if (quantities[0] == 1) {
                    completionTxt.text = "COMPLETE";
                    backgroundColor.color = completedColor;
                } else {
                    completionTxt.text = "INCOMPLETE";
                    backgroundColor.color = defaultColor;
                }
                break;
            case "05":
                if (quantities[0] == 1) {
                    completionTxt.text = "COMPLETE";
                    backgroundColor.color = completedColor;
                } else {
                    completionTxt.text = "INCOMPLETE";
                    backgroundColor.color = defaultColor;
                }
                break;
            case "06":
                if (quantities[0] == 1) {
                    completionTxt.text = "COMPLETE";
                    backgroundColor.color = completedColor;
                } else {
                    completionTxt.text = "INCOMPLETE";
                    backgroundColor.color = defaultColor;
                }
                break;
            case "07":
                if (quantities[0] == 1) {
                    completionTxt.text = "COMPLETE";
                    backgroundColor.color = completedColor;
                } else {
                    completionTxt.text = "INCOMPLETE";
                    backgroundColor.color = defaultColor;
                }
                break;
            case "08":
                if (quantities[0] == 1) {
                    completionTxt.text = "COMPLETE";
                    backgroundColor.color = completedColor;
                } else {
                    completionTxt.text = "INCOMPLETE";
                    backgroundColor.color = defaultColor;
                }
                break;
            case "09":
                if (quantities[0] == 1) {
                    completionTxt.text = "COMPLETE";
                    backgroundColor.color = completedColor;
                } else {
                    completionTxt.text = "INCOMPLETE";
                    backgroundColor.color = defaultColor;
                }
                break;
            default:
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        detailTitle.text = "Achievements";
        detailDescription.text = "Hover over an achievement to see your progress on them. Completed achievements will be highlighted green.";
        achievementImageHolder.gameObject.SetActive(false);
        defaultImageHolder.gameObject.SetActive(true);
        defaultBackgroundHolder.gameObject.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        detailTitle.text = title;
        detailDescription.text = description;
        defaultImageHolder.gameObject.SetActive(false);
        defaultBackgroundHolder.gameObject.SetActive(false);
        achievementImageHolder.sprite = achievementSprite;
        achievementImageHolder.gameObject.SetActive(true);
    }
}
