using System;
using System.Collections.Generic;

[Serializable]
public class GrowthStageData
{
    public string stageID;
    public string stageTitle;
    public List<GrowthTask> tasks;
    public List<RewardItem> rewards;
    public int titleID;
}
