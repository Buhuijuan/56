using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlBtnManager : MonoBehaviour
{
    public static ControlBtnManager Instance;

    public Joystick moveJoystick;
    public Joystick viewJoystick;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        ApplySensitivityFromSetting();
    }

    public void ApplySensitivityFromSetting()
    {
        var data = SettingSystem.Instance.data;

        moveJoystick.sensitivity = ConvertMoveSensitivity(data.moveSensitivity);
        viewJoystick.sensitivity = ConvertViewSensitivity(data.viewSensitivity);
    }

    private float ConvertMoveSensitivity(SensitivityLevel level)
    {
        switch (level)
        {
            case SensitivityLevel.Stable: return 0.6f;
            case SensitivityLevel.Balanced: return 1.0f;
            case SensitivityLevel.Sensitive: return 1.5f;
        }
        return 1f;
    }

    private float ConvertViewSensitivity(SensitivityLevel level)
    {
        switch (level)
        {
            // Lower overall camera sensitivity:
            // previous lowest (0.5) is now middle baseline.
            case SensitivityLevel.Stable: return 0.3f;
            case SensitivityLevel.Balanced: return 0.5f;
            case SensitivityLevel.Sensitive: return 0.8f;
        }
        return 1f;
    }
}
