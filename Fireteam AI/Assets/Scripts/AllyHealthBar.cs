using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AllyHealthBar : MonoBehaviour
{
    public int viewId;
    public Slider healthSlider;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI nametag;

    void Awake()
    {
        ResetData();
    }

    public void InitData(int viewId, string nametag, int health)
    {
        this.viewId = viewId;
        this.nametag.text = nametag;
        this.healthSlider.value = (float)health / 100f;
    }

    public void ResetData()
    {
        viewId = -1;
        gameObject.SetActive(false);
    }

    public void SetHealth(int health)
    {
        this.healthSlider.value = (float)health / 100f;
    }
}
