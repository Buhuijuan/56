using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrowthTaskUI : MonoBehaviour
{
    public Toggle toggle;
    public Text description;
    private GrowthTask data;
    public void Setup(GrowthTask data, bool isCompleted)
    {
        this.data = data;
        toggle.interactable = false;
        toggle.isOn = isCompleted;
        description.text = data.description;
    }
}
