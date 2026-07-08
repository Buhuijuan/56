using System;
using System.Collections.Generic;

[Serializable]
public class QuizEventConfig
{
    public string eventId;
    public string theme;
    public string startTimeString;
    [NonSerialized]
    public DateTime startTime;
    public int totalQuestions;
    public int durationDays;
    public string questionsFile;
    public List<RewardItem> finalRewards;
}
