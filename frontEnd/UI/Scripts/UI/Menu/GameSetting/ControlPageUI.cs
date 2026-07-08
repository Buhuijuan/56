using UnityEngine;
using UnityEngine.UI;

public class ControlPageUI : PageUI
{
    public Toggle stableView, balancedView, sensitiveView;
    public Toggle stableMove, balancedMove, sensitiveMove;

    void Start()
    {
        var data = SettingSystem.Instance.data;

        // 初始化 UI
        stableMove.isOn = data.moveSensitivity == SensitivityLevel.Stable;
        balancedMove.isOn = data.moveSensitivity == SensitivityLevel.Balanced;
        sensitiveMove.isOn = data.moveSensitivity == SensitivityLevel.Sensitive;

        stableView.isOn = data.viewSensitivity == SensitivityLevel.Stable;
        balancedView.isOn = data.viewSensitivity == SensitivityLevel.Balanced;
        sensitiveView.isOn = data.viewSensitivity == SensitivityLevel.Sensitive;

        // 添加监听（只保存，不应用）
        stableMove.onValueChanged.AddListener(v => { if (v) SaveMoveSensitivity(SensitivityLevel.Stable); });
        balancedMove.onValueChanged.AddListener(v => { if (v) SaveMoveSensitivity(SensitivityLevel.Balanced); });
        sensitiveMove.onValueChanged.AddListener(v => { if (v) SaveMoveSensitivity(SensitivityLevel.Sensitive); });

        stableView.onValueChanged.AddListener(v => { if (v) SaveViewSensitivity(SensitivityLevel.Stable); });
        balancedView.onValueChanged.AddListener(v => { if (v) SaveViewSensitivity(SensitivityLevel.Balanced); });
        sensitiveView.onValueChanged.AddListener(v => { if (v) SaveViewSensitivity(SensitivityLevel.Sensitive); });
    }

    void SaveMoveSensitivity(SensitivityLevel level)
    {
        var data = SettingSystem.Instance.data;
        data.moveSensitivity = level;
        SettingSystem.Instance.Save();
        if (ControlBtnManager.Instance != null)
            ControlBtnManager.Instance.ApplySensitivityFromSetting();
    }

    void SaveViewSensitivity(SensitivityLevel level)
    {
        var data = SettingSystem.Instance.data;
        data.viewSensitivity = level;
        SettingSystem.Instance.Save();
        if (ControlBtnManager.Instance != null)
            ControlBtnManager.Instance.ApplySensitivityFromSetting();
    }
}
