using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TaskDisplaySystem
{
    public static List<TaskViewData> GetTasks()
    {
        if (BackendTaskStore.Page == null)
        {
            Debug.LogWarning("[TaskDisplaySystem] GetTasks -> BackendTaskStore.Page is null.");
            return new List<TaskViewData>();
        }

        Dictionary<string, BackendTaskEntry> progress = BuildProgressMap();
        List<BackendTaskChapterEntry> unlockedChapters = FilterUnlockedChapters(progress);

        Debug.Log(
            $"[TaskDisplaySystem] GetTasks. AllTaskCount={progress.Count}, " +
            $"ChaptersNull={BackendTaskStore.Chapters == null}, " +
            $"UnlockedChapterCount={unlockedChapters.Count}, " +
            $"CurrentMain={BackendTaskStore.GetCurrentMainTask()?.taskCode}");

        List<TaskViewData> views = unlockedChapters
            .Select(chapter => ToFilteredChapterView(chapter, progress))
            .ToList();

        Debug.Log($"[TaskDisplaySystem] GetTasks result. ViewCount={views.Count}");
        return views;
    }

    public static TaskViewData GetDefaultTask()
    {
        List<TaskViewData> tasks = GetTasks();
        return tasks.Count > 0 ? tasks.First() : null;
    }

    public static TaskViewData FindTask(string taskCodeOrId)
    {
        return GetTasks().FirstOrDefault(task =>
            task.id == taskCodeOrId || task.taskCode == taskCodeOrId);
    }

    public static string MapGoalIdToTaskCode(string goalId)
    {
        return goalId switch
        {
            "CH01_G01" => "M_1_1",
            "CH01_G02" => "M_1_2",
            "CH01_G03" => "M_1_3",
            "CH01_G04" => "M_1_4",

            "CH02_G01" => "M_2_1",
            "CH02_G02" => "M_2_2",
            "CH02_G03" => "M_2_3",

            "CH03_G01" => "M_3_1",
            "CH03_G02" => "M_3_2",
            "CH03_G03" => "M_3_3",
            "CH03_G04" => "M_3_4",

            "BR01_G01" => "B_1_1",
            "BR02_G01" => "B_2_1",

            _ => null
        };
    }

    private static string MapTaskCodeToGoalId(string taskCode)
    {
        return taskCode switch
        {
            "M_1_1" => "CH01_G01",
            "M_1_2" => "CH01_G02",
            "M_1_3" => "CH01_G03",
            "M_1_4" => "CH01_G04",

            "M_2_1" => "CH02_G01",
            "M_2_2" => "CH02_G02",
            "M_2_3" => "CH02_G03",

            "M_3_1" => "CH03_G01",
            "M_3_2" => "CH03_G02",
            "M_3_3" => "CH03_G03",
            "M_3_4" => "CH03_G04",

            "B_1_1" => "BR01_G01",
            "B_2_1" => "BR02_G01",

            _ => null
        };
    }

    private static Dictionary<string, BackendTaskEntry> BuildProgressMap()
    {
        Dictionary<string, BackendTaskEntry> map = BackendTaskStore
            .GetAllTasks()
            .ToDictionary(t => t.taskCode, t => t);

        BackendTaskEntry currentMain = BackendTaskStore.GetCurrentMainTask();
        if (currentMain != null && !string.IsNullOrEmpty(currentMain.taskCode))
            map[currentMain.taskCode] = currentMain;

        return map;
    }

    private static List<BackendTaskChapterEntry> FilterUnlockedChapters(Dictionary<string, BackendTaskEntry> progress)
    {
        List<BackendTaskChapterEntry> configured = FilterConfiguredUnlockedChapters(progress);
        if (configured.Count > 0)
            return configured;

        return BuildFallbackUnlockedChapters(progress);
    }

    private static List<BackendTaskChapterEntry> FilterConfiguredUnlockedChapters(Dictionary<string, BackendTaskEntry> progress)
    {
        List<BackendTaskChapterEntry> result = new();

        if (BackendTaskStore.Chapters == null)
            return result;

        foreach (BackendTaskChapterEntry chapter in BackendTaskStore.Chapters)
        {
            if (chapter?.goals == null || chapter.goals.Count == 0)
                continue;

            string firstGoalTaskCode = MapGoalIdToTaskCode(chapter.goals[0].goalId);
            if (!string.IsNullOrEmpty(firstGoalTaskCode) && progress.ContainsKey(firstGoalTaskCode))
                result.Add(chapter);
        }

        return result;
    }

    private static List<BackendTaskChapterEntry> BuildFallbackUnlockedChapters(Dictionary<string, BackendTaskEntry> progress)
    {
        List<BackendTaskChapterEntry> chapters = progress.Values
            .GroupBy(task => new { task.taskType, task.chapterNo })
            .OrderBy(group => group.Key.taskType == "MAIN" ? 0 : 1)
            .ThenBy(group => group.Key.chapterNo)
            .Select(group =>
            {
                List<BackendTaskEntry> ordered = group.OrderBy(task => task.stepNo).ToList();
                List<BackendTaskGoalEntry> goals = ordered
                    .Select(task => new BackendTaskGoalEntry
                    {
                        goalId = MapTaskCodeToGoalId(task.taskCode),
                        description = task.taskName,
                        completed = task.status == "COMPLETED" || task.status == "CLAIMED"
                    })
                    .Where(goal => !string.IsNullOrEmpty(goal.goalId))
                    .ToList();

                if (goals.Count == 0)
                    return null;

                return new BackendTaskChapterEntry
                {
                    typeString = group.Key.taskType == "MAIN" ? "Main" : "Branch",
                    taskChapterID = BuildFallbackChapterId(group.Key.taskType, group.Key.chapterNo),
                    taskChapterTitle = BuildFallbackChapterTitle(group.Key.taskType, group.Key.chapterNo),
                    taskChapterDescription = BuildFallbackChapterDescription(group.Key.taskType, group.Key.chapterNo),
                    goals = goals
                };
            })
            .Where(chapter => chapter != null)
            .ToList();

        AppendCompletedMainChapters(chapters);

        return chapters
            .OrderBy(chapter => chapter.typeString == "Main" ? 0 : 1)
            .ThenBy(chapter => ParseChapterNo(chapter.taskChapterID))
            .ToList();
    }

    private static void AppendCompletedMainChapters(List<BackendTaskChapterEntry> chapters)
    {
        HashSet<string> existingChapterIds = new(chapters.Select(chapter => chapter.taskChapterID));

        foreach (BackendTaskChapterEntry chapter in BuildStaticMainChapters())
        {
            if (existingChapterIds.Contains(chapter.taskChapterID))
                continue;

            if (!IsChapterCompletedFromTaskState(chapter))
                continue;

            chapters.Add(CloneChapterAsCompleted(chapter));
            existingChapterIds.Add(chapter.taskChapterID);
        }
    }

    private static bool IsChapterCompletedFromTaskState(BackendTaskChapterEntry chapter)
    {
        PlayerTaskState taskState = AccountSystem.GetCurrentRole()?.taskState;
        if (taskState == null || chapter?.goals == null || chapter.goals.Count == 0)
            return false;

        return chapter.goals.All(goal => IsGoalCompletedFromTaskState(goal.goalId, taskState));
    }

    private static bool IsGoalCompletedFromTaskState(string goalId, PlayerTaskState taskState)
    {
        if (taskState == null)
            return false;

        if (taskState.completedGoals != null && taskState.completedGoals.Contains(goalId))
            return true;

        string taskCode = MapGoalIdToTaskCode(goalId);
        if (string.IsNullOrEmpty(taskCode))
            return false;

        if (taskState.completedTasks != null && taskState.completedTasks.Contains(taskCode))
            return true;

        if (taskState.claimedTasks != null && taskState.claimedTasks.Contains(taskCode))
            return true;

        return false;
    }

    private static BackendTaskChapterEntry CloneChapterAsCompleted(BackendTaskChapterEntry source)
    {
        return new BackendTaskChapterEntry
        {
            typeString = source.typeString,
            taskChapterID = source.taskChapterID,
            taskChapterTitle = source.taskChapterTitle,
            taskChapterDescription = source.taskChapterDescription,
            goals = source.goals.Select(goal => new BackendTaskGoalEntry
            {
                goalId = goal.goalId,
                description = goal.description,
                completed = true
            }).ToList()
        };
    }

    private static List<BackendTaskChapterEntry> BuildStaticMainChapters()
    {
        return new List<BackendTaskChapterEntry>
        {
            new BackendTaskChapterEntry
            {
                typeString = "Main",
                taskChapterID = "CH01",
                taskChapterTitle = "第1章",
                taskChapterDescription = "第1章任务",
                goals = BuildGoals(
                    ("CH01_G01", "认识你的AI伙伴"),
                    ("CH01_G02", "前往报到点"),
                    ("CH01_G03", "找到你的宿舍楼"),
                    ("CH01_G04", "前往校医院体检"))
            },
            new BackendTaskChapterEntry
            {
                typeString = "Main",
                taskChapterID = "CH02",
                taskChapterTitle = "第2章",
                taskChapterDescription = "第2章任务",
                goals = BuildGoals(
                    ("CH02_G01", "探访缙湖"),
                    ("CH02_G02", "走近植物园"),
                    ("CH02_G03", "初识图书馆"))
            },
            new BackendTaskChapterEntry
            {
                typeString = "Main",
                taskChapterID = "CH03",
                taskChapterTitle = "第3章",
                taskChapterDescription = "第3章任务",
                goals = BuildGoals(
                    ("CH03_G01", "初识综合楼"),
                    ("CH03_G02", "认识第一教学楼"),
                    ("CH03_G03", "走近第一实验楼"),
                    ("CH03_G04", "探访艺术楼"))
            }
        };
    }

    private static TaskViewData ToFilteredChapterView(
        BackendTaskChapterEntry chapter,
        Dictionary<string, BackendTaskEntry> progress)
    {
        bool isMainType = string.Equals(chapter.typeString, "Main", System.StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(chapter.typeString, "MAIN", System.StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(chapter.typeString, "主线", System.StringComparison.OrdinalIgnoreCase);

        Debug.Log(
            $"[TaskDisplaySystem] ToFilteredChapterView. chapterId={chapter?.taskChapterID}, title={chapter?.taskChapterTitle}, " +
            $"typeString={chapter?.typeString}, resolvedType={(isMainType ? "Main" : "Branch")}, goalCount={chapter?.goals?.Count ?? 0}");

        TaskViewData view = new TaskViewData
        {
            id = chapter.taskChapterID,
            taskCode = chapter.taskChapterID,
            title = chapter.taskChapterTitle,
            description = chapter.taskChapterDescription,
            type = isMainType ? TaskType.Main : TaskType.Branch,
            chapterNo = ParseChapterNo(chapter.taskChapterID),
            isBackend = true,
            goals = new List<TaskGoalViewData>()
        };

        int completedCount = 0;
        int totalGoals = chapter.goals.Count;
        bool foundCurrentGoal = false;

        foreach (BackendTaskGoalEntry goal in chapter.goals)
        {
            bool completed = IsGoalCompleted(goal.goalId, progress);
            if (completed)
            {
                completedCount++;
                view.goals.Add(new TaskGoalViewData
                {
                    id = goal.goalId,
                    description = goal.description,
                    completed = true
                });
                continue;
            }

            if (!foundCurrentGoal)
            {
                foundCurrentGoal = true;
                view.goals.Add(new TaskGoalViewData
                {
                    id = goal.goalId,
                    description = goal.description,
                    completed = false
                });
            }
        }

        view.progressCurrent = completedCount;
        view.progressTarget = totalGoals;
        return view;
    }

    private static bool IsGoalCompleted(string goalId, Dictionary<string, BackendTaskEntry> progress)
    {
        string taskCode = MapGoalIdToTaskCode(goalId);
        if (!string.IsNullOrEmpty(taskCode) &&
            progress.TryGetValue(taskCode, out BackendTaskEntry task) &&
            (task.status == "COMPLETED" || task.status == "CLAIMED"))
        {
            return true;
        }

        return IsGoalCompletedFromTaskState(goalId, AccountSystem.GetCurrentRole()?.taskState);
    }

    private static List<BackendTaskGoalEntry> BuildGoals(params (string goalId, string description)[] goals)
    {
        List<BackendTaskGoalEntry> result = new();
        foreach ((string goalId, string description) in goals)
        {
            result.Add(new BackendTaskGoalEntry
            {
                goalId = goalId,
                description = description,
                completed = false
            });
        }

        return result;
    }

    private static string BuildFallbackChapterId(string taskType, int chapterNo)
    {
        return taskType == "MAIN" ? $"CH{chapterNo:00}" : $"BR{chapterNo - 10:00}";
    }

    private static string BuildFallbackChapterTitle(string taskType, int chapterNo)
    {
        return taskType == "MAIN" ? $"第{chapterNo}章" : $"支线{chapterNo - 10}";
    }

    private static string BuildFallbackChapterDescription(string taskType, int chapterNo)
    {
        return taskType == "MAIN"
            ? $"第{chapterNo}章任务"
            : $"支线任务 {chapterNo - 10}";
    }

    private static int ParseChapterNo(string chapterId)
    {
        if (string.IsNullOrEmpty(chapterId) || chapterId.Length < 4)
            return 0;

        string number = chapterId.Substring(2);
        return int.TryParse(number, out int chapterNo) ? chapterNo : 0;
    }
}
