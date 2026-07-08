using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TaskProgressPromptSystem
{
    private static Dictionary<string, TaskProgressPromptConfig> configMap;

    public static bool TryGetConfig(string taskCode, out TaskProgressPromptConfig config)
    {
        config = null;
        EnsureLoaded();
        return !string.IsNullOrWhiteSpace(taskCode) &&
               configMap != null &&
               configMap.TryGetValue(taskCode, out config) &&
               config != null;
    }

    public static BackendElfPrompt BuildPrompt(TaskProgressPromptConfig config)
    {
        if (config == null)
            return null;

        return new BackendElfPrompt
        {
            taskCode = config.taskCode,
            stage = "progress",
            npcName = string.IsNullOrWhiteSpace(config.npcName) ? "AI小精灵" : config.npcName,
            avatarKey = string.IsNullOrWhiteSpace(config.avatarKey) ? "elf_default" : config.avatarKey,
            autoPopup = true,
            contents = config.contents != null ? new List<string>(config.contents) : new List<string>()
        };
    }

    private static void EnsureLoaded()
    {
        if (configMap != null)
            return;

        configMap = new Dictionary<string, TaskProgressPromptConfig>();
        TextAsset json = Resources.Load<TextAsset>("Jsons/TaskProgressPromptConfig");
        if (json == null || string.IsNullOrWhiteSpace(json.text))
            return;

        TaskProgressPromptConfigListWrapper wrapper = JsonUtility.FromJson<TaskProgressPromptConfigListWrapper>(json.text);
        if (wrapper?.prompts == null)
            return;

        foreach (TaskProgressPromptConfig config in wrapper.prompts.Where(item => item != null && !string.IsNullOrWhiteSpace(item.taskCode)))
            configMap[config.taskCode] = config;
    }
}
