using System.Collections;
using UnityEngine;

public class TaskPanelUI : MonoBehaviour
{
    public static TaskPanelUI Instance;

    public TaskChapterPanelUI chapterPanel;
    public TaskDetailPanelUI detailPanel;

    private TaskViewData currentViewTask;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // 打开面板时自动刷新后端任务（静音）
        BackendRuntime.Run(RefreshBackendAndOpen());
    }

    // -------------------------------
    //  打开任务面板（外部调用）
    // -------------------------------
    public void OpenTaskPanel(TaskViewData data)
    {
        currentViewTask = data;
        gameObject.SetActive(true);
        BackendRuntime.Run(RefreshBackendAndOpen());
    }

    // -------------------------------
    //  刷新任务面板（后端模式）
    // -------------------------------
    public void Refresh()
    {
        var tasks = TaskDisplaySystem.GetTasks();

        if (currentViewTask != null)
        {
            TaskViewData refreshedTask = tasks.Find(task => task.taskCode == currentViewTask.taskCode);
            currentViewTask = refreshedTask;
        }

        if (currentViewTask == null)
        {
            if (tasks.Count > 0)
                currentViewTask = tasks[0];
        }

        chapterPanel.currentSelectedViewTask = currentViewTask;
        chapterPanel.Open();
        chapterPanel.Refresh();

        detailPanel.Refresh(currentViewTask);
    }

    public void OnSelectTask(TaskViewData data)
    {
        currentViewTask = data;
        detailPanel.Refresh(data);
    }

    // -------------------------------
    //  打开面板时自动刷新后端任务（静音）
    // -------------------------------
    private IEnumerator RefreshBackendAndOpen()
    {
        // ⭐ 打开面板时不让后端自动弹小精灵
        BackendFacade.SetElfPromptSuppressed(true);

        yield return BackendFacade.RefreshTasks(null);
        yield return BackendFacade.RefreshCurrentMainTask(null);
        yield return BackendFacade.RefreshTaskChapters(null);

        BackendFacade.SetElfPromptSuppressed(false);

        if (gameObject.activeInHierarchy)
            Refresh();
    }
}
