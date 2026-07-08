using UnityEngine;

public static class BackendSettings
{
    private const string UseBackendModeKey = "backend.use_mode";
    private const bool DefaultUseBackendMode = true;
    private const string AutoLoginKey = "backend.auto_login";
    private const bool DefaultAutoLogin = true;

    private const string BaseUrlKey = "backend.base_url";
    private const string DefaultBaseUrl = "http://47.109.31.215:8080";


    public static bool UseBackendMode
    {
        get => PlayerPrefs.GetInt(UseBackendModeKey, DefaultUseBackendMode ? 1 : 0) == 1;
        set
        {
            PlayerPrefs.SetInt(UseBackendModeKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static string BaseUrl
    {
        get
        {
            string stored = PlayerPrefs.GetString(BaseUrlKey, DefaultBaseUrl);
            string normalized = NormalizeBaseUrl(stored);
            if (!string.Equals(stored, normalized))
            {
                PlayerPrefs.SetString(BaseUrlKey, normalized);
                PlayerPrefs.Save();
            }

            return normalized;
        }
        set
        {
            PlayerPrefs.SetString(BaseUrlKey, NormalizeBaseUrl(value));
            PlayerPrefs.Save();
        }
    }

    public static bool AutoLoginEnabled
    {
        get => PlayerPrefs.GetInt(AutoLoginKey, DefaultAutoLogin ? 1 : 0) == 1;
        set
        {
            PlayerPrefs.SetInt(AutoLoginKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static void ResetBaseUrlToDefault()
    {
        PlayerPrefs.SetString(BaseUrlKey, DefaultBaseUrl);
        PlayerPrefs.Save();
    }

    public static void ResetBackendModeToDefault()
    {
        PlayerPrefs.SetInt(UseBackendModeKey, DefaultUseBackendMode ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void ResetAutoLoginToDefault()
    {
        PlayerPrefs.SetInt(AutoLoginKey, DefaultAutoLogin ? 1 : 0);
        PlayerPrefs.Save();
    }

    private static string NormalizeBaseUrl(string value)
    {
        string normalized = string.IsNullOrWhiteSpace(value) ? DefaultBaseUrl : value.Trim();
        normalized = normalized.TrimEnd('/');

        if (normalized.EndsWith("/api"))
            normalized = normalized.Substring(0, normalized.Length - 4);

        bool isHttp = normalized.StartsWith("http://");
        bool isHttps = normalized.StartsWith("https://");
        if (!isHttp && !isHttps)
            return DefaultBaseUrl;

        return normalized;
    }
}
