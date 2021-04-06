using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveSkill : MonoBehaviour
{
    public RawImage skillImage;
    public Image durationOverlay;
    private float duration;
    private float timeElapsed;

    public void Initialize(Texture r, float t)
    {
        skillImage.texture = r;
        duration = t;
        timeElapsed = 0f;
    }

    void Update()
    {
        if (duration > 0f) {
            UpdateTimeElapsed();
        }
    }

    void UpdateTimeElapsed()
    {
        timeElapsed += Time.deltaTime;
        RectTransform r = durationOverlay.rectTransform;
        float t = timeElapsed / duration;
        r.sizeDelta = new Vector2(r.sizeDelta.x, 25f * t);
        if (t >= 1f) {
            ExpireSkill();
        }
    }

    public void ExpireSkill()
    {
        Destroy(gameObject);
    }

}
