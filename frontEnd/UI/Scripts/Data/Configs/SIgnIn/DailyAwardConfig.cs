using System;
using System.Collections.Generic;

[Serializable]
public class DailyAwardConfig
{
    public int baseAwardID;
    public int dayIndex;
    public RewardItem baseReward;
    public int? extraAwardID;
    public RewardItem extraReward;
}
