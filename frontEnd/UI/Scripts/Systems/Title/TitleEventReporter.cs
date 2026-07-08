using UnityEngine;

public static class TitleEventReporter
{
    public static void ReportCharacterCreated()
    {
        TitleProgressSystem.MarkCharacterCreated();
    }

    public static void ReportSceneEntered(string sceneName)
    {
        TitleProgressSystem.MarkSceneEntered(sceneName);
    }

    public static void ReportNpcDialogue(string npcId)
    {
        TitleProgressSystem.RecordNpcConversation(npcId);
    }

    public static void ReportPetInteraction()
    {
        TitleProgressSystem.RecordPetInteraction();
    }

    public static void ReportElfAnswer()
    {
        TitleProgressSystem.RecordElfAnswer();
    }

    public static void ReportSavedPhoto(string photoId)
    {
        TitleProgressSystem.RecordSavedPhoto(photoId);
    }

    public static void ReportBikeRide()
    {
        TitleProgressSystem.RecordBikeRide();
    }

    public static void ReportClockInSuccess(string locationId, string locationName)
    {
        RoleData role = AccountSystem.GetCurrentRole();
        if (role?.clockInEventState == null || string.IsNullOrWhiteSpace(locationId))
            return;

        role.clockInEventState.checkedIn ??= new System.Collections.Generic.Dictionary<string, bool>();
        if (!role.clockInEventState.checkedIn.TryGetValue(locationId, out bool alreadyChecked) || !alreadyChecked)
            role.clockInEventState.totalCheckInCount++;

        role.clockInEventState.checkedIn[locationId] = true;
        role.clockInEventState.lastCheckInDate = System.DateTime.Today;
        role.clockInEventState.lastCheckedLocationId = locationId;
        role.clockInEventState.historyRecords ??= new System.Collections.Generic.List<string>();

        string displayName = string.IsNullOrWhiteSpace(locationName) ? locationId : locationName;
        role.clockInEventState.historyRecords.Add($"{System.DateTime.Today:yyyy-MM-dd} | {displayName}");

        LocalProfileSaveSystem.SaveCurrentAccount();
    }

    public static void ReportQuizCorrect(int correctCount)
    {
        if (correctCount <= 0)
            return;

        RoleData role = AccountSystem.GetCurrentRole();
        if (role?.quizEventState == null)
            return;

        role.quizEventState.totalCorrectAnswers += correctCount;
        role.quizEventState.totalSessions++;
        LocalProfileSaveSystem.SaveCurrentAccount();
    }

    public static void ReportStoryFinished(string storyId)
    {
        RoleData role = AccountSystem.GetCurrentRole();
        if (role?.storyEventState == null)
            return;

        if (!string.IsNullOrWhiteSpace(storyId))
            role.storyEventState.currentStoryId = storyId;

        role.storyEventState.hasFinished = true;
        role.storyEventState.lastPlayDate = System.DateTime.Today;
        LocalProfileSaveSystem.SaveCurrentAccount();
    }
}
