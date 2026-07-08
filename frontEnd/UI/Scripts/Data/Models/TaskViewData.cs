using System.Collections.Generic;

public class TaskViewData
{
    public string id;
    public string taskCode;
    public string title;
    public string description;
    public TaskType type;
    public int chapterNo;
    public int stepNo;
    public string status;
    public int progressCurrent;
    public int progressTarget;
    public int rewardExp;
    public int rewardCoin;
    public string rewardUnlockFeature;
    public string targetType;
    public long targetId;
    public bool isBackend;
    public List<TaskGoalViewData> goals = new List<TaskGoalViewData>();
}

public class TaskGoalViewData
{
    public string id;
    public string description;
    public bool completed;
}
