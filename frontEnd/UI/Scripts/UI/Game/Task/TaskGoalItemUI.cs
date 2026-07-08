using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskGoalItemUI : MonoBehaviour
{
    public Toggle toggle;
    public Text goalDescription;
    public TMP_Text goalDescriptionTMP;
    public Button clickButton;

    private TaskGoalViewData goalData;
    private TaskDetailPanelUI parent;

    public void Setup(TaskGoalViewData data, TaskDetailPanelUI parentPanel)
    {
        goalData = data;
        parent = parentPanel;

        if (toggle != null)
        {
            toggle.interactable = false;
            toggle.isOn = goalData.completed;
        }

        if (goalDescription != null)
            goalDescription.text = goalData.description;

        if (goalDescriptionTMP != null)
            goalDescriptionTMP.text = goalData.description;

        if (clickButton != null)
        {
            clickButton.onClick.RemoveAllListeners();
            clickButton.onClick.AddListener(OnGoalClicked);
        }
    }

    private void OnGoalClicked()
    {
        string taskCode = TaskDisplaySystem.MapGoalIdToTaskCode(goalData.id);
        if (string.IsNullOrEmpty(taskCode))
            return;

        BackendTaskEntry backendTask = BackendTaskStore.FindTask(taskCode);
        if (backendTask == null)
        {
            BackendTaskEntry currentMain = BackendTaskStore.GetCurrentMainTask();
            if (currentMain != null && currentMain.taskCode == taskCode)
                backendTask = currentMain;
        }

        Debug.Log($"[TaskGoalItemUI] Click goalId={goalData.id}, taskCode={taskCode}, taskStatus={backendTask?.status}");

        if (backendTask == null)
            return;

        if (backendTask.status == "AVAILABLE")
        {
            BackendRuntime.Run(AcceptTaskAndRefresh(taskCode));
            return;
        }

        if (backendTask.status == "IN_PROGRESS")
        {
            TryCloseTaskPanelIfPromptWillShow(taskCode);
            TaskPromptSystem.ShowPromptForTask(taskCode);
            return;
        }

        if (backendTask.status == "COMPLETED")
        {
            TaskRewardPromptSystem.ShowClaimPrompt(taskCode);
            return;
        }
    }

    private static System.Collections.IEnumerator AcceptTaskAndRefresh(string taskCode)
    {
        BackendFacade.SetElfPromptSuppressed(true);
        BackendApiResult<BackendTaskOperationEnvelope> acceptResult = null;
        yield return BackendFacade.AcceptTask(taskCode, result => acceptResult = result);
        Debug.Log(
            $"[TaskGoalItemUI] AcceptTask result. taskCode={taskCode}, Success={acceptResult?.Success}, " +
            $"Status={acceptResult?.StatusCode}, Message={acceptResult?.Message}, ApiSuccess={acceptResult?.Data?.success}, " +
            $"Accepted={acceptResult?.Data?.data?.accepted}");

        bool accepted = acceptResult != null &&
                        acceptResult.Success &&
                        acceptResult.Data != null &&
                        acceptResult.Data.success &&
                        acceptResult.Data.data != null &&
                        acceptResult.Data.data.accepted;
        if (accepted)
        {
            // Show the task start prompt as soon as acceptance succeeds so it is
            // not pre-empted by world-triggered progress prompts in the same area.
            TryCloseTaskPanelIfPromptWillShow(taskCode);
            TaskPromptSystem.ShowPromptForTask(taskCode);
        }

        yield return BackendFacade.RefreshTasks(null);
        yield return BackendFacade.RefreshCurrentMainTask(null);
        yield return BackendFacade.RefreshTaskChapters(null);
        BackendFacade.SetElfPromptSuppressed(false);

        BackendTaskEntry refreshedTask = BackendTaskStore.FindTask(taskCode);
        if (refreshedTask == null)
        {
            BackendTaskEntry currentMain = BackendTaskStore.GetCurrentMainTask();
            if (currentMain != null && currentMain.taskCode == taskCode)
                refreshedTask = currentMain;
        }

        Debug.Log(
            $"[TaskGoalItemUI] After accept refresh. taskCode={taskCode}, " +
            $"taskStatus={refreshedTask?.status}, currentMain={BackendTaskStore.GetCurrentMainTask()?.taskCode}, " +
            $"currentMainStatus={BackendTaskStore.GetCurrentMainTask()?.status}");

        if (TaskPanelUI.Instance != null)
            TaskPanelUI.Instance.Refresh();

        foreach (TaskListPanelUI panel in Object.FindObjectsOfType<TaskListPanelUI>(true))
        {
            if (panel != null && panel.gameObject.activeInHierarchy)
                panel.Refresh();
        }
    }

    private static void TryCloseTaskPanelIfPromptWillShow(string taskCode)
    {
        if (string.IsNullOrEmpty(taskCode))
            return;

        BackendElfPrompt prompt = TaskPromptSystem.GetPromptForTask(taskCode);
        if (prompt == null)
            return;

        if (TaskPanelUI.Instance != null && TaskPanelUI.Instance.gameObject.activeInHierarchy)
            TaskPanelUI.Instance.gameObject.SetActive(false);
    }
}
