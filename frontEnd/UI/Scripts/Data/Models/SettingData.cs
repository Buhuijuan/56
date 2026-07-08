public enum QualityLevel
{
    Low,
    Medium,
    High
}

public enum FrameRateLevel
{
    Stable,
    Smooth,
    Extreme
}
public enum SensitivityLevel
{
    Stable,
    Balanced,
    Sensitive
}

[System.Serializable]
public class SettingData
{
    //画面设置
    public QualityLevel quality = QualityLevel.Medium;
    public FrameRateLevel frameRate = FrameRateLevel.Smooth;
    public float brightness = 1f;
    public bool colorBlindMode = false;

    //音频设置
    public bool musicEnabled = true;
    public float musicVolume = 1f;

    public bool effectEnabled = true;
    public float effectVolume = 1f;

    public bool soundEnabled = true;
    public float soundVolume = 1f;

    //控制设置
    public SensitivityLevel moveSensitivity = SensitivityLevel.Balanced;
    public SensitivityLevel viewSensitivity = SensitivityLevel.Balanced;

}

