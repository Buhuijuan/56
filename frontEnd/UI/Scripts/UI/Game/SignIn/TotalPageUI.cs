using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TotalPageUI : PageUI
{
    public TMP_Text dayCount;
    public PagesSwitcher pagesSwitcher;
    public Transform pageRoot;
    public GameObject page;
    public TotalAwardItemUI totalAwardItemPrefab;

    private readonly List<GameObject> pages = new();

    private void Awake()
    {
        pagesSwitcher.SetPages(pages);
    }

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
                    "累计奖励加载失败",
                    "知道了",
                    string.IsNullOrEmpty(result.Message) ? "无法加载累计登录奖励。" : result.Message);
                return;
            }

            PlayerSignInState state = BackendStateMapper.ToFrontSignIn(result.Data.data.state);
            List<TotalAwardConfig> configs = result.Data.data.totalAwards;
            Render(state, configs);
        });
    }

    public void Render(PlayerSignInState signInState, List<TotalAwardConfig> configs)
    {
        Clear();

        dayCount.text = $"<size=36>您已累计登录 <size=72>{signInState.totalLoginDays}</size><size=36> 天</size>";

        const int itemsPerPage = 3;
        int totalPages = Mathf.CeilToInt(configs.Count / (float)itemsPerPage);

        for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
        {
            GameObject pageObj = Instantiate(page, pageRoot);
            pages.Add(pageObj);

            for (int i = 0; i < itemsPerPage; i++)
            {
                int configIndex = pageIndex * itemsPerPage + i;
                if (configIndex >= configs.Count)
                    break;

                TotalAwardConfig config = configs[configIndex];
                TotalAwardItemUI item = Instantiate(totalAwardItemPrefab, pageObj.transform);
                item.SetCurrentState(GetAwardState(config, signInState));
                item.Setup(config, () =>
                {
                    if (item.GetCurrentState() != AwardStateType.UnClaimed)
                        return;

                    BackendRuntime.Run(ClaimTotalAndRefresh(config));
                });
                item.UpdateCondition(signInState.totalLoginDays);
            }
        }

        pagesSwitcher.SetPage(0);
    }

    private IEnumerator ClaimTotalAndRefresh(TotalAwardConfig config)
    {
        bool success = false;
        int addedCoin = 0;

        yield return SignInSystem.TotalLoginModule.ClaimTotalAward(config.awardID, (ok, msg, coin) =>
        {
            if (!ok)
            {
                UIManager.Instance.ShowRemind("领取失败", "知道了", string.IsNullOrEmpty(msg) ? "累计奖励领取失败。" : msg);
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

        string message = config.rewardCharacterID > 0 ? "累计登录奖励已领取，角色奖励已解锁。" : "累计登录奖励已领取。";
        if (addedCoin > 0)
            message += $"\n纪念币 +{addedCoin}";
        UIManager.Instance.ShowRemind("领取成功", "知道了", message, BuildCoinRewards(addedCoin));
    }

    private static List<RewardItem> BuildCoinRewards(int addedCoin)
    {
        if (addedCoin <= 0)
            return null;

        RewardItem baseItem = RewardItemSystem.GetRewardItem(1);
        return new List<RewardItem>
        {
            new RewardItem
            {
                rewardId = 1,
                rewardName = baseItem != null ? baseItem.rewardName : "纪念币",
                rewardSprite = baseItem != null ? baseItem.rewardSprite : null,
                spritePath = baseItem != null ? baseItem.spritePath : null,
                amount = addedCoin
            }
        };
    }

    public void Clear()
    {
        dayCount.text = null;

        foreach (GameObject pageObj in pages)
            Destroy(pageObj);

        pages.Clear();
    }

    public AwardStateType GetAwardState(TotalAwardConfig config, PlayerSignInState signInState)
    {
        if (config.rewardCharacterID == 0)
            return AwardStateType.None;

        if (signInState == null || signInState.totalLoginDays < config.requiredDays)
            return AwardStateType.Locked;

        if (!signInState.totalRewardClaimed.Contains(config.awardID))
            return AwardStateType.UnClaimed;

        return AwardStateType.Claimed;
    }
}
