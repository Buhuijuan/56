using System.Collections.Generic;
using System.Linq;

public static class BackendTaskStore
{
    public static BackendTaskPageData Page;
    public static List<BackendTaskChapterEntry> Chapters;

    public static void Clear()
    {
        Page = null;
        Chapters = null;
    }

    public static void SetPage(BackendTaskPageData data)
    {
        Page = data;
    }

    public static void SetChapters(List<BackendTaskChapterEntry> chapters)
    {
        Chapters = chapters;
    }

    public static List<BackendTaskEntry> GetAllTasks()
    {
        if (Page == null)
            return new List<BackendTaskEntry>();

        Dictionary<string, BackendTaskEntry> map = new Dictionary<string, BackendTaskEntry>();
        AddEntries(map, Page.acceptedTasks);
        AddEntries(map, Page.availableTasks);
        AddEntries(map, Page.completedTasks);
        AddEntries(map, Page.claimedTasks);

        return map.Values
            .OrderBy(entry => entry.taskType == "MAIN" ? 0 : 1)
            .ThenBy(entry => entry.chapterNo)
            .ThenBy(entry => entry.stepNo)
            .ToList();
    }

    public static BackendTaskEntry GetCurrentMainTask()
    {
        return Page != null ? Page.currentMainTask : null;
    }

    public static BackendTaskEntry FindTask(string taskCode)
    {
        if (string.IsNullOrEmpty(taskCode))
            return null;

        return GetAllTasks().FirstOrDefault(task => task.taskCode == taskCode);
    }

    private static void AddEntries(Dictionary<string, BackendTaskEntry> map, List<BackendTaskEntry> entries)
    {
        if (entries == null)
            return;

        foreach (BackendTaskEntry entry in entries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.taskCode))
                continue;

            map[entry.taskCode] = entry;
        }
    }
}
