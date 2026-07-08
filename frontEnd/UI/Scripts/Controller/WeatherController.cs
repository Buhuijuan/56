using System.Collections;
using UnityEngine;

public class WeatherController : MonoBehaviour
{
    public enum WeatherType
    {
        Sunny,
        Cloudy,
        Snowy,
        Rainy,
        Foggy
    }

    [Header("当前天气")]
    public WeatherType CurrentType = WeatherType.Sunny;

    [Header("进入场景时强制设置为晴天")]
    public bool forceSunnyOnSceneEnter = true;

    [Header("启动前几帧强制清理天气特效，防止初始乱飘")]
    public int startupLockFrames = 3;

    [Header("雨天对象")]
    public GameObject rainRoot;
    public ParticleSystem rainNear;
    public ParticleSystem rainFar;
    public AudioSource rainAudio;

    [Header("雪天对象")]
    public GameObject snowRoot;
    public ParticleSystem snowNear;
    public ParticleSystem snowFar;
    public AudioSource snowAudio;

    [Header("场景主光")]
    public Light directionalLight;

    [Header("天空盒（可选）")]
    public Material sunnySkybox;
    public Material cloudySkybox;
    public Material rainySkybox;
    public Material snowySkybox;
    public Material foggySkybox;

    [Header("晴天参数")]
    public float sunnyLightIntensity = 1.0f;
    public Color sunnyLightColor = Color.white;

    [Header("阴天参数")]
    public float cloudyLightIntensity = 0.7f;
    public Color cloudyLightColor = new Color(0.82f, 0.82f, 0.86f);
    public Color cloudyFogColor = new Color(0.76f, 0.76f, 0.78f);
    public float cloudyFogDensity = 0.004f;

    [Header("雨天参数")]
    public float rainyLightIntensity = 0.4f;
    public Color rainyLightColor = new Color(0.68f, 0.72f, 0.78f);
    public bool rainyUseFog = true;
    public Color rainyFogColor = new Color(0.45f, 0.48f, 0.52f);
    public float rainyFogDensity = 0.012f;

    [Header("雪天参数")]
    public float snowyLightIntensity = 0.72f;
    public Color snowyLightColor = new Color(0.9f, 0.93f, 0.98f);
    public bool snowyUseFog = true;
    public Color snowyFogColor = new Color(0.82f, 0.86f, 0.90f);
    public float snowyFogDensity = 0.008f;

    [Header("雾天参数")]
    public float foggyLightIntensity = 0.65f;
    public Color foggyLightColor = new Color(0.78f, 0.78f, 0.78f);
    public Color foggyFogColor = new Color(0.70f, 0.70f, 0.70f);
    public float foggyFogDensity = 0.015f;

    [Header("Debug")]
    public bool verboseWeatherLogging = true;

    private bool defaultFogEnabled;
    private Color defaultFogColor;
    private float defaultFogDensity;
    private Material defaultSkybox;
    private float defaultLightIntensity;
    private Color defaultLightColor;

    private void Awake()
    {
        defaultFogEnabled = RenderSettings.fog;
        defaultFogColor = RenderSettings.fogColor;
        defaultFogDensity = RenderSettings.fogDensity;
        defaultSkybox = RenderSettings.skybox;

        if (directionalLight != null)
        {
            defaultLightIntensity = directionalLight.intensity;
            defaultLightColor = directionalLight.color;
        }

        if (forceSunnyOnSceneEnter)
        {
            CurrentType = WeatherType.Sunny;
        }

        ForceStopAllWeatherEffectsImmediate();
        LogWeatherBindings();
    }

    private IEnumerator Start()
    {
        ApplyWeather(CurrentType);

        for (int i = 0; i < startupLockFrames; i++)
        {
            yield return null;

            if (CurrentType != WeatherType.Rainy)
            {
                ForceStopRainImmediate();
            }

            if (CurrentType != WeatherType.Snowy)
            {
                ForceStopSnowImmediate();
            }
        }
    }

    public void ApplyWeather(WeatherType type)
    {
        CurrentType = type;
        if (verboseWeatherLogging)
        {
            Debug.Log($"[Weather] ApplyWeather => {type}, frame = {Time.frameCount}", this);
        }

        switch (type)
        {
            case WeatherType.Sunny:
                ApplySunny();
                break;

            case WeatherType.Cloudy:
                ApplyCloudy();
                break;

            case WeatherType.Snowy:
                ApplySnowy();
                break;

            case WeatherType.Rainy:
                ApplyRainy();
                break;

            case WeatherType.Foggy:
                ApplyFoggy();
                break;
        }
    }

    private void ApplySunny()
    {
        SetRainActive(false);
        SetSnowActive(false);

        SetDirectionalLight(sunnyLightIntensity, sunnyLightColor);
        SetSkyboxOrDefault(sunnySkybox);

        RenderSettings.fog = defaultFogEnabled;
        RenderSettings.fogColor = defaultFogColor;
        RenderSettings.fogDensity = defaultFogDensity;
    }

    private void ApplyCloudy()
    {
        SetRainActive(false);
        SetSnowActive(false);

        SetDirectionalLight(cloudyLightIntensity, cloudyLightColor);
        SetSkyboxOrDefault(cloudySkybox);

        RenderSettings.fog = true;
        RenderSettings.fogColor = cloudyFogColor;
        RenderSettings.fogDensity = cloudyFogDensity;
    }

    private void ApplyRainy()
    {
        SetSnowActive(false);
        SetRainActive(true);

        SetDirectionalLight(rainyLightIntensity, rainyLightColor);
        SetSkyboxOrDefault(rainySkybox);

        if (rainyUseFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = rainyFogColor;
            RenderSettings.fogDensity = rainyFogDensity;
        }
        else
        {
            RenderSettings.fog = false;
        }

    }

    private void ApplySnowy()
    {
        SetRainActive(false);
        SetSnowActive(true);

        SetDirectionalLight(snowyLightIntensity, snowyLightColor);
        SetSkyboxOrDefault(snowySkybox);

        if (snowyUseFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = snowyFogColor;
            RenderSettings.fogDensity = snowyFogDensity;
        }
        else
        {
            RenderSettings.fog = false;
        }

    }

    private void ApplyFoggy()
    {
        SetRainActive(false);
        SetSnowActive(false);

        SetDirectionalLight(foggyLightIntensity, foggyLightColor);
        SetSkyboxOrDefault(foggySkybox);

        RenderSettings.fog = true;
        RenderSettings.fogColor = foggyFogColor;
        RenderSettings.fogDensity = foggyFogDensity;
    }

    private void SetRainActive(bool active)
    {
        if (active)
        {
            if (rainRoot != null)
                rainRoot.SetActive(true);

            if (rainNear != null)
            {
                rainNear.Clear();
                rainNear.Play();
            }

            if (rainFar != null)
            {
                rainFar.Clear();
                rainFar.Play();
            }

            if (rainAudio != null && !rainAudio.isPlaying)
                rainAudio.Play();

            if (verboseWeatherLogging)
            {
                StartCoroutine(LogRainDiagnosticsNextFrame("SetRainActive(true)"));
            }
        }
        else
        {
            ForceStopRainImmediate();
        }
    }

    private void SetSnowActive(bool active)
    {
        if (active)
        {
            if (snowRoot != null)
                snowRoot.SetActive(true);

            if (snowNear != null)
            {
                snowNear.Clear();
                snowNear.Play();
            }

            if (snowFar != null)
            {
                snowFar.Clear();
                snowFar.Play();
            }

            if (snowAudio != null && !snowAudio.isPlaying)
                snowAudio.Play();
        }
        else
        {
            ForceStopSnowImmediate();
        }
    }

    private void ForceStopRainImmediate()
    {
        if (rainNear != null)
        {
            rainNear.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            rainNear.Clear();
        }

        if (rainFar != null)
        {
            rainFar.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            rainFar.Clear();
        }

        if (rainAudio != null && rainAudio.isPlaying)
            rainAudio.Stop();

        if (rainRoot != null)
            rainRoot.SetActive(false);
    }

    private void ForceStopSnowImmediate()
    {
        if (snowNear != null)
        {
            snowNear.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            snowNear.Clear();
        }

        if (snowFar != null)
        {
            snowFar.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            snowFar.Clear();
        }

        if (snowAudio != null && snowAudio.isPlaying)
            snowAudio.Stop();

        if (snowRoot != null)
            snowRoot.SetActive(false);
    }

    private void ForceStopAllWeatherEffectsImmediate()
    {
        ForceStopRainImmediate();
        ForceStopSnowImmediate();
    }

    private void SetDirectionalLight(float intensity, Color color)
    {
        if (directionalLight == null) return;

        directionalLight.intensity = intensity;
        directionalLight.color = color;
    }

    private void SetSkyboxOrDefault(Material mat)
    {
        RenderSettings.skybox = mat != null ? mat : defaultSkybox;
    }

    [ContextMenu("Weather/Apply Rainy (Debug)")]
    private void DebugApplyRainy()
    {
        ApplyWeather(WeatherType.Rainy);
        LogRainDiagnostics("ContextMenu ApplyRainy");
    }

    [ContextMenu("Weather/Log Rain Diagnostics")]
    private void DebugLogRainDiagnostics()
    {
        LogRainDiagnostics("ContextMenu");
    }

    private IEnumerator LogRainDiagnosticsNextFrame(string source)
    {
        yield return null;
        LogRainDiagnostics(source);
    }

    private void LogWeatherBindings()
    {
        if (!verboseWeatherLogging)
        {
            return;
        }

        Debug.Log(
            $"[Weather] Bindings => rainRoot={(rainRoot != null ? rainRoot.name : "NULL")}, " +
            $"rainNear={(rainNear != null ? rainNear.name : "NULL")}, " +
            $"rainFar={(rainFar != null ? rainFar.name : "NULL")}, " +
            $"rainAudio={(rainAudio != null ? rainAudio.name : "NULL")}",
            this);
    }

    private void LogRainDiagnostics(string source)
    {
        if (!verboseWeatherLogging)
        {
            return;
        }

        Debug.Log(
            $"[Weather] Rain diagnostics from {source} => " +
            $"CurrentType={CurrentType}, " +
            $"rainRootActiveSelf={(rainRoot != null && rainRoot.activeSelf)}, " +
            $"rainRootActiveInHierarchy={(rainRoot != null && rainRoot.activeInHierarchy)}, " +
            $"rainAudioPlaying={(rainAudio != null && rainAudio.isPlaying)}",
            this);

        LogParticleSystemState("RainNear", rainNear);
        LogParticleSystemState("RainFar", rainFar);
    }

    private void LogParticleSystemState(string label, ParticleSystem particleSystem)
    {
        if (particleSystem == null)
        {
            Debug.LogWarning($"[Weather] {label} is NULL", this);
            return;
        }

        var main = particleSystem.main;
        var emission = particleSystem.emission;
        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        var material = renderer != null ? renderer.sharedMaterial : null;
        var shaderName = material != null && material.shader != null ? material.shader.name : "NULL";

        Debug.Log(
            $"[Weather] {label} => " +
            $"activeSelf={particleSystem.gameObject.activeSelf}, " +
            $"activeInHierarchy={particleSystem.gameObject.activeInHierarchy}, " +
            $"isPlaying={particleSystem.isPlaying}, " +
            $"isEmitting={particleSystem.isEmitting}, " +
            $"particleCount={particleSystem.particleCount}, " +
            $"simulationSpace={main.simulationSpace}, " +
            $"startSpeed={main.startSpeed.constant}, " +
            $"startLifetime={main.startLifetime.constant}, " +
            $"rateOverTime={emission.rateOverTime.constant}, " +
            $"material={(material != null ? material.name : "NULL")}, " +
            $"shader={shaderName}",
            particleSystem);
    }
}