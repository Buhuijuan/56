using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class TaskData
{
    [NonSerialized]
    public TaskType type;
    public string typeString;
    public string taskChapterID;
    public string taskChapterTitle;
    public string taskChapterDescription;
    public List<TaskGoal> goals;
    public int TotalNumber => goals.Count;
}
