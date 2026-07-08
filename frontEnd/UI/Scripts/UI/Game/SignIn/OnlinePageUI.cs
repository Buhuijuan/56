using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlinePageUI : PageUI
{
    public Transform awardRoot;
    public OnlineAwardItemUI awardPrefab;

    private readonly List<OnlineAwardItemUI> generatedItems = new();

    private void OnEnable()
    {
        StartCoroutine(RefreshRemote());
    }

    private IEnumerator RefreshRemote()
    {
        yield return BackendFacade.RefreshSignIn(result =>
        {
            if (!result.Success || result.Data == null || !result.Data.success || result.Data.data == null)
            {
                UIManager.Instance.ShowRemind(
                    "在线奖励加载失败",
                    "知道了",
                    string.IsNullOrEmpty(result.Message) ? "无法加载在线奖励。" : result.Message);
                return;
            }

            PlayerSignInState state = BackendStateMapper.ToFrontSignIn(result.Data.data.state);
            List<OnlineAwardConfig> configs = result.Data.data.onlineAwards;
            EnrichRewardSprites(configs);
            Render(state, configs);
        });
    }

    private static void EnrichRewardSprites(List<OnlineAwardConfig> configs)
    {
        if (configs == null)
            return;

        foreach (OnlineAwardConfig config in configs)
            RewardItemSystem.EnrichRewards(config.rewards);
    }

    private void Render(PlayerSignInState signInState, List<OnlineAwardConfig> configs)
    {
        Clear();

        foreach (OnlineAwardConfig config in configs)
        {
            OnlineAwardItemUI item = Instantiate(awardPrefab, awardRoot);

            if (config.requiredMinutes > signInState.todayOnlineMinutes)
                item.SetCurrentState(AwardStateType.Locked);
            else if (!signInState.onlineRewardClaimed.Contains(config.awardID))
                item.SetCurrentState(AwardStateType.UnClaimed);
            else
                item.SetCurrentState(AwardStateType.Claimed);

            item.Setup(config, () =>
            {
                if (item.GetCurrentState() == AwardStateType.UnClaimed)
                    StartCoroutine(ClaimOnlineAndRefresh(config));
            });

            item.UpdateButtonText(signInState.todayOnlineMinutes);
            generatedItems.Add(item);
        }
    }

    private IEnumerator ClaimOnlineAndRefresh(OnlineAwardConfig config)
    {
        bool success = false;
        int addedCoin = 0;

        yield return SignInSystem.OnlineTimeModule.ClaimOnlineAward(config.awardID, (ok, msg, coin) =>
        {
            if (!ok)
            {
                UIManager.Instance.ShowRemind("领取失败", "知道了", string.IsNullOrEmpty(msg) ? "在线奖励领取失败。" : msg);
                return;
            }

            success = true;
            addedCoin = coin;
        });

        if (!success)
            yield break;

        yield return BackendFacade.RefreshHome(null);
        yield return BackendFacade.RefreshGrowth(null);
        yield return RefreshRemote();

        string message = addedCoin > 0 ? $"在线奖励已发放。\n纪念币 +{addedCoin}" : "在线奖励已发放。";
        UIManager.Instance.ShowRemind("领取成功", "知道了", message, CloneRewards(config?.rewards));
    }

    private static List<RewardItem> CloneRewards(List<RewardItem> rewards)
    {
        if (rewards == null || rewards.Count == 0)
            return null;

        List<RewardItem> result = new();
        foreach (RewardItem reward in rewards)
        {
            if (reward == null || reward.amount <= 0)
                continue;

            RewardItemSystem.EnrichReward(reward);
            result.Add(new RewardItem
            {
                rewardId = reward.rewardId,
                rewardName = reward.rewardName,
                rewardSprite = reward.rewardSprite,
                spritePath = reward.spritePath,
                amount = reward.amount
            });
        }

        return result.Count > 0 ? result : null;
    }

    public void Clear()
    {
        foreach (OnlineAwardItemUI item in generatedItems)
            Destroy(item.gameObject);
        generatedItems.Clear();
    }
}
