using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TaskChapterCompletionPromptSystem
{
    private static readonly Dictionary<int, int> ChapterTitleToChapterNo = new()
    {
        [7] = 1,
        [8] = 2,
        [9] = 3
    };

    private static readonly Queue<int> pendingTitlePrompts = new();
    private static readonly HashSet<int> pendingTitlePromptSet = new();
    private static bool isShowingPrompt;

    public static HashSet<int> CaptureUnlockedTitleIds()
    {
        RoleData role = AccountSystem.GetCurrentRole();
        if (role?.titleState?.unlockedTitles == null)
            return new HashSet<int>();

        return new HashSet<int>(role.titleState.unlockedTitles);
    }

    public static IEnumerator RefreshGrowthAndQueueChapterPrompts(HashSet<int> beforeUnlockedTitleIds)
    {
        yield return BackendFacade.RefreshGrowth(null);

        HashSet<int> previous = beforeUnlockedTitleIds ?? new HashSet<int>();
        HashSet<int> current = CaptureUnlockedTitleIds();

        foreach (int titleId in current)
        {
            if (previous.Contains(titleId))
                continue;
            if (!ChapterTitleToChapterNo.ContainsKey(titleId))
                continue;

            QueuePrompt(titleId);
        }
    }

    public static void FlushPendingPrompts()
    {
        if (isShowingPrompt || pendingTitlePrompts.Count == 0)
            return;

        BackendRuntime.Run(ShowNextPromptWhenReady());
    }

    private static void QueuePrompt(int titleId)
    {
        if (!pendingTitlePromptSet.Add(titleId))
            return;

        pendingTitlePrompts.Enqueue(titleId);
    }

    private static IEnumerator ShowNextPromptWhenReady()
    {
        if (isShowingPrompt)
            yield break;

        isShowingPrompt = true;
        yield return new WaitUntil(() => UIManager.Instance != null && !UIManager.Instance.IsTaskPromptBlocked);

        if (pendingTitlePrompts.Count == 0)
        {
            isShowingPrompt = false;
            yield break;
        }

        int titleId = pendingTitlePrompts.Dequeue();
        pendingTitlePromptSet.Remove(titleId);

        int chapterNo = ChapterTitleToChapterNo.TryGetValue(titleId, out int chapter) ? chapter : 0;
        string chapterText = chapterNo > 0 ? $"第{chapterNo}章" : "当前章节";
        string titleName = ResolveTitleName(titleId);
        string message = $"{chapterText}已完成。\n已解锁称号：{titleName}";

        UIManager.Instance.ShowRemind(
            "章节完成",
            "知道了",
            message,
            null,
            OnPromptClosed,
            false
        );
    }

    private static void OnPromptClosed()
    {
        isShowingPrompt = false;
        FlushPendingPrompts();
    }

    private static string ResolveTitleName(int titleId)
    {
        List<TitleData> titleData = TitleSystem.GetTitleDatas();
        TitleData data = titleData?.FirstOrDefault(item => item != null && item.titleID == titleId);
        return data != null && !string.IsNullOrWhiteSpace(data.titleName)
            ? data.titleName
            : $"#{titleId}";
    }
}
