using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class GrowthSystem : MonoBehaviour
{
    public static RoleData currentRole => AccountSystem.GetCurrentRole();
    public static PlayerGrowthState currentRoleGrowthState => currentRole?.growthState;
    private static List<GrowthStageData> growthStages;
    private static readonly HashSet<string> CoreBuildingGoalIds = new()
    {
        "CH01_G02", "CH01_G03", "CH01_G04",
        "CH02_G01", "CH02_G02", "CH02_G03",
        "CH03_G01", "CH03_G02", "CH03_G03", "CH03_G04"
    };
    static GrowthSystem()
    {
        LoadGrowthConfig();
    }
    public static void LoadGrowthConfig()
    {
        if (BackendSettings.UseBackendMode && SessionStore.HasToken)
            return;

        TextAsset json = Resources.Load<TextAsset>("Jsons/GrowthConfig");
        growthStages = JsonUtility.FromJson<GrowthConfigListWrapper>(json.text).stages;
        BindRewardMetadata(growthStages);
    }

    public static void SetGrowthConfig(List<GrowthStageData> configStages)
    {
        growthStages = configStages != null
            ? new List<GrowthStageData>(configStages)
            : new List<GrowthStageData>();

        BindRewardMetadata(growthStages);
    }

    private static void BindRewardMetadata(List<GrowthStageData> stages)
    {
        if (stages == null)
            return;

        foreach (var stage in growthStages)
        {
            if (stage.rewards == null) continue;
            foreach (var reward in stage.rewards)
            {
                RewardItem baseItem = RewardItemSystem.GetRewardItem(reward.rewardId);
                if (baseItem != null)
                {
                    reward.rewardName = baseItem.rewardName;
                    reward.rewardSprite = baseItem.rewardSprite;
                }
            }
        }
    }
    public static List<GrowthStageData> GetStageDatas()
    {
        RefreshProgress();
        return growthStages ?? new List<GrowthStageData>();
    }
    public static bool IsTaskCompleted(string taskID)
    {
        RefreshProgress();
        return currentRoleGrowthState != null
               && currentRoleGrowthState.taskCompleted != null
               && currentRoleGrowthState.taskCompleted.Contains(taskID);
    }
    public static bool IsStageCompleted(string stageID)
    {
        RefreshProgress();
        return currentRoleGrowthState != null
               && currentRoleGrowthState.stageCompleted != null
               && currentRoleGrowthState.stageCompleted.Contains(stageID);
    }

    public static bool IsRewardClaimed(string stageID)
    {
        RefreshProgress();
        return currentRoleGrowthState != null
               && currentRoleGrowthState.rewardClaimed != null
               && currentRoleGrowthState.rewardClaimed.Contains(stageID);
    }

    public static bool CanClaimReward(string stageID)
    {
        RefreshProgress();
        return IsStageCompleted(stageID) && !IsRewardClaimed(stageID);
    }

    public static RewardGrantResult ClaimStageReward(string stageID)
    {
        if (BackendSettings.UseBackendMode && SessionStore.HasToken)
            return default;

        RefreshProgress();
        if (!CanClaimReward(stageID))
            return default;

        GrowthStageData stage = growthStages.Find(i => i.stageID == stageID);
        if (stage == null)
            return default;

        currentRoleGrowthState.rewardClaimed.Add(stageID);
        RewardGrantResult result = ActivityRewardSystem.GrantRewards($"成长阶段：{stage.stageTitle}", 0, CloneRewards(stage.rewards));

        if (stage.titleID > 0)
            TitleSystem.UnLockTitle(stage.titleID);

        TitleSystem.RefreshUnlockedTitles();
        LocalProfileSaveSystem.SaveCurrentAccount();
        return result;
    }

    public static void RefreshProgress()
    {
        if (currentRole == null || currentRoleGrowthState == null)
            return;

        currentRoleGrowthState.taskCompleted ??= new HashSet<string>();
        currentRoleGrowthState.stageCompleted ??= new HashSet<string>();
        currentRoleGrowthState.rewardClaimed ??= new HashSet<string>();

        if (BackendSettings.UseBackendMode && SessionStore.HasToken)
            return;
        if (growthStages == null)
            return;

        bool changed = false;

        foreach (GrowthStageData stage in growthStages)
        {
            if (stage.tasks == null)
                continue;

            foreach (GrowthTask task in stage.tasks)
            {
                if (task == null || string.IsNullOrWhiteSpace(task.taskId))
                    continue;

                if (!currentRoleGrowthState.taskCompleted.Contains(task.taskId) && EvaluateTaskCondition(task))
                {
                    currentRoleGrowthState.taskCompleted.Add(task.taskId);
                    changed = true;
                }
            }

            bool allTasksCompleted = stage.tasks.All(task => task != null && currentRoleGrowthState.taskCompleted.Contains(task.taskId));
            if (allTasksCompleted && !currentRoleGrowthState.stageCompleted.Contains(stage.stageID))
            {
                currentRoleGrowthState.stageCompleted.Add(stage.stageID);
                changed = true;
            }
        }

        if (changed)
        {
            TitleSystem.RefreshUnlockedTitles();
            LocalProfileSaveSystem.SaveCurrentAccount();
        }
    }

    public static void CompleteTask(string taskID)
    {
        if (currentRoleGrowthState == null)
            return;

        currentRoleGrowthState.taskCompleted.Add(taskID);
        RefreshProgress();
        LocalProfileSaveSystem.SaveCurrentAccount();
    }
    public static void CompleteStage(string stageID)
    {
        if (currentRoleGrowthState == null)
            return;

        currentRoleGrowthState.stageCompleted.Add(stageID);
        RefreshProgress();
        LocalProfileSaveSystem.SaveCurrentAccount();
    }

    private static bool EvaluateTaskCondition(GrowthTask task)
    {
        string description = task.description ?? string.Empty;

        if (description.Contains("报到日"))
            return TaskSystem.taskState.completedTasks.Contains("CH01");

        if (description.Contains("核心建筑"))
            return CoreBuildingGoalIds.Count(goalId => TaskSystem.taskState.completedGoals.Contains(goalId)) >= 3;

        if (description.Contains("累计登录天数"))
        {
            Match match = Regex.Match(description, @"(\d+)");
            int requiredDays = match.Success ? int.Parse(match.Value) : 3;
            return currentRole.signInState != null && currentRole.signInState.totalLoginDays >= requiredDays;
        }

        return false;
    }

    private static List<RewardItem> CloneRewards(List<RewardItem> rewards)
    {
        if (rewards == null)
            return new List<RewardItem>();

        return rewards
            .Where(i => i != null)
            .Select(i => new RewardItem
            {
                rewardId = i.rewardId,
                rewardName = i.rewardName,
                rewardSprite = i.rewardSprite,
                spritePath = i.spritePath,
                amount = i.amount
            })
            .ToList();
    }

    [Serializable]
    public class GrowthConfigListWrapper
    {
        public List<GrowthStageData> stages;
    }
}
