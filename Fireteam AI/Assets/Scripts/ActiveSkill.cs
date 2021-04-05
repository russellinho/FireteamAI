using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveSkill : MonoBehaviour
{
    public Image durationOverlay;
    private float duration;
    private float timeElapsed;

    public void Initialize(float t)
    {
        duration = t;
        timeElapsed = 0f;
    }

    void Update()
    {
        if (duration > 0f) {
            timeElapsed += Time.deltaTime;
            UpdateTimeElapsed();
        }
    }

    void UpdateTimeElapsed()
    {
        RectTransform r = durationOverlay.rectTransform;
        r.sizeDelta = new Vector2(r.sizeDelta.x, 25f * (timeElapsed / duration));
    }
}
