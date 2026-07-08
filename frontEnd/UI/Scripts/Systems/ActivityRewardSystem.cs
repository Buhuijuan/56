using System.Collections.Generic;
using UnityEngine;

public static class ActivityRewardSystem
{
    public static RewardGrantResult GrantQuizSessionReward(int correctCount)
    {
        return GrantRewards("校园问答", 0, null);
    }

    public static RewardGrantResult GrantClockInReward(List<RewardItem> rewards)
    {
        return GrantRewards("晨光打卡", 0, rewards);
    }

    public static int GetMemorialCoin()
    {
        RoleData role = AccountSystem.GetCurrentRole();
        return role?.rewardState != null ? role.rewardState.totalMemorialCoin : 0;
    }

    public static int GetOwnedRewardCount(int rewardId)
    {
        RoleData role = AccountSystem.GetCurrentRole();
        return role?.rewardState != null ? role.rewardState.GetRewardCount(rewardId) : 0;
    }

    public static RewardGrantResult GrantRewards(string source, int expAmount, List<RewardItem> rewards)
    {
        RoleData role = AccountSystem.GetCurrentRole();
        if (role == null)
            return new RewardGrantResult(source, 0, 0, new List<RewardItem>());

        role.rewardState ??= new PlayerRewardState();
        role.levelState ??= new PlayerLevelState();

        int previousLevel = role.levelState.level <= 0 ? 1 : role.levelState.level;
        role.levelState.level = previousLevel;

        if (expAmount > 0)
        {
            role.levelState.exp += expAmount;
            role.rewardState.totalActivityExpGained += expAmount;
            RecalculateLevel(role.levelState);
        }

        List<RewardItem> grantedRewards = new();
        if (rewards != null)
        {
            foreach (RewardItem reward in rewards)
            {
                if (reward == null || reward.amount <= 0)
                    continue;

                role.rewardState.AddReward(reward.rewardId, reward.amount);
                grantedRewards.Add(new RewardItem
                {
                    rewardId = reward.rewardId,
                    rewardName = reward.rewardName,
                    rewardSprite = reward.rewardSprite,
                    spritePath = reward.spritePath,
                    amount = reward.amount
                });
            }
        }

        int levelGained = Mathf.Max(0, role.levelState.level - previousLevel);
        TitleSystem.RefreshUnlockedTitles();
        HeadAreaUI.RefreshAll();
        LocalProfileSaveSystem.SaveCurrentAccount();
        return new RewardGrantResult(source, expAmount, levelGained, grantedRewards);
    }

    private static void RecalculateLevel(PlayerLevelState levelState)
    {
        int newLevel = 1;
        int lastValidRequiredExp = int.MinValue;
        foreach (LevelConfig config in LevelSystem.GetAllLevelConfigs())
        {
            if (config.requiredExp < lastValidRequiredExp)
                continue;

            if (levelState.exp >= config.requiredExp)
                newLevel = Mathf.Max(newLevel, config.level);

            lastValidRequiredExp = config.requiredExp;
        }

        levelState.level = newLevel;

        LevelConfig currentConfig = LevelSystem.GetLevelConfig(newLevel);
        LevelConfig nextConfig = LevelSystem.GetLevelConfig(newLevel + 1);
        if (currentConfig == null || nextConfig == null || nextConfig.requiredExp <= currentConfig.requiredExp)
        {
            levelState.progress = 1f;
            return;
        }

        float currentRequired = currentConfig.requiredExp;
        float nextRequired = nextConfig.requiredExp;
        levelState.progress = Mathf.Clamp01((levelState.exp - currentRequired) / (nextRequired - currentRequired));
    }
}

public readonly struct RewardGrantResult
{
    public readonly string source;
    public readonly int expGained;
    public readonly int levelsGained;
    public readonly List<RewardItem> rewards;

    public RewardGrantResult(string source, int expGained, int levelsGained, List<RewardItem> rewards)
    {
        this.source = source;
        this.expGained = expGained;
        this.levelsGained = levelsGained;
        this.rewards = rewards ?? new List<RewardItem>();
    }

    public string BuildSummary()
    {
        string text = $"{source}奖励已发放。";
        if (expGained > 0)
            text += $"\n获得经验：{expGained}";
        if (levelsGained > 0)
            text += $"\n提升等级：{levelsGained}";
        if (rewards != null && rewards.Count > 0)
            text += $"\n获得物品：{string.Join("，", rewards.ConvertAll(item => $"{item.rewardName} x{item.amount}"))}";
        return text;
    }
}
