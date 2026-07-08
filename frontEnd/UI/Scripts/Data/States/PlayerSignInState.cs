using System;
using System.Collections.Generic;

public class PlayerSignInState
{
    public DateTime lastLoginCountDate;
    public DateTime lastDailyResetDate;
    public int todayOnlineMinutes;
    public int todayOnlineSeconds;
    public bool dailySigned;
    public DateTime lastSignInDate;
    public int continuousSignDays;
    public int currentWeekIndex;
    public int totalLoginDays;

    public HashSet<int> onlineRewardClaimed = new();
    public HashSet<int> dailyRewardClaimed = new();
    public HashSet<int> totalRewardClaimed = new();
}
