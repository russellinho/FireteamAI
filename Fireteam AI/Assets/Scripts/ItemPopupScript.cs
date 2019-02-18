using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPopupScript : MonoBehaviour
{

    public Text title;
    public RawImage thumbnail;
    public Text description;

    // Update is called once per frame
    void Update()
    {
        // Follow mouse position
        if (gameObject.activeInHierarchy) {
            UpdatePosition();
        }
    }

    void UpdatePosition() {
        transform.position = Input.mousePosition;
    }

    public void SetTitle(string s) {
        title.text = s;
    }

    public void SetThumbnail(RawImage r) {
        thumbnail.texture = r.texture;
    }

    public void SetDescription(string s) {
        description.text = s;
    }

}
