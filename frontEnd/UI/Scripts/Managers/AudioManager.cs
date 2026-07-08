using UnityEngine;
using UnityEngine.Audio;

public static class AudioManager
{
    public enum AudioGroupType
    {
        Music,
        Effect,
        Sound
    }

    public static AudioMixer mixer;

    public static void Init(AudioMixer m)
    {
        mixer = m;
        ApplyAllSettings();
    }

    public static void ApplyAllSettings()
    {
        var data = SettingSystem.Instance.data;

        SetAudio(AudioGroupType.Music, data.musicVolume);
        SetAudioEnabled(AudioGroupType.Music, data.musicEnabled);

        SetAudio(AudioGroupType.Effect, data.effectVolume);
        SetAudioEnabled(AudioGroupType.Effect, data.effectEnabled);

        SetAudio(AudioGroupType.Sound, data.soundVolume);
        SetAudioEnabled(AudioGroupType.Sound, data.soundEnabled);
    }

    public static void SetAudioEnabled(AudioGroupType type, bool enabled)
    {
        if (mixer == null)
            return;

        string param = type + "Volume";

        if (enabled)
            mixer.SetFloat(param, Mathf.Log10(GetVolume(type)) * 20);
        else
            mixer.SetFloat(param, -80f);
    }

    public static void SetAudio(AudioGroupType type, float value)
    {
        if (mixer == null)
            return;

        string param = type + "Volume";

        var data = SettingSystem.Instance.data;
        switch (type)
        {
            case AudioGroupType.Music:
                data.musicVolume = value;
                break;
            case AudioGroupType.Effect:
                data.effectVolume = value;
                break;
            case AudioGroupType.Sound:
                data.soundVolume = value;
                break;
        }

        SettingSystem.Instance.Save();

        if (!IsEnabled(type))
        {
            mixer.SetFloat(param, -80f);
            return;
        }

        float v = Mathf.Clamp(value, 0.0001f, 1f);
        mixer.SetFloat(param, Mathf.Log10(v) * 20);
    }

    private static float GetVolume(AudioGroupType type)
    {
        var data = SettingSystem.Instance.data;
        return type switch
        {
            AudioGroupType.Music => data.musicVolume,
            AudioGroupType.Effect => data.effectVolume,
            AudioGroupType.Sound => data.soundVolume,
            _ => 1f
        };
    }

    private static bool IsEnabled(AudioGroupType type)
    {
        var data = SettingSystem.Instance.data;
        return type switch
        {
            AudioGroupType.Music => data.musicEnabled,
            AudioGroupType.Effect => data.effectEnabled,
            AudioGroupType.Sound => data.soundEnabled,
            _ => true
        };
    }
}
