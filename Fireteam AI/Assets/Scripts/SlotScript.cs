using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotScript : MonoBehaviour
{
    public RawImage thumbnailPic;
    public TextMeshProUGUI thumbnailText;

    public void ToggleThumbnail(bool b, string thumbnailPath) {
        if (b) {
            thumbnailText.enabled = false;
            thumbnailPic.enabled = true;
            thumbnailPic.texture = (Texture)Resources.Load(thumbnailPath);
        } else {
            thumbnailPic.enabled = false;
            thumbnailPic.texture = null;
            thumbnailText.enabled = true;
        }
    }
}
