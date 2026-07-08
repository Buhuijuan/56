using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelPageUI : PageUI
{
    public TMP_Text levelText;
    public TMP_Text lastUnClaimedLevelText;
    public Transform rewardRoot;
    public Button getButton;
    public RewardItemUI rewardPrefab;
    public List<AwardBoxItemUI> boxes = new();

    private readonly List<RewardItemUI> generatedrewards = new();

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        Clear();

        int currentLevel = LevelSystem.currentLevel;
        int lastUnClaimedLevel = LevelSystem.GetLastUnClaimedLevel();
        lastUnClaimedLevelText.text = $"Lv.{lastUnClaimedLevel}";
        LevelConfig lastUnClaimedLevelConfig = LevelSystem.GetLevelConfig(lastUnClaimedLevel);

        if (currentLevel < 10)
        {
            LevelConfig nextLevelConfig = LevelSystem.GetLevelConfig(currentLevel + 1);
            int nextRequiredExp = Mathf.Max(0, nextLevelConfig.requiredExp - LevelSystem.currentExp);
            levelText.text = $"Lv.<size=67.5>{currentLevel}</size>｜下次升级还需<size=67.5> {nextRequiredExp}</size> EXP";
        }
        else
        {
            levelText.text = $"Lv.<size=67.5>{currentLevel}</size>｜已满级";
        }
        rewardRoot.localScale = new Vector3(1.3f, 1.3f, 1f);
        foreach (RewardItem reward in lastUnClaimedLevelConfig.rewards)
        {
            RewardItemUI item = Instantiate(rewardPrefab, rewardRoot);
            item.Setup(reward);
            generatedrewards.Add(item);
        }

        getButton.interactable = lastUnClaimedLevel <= currentLevel;
        getButton.onClick.RemoveAllListeners();
        getButton.onClick.AddListener(OnClickGetRewards);

        for (int level = 1; level <= 10; level++)
        {
            AwardBoxItemUI box = boxes[level - 1];
            int capturedLevel = level;
            box.button.onClick.RemoveAllListeners();
            box.button.onClick.AddListener(() =>
            {
                if (box.GetCurrentState() != AwardStateType.UnClaimed)
                    return;

                if (BackendSettings.UseBackendMode && SessionStore.HasToken)
                {
                    StartCoroutine(ClaimLevelTitleAndRefresh(capturedLevel));
                    return;
                }

                LevelSystem.OpenBox(capturedLevel);
                TitleSystem.RefreshUnlockedTitles();
                box.SetCurrentState(AwardStateType.Claimed);
                box.UpdateSprite();
            });

            if (level > currentLevel)
            {
                box.SetCurrentState(AwardStateType.Locked);
            }
            else if (!LevelSystem.IsBoxOpened(level))
            {
                box.SetCurrentState(AwardStateType.UnClaimed);
            }
            else
            {
                box.SetCurrentState(AwardStateType.Claimed);
            }

            box.UpdateSprite();
        }
    }

    public void Clear()
    {
        foreach (RewardItemUI reward in generatedrewards)
            Destroy(reward.gameObject);
        generatedrewards.Clear();
    }

    public void OnClickGetRewards()
    {
        if (BackendSettings.UseBackendMode && SessionStore.HasToken)
        {
            StartCoroutine(ClaimLevelRewardAndRefresh());
            return;
        }

        int lastUnClaimedLevel = LevelSystem.GetLastUnClaimedLevel();
        if (lastUnClaimedLevel > LevelSystem.currentLevel)
            return;

        LevelConfig config = LevelSystem.GetLevelConfig(lastUnClaimedLevel);
        RewardGrantResult result = ActivityRewardSystem.GrantRewards(
            $"等级奖励 Lv.{lastUnClaimedLevel}",
            0,
            CloneRewards(config?.rewards));

        LevelSystem.ClaimReward(lastUnClaimedLevel);
        UIManager.Instance.ShowRemind("领取成功", "知道了", result.BuildSummary(), result.rewards);
        Refresh();
    }

    private IEnumerator ClaimLevelRewardAndRefresh()
    {
        int level = LevelSystem.GetLastUnClaimedLevel();
        if (level > LevelSystem.currentLevel)
            yield break;

        LevelConfig config = LevelSystem.GetLevelConfig(level);
        BackendApiResult<BackendLevelRewardClaimEnvelope> result = null;
        yield return BackendFacade.ClaimLevelReward(level, value => result = value);

        if (result == null || !result.Success || result.Data == null || !result.Data.success || result.Data.data == null || !result.Data.data.claimed)
        {
            UIManager.Instance.ShowRemind("领取失败", "知道了", result != null ? result.Message : "等级奖励领取失败。");
            yield break;
        }

        yield return BackendFacade.RefreshGrowth(null);
        yield return BackendFacade.RefreshHome(null);

        int addedCoin = result.Data.data.addedCoin;
        string message = addedCoin > 0 ? $"等级奖励已发放。\n纪念币 +{addedCoin}" : $"等级奖励 Lv.{level} 已领取。";
        UIManager.Instance.ShowRemind("领取成功", "知道了", message, BuildRemoteRewards(config, addedCoin));
        Refresh();
    }

    private IEnumerator ClaimLevelTitleAndRefresh(int level)
    {
        if (level > LevelSystem.currentLevel || level <= 0)
            yield break;

        BackendApiResult<BackendLevelTitleClaimEnvelope> result = null;
        yield return BackendFacade.ClaimLevelTitle(level, value => result = value);

        if (result == null || !result.Success || result.Data == null || !result.Data.success || result.Data.data == null || !result.Data.data.claimed)
        {
            UIManager.Instance.ShowRemind("领取失败", "知道了", result != null ? result.Message : "等级称号领取失败。");
            yield break;
        }

        yield return BackendFacade.RefreshGrowth(null);
        yield return BackendFacade.RefreshHome(null);

        int titleId = result.Data.data.titleId;
        string message = BuildTitleClaimMessage(titleId);
        UIManager.Instance.ShowRemind("领取成功", "知道了", message);
        Refresh();
    }

    private static string BuildTitleClaimMessage(int titleId)
    {
        if (titleId <= 0)
            return "等级称号宝箱已开启。";

        TitleData titleData = TitleSystem.GetTitleDatas().Find(item => item != null && item.titleID == titleId);
        string titleName = titleData != null ? titleData.titleName : null;
        return !string.IsNullOrWhiteSpace(titleName)
            ? $"等级称号宝箱已开启，解锁称号「{titleName}」。"
            : $"等级称号宝箱已开启，解锁称号 #{titleId}。";
    }

    private static List<RewardItem> CloneRewards(List<RewardItem> rewards)
    {
        if (rewards == null)
            return new List<RewardItem>();

        List<RewardItem> result = new();
        foreach (RewardItem reward in rewards)
        {
            if (reward == null || reward.amount <= 0)
                continue;

            result.Add(new RewardItem
            {
                rewardId = reward.rewardId,
                rewardName = reward.rewardName,
                rewardSprite = reward.rewardSprite,
                spritePath = reward.spritePath,
                amount = reward.amount
            });
        }

        return result;
    }

    private static List<RewardItem> BuildRemoteRewards(LevelConfig config, int addedCoin)
    {
        List<RewardItem> rewards = CloneRewards(config?.rewards);
        if (addedCoin <= 0)
            return rewards;

        bool hasCoinReward = false;
        foreach (RewardItem reward in rewards)
        {
            if (reward != null && reward.rewardId == 1)
            {
                reward.amount = addedCoin;
                hasCoinReward = true;
                break;
            }
        }

        if (!hasCoinReward)
        {
            RewardItem baseItem = RewardItemSystem.GetRewardItem(1);
            rewards.Add(new RewardItem
            {
                rewardId = 1,
                rewardName = baseItem != null ? baseItem.rewardName : "纪念币",
                rewardSprite = baseItem != null ? baseItem.rewardSprite : null,
                spritePath = baseItem != null ? baseItem.spritePath : null,
                amount = addedCoin
            });
        }

        return rewards;
    }
}
