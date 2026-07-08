using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TaskNpcDialogueSystem
{
    private static Dictionary<string, TaskNpcDialogueConfig> configMap;

    public static bool TryGetConfig(string taskCode, out TaskNpcDialogueConfig config)
    {
        config = null;
        EnsureLoaded();
        return !string.IsNullOrWhiteSpace(taskCode) &&
               configMap != null &&
               configMap.TryGetValue(taskCode, out config) &&
               config != null;
    }

    public static bool ShouldHandleLocally(string taskCode)
    {
        return TryGetConfig(taskCode, out _);
    }

    public static bool ShouldDeferArrivalToNpcDialogue(string taskCode)
    {
        return TryGetConfig(taskCode, out TaskNpcDialogueConfig config) &&
               config != null &&
               config.reportTaskEvent &&
               config.eventType == "ARRIVE_BUILDING";
    }

    public static BackendElfPrompt BuildPrompt(TaskNpcDialogueConfig config)
    {
        if (config == null)
            return null;

        return new BackendElfPrompt
        {
            taskCode = config.taskCode,
            stage = "npc_local",
            npcName = config.npcName,
            avatarKey = config.avatarKey,
            autoPopup = true,
            contents = config.contents != null ? new List<string>(config.contents) : new List<string>()
        };
    }

    private static void EnsureLoaded()
    {
        if (configMap != null)
            return;

        configMap = new Dictionary<string, TaskNpcDialogueConfig>();
        TextAsset json = Resources.Load<TextAsset>("Jsons/TaskNpcDialogueConfig");
        if (json == null || string.IsNullOrWhiteSpace(json.text))
            return;

        TaskNpcDialogueConfigListWrapper wrapper = JsonUtility.FromJson<TaskNpcDialogueConfigListWrapper>(json.text);
        if (wrapper?.dialogues == null)
            return;

        foreach (TaskNpcDialogueConfig config in wrapper.dialogues.Where(item => item != null && !string.IsNullOrWhiteSpace(item.taskCode)))
            configMap[config.taskCode] = config;
    }
}
