using System;
using System.Collections.Generic;

[Serializable]
public class PlayerLevelState
{
    public int level;
    public float progress;
    public int exp;
    public HashSet<int> rewardClaimed = new();
    public HashSet<int> boxOpened = new();
}
