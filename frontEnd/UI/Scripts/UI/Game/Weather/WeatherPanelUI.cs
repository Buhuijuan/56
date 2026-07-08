using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeatherPanelUI : MonoBehaviour
{
    public Toggle sunnyToggle;
    public Toggle cloudyToggle;
    public Toggle snowyToggle;
    public Toggle rainyToggle;
    public Toggle foggyToggle;

    public Dictionary<WeatherController.WeatherType, Toggle> weathermap;

    private WeatherController controller;

    private WeatherController.WeatherType pendingType;

    void Awake()
    {
        weathermap = new Dictionary<WeatherController.WeatherType, Toggle>
        {
            {WeatherController.WeatherType.Sunny, sunnyToggle},
            {WeatherController.WeatherType.Cloudy, cloudyToggle},
            {WeatherController.WeatherType.Snowy, snowyToggle},
            {WeatherController.WeatherType.Rainy, rainyToggle},
            {WeatherController.WeatherType.Foggy, foggyToggle},
        };

        foreach (var kv in weathermap)
        {
            kv.Value.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    pendingType = kv.Key;
                }
            });
        }

        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (!EnsureController()) return;
        pendingType = controller.CurrentType;

        foreach (var kv in weathermap)
        {
            kv.Value.SetIsOnWithoutNotify(kv.Key == controller.CurrentType);
        }
    }

    public void OnClickApply()
    {
        if (!EnsureController()) return;

        controller.ApplyWeather(pendingType);
        DynamicGI.UpdateEnvironment();

        gameObject.SetActive(false);
    }

    public void OnClickCancel()
    {
        gameObject.SetActive(false);
    }

    private bool EnsureController()
    {
        if (controller != null) return true;

        controller = FindObjectOfType<WeatherController>(true);
        return controller != null;
    }
}
