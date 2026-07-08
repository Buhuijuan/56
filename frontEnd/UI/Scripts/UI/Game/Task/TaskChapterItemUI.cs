using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskChapterItemUI : MonoBehaviour
{
    public TMP_Text chapterName;
    public Toggle toggle;

    public Image normalBG;
    public Image checkedBG;

    private TaskViewData viewData;

    // -------------------------------
    //  只保留后端任务 Setup
    // -------------------------------
    public void Setup(TaskViewData data, TaskChapterPanelUI parent)
    {
        viewData = data;

        ApplySkin(data.type, data.title);
        BindToggle();
    }

    private void ApplySkin(TaskType type, string title)
    {
        TaskChapterItemSkin skin = type == TaskType.Main
            ? TaskSkinLibrary.Instance.mainChapterSkin
            : TaskSkinLibrary.Instance.branchChapterSkin;

        normalBG.sprite = skin.normalSprite;
        checkedBG.sprite = skin.checkedSprite;

        chapterName.text = title;
    }

    private void BindToggle()
    {
        toggle.isOn = false;
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void OnToggleValueChanged(bool isChecked)
    {
        if (!isChecked || TaskPanelUI.Instance == null)
            return;

        // 纯后端：只通知 TaskPanelUI 选择 TaskViewData
        TaskPanelUI.Instance.OnSelectTask(viewData);
    }
}
