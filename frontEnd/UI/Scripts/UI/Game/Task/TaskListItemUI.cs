using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskListItemUI : MonoBehaviour
{
    public Button TaskButton;

    public Image taskListBG;
    public TMP_Text taskTypeText;
    public TMP_Text taskName;

    public Slider taskSlider;
    public Image sliderBG;
    public Image slideFill;

    public TMP_Text progress;

    private TaskViewData viewData;

    public void Setup(TaskViewData data, TaskListPanelUI parent)
    {
        viewData = data;

        Debug.Log(
            $"[TaskListItemUI] Setup. taskCode={data?.taskCode}, title={data?.title}, " +
            $"type={data?.type}, progress={data?.progressCurrent}/{data?.progressTarget}");

        ApplySkin(
            data.type,
            data.title,
            data.progressCurrent,
            data.progressTarget
        );

        BindClick();
    }

    public void OnClickTaskListItem()
    {
        if (TaskPanelUI.Instance == null)
            return;

        TaskPanelUI.Instance.OpenTaskPanel(viewData);
    }

    private void ApplySkin(TaskType type, string title, int finished, int total)
    {
        TaskListItemSkin skin = null;
        if (TaskSkinLibrary.Instance != null)
            skin = type == TaskType.Main ? TaskSkinLibrary.Instance.mainListSkin : TaskSkinLibrary.Instance.branchListSkin;

        if (taskListBG != null && skin != null)
            taskListBG.sprite = skin.background;

        if (taskTypeText != null)
        {
            if (skin != null)
            {
                taskTypeText.text = skin.typeLabel;
                taskTypeText.color = skin.labelColor;
            }
            else
            {
                taskTypeText.text = type == TaskType.Main ? "主线" : "支线";
            }
        }

        if (sliderBG != null && skin != null)
            sliderBG.sprite = skin.sliderBG;

        if (slideFill != null && skin != null)
            slideFill.sprite = skin.sliderFill;

        if (taskName != null)
            taskName.text = title;

        if (taskSlider != null)
            taskSlider.value = total > 0 ? (float)finished / total : 0f;

        if (progress != null)
            progress.text = finished + "/" + Mathf.Max(1, total);
    }

    private void BindClick()
    {
        if (TaskButton == null)
            return;

        TaskButton.onClick.RemoveAllListeners();
        TaskButton.onClick.AddListener(OnClickTaskListItem);
    }
}
