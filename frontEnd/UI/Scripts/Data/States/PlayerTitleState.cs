using System;
using System.Collections.Generic;

[Serializable]
public class PlayerTitleState
{
    public HashSet<int> unlockedTitles = new();
    public HashSet<int> backendUnlockedTitles = new();
    public HashSet<int> extraUnlockedTitles = new();
    public int equippedTitleID = 0;
    public bool hasCreatedCharacter;
    public bool hasEnteredCampusOnce;
    public HashSet<string> talkedNpcIds = new();
    public Dictionary<string, int> npcConversationCounts = new();
    public int petInteractionCount;
    public int elfAnswerCount;
    public HashSet<string> savedPhotoIds = new();
    public int bikeRideCount;
}
