using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BikeToggleUI : MonoBehaviour
{
    public Toggle bikeToggle;

    private bool suppressCallback;

    void Start()
    {
        bikeToggle.onValueChanged.AddListener(OnBikeToggleChanged);
    }

    void OnBikeToggleChanged(bool isOn)
    {
        if (suppressCallback)
            return;

        if (!CanUseBikeToggle())
        {
            suppressCallback = true;
            bikeToggle.isOn = false;
            suppressCallback = false;

            ShowBikeLockedPrompt();
            return;
        }

        if (PlayerModelSwitcher.Instance != null)
            PlayerModelSwitcher.Instance.SetBikeMode(isOn);
    }

    private static bool CanUseBikeToggle()
    {
        if (!BackendSettings.UseBackendMode)
            return true;

        BackendTaskEntry bikeTask = BackendTaskStore.FindTask("B_2_1");
        if (bikeTask == null)
            return false;

        return bikeTask.status == "IN_PROGRESS"
               || bikeTask.status == "COMPLETED"
               || bikeTask.status == "CLAIMED";
    }

    private static void ShowBikeLockedPrompt()
    {
        if (UIManager.Instance == null)
            return;

        AIPromptConfig config = new AIPromptConfig
        {
            npcName = "AI小精灵",
            npcAvatar = Resources.Load<Sprite>("Sprites/NPCImages/elf_default"),
            title = "单车功能暂未开放",
            contents = new List<string>
            {
                "校园单车功能还没有向你开放哦。",
                "先在校园里找找单车停放点，靠近之后我会带你完成单车初体验。",
                "等功能解锁后，这个按钮就能正常切换自行车模式了。"
            }
        };

        UIManager.Instance.ShowAIPrompt(config);
    }
}
