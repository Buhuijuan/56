using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class PlayerRewardState
{
    public int totalActivityExpGained;
    public int totalMemorialCoin;
    public int syncedBackendMemorialCoin;
    public List<RewardEntry> ownedRewards = new();

    public void ApplyBackendMemorialCoin(int backendCoin)
    {
        int localDelta = totalMemorialCoin - syncedBackendMemorialCoin;
        if (localDelta < 0)
            localDelta = 0;

        syncedBackendMemorialCoin = backendCoin;
        totalMemorialCoin = backendCoin + localDelta;
    }

    public int GetRewardCount(int rewardId)
    {
        RewardEntry entry = ownedRewards.FirstOrDefault(i => i.rewardId == rewardId);
        return entry != null ? entry.amount : 0;
    }

    public void AddReward(int rewardId, int amount)
    {
        if (amount <= 0)
            return;

        RewardEntry entry = ownedRewards.FirstOrDefault(i => i.rewardId == rewardId);
        if (entry == null)
        {
            entry = new RewardEntry { rewardId = rewardId, amount = amount };
            ownedRewards.Add(entry);
        }
        else
        {
            entry.amount += amount;
        }

        if (rewardId == 1)
            totalMemorialCoin += amount;
    }
}

[Serializable]
public class RewardEntry
{
    public int rewardId;
    public int amount;
}
