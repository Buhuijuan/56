using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleToggle : MonoBehaviour
{
    public Toggle toggle;
    public TMP_Text titleName;
    private TitleData data;
    private TitlePageUI parent;

    public void Setup(TitleData data, TitlePageUI parent)
    {
        this.data = data;
        this.parent = parent;
        titleName.text = data.titleName;

        toggle.interactable = TitleSystem.IsTitleUnlocked(data.titleID);
        toggle.isOn = false;
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(OnTitleToggleValueChanged);
    }

    public void OnTitleToggleValueChanged(bool isSelected)
    {
        if (isSelected)
            parent.SelectTitle(data.titleID);
    }
    public TitleData GetToggleTitleData()
    {
        return data;
    }
}
