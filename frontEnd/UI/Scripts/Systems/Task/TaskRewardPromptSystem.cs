using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TaskRewardPromptSystem
{
    private static readonly HashSet<string> queuedClaimPrompts = new();

    public static void ShowClaimPrompt(string taskCode)
    {
        if (string.IsNullOrEmpty(taskCode) || UIManager.Instance == null)
            return;

        BackendTaskEntry task = BackendTaskStore.FindTask(taskCode);
        if (task == null || task.status != "COMPLETED")
            return;

        if (UIManager.Instance.IsTaskPromptBlocked)
        {
            if (queuedClaimPrompts.Add(taskCode))
                BackendRuntime.Run(ShowClaimPromptWhenReady(taskCode));
            return;
        }

        UIManager.Instance.ShowRemind(
            "任务完成",
            "领取奖励",
            BuildClaimMessage(task),
            null,
            () => BackendRuntime.Run(ClaimTaskAndRefresh(taskCode)),
            false
        );
    }

    public static void ShowClaimPromptForCompletedTasks(IEnumerable<string> taskCodes)
    {
        if (taskCodes == null)
            return;

        string taskCode = taskCodes.FirstOrDefault(code =>
        {
            BackendTaskEntry task = BackendTaskStore.FindTask(code);
            return task != null && task.status == "COMPLETED";
        });

        if (!string.IsNullOrEmpty(taskCode))
            ShowClaimPrompt(taskCode);
    }

    private static IEnumerator ShowClaimPromptWhenReady(string taskCode)
    {
        yield return new WaitUntil(() => UIManager.Instance != null && !UIManager.Instance.IsTaskPromptBlocked);
        queuedClaimPrompts.Remove(taskCode);
        ShowClaimPrompt(taskCode);
    }

    private static IEnumerator ClaimTaskAndRefresh(string taskCode)
    {
        BackendApiResult<BackendTaskOperationEnvelope> claimResult = null;
        yield return BackendFacade.ClaimTask(taskCode, result => claimResult = result);

        if (claimResult == null || !claimResult.Success || claimResult.Data == null || !claimResult.Data.success)
        {
            UIManager.Instance?.ShowRemind(
                "领取失败",
                "好的",
                claimResult != null ? claimResult.Message : "任务奖励领取失败",
                null,
                null,
                false
            );
            yield break;
        }

        yield return BackendFacade.RefreshHome(null);
        yield return BackendFacade.RefreshTasks(null);
        yield return BackendFacade.RefreshCurrentMainTask(null);
        yield return BackendFacade.RefreshTaskChapters(null);
        yield return BackendFacade.RefreshGrowth(null);

        RefreshVisibleTaskUIs();
        TaskChapterCompletionPromptSystem.FlushPendingPrompts();
    }

    private static string BuildClaimMessage(BackendTaskEntry task)
    {
        List<string> lines = new();

        if (task.rewardExp > 0)
            lines.Add($"经验值 +{task.rewardExp}");

        if (task.rewardCoin > 0)
            lines.Add($"纪念币 +{task.rewardCoin}");

        if (!string.IsNullOrEmpty(task.rewardUnlockFeature))
            lines.Add($"Unlocked feature: {task.rewardUnlockFeature}");

        if (lines.Count == 0)
            lines.Add("领取奖励以继续");

        return string.Join("\n", lines);
    }

    public static void RefreshVisibleTaskUIs()
    {
        if (TaskPanelUI.Instance != null && TaskPanelUI.Instance.gameObject.activeInHierarchy)
            TaskPanelUI.Instance.Refresh();

        foreach (TaskListPanelUI panel in UnityEngine.Object.FindObjectsOfType<TaskListPanelUI>(true))
        {
            if (panel != null && panel.gameObject.activeInHierarchy)
                panel.Refresh();
        }
    }
}
