using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DailyPageUI : PageUI
{
    public TMP_Text dayCount;
    public Transform baseAwardRoot;
    public Transform extraAwardRoot;
    public BaseDailyAwardItemUI baseAwardPrefab;
    public ExtraDailyAwardItemUI extraAwardPrefab;

    private readonly List<MonoBehaviour> generatedItems = new();

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
                    "\u7b7e\u5230\u52a0\u8f7d\u5931\u8d25",
                    "\u77e5\u9053\u4e86",
                    string.IsNullOrEmpty(result.Message) ? "\u65e0\u6cd5\u52a0\u8f7d\u7b7e\u5230\u6570\u636e\u3002" : result.Message);
                return;
            }

            PlayerSignInState state = BackendStateMapper.ToFrontSignIn(result.Data.data.state);
            List<DailyAwardConfig> configs = result.Data.data.dailyAwards;
            EnrichRewardSprites(configs);
            Render(state, configs);
        });
    }

    private static void EnrichRewardSprites(List<DailyAwardConfig> configs)
    {
        if (configs == null)
            return;

        foreach (DailyAwardConfig config in configs)
        {
            RewardItemSystem.EnrichReward(config.baseReward);
            RewardItemSystem.EnrichReward(config.extraReward);
        }
    }

    private void Render(PlayerSignInState signInState, List<DailyAwardConfig> configs)
    {
        Clear();

        int todayIndex = GetTodayDayIndexInWeek();
        dayCount.text = $"<size=36>\u672c\u5468\u5df2\u8fde\u7eed\u7b7e\u5230 <size=72>{signInState.continuousSignDays}</size><size=36> \u5929</size>";

        foreach (DailyAwardConfig config in configs)
        {
            if (config.dayIndex < 7)
            {
                BaseDailyAwardItemUI item = Instantiate(baseAwardPrefab, baseAwardRoot);
                item.SetCurrentState(GetAwardState(config, signInState, todayIndex));
                item.Setup(config, () =>
                {
                    if (item.GetCurrentState() == AwardStateType.UnClaimed)
                        StartCoroutine(DailySignAndRefresh(config));
                });
                item.UpdateButtonState();
                generatedItems.Add(item);
            }
            else
            {
                ExtraDailyAwardItemUI item = Instantiate(extraAwardPrefab, extraAwardRoot);
                item.SetCurrentState(GetAwardState(config, signInState, todayIndex));
                string label = signInState.continuousSignDays >= 7 ? "\u4e03\u65e5\u5956\u52b1\u5df2\u89e3\u9501" : "\u7b7e\u5230\u6ee1 7 \u5929\u89e3\u9501";
                item.Setup(config, label, () =>
                {
                    if (item.GetCurrentState() == AwardStateType.UnClaimed)
                        StartCoroutine(DailySignAndRefresh(config));
                });
                item.UpdateButtonState();
                generatedItems.Add(item);
            }
        }
    }

    private IEnumerator DailySignAndRefresh(DailyAwardConfig config)
    {
        bool success = false;
        int addedCoin = 0;

        yield return SignInSystem.DailySignInModule.SignInToday((ok, msg, coin) =>
        {
            if (!ok)
            {
                UIManager.Instance.ShowRemind(
                    "\u7b7e\u5230\u5931\u8d25",
                    "\u77e5\u9053\u4e86",
                    string.IsNullOrEmpty(msg) ? "\u65e0\u6cd5\u5b8c\u6210\u7b7e\u5230\u3002" : msg);
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

        HeadAreaUI.RefreshAll();

        List<RewardItem> rewards = BuildDailyRewards(config);
        string message = addedCoin > 0
            ? $"\u672c\u6b21\u7b7e\u5230\u5956\u52b1\u5df2\u53d1\u653e\u3002\n\u7eaa\u5ff5\u5e01 +{addedCoin}"
            : "\u672c\u6b21\u7b7e\u5230\u5956\u52b1\u5df2\u53d1\u653e\u3002";
        UIManager.Instance.ShowRemind("\u7b7e\u5230\u6210\u529f", "\u77e5\u9053\u4e86", message, rewards);
    }

    private static List<RewardItem> BuildDailyRewards(DailyAwardConfig config)
    {
        List<RewardItem> rewards = new();
        AddReward(rewards, config?.baseReward);
        AddReward(rewards, config?.extraReward);
        return rewards.Count > 0 ? rewards : null;
    }

    private static void AddReward(List<RewardItem> rewards, RewardItem reward)
    {
        if (rewards == null || reward == null || reward.amount <= 0)
            return;

        RewardItemSystem.EnrichReward(reward);
        rewards.Add(new RewardItem
        {
            rewardId = reward.rewardId,
            rewardName = reward.rewardName,
            rewardSprite = reward.rewardSprite,
            spritePath = reward.spritePath,
            amount = reward.amount
        });
    }

    public void Clear()
    {
        foreach (MonoBehaviour item in generatedItems)
            Destroy(item.gameObject);
        generatedItems.Clear();
    }

    private AwardStateType GetAwardState(DailyAwardConfig config, PlayerSignInState signInState, int todayIndex)
    {
        bool baseClaimed = signInState.dailyRewardClaimed.Contains(config.baseAwardID);

        if (config.dayIndex < todayIndex)
            return baseClaimed ? AwardStateType.Claimed : AwardStateType.Expired;

        if (config.dayIndex == todayIndex)
            return baseClaimed ? AwardStateType.Claimed : AwardStateType.UnClaimed;

        return AwardStateType.Locked;
    }

    private int GetTodayDayIndexInWeek()
    {
        int dayOfWeek = (int)System.DateTime.Today.DayOfWeek;
        return dayOfWeek == 0 ? 7 : dayOfWeek;
    }
}
