using System;
using System.Collections.Generic;

[Serializable]
public class PlayerStoryEventState
{
    public string eventId;
    public bool hasFinished;
    public bool rewardClaimed;
    public DateTime lastPlayDate;
    public string currentStoryId;
    public HashSet<string> uploadedStoryIds = new();
}
