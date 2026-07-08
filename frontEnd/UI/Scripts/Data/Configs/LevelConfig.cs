using System;
using System.Collections.Generic;

[Serializable]
public class LevelConfig
{
    public int level;
    public int requiredExp;
    public List<RewardItem> rewards;
    public int titleID;
}
