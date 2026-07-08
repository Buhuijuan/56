using System;
using System.Collections.Generic;

[Serializable]
public class PlayerGrowthState
{
    public HashSet<string> stageCompleted = new();
    public HashSet<string> rewardClaimed = new();
    public HashSet<string> taskCompleted = new();
}
