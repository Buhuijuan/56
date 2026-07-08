using System;
using System.Collections.Generic;

[Serializable]
public class StoryEventConfig
{
    public string eventId;
    public string theme;
    public string themeDescription;
    public string startTimeString;
    [NonSerialized]
    public DateTime startTime;
    public int durationDays;
    public List<RewardItem> rewards;
}
