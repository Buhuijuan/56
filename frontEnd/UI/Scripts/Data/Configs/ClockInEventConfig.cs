using System;
using System.Collections.Generic;

[Serializable]
public class ClockInEventConfig
{
    public string eventId;
    public List<ClockInLocation> locations;
    public string refreshTimeString;
    public string openStartTimeString;
    public string openEndTimeString;
    public string description;
    public List<RewardItem> rewards;
    [NonSerialized]
    public TimeSpan refreshTime;
    [NonSerialized]
    public TimeSpan openStartTime;
    [NonSerialized]
    public TimeSpan openEndTime;
}
