using System.Collections.Generic;
using UnityEngine;

public static class RewardItemSystem
{
    private static List<RewardItem> rewards;

    static RewardItemSystem()
    {
        LoadRewardItems();
    }
    private static void LoadRewardItems()
    {
        TextAsset json = Resources.Load<TextAsset>("Jsons/RewardItem");
        rewards = JsonUtility.FromJson<RewardItemListWrapper>(json.text).rewards;

        foreach (var reward in rewards)
        {
            reward.rewardSprite = Resources.Load<Sprite>(reward.spritePath);
            Debug.Log($"加载图标: {reward.rewardId} | 路径: {reward.spritePath} | Sprite: {reward.rewardSprite}");

        }

    }
    public static RewardItem GetRewardItem(int rewardId)
    {
        return rewards.Find(i => i.rewardId == rewardId);
    }

    public static void EnrichReward(RewardItem reward)
    {
        if (reward == null)
            return;

        RewardItem baseItem = GetRewardItem(reward.rewardId);
        if (baseItem == null)
            return;

        reward.rewardName = string.IsNullOrWhiteSpace(reward.rewardName) ? baseItem.rewardName : reward.rewardName;
        reward.spritePath = string.IsNullOrWhiteSpace(reward.spritePath) ? baseItem.spritePath : reward.spritePath;
        reward.rewardSprite = baseItem.rewardSprite;
    }

    public static void EnrichRewards(IEnumerable<RewardItem> rewardItems)
    {
        if (rewardItems == null)
            return;

        foreach (RewardItem reward in rewardItems)
            EnrichReward(reward);
    }

    public static List<RewardItem> GetAllItems()
    {
        return rewards;
    }
    [System.Serializable]
    public class RewardItemListWrapper
    {
        public List<RewardItem> rewards;
    }
}
