using System;
using System.Linq;
using UnityEngine;

public static class TitleProgressSystem
{
    private static readonly string[] CampusSceneNames =
    {
        "04_Campus",
        "05_Campus",
        "05_Game"
    };

    private static PlayerTitleState GetState()
    {
        RoleData role = AccountSystem.GetCurrentRole();
        if (role == null)
            return null;

        role.titleState ??= new PlayerTitleState();
        role.titleState.talkedNpcIds ??= new System.Collections.Generic.HashSet<string>();
        role.titleState.npcConversationCounts ??= new System.Collections.Generic.Dictionary<string, int>();
        role.titleState.savedPhotoIds ??= new System.Collections.Generic.HashSet<string>();
        return role.titleState;
    }

    public static void MarkCharacterCreated()
    {
        PlayerTitleState state = GetState();
        if (state == null)
            return;

        state.hasCreatedCharacter = true;
        TitleSystem.RefreshUnlockedTitles();
        LocalProfileSaveSystem.SaveCurrentAccount();
    }

    public static void MarkSceneEntered(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        bool isCampusScene = CampusSceneNames.Any(name => string.Equals(sceneName, name, StringComparison.Ordinal));
        if (!isCampusScene)
            return;

        PlayerTitleState state = GetState();
        if (state == null)
            return;

        state.hasEnteredCampusOnce = true;
        TitleSystem.RefreshUnlockedTitles();
        LocalProfileSaveSystem.SaveCurrentAccount();
    }

    public static void RecordNpcConversation(string npcId)
    {
        if (string.IsNullOrWhiteSpace(npcId))
            return;

        PlayerTitleState state = GetState();
        if (state == null)
            return;

        state.talkedNpcIds.Add(npcId);
        state.npcConversationCounts.TryGetValue(npcId, out int count);
        state.npcConversationCounts[npcId] = count + 1;
        TitleSystem.RefreshUnlockedTitles();
        LocalProfileSaveSystem.SaveCurrentAccount();
    }

    public static void RecordPetInteraction()
    {
        PlayerTitleState state = GetState();
        if (state == null)
            return;

        state.petInteractionCount++;
        TitleSystem.RefreshUnlockedTitles();
        LocalProfileSaveSystem.SaveCurrentAccount();
    }

    public static void RecordElfAnswer()
    {
        PlayerTitleState state = GetState();
        if (state == null)
            return;

        state.elfAnswerCount++;
        TitleSystem.RefreshUnlockedTitles();
        LocalProfileSaveSystem.SaveCurrentAccount();
    }

    public static void RecordSavedPhoto(string photoId)
    {
        if (string.IsNullOrWhiteSpace(photoId))
            photoId = $"photo_{DateTime.UtcNow.Ticks}";

        PlayerTitleState state = GetState();
        if (state == null)
            return;

        state.savedPhotoIds.Add(photoId);
        TitleSystem.RefreshUnlockedTitles();
        LocalProfileSaveSystem.SaveCurrentAccount();
    }

    public static void RecordBikeRide()
    {
        PlayerTitleState state = GetState();
        if (state == null)
            return;

        state.bikeRideCount++;
        TitleSystem.RefreshUnlockedTitles();
        LocalProfileSaveSystem.SaveCurrentAccount();
    }
}
