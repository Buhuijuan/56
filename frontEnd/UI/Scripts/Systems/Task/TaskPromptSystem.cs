using System.Collections.Generic;
using UnityEngine;

public static class TaskPromptSystem
{
    private struct PendingPrompt
    {
        public string key;
        public BackendElfPrompt prompt;
        public System.Action onClosed;
    }

    private static readonly Queue<PendingPrompt> pendingPrompts = new();
    private static readonly HashSet<string> pendingKeys = new();
    private static readonly Dictionary<string, float> recentAutoPromptTimes = new();

    private static string currentPromptKey;
    private static System.Action currentPromptClosedAction;

    public static void EnqueueAutoPrompt(BackendElfPrompt prompt)
    {
        Enqueue(prompt, false, null);
    }

    public static void ShowPromptForTask(string taskCode)
    {
        if (string.IsNullOrEmpty(taskCode))
            return;

        // Manual task clicks should win over previously queued auto-prompts
        // from other tasks. This keeps branch/main prompts from cross-playing
        // when both tasks are already in progress.
        RemovePendingPromptsExceptTask(taskCode);

        BackendElfPrompt prompt = GetPromptForTask(taskCode);
        if (prompt == null)
            return;

        Enqueue(prompt, true, null);
    }

    public static BackendElfPrompt GetPromptForTask(string taskCode)
    {
        if (string.IsNullOrEmpty(taskCode))
            return null;

        BackendTaskEntry task = BackendTaskStore.FindTask(taskCode);
        BackendElfPrompt prompt = task?.elfPrompt;

        if (prompt == null)
        {
            BackendTaskEntry currentMainTask = GameStateStore.Tasks != null ? GameStateStore.Tasks.currentMainTask : null;
            if (currentMainTask != null && currentMainTask.taskCode == taskCode)
                prompt = currentMainTask.elfPrompt;
        }

        if (prompt == null && GameStateStore.Tasks != null && GameStateStore.Tasks.elfPrompt != null)
        {
            BackendElfPrompt pagePrompt = GameStateStore.Tasks.elfPrompt;
            if (pagePrompt.taskCode == taskCode)
                prompt = pagePrompt;
        }

        return prompt;
    }

    public static void ShowPrompt(BackendElfPrompt prompt, System.Action onClosed = null)
    {
        Enqueue(prompt, true, onClosed);
    }

    public static void EnqueueAutoPromptsForTasks(IEnumerable<string> taskCodes)
    {
        if (taskCodes == null)
            return;

        foreach (string taskCode in taskCodes)
        {
            if (string.IsNullOrEmpty(taskCode))
                continue;

            BackendTaskEntry task = BackendTaskStore.FindTask(taskCode);
            if (task?.status != "IN_PROGRESS" || task.elfPrompt == null)
                continue;

            Enqueue(task.elfPrompt, false, null);
        }
    }

    public static void NotifyPromptClosed()
    {
        System.Action onClosed = currentPromptClosedAction;
        currentPromptClosedAction = null;
        currentPromptKey = null;
        onClosed?.Invoke();
        TryShowNext();
    }

    public static void NotifyUiAvailabilityChanged()
    {
        TryShowNext();
    }

    public static void ClearPending()
    {
        pendingPrompts.Clear();
        pendingKeys.Clear();
        currentPromptKey = null;
        currentPromptClosedAction = null;
    }

    public static void RemovePendingPromptsForTask(string taskCode)
    {
        if (string.IsNullOrEmpty(taskCode) || pendingPrompts.Count == 0)
            return;

        Queue<PendingPrompt> remaining = new();
        pendingKeys.Clear();

        while (pendingPrompts.Count > 0)
        {
            PendingPrompt pending = pendingPrompts.Dequeue();
            if (pending.prompt != null && pending.prompt.taskCode == taskCode)
                continue;

            remaining.Enqueue(pending);
            pendingKeys.Add(pending.key);
        }

        while (remaining.Count > 0)
            pendingPrompts.Enqueue(remaining.Dequeue());
    }

    public static void RemovePendingPromptsExceptTask(string taskCode)
    {
        if (pendingPrompts.Count == 0)
            return;

        Queue<PendingPrompt> remaining = new();
        pendingKeys.Clear();

        while (pendingPrompts.Count > 0)
        {
            PendingPrompt pending = pendingPrompts.Dequeue();
            if (!string.IsNullOrEmpty(taskCode) && pending.prompt != null && pending.prompt.taskCode == taskCode)
            {
                remaining.Enqueue(pending);
                pendingKeys.Add(pending.key);
            }
        }

        while (remaining.Count > 0)
            pendingPrompts.Enqueue(remaining.Dequeue());
    }

    public static void RemovePendingPromptsExceptTasks(IEnumerable<string> taskCodes)
    {
        if (pendingPrompts.Count == 0)
            return;

        HashSet<string> keepTasks = taskCodes != null
            ? new HashSet<string>(taskCodes)
            : new HashSet<string>();

        if (keepTasks.Count == 0)
            return;

        Queue<PendingPrompt> remaining = new();
        pendingKeys.Clear();

        while (pendingPrompts.Count > 0)
        {
            PendingPrompt pending = pendingPrompts.Dequeue();
            if (pending.prompt != null && !string.IsNullOrEmpty(pending.prompt.taskCode) && keepTasks.Contains(pending.prompt.taskCode))
            {
                remaining.Enqueue(pending);
                pendingKeys.Add(pending.key);
            }
        }

        while (remaining.Count > 0)
            pendingPrompts.Enqueue(remaining.Dequeue());
    }

    public static bool WasPromptShownRecently(BackendElfPrompt prompt, float seconds)
    {
        if (prompt == null)
            return false;

        string key = BuildPromptKey(prompt);
        if (string.IsNullOrEmpty(key))
            return false;

        if (!recentAutoPromptTimes.TryGetValue(key, out float lastTime))
            return false;

        return Time.unscaledTime - lastTime < seconds;
    }

    public static bool WasTaskStageShownRecently(string taskCode, string stage, float seconds)
    {
        if (string.IsNullOrEmpty(taskCode))
            return false;

        BackendElfPrompt probe = new BackendElfPrompt
        {
            taskCode = taskCode,
            stage = string.IsNullOrEmpty(stage) ? "default" : stage
        };
        return WasPromptShownRecently(probe, seconds);
    }

    public static bool IsPromptQueuedOrShowing(string taskCode, string stage = null)
    {
        if (string.IsNullOrEmpty(taskCode))
            return false;

        string expectedStage = string.IsNullOrEmpty(stage) ? null : stage;

        if (!string.IsNullOrEmpty(currentPromptKey))
        {
            string currentStage = ExtractStageFromKey(currentPromptKey);
            string currentTaskCode = ExtractTaskCodeFromKey(currentPromptKey);
            if (currentTaskCode == taskCode && (expectedStage == null || currentStage == expectedStage))
                return true;
        }

        foreach (PendingPrompt pending in pendingPrompts)
        {
            if (pending.prompt == null || pending.prompt.taskCode != taskCode)
                continue;
            if (expectedStage == null || pending.prompt.stage == expectedStage)
                return true;
        }

        return false;
    }

    private static void Enqueue(BackendElfPrompt prompt, bool force, System.Action onClosed)
    {
        if (prompt == null)
            return;

        string key = BuildPromptKey(prompt);
        if (string.IsNullOrEmpty(key))
            return;

        if (!force && WasRecentlyAutoShown(key))
            return;

        if (currentPromptKey == key || pendingKeys.Contains(key))
        {
            // Startup sync may enqueue the current main-task prompt before the
            // player manually clicks the corresponding TaskGoalItem. In that
            // case we should still try to consume the already-pending prompt
            // instead of treating the click as a no-op.
            if (force)
                TryShowNext();
            return;
        }

        pendingPrompts.Enqueue(new PendingPrompt
        {
            key = key,
            prompt = ClonePrompt(prompt),
            onClosed = onClosed
        });
        pendingKeys.Add(key);

        TryShowNext();
    }

    private static void TryShowNext()
    {
        if (UIManager.Instance == null || UIManager.Instance.IsTaskPromptBlocked)
            return;

        while (pendingPrompts.Count > 0)
        {
            PendingPrompt pending = pendingPrompts.Dequeue();
            pendingKeys.Remove(pending.key);

            AIPromptConfig config = AIPromptMapper.Map(pending.prompt);
            if (config == null)
                continue;

            currentPromptKey = pending.key;
            currentPromptClosedAction = pending.onClosed;
            recentAutoPromptTimes[pending.key] = Time.unscaledTime;
            UIManager.Instance.ShowAIPrompt(config);
            return;
        }
    }

    private static bool WasRecentlyAutoShown(string key)
    {
        if (!recentAutoPromptTimes.TryGetValue(key, out float lastTime))
            return false;

        return Time.unscaledTime - lastTime < 1.5f;
    }

    private static string BuildPromptKey(BackendElfPrompt prompt)
    {
        long roleId = SessionStore.Current.currentRoleId;
        string taskCode = string.IsNullOrEmpty(prompt.taskCode) ? "unknown" : prompt.taskCode;
        string stage = string.IsNullOrEmpty(prompt.stage) ? "default" : prompt.stage;
        return $"{roleId}:{taskCode}:{stage}";
    }

    private static string ExtractTaskCodeFromKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;

        string[] parts = key.Split(':');
        return parts.Length >= 3 ? parts[1] : null;
    }

    private static string ExtractStageFromKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;

        string[] parts = key.Split(':');
        return parts.Length >= 3 ? parts[2] : null;
    }

    private static BackendElfPrompt ClonePrompt(BackendElfPrompt prompt)
    {
        return new BackendElfPrompt
        {
            npcName = prompt.npcName,
            avatarKey = prompt.avatarKey,
            stage = prompt.stage,
            title = prompt.title,
            contents = prompt.contents != null ? new List<string>(prompt.contents) : null,
            autoPopup = prompt.autoPopup,
            taskCode = prompt.taskCode
        };
    }
}
