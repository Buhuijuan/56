using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettingUI : MonoBehaviour
{
    public AudioManager.AudioGroupType type;
    public Toggle toggle;
    public Slider slider;
    public TMP_Text slider_value;

    private void OnEnable()
    {
        RefreshInteractableState();
    }

    void Start()
    {
        var data = SettingSystem.Instance.data;

        switch (type)
        {
            case AudioManager.AudioGroupType.Music:
                toggle.isOn = data.musicEnabled;
                slider.value = data.musicVolume;
                break;

            case AudioManager.AudioGroupType.Effect:
                toggle.isOn = data.effectEnabled;
                slider.value = data.effectVolume;
                break;

            case AudioManager.AudioGroupType.Sound:
                toggle.isOn = data.soundEnabled;
                slider.value = data.soundVolume;
                break;
        }

        RefreshInteractableState();

        toggle.onValueChanged.AddListener(OnToggleValueCnanged);
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    public void OnToggleValueCnanged(bool isChecked)
    {
        RefreshInteractableState();

        var data = SettingSystem.Instance.data;

        switch (type)
        {
            case AudioManager.AudioGroupType.Music: data.musicEnabled = isChecked; break;
            case AudioManager.AudioGroupType.Effect: data.effectEnabled = isChecked; break;
            case AudioManager.AudioGroupType.Sound: data.soundEnabled = isChecked; break;
        }

        SettingSystem.Instance.Save();
        AudioManager.SetAudioEnabled(type, isChecked);
    }

    public void OnSliderValueChanged(float value)
    {
        int percent = Mathf.RoundToInt(value * 100f);
        slider_value.text = percent + "%";

        var data = SettingSystem.Instance.data;

        switch (type)
        {
            case AudioManager.AudioGroupType.Music: data.musicVolume = value; break;
            case AudioManager.AudioGroupType.Effect: data.effectVolume = value; break;
            case AudioManager.AudioGroupType.Sound: data.soundVolume = value; break;
        }

        SettingSystem.Instance.Save();
        AudioManager.SetAudio(type, value);
    }

    private void RefreshInteractableState()
    {
        if (slider != null && toggle != null)
            slider.interactable = toggle.isOn;
    }

}
