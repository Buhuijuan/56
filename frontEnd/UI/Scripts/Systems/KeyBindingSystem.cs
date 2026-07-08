using System.Collections.Generic;
using UnityEngine;

public static class KeyBindingSystem
{
    public const float MinScale = 0.8f;
    public const float MaxScale = 2.0f;
    public const float DefaultScale = 1.0f;

    public static Dictionary<string, Vector2> normalizedPositions = new();
    public static Dictionary<string, Vector2> defaultNormalizedPositions = new();
    public static Dictionary<string, float> normalizedScales = new();
    public static Dictionary<string, float> defaultNormalizedScales = new();

    private static bool initialized = false;

    public static void InitializeDefaultsIfNeeded(Dictionary<string, Vector2> initialDefaults)
    {
        if (initialized) return;

        defaultNormalizedPositions = new Dictionary<string, Vector2>(initialDefaults);
        normalizedPositions = new Dictionary<string, Vector2>(initialDefaults);
        defaultNormalizedScales = new Dictionary<string, float>();
        normalizedScales = new Dictionary<string, float>();

        foreach (var kv in initialDefaults)
        {
            defaultNormalizedScales[kv.Key] = DefaultScale;
            normalizedScales[kv.Key] = DefaultScale;
        }

        Load();
        initialized = true;
    }


    public static void ResetToDefault()
    {
        normalizedPositions = new Dictionary<string, Vector2>(defaultNormalizedPositions);
        normalizedScales = new Dictionary<string, float>(defaultNormalizedScales);
        Save();
    }

    public static float GetScale(string keyId)
    {
        if (string.IsNullOrWhiteSpace(keyId))
            return DefaultScale;

        if (!normalizedScales.TryGetValue(keyId, out float scale))
            return DefaultScale;

        return Mathf.Clamp(scale, MinScale, MaxScale);
    }

    public static void Save()
    {
        foreach (var kv in normalizedPositions)
        {
            PlayerPrefs.SetFloat($"{kv.Key}_nx", kv.Value.x);
            PlayerPrefs.SetFloat($"{kv.Key}_ny", kv.Value.y);
        }

        foreach (var kv in normalizedScales)
            PlayerPrefs.SetFloat($"{kv.Key}_scale", Mathf.Clamp(kv.Value, MinScale, MaxScale));
    }

    public static void Load()
    {
        foreach (var kv in defaultNormalizedPositions)
        {
            float x = PlayerPrefs.GetFloat($"{kv.Key}_nx", kv.Value.x);
            float y = PlayerPrefs.GetFloat($"{kv.Key}_ny", kv.Value.y);
            normalizedPositions[kv.Key] = new Vector2(x, y);

            float defaultScale = defaultNormalizedScales.TryGetValue(kv.Key, out float scale) ? scale : DefaultScale;
            float loadedScale = PlayerPrefs.GetFloat($"{kv.Key}_scale", defaultScale);
            normalizedScales[kv.Key] = Mathf.Clamp(loadedScale, MinScale, MaxScale);
        }
    }
}
