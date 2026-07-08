using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LevelSystem
{
    public static RoleData currentRole => AccountSystem.GetCurrentRole();
    public static PlayerLevelState currentRoleLevelState => currentRole.levelState;
    public static int currentLevel => currentRoleLevelState.level;
    public static int currentExp => currentRoleLevelState.exp;
    private static List<LevelConfig> levelConfigs;
    static LevelSystem()
    {
        LoadLevelConfigs();
    }
    private static void LoadLevelConfigs()
    {
        TextAsset json = Resources.Load<TextAsset>("Jsons/LevelConfig");
        levelConfigs = JsonUtility.FromJson<LevelConfigListWrapper>(json.text).levels;

        foreach (var config in levelConfigs)
        {
            foreach (var reward in config.rewards)
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
    public static LevelConfig GetLevelConfig(int level)
    {
        return levelConfigs.Find(i => i.level == level);
    }
    public static List<LevelConfig> GetAllLevelConfigs()
    {
        return levelConfigs;
    }
    public static int GetLastUnClaimedLevel()
    {
        for (int level = 1; level <= currentLevel; level++)
        {
            if (!IsRewardClaimed(level))
            {
                return level;
            }
        }
        return currentLevel + 1;
    }
    public static bool IsRewardClaimed(int level)
    {
        return currentRoleLevelState.rewardClaimed.Contains(level);
    }
    public static bool IsBoxOpened(int level)
    {
        return currentRoleLevelState.boxOpened.Contains(level);
    }
    public static void ClaimReward(int level)
    {
        currentRoleLevelState.rewardClaimed.Add(level);
        LocalProfileSaveSystem.SaveCurrentAccount();
    }
    public static void OpenBox(int level)
    {
        currentRoleLevelState.boxOpened.Add(level);
        LocalProfileSaveSystem.SaveCurrentAccount();
    }
    [System.Serializable]
    public class LevelConfigListWrapper
    {
        public List<LevelConfig> levels;
    }
}
