using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PicturePageUI : PageUI
{
    public Toggle lowQuality, mediumQuality, highQuality;
    public Toggle stableFrame, smoothFrame, extremeFrame;
    public Slider brightnessSlider;
    public Toggle colorMode;
    private Dictionary<QualityLevel, Toggle> quality = new();
    private Dictionary<FrameRateLevel, Toggle> frameRate = new();
    void Start()
    {
        quality = new()
        {
            {QualityLevel.Low,lowQuality},
            {QualityLevel.Medium,mediumQuality},
            {QualityLevel.High,highQuality}
        };
        frameRate = new()
        {
          {FrameRateLevel.Stable,stableFrame},
          {FrameRateLevel.Smooth,smoothFrame},
          {FrameRateLevel.Extreme,extremeFrame}
        };
        LoadUI();
        AddListeners();
    }
    void LoadUI()
    {
        var data = SettingSystem.Instance.data;
        foreach (var kv in quality)
            kv.Value.isOn = kv.Key == data.quality;
        foreach (var kv in frameRate)
            kv.Value.isOn = kv.Key == data.frameRate;

        brightnessSlider.value = data.brightness;

        colorMode.isOn = data.colorBlindMode;
    }

    void AddListeners()
    {
        foreach (var kv in quality)
        {
            var key = kv.Key;
            kv.Value.onValueChanged.AddListener(isSelected =>
            {
                if (isSelected) SetQuality(key);
            });
        }

        foreach (var kv in frameRate)
        {
            var key = kv.Key;
            kv.Value.onValueChanged.AddListener(isSelected =>
            {
                if (isSelected) SetFrameRate(key);
            });
        }

        colorMode.onValueChanged.AddListener(SetColorBlindMode);
    }

    void SetQuality(QualityLevel level)
    {
        SettingSystem.Instance.data.quality = level;
        SettingSystem.Instance.Save();
        PictureManager.SetQuality(level);
    }

    void SetFrameRate(FrameRateLevel level)
    {
        SettingSystem.Instance.data.frameRate = level;
        SettingSystem.Instance.Save();
        PictureManager.SetFrameRate(level);
    }

    void SetColorBlindMode(bool value)
    {
        SettingSystem.Instance.data.colorBlindMode = value;
        SettingSystem.Instance.Save();
        PictureManager.SetColorBlindMode(value);
    }
}
