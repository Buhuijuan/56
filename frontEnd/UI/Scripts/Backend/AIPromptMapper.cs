using System.Collections.Generic;
using UnityEngine;

public static class AIPromptMapper
{
    private const string ElfPath = "Sprites/NPCImages/elf_default";
    private const string DefaultNpcPath = "Sprites/NPCImages/DefaultHead";

    private static readonly Dictionary<string, string> AvatarPathMap = new()
    {
        ["elf_default"] = ElfPath,
        ["default"] = DefaultNpcPath,
        ["default_head"] = DefaultNpcPath,
        ["head1"] = "Sprites/NPCImages/Head1",
        ["head2"] = "Sprites/NPCImages/Head2",
        ["npc_welcome_volunteer"] = "Sprites/NPCImages/npc_welcome_volunteer",
        ["npc_reception_volunteer"] = "Sprites/NPCImages/npc_reception_volunteer",
        ["npc_dorm_manager"] = "Sprites/NPCImages/npc_dorm_manager",
        ["npc_nurse"] = "Sprites/NPCImages/npc_nurse",
        ["npc_botanical_gardener"] = "Sprites/NPCImages/npc_botanical_gardener",
        ["npc_librarian"] = "Sprites/NPCImages/npc_librarian",
        ["npc_complex_guard"] = "Sprites/NPCImages/npc_complex_guard"
    };

    public static AIPromptConfig Map(BackendElfPrompt prompt)
    {
        if (prompt == null)
            return null;

        return new AIPromptConfig
        {
            npcName = string.IsNullOrWhiteSpace(prompt.npcName) ? ResolveFallbackNpcName(prompt) : prompt.npcName,
            npcAvatar = LoadAvatar(prompt),
            title = prompt.title,
            contents = prompt.contents
        };
    }

    private static Sprite LoadAvatar(BackendElfPrompt prompt)
    {
        string stage = prompt.stage?.Trim().ToLowerInvariant();
        bool isNpcLocalPrompt = !string.IsNullOrEmpty(stage) && stage.Contains("npc");
        if (!isNpcLocalPrompt)
            return Resources.Load<Sprite>(ElfPath);

        string avatarKey = prompt.avatarKey?.Trim();
        if (!string.IsNullOrWhiteSpace(avatarKey))
        {
            Sprite fromKey = TryLoadByKey(avatarKey);
            if (fromKey != null)
                return fromKey;
        }

        string fallbackKey = ResolveFallbackAvatarKey(prompt);
        Sprite fromFallback = TryLoadByKey(fallbackKey);
        if (fromFallback != null)
            return fromFallback;

        return Resources.Load<Sprite>(ElfPath);
    }

    private static Sprite TryLoadByKey(string avatarKey)
    {
        if (string.IsNullOrWhiteSpace(avatarKey))
            return null;

        string normalizedKey = avatarKey.Trim().ToLowerInvariant();

        if (AvatarPathMap.TryGetValue(normalizedKey, out string mappedPath))
        {
            Sprite mappedSprite = Resources.Load<Sprite>(mappedPath);
            if (mappedSprite != null)
                return mappedSprite;
        }

        Sprite directSprite = Resources.Load<Sprite>($"Sprites/NPCImages/{avatarKey}");
        if (directSprite != null)
            return directSprite;

        return null;
    }

    private static string ResolveFallbackAvatarKey(BackendElfPrompt prompt)
    {
        string stage = prompt.stage?.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(stage) && !stage.Contains("npc"))
            return "elf_default";

        string taskCode = prompt.taskCode?.Trim().ToUpperInvariant();
        if (taskCode == "M_1_1")
            return "elf_default";
        if (taskCode == "M_1_3")
            return "npc_dorm_manager";
        if (taskCode == "M_1_4")
            return "npc_nurse";

        string npcName = prompt.npcName?.Trim();
        if (!string.IsNullOrWhiteSpace(npcName))
        {
            if (npcName.Contains("宿管") || npcName.Contains("楼管"))
                return "npc_dorm_manager";
            if (npcName.Contains("护士"))
                return "npc_nurse";
            if (npcName.Contains("志愿者") || npcName.Contains("接待"))
                return "npc_reception_volunteer";
        }

        return "elf_default";
    }

    private static string ResolveFallbackNpcName(BackendElfPrompt prompt)
    {
        string taskCode = prompt.taskCode?.Trim().ToUpperInvariant();
        return taskCode switch
        {
            "M_1_1" => "AI小精灵",
            "M_1_3" => "宿管阿姨",
            "M_1_4" => "校医院护士",
            _ => "NPC"
        };
    }
}
