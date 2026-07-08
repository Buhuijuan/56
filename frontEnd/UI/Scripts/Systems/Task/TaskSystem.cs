using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TaskSystem
{
    public static PlayerTaskState taskState => AccountSystem.GetCurrentRole().taskState;
    private static List<TaskData> tasks;
    static TaskSystem()
    {
        LoadTaskConfig();
    }
    public static void LoadTaskConfig()
    {
        TextAsset json = Resources.Load<TextAsset>("Jsons/TaskConfig");
        tasks = JsonUtility.FromJson<TaskConfigListWrapper>(json.text).tasks;
        foreach (var task in tasks)
        {
            if (!Enum.TryParse(task.typeString, out TaskType parsed))
            {
                parsed = TaskType.Main;
            }
            task.type = parsed;
        }
    }
    public static List<TaskData> GetTaskDatas()
    {
        return tasks;
    }
    public static int GetFinishedNumber(TaskData task)
    {
        return task.goals.Count(g => taskState.completedGoals.Contains(g.goalId));
    }

    public static bool IsTaskCompleted(TaskData task)
    {
        return taskState.completedTasks.Contains(task.taskChapterID);
    }
    [Serializable]
    public class TaskConfigListWrapper
    {
        public List<TaskData> tasks;
    }
}
