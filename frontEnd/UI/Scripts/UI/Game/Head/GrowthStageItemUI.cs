using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GrowthStageItemUI : MonoBehaviour
{
    public enum GrowthStageState
    {
        Locked,
        Lighted
    }

    public Image Note;
    public Sprite LockedImage;
    public Sprite LightedImage;
    public TMP_Text stageTitle;
    public TMP_Text rewardStateLabel;
    public Button claimButton;
    public Transform growthTaskRoot;
    public GrowthTaskUI growthTaskPrefab;

    private GrowthStageState currentState;
    private readonly List<GrowthTaskUI> generatedItems = new();
    private GrowthStageData data;
    private Action refreshCallback;

    public void Setup(GrowthStageData data, Action refreshCallback)
    {
        Clear();
        this.data = data;
        this.refreshCallback = refreshCallback;

        foreach (GrowthTask task in data.tasks)
        {
            GrowthTaskUI item = Instantiate(growthTaskPrefab, growthTaskRoot);
            bool isTaskCompleted = GrowthSystem.IsTaskCompleted(task.taskId);
            item.Setup(task, isTaskCompleted);
            generatedItems.Add(item);
        }

        bool isStageCompleted = GrowthSystem.IsStageCompleted(data.stageID);
        bool isRewardClaimed = GrowthSystem.IsRewardClaimed(data.stageID);
        currentState = isStageCompleted ? GrowthStageState.Lighted : GrowthStageState.Locked;

        stageTitle.text = data.stageTitle;

        if (rewardStateLabel != null)
        {
            rewardStateLabel.text = string.Empty;
            rewardStateLabel.gameObject.SetActive(false);
        }

        Button effectiveButton = claimButton != null ? claimButton : GetComponent<Button>();
        if (effectiveButton == null)
        {
            effectiveButton = gameObject.AddComponent<Button>();
            Image target = GetComponent<Image>();
            if (target == null)
            {
                target = gameObject.AddComponent<Image>();
                target.color = new Color(1f, 1f, 1f, 0.01f);
            }
            effectiveButton.targetGraphic = target;
        }

        effectiveButton.onClick.RemoveAllListeners();
        effectiveButton.onClick.AddListener(OnClickClaimReward);
        effectiveButton.interactable = isStageCompleted && !isRewardClaimed;
        UpdateStateSprite();
    }

    public void UpdateStateSprite()
    {
        switch (currentState)
        {
            case GrowthStageState.Locked:
                Note.sprite = LockedImage;
                break;
            case GrowthStageState.Lighted:
                Note.sprite = LightedImage;
                break;
        }
    }

    public void Clear()
    {
        foreach (GrowthTaskUI item in generatedItems)
            Destroy(item.gameObject);
        generatedItems.Clear();
    }

    private void OnClickClaimReward()
    {
        if (!GrowthSystem.CanClaimReward(data.stageID))
            return;

        if (BackendSettings.UseBackendMode && SessionStore.HasToken)
        {
            StartCoroutine(ClaimRemoteReward());
            return;
        }

        RewardGrantResult result = GrowthSystem.ClaimStageReward(data.stageID);
        string message = result.BuildSummary();
        if (data.titleID > 0)
            message += $"\n已解锁成长称号：#{data.titleID}";

        UIManager.Instance.ShowRemind("成长奖励已领取", "知道了", message);
        refreshCallback?.Invoke();
    }

    private IEnumerator ClaimRemoteReward()
    {
        yield return BackendFacade.ClaimGrowthReward(data.stageID, result =>
        {
            if (!result.Success || result.Data == null || !result.Data.success || result.Data.data == null)
            {
                UIManager.Instance.ShowRemind("领取失败", "知道了", string.IsNullOrEmpty(result.Message) ? "成长奖励领取失败。" : result.Message);
                return;
            }

            int addedCoin = result.Data.data.addedCoin;
            string message = "成长奖励已领取。";
            if (addedCoin > 0)
                message += $"\n纪念币 +{addedCoin}";
            if (result.Data.data.titleUnlocked && result.Data.data.titleId > 0)
                message += $"\n已解锁成长称号：#{result.Data.data.titleId}";

            UIManager.Instance.ShowRemind("成长奖励已领取", "知道了", message, BuildRemoteRewards(data.rewards, addedCoin));
            StartCoroutine(RefreshGrowthAndHome());
        });
    }

    private IEnumerator RefreshGrowthAndHome()
    {
        yield return BackendFacade.RefreshGrowth(null);
        yield return BackendFacade.RefreshHome(null);
        foreach (HeadAreaUI headArea in FindObjectsOfType<HeadAreaUI>(true))
            headArea.Refresh();
        refreshCallback?.Invoke();
    }

    private static List<RewardItem> CloneRewards(List<RewardItem> rewards)
    {
        if (rewards == null)
            return new List<RewardItem>();

        return rewards
            .Where(reward => reward != null)
            .Select(reward => new RewardItem
            {
                rewardId = reward.rewardId,
                rewardName = reward.rewardName,
                rewardSprite = reward.rewardSprite,
                spritePath = reward.spritePath,
                amount = reward.amount
            })
            .ToList();
    }

    private static List<RewardItem> BuildRemoteRewards(List<RewardItem> rewards, int addedCoin)
    {
        List<RewardItem> result = CloneRewards(rewards);
        if (addedCoin <= 0)
            return result;

        bool updatedCoinReward = false;
        foreach (RewardItem reward in result)
        {
            if (reward != null && reward.rewardId == 1)
            {
                reward.amount = addedCoin;
                updatedCoinReward = true;
                break;
            }
        }

        if (!updatedCoinReward)
        {
            RewardItem baseItem = RewardItemSystem.GetRewardItem(1);
            result.Add(new RewardItem
            {
                rewardId = 1,
                rewardName = baseItem != null ? baseItem.rewardName : "纪念币",
                rewardSprite = baseItem != null ? baseItem.rewardSprite : null,
                spritePath = baseItem != null ? baseItem.spritePath : null,
                amount = addedCoin
            });
        }

        return result;
    }
}
