using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskChapterPanelUI : BaseMultiplePagePanelUI
{
    public Toggle mainToggle;
    public Toggle branchToggle;

    public PageUI mainChapterPage;
    public PageUI branchChapterPage;

    public TaskChapterItemUI chapterItemPrefab;

    // 只保留后端任务
    public TaskViewData currentSelectedViewTask;

    private readonly List<TaskChapterItemUI> mainGeneratedItems = new();
    private readonly List<TaskChapterItemUI> branchGeneratedItems = new();

    protected override void RegisterPages()
    {
        map.Add(mainToggle, mainChapterPage);
        map.Add(branchToggle, branchChapterPage);
    }

    protected override void SetDefaultPage()
    {
        // 纯后端：只根据 currentSelectedViewTask 判断
        bool isMain = currentSelectedViewTask == null || currentSelectedViewTask.type == TaskType.Main;

        Toggle toggle = isMain ? mainToggle : branchToggle;
        toggle.isOn = true;
        map[toggle].Show();
    }

    public void Refresh()
    {
        Clear();

        // 纯后端：从 TaskDisplaySystem 获取任务
        List<TaskViewData> tasks = TaskDisplaySystem.GetTasks();

        foreach (TaskViewData task in tasks)
            CreateBackendItem(task);
    }

    public void Clear()
    {
        foreach (TaskChapterItemUI item in mainGeneratedItems)
            Destroy(item.gameObject);
        mainGeneratedItems.Clear();

        foreach (TaskChapterItemUI item in branchGeneratedItems)
            Destroy(item.gameObject);
        branchGeneratedItems.Clear();
    }

    private void CreateBackendItem(TaskViewData task)
    {
        bool isMain = task.type == TaskType.Main;
        Transform root = isMain ? mainChapterPage.transform : branchChapterPage.transform;

        TaskChapterItemUI item = Object.Instantiate(chapterItemPrefab, root);
        item.Setup(task, this);

        // 设置 ToggleGroup
        ToggleGroup group = (isMain ? mainChapterPage : branchChapterPage).GetComponent<ToggleGroup>();
        item.toggle.group = group;

        // 设置选中状态
        item.toggle.isOn = currentSelectedViewTask != null && currentSelectedViewTask.taskCode == task.taskCode;

        if (isMain)
            mainGeneratedItems.Add(item);
        else
            branchGeneratedItems.Add(item);
    }
}
