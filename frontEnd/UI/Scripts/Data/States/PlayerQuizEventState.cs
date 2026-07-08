using System;
using System.Collections.Generic;

[Serializable]
public class PlayerQuizEventState
{
    public string eventId;
    public int weeklyScore;
    public int totalCorrectAnswers;
    public int totalSessions;
    public DateTime lastPlayDate;
    public bool hasPlayedToday;
    public string rewardClaimedEventId;
    public int claimedWeeklyCoinAmount;
    public List<string> answerHistory;
}
