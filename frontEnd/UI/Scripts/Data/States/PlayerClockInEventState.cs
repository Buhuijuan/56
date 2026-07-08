using System;
using System.Collections.Generic;

[Serializable]
public class PlayerClockInEventState
{
    public DateTime lastCheckInDate;
    public int totalCheckInCount;
    public Dictionary<string, bool> checkedIn;
    public string lastCheckedLocationId;
    public List<string> historyRecords;
}
