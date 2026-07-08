using UnityEngine;
using UnityEngine.SceneManagement;

public static class PictureManager
{
    private static FullscreenEffect[] effects;
    public static void Init()
    {
        ApplyAllSettings();
    }

    static PictureManager()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Refresh();
        ApplyAllSettings();
    }

    public static void Refresh()
    {
        effects = Object.FindObjectsOfType<FullscreenEffect>(true);
    }

    private static void ApplyAllSettings()
    {
        var data = SettingSystem.Instance.data;

        SetQuality(data.quality);
        SetFrameRate(data.frameRate);
        SetColorBlindMode(data.colorBlindMode);
    }

    public static void SetQuality(QualityLevel level)
    {
        QualitySettings.SetQualityLevel((int)level);
    }

    public static void SetFrameRate(FrameRateLevel level)
    {
        QualitySettings.vSyncCount = 0;
        switch (level)
        {
            case FrameRateLevel.Stable: Application.targetFrameRate = 30; break;
            case FrameRateLevel.Smooth: Application.targetFrameRate = 60; break;
            case FrameRateLevel.Extreme: Application.targetFrameRate = 120; break;
        }
    }

    public static void SetColorBlindMode(bool enabled)
    {
        if (effects == null || effects.Length == 0)
            Refresh();

        foreach (var e in effects)
        {
            if (e != null)
                e.colorBlind = enabled ? 1 : 0;
        }
    }
}
