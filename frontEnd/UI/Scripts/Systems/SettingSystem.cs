using UnityEngine;

public class SettingSystem : MonoBehaviour
{
    public static SettingSystem Instance;
    public SettingData data = new();
    public void Awake()
    {
        Instance = this;
        Load();
    }
    public void Load()
    {
        if (PlayerPrefs.HasKey("SettingData"))
            data = JsonUtility.FromJson<SettingData>(PlayerPrefs.GetString("SettingData"));
    }
    public void Save()
    {
        PlayerPrefs.SetString("SettingData", JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }
}
