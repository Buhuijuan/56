using System;
using System.Collections.Generic;

[Serializable]
public class OnlineAwardConfig
{
    public int awardID;
    public int requiredMinutes;
    public List<RewardItem> rewards;
}
