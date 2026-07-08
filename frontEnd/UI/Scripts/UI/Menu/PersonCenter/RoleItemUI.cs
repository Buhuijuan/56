using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class RoleItemUI : MonoBehaviour
{
    public Image headImage;
    public TMP_Text currentCampus, nickName, level, progress;
    public Button deleteButton;
    private RolePageUI parent;

    private RoleData data;

    public void Setup(RoleData data, RolePageUI parent)
    {
        this.data = data;
        this.parent = parent;

        headImage.sprite = AvatarLoader.LoadAvatar(data);

        currentCampus.text = data.campusName;
        nickName.text = data.nickName;
        level.text = "Lv." + data.levelState.level;
        progress.text = BuildTaskProgressText(data);

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(OnClickDelete);
    }

    public void OnClickDelete()
    {
        UIManager.Instance.ShowConfirm(
            $"确认删除角色 {data.nickName} 吗？",
            "删除后，该角色的所有游戏进度将被清空。此操作无法撤销。",
            () =>
            {
                parent.StartCoroutine(AccountSystem.DeleteRole(data, (ok, msg) =>
                {
                    if (ok)
                    {
                        parent.Refresh();
                    }
                    else
                    {
                        UIManager.Instance.ShowRemind("删除失败", "知道了", msg);
                    }
                }));
            }
        );
    }

    private static string BuildTaskProgressText(RoleData role)
    {
        if (TryGetBackendCurrentRoleTaskProgress(role, out int backendCompleted, out int backendTotal))
            return FormatPercent(backendCompleted, backendTotal);

        if (TryGetLocalTaskProgress(role, out int localCompleted, out int localTotal))
            return FormatPercent(localCompleted, localTotal);

        return "0%";
    }

    private static bool TryGetBackendCurrentRoleTaskProgress(RoleData role, out int completed, out int total)
    {
        completed = 0;
        total = 0;

        RoleData currentRole = AccountSystem.GetCurrentRole();
        bool isCurrentRole = role != null &&
                             currentRole != null &&
                             !string.IsNullOrEmpty(role.roleID) &&
                             role.roleID == currentRole.roleID;
        if (!BackendSettings.UseBackendMode || !isCurrentRole)
            return false;

        List<BackendTaskEntry> tasks = BackendTaskStore.GetAllTasks();
        if (tasks == null || tasks.Count == 0)
            return false;

        HashSet<string> allCodes = new HashSet<string>();
        HashSet<string> doneCodes = new HashSet<string>();
        foreach (BackendTaskEntry task in tasks)
        {
            if (task == null || string.IsNullOrEmpty(task.taskCode))
                continue;

            allCodes.Add(task.taskCode);
            if (task.status == "COMPLETED" || task.status == "CLAIMED")
                doneCodes.Add(task.taskCode);
        }

        total = allCodes.Count;
        completed = Mathf.Clamp(doneCodes.Count, 0, total);
        return total > 0;
    }

    private static bool TryGetLocalTaskProgress(RoleData role, out int completed, out int total)
    {
        completed = 0;
        total = 0;

        List<TaskData> taskConfigs = TaskSystem.GetTaskDatas();
        if (taskConfigs == null || taskConfigs.Count == 0)
            return false;

        HashSet<string> allTaskCodes = new HashSet<string>();
        Dictionary<string, string> goalToCode = new Dictionary<string, string>();
        foreach (TaskData task in taskConfigs)
        {
            if (task == null || task.goals == null)
                continue;

            foreach (TaskGoal goal in task.goals)
            {
                if (goal == null || string.IsNullOrEmpty(goal.goalId))
                    continue;

                string code = TaskDisplaySystem.MapGoalIdToTaskCode(goal.goalId);
                if (string.IsNullOrEmpty(code))
                    continue;

                allTaskCodes.Add(code);
                goalToCode[goal.goalId] = code;
            }
        }

        total = allTaskCodes.Count;
        if (total <= 0)
            return false;

        HashSet<string> doneCodes = new HashSet<string>();
        if (role?.taskState?.completedTasks != null)
        {
            foreach (string id in role.taskState.completedTasks)
            {
                if (!string.IsNullOrEmpty(id) && allTaskCodes.Contains(id))
                    doneCodes.Add(id);
            }
        }

        if (role?.taskState?.completedGoals != null)
        {
            foreach (string goalId in role.taskState.completedGoals)
            {
                if (string.IsNullOrEmpty(goalId))
                    continue;
                if (goalToCode.TryGetValue(goalId, out string code) && !string.IsNullOrEmpty(code))
                    doneCodes.Add(code);
            }
        }

        completed = Mathf.Clamp(doneCodes.Count, 0, total);
        return true;
    }

    private static string FormatPercent(int completed, int total)
    {
        if (total <= 0)
            return "0%";

        int percent = Mathf.RoundToInt(Mathf.Clamp(completed, 0, total) * 100f / total);
        return $"{percent}%";
    }

}
