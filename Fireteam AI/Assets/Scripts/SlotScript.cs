using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotScript : MonoBehaviour
{
    public RawImage thumbnailPic;

    public void ToggleThumbnail(bool b, string thumbnailPath) {
        if (b) {
            thumbnailPic.enabled = true;
            thumbnailPic.texture = (Texture)Resources.Load(thumbnailPath);
        } else {
            thumbnailPic.enabled = false;
            thumbnailPic.texture = null;
        }
    }
}
