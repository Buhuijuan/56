using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AmbientNpcDialogueSystem
{
    private static Dictionary<string, AmbientNpcDialogueConfig> configByLocationId;

    public static bool TryGetConfig(string locationId, out AmbientNpcDialogueConfig config)
    {
        config = null;
        EnsureLoaded();
        return !string.IsNullOrWhiteSpace(locationId) &&
               configByLocationId != null &&
               configByLocationId.TryGetValue(locationId, out config) &&
               config != null;
    }

    public static IEnumerable<AmbientNpcDialogueConfig> GetAllConfigs()
    {
        EnsureLoaded();
        if (configByLocationId == null)
            return System.Array.Empty<AmbientNpcDialogueConfig>();

        return configByLocationId.Values;
    }

    public static string GetRandomLine(AmbientNpcDialogueConfig config)
    {
        if (config?.contents == null || config.contents.Count == 0)
            return null;

        int index = Random.Range(0, config.contents.Count);
        return config.contents[index];
    }

    public static BackendElfPrompt BuildPrompt(AmbientNpcDialogueConfig config)
    {
        string line = GetRandomLine(config);
        if (config == null || string.IsNullOrWhiteSpace(line))
            return null;

        return new BackendElfPrompt
        {
            taskCode = string.Empty,
            stage = "npc_ambient",
            npcName = config.npcName,
            avatarKey = config.avatarKey,
            autoPopup = true,
            contents = new List<string> { line }
        };
    }

    private static void EnsureLoaded()
    {
        if (configByLocationId != null)
            return;

        configByLocationId = new Dictionary<string, AmbientNpcDialogueConfig>();
        TextAsset json = Resources.Load<TextAsset>("Jsons/AmbientNpcDialogueConfig");
        if (json == null || string.IsNullOrWhiteSpace(json.text))
            return;

        AmbientNpcDialogueConfigListWrapper wrapper = JsonUtility.FromJson<AmbientNpcDialogueConfigListWrapper>(json.text);
        if (wrapper?.dialogues == null)
            return;

        foreach (AmbientNpcDialogueConfig config in wrapper.dialogues.Where(item => item != null && !string.IsNullOrWhiteSpace(item.locationId)))
            configByLocationId[config.locationId] = config;
    }
}
