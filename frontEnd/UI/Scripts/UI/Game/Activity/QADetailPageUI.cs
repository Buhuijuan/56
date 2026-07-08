using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QADetailPageUI : PageUI
{
    public TMP_Text theme;
    public TMP_Text timeLeft;
    public TMP_Text totalGrades;
    public Transform rewardRoot;
    public RewardItemUI rewardItemPrefab;
    public Button startButton;
    public ActivityPanelUI parent;

    private const string LabelClaimWeekly = "\u9886\u53d6\u5468\u5956\u52b1";
    private const string LabelPlayedToday = "\u4eca\u65e5\u5df2\u5b8c\u6210";
    private const string LabelStart = "\u5f00\u59cb\u7b54\u9898";
    private const string LabelEnded = "\u6d3b\u52a8\u5df2\u7ed3\u675f";
    private const string TitleClaimSuccess = "\u9886\u53d6\u6210\u529f";
    private const string ButtonAcknowledge = "\u77e5\u9053\u4e86";

    private void OnEnable()
    {
        if (BackendSettings.UseBackendMode && SessionStore.HasToken)
        {
            StartCoroutine(RefreshRemote());
            return;
        }

        Refresh();
    }

    public void Refresh()
    {
        QuizEventConfig config = QuizEventSystem.GetCurrentConfig();
        PlayerQuizEventState state = QuizEventSystem.quizEventState;
        if (config == null || state == null)
            return;

        theme.text = $"\u672c\u5468\u4e3b\u9898\uff1a{config.theme}";

        TimeSpan time = QuizEventSystem.GetTimeLeft();
        DateTime endTime = config.startTime.AddDays(config.durationDays);
        TMP_Text buttonText = startButton.GetComponentInChildren<TMP_Text>();

        if (endTime < DateTime.Now)
        {
            timeLeft.text = LabelEnded;
            startButton.interactable = false;
            if (buttonText != null)
                buttonText.text = LabelEnded;
            startButton.onClick.RemoveAllListeners();
            return;
        }

        if (time.TotalSeconds <= 0)
            timeLeft.text = "\u5373\u5c06\u7ed3\u675f";
        else
            timeLeft.text = $"{time.Days}\u5929 {time.Hours}\u5c0f\u65f6 {time.Minutes}\u5206\u949f";

        totalGrades.text = $"{state.weeklyScore} \u5206";

        foreach (Transform child in rewardRoot)
            Destroy(child.gameObject);

        rewardRoot.localScale = new Vector3(0.6f, 0.6f, 1f);
        foreach (RewardItem reward in QuizEventSystem.GetCurrentTierPreviewRewards())
        {
            RewardItemUI item = Instantiate(rewardItemPrefab, rewardRoot);
            item.Setup(reward);
        }

        startButton.onClick.RemoveAllListeners();

        if (QuizEventSystem.CanClaimWeeklyReward())
        {
            if (buttonText != null)
                buttonText.text = LabelClaimWeekly;

            startButton.interactable = true;
            startButton.onClick.AddListener(() =>
            {
                if (BackendSettings.UseBackendMode && SessionStore.HasToken)
                {
                    StartCoroutine(ClaimRemoteWeeklyReward());
                }
                else
                {
                    List<RewardItem> rewards = QuizEventSystem.ClaimWeeklyReward();
                    UIManager.Instance.ShowRemind(
                        TitleClaimSuccess,
                        ButtonAcknowledge,
                        QuizEventSystem.GetWeeklyRewardDescription(),
                        rewards,
                        Refresh);
                }
            });
            return;
        }

        if (QuizEventSystem.HasPlayedToday())
        {
            if (buttonText != null)
                buttonText.text = LabelPlayedToday;
            startButton.interactable = false;
            return;
        }

        if (buttonText != null)
            buttonText.text = LabelStart;

        startButton.interactable = true;
        startButton.onClick.AddListener(() =>
        {
            if (BackendSettings.UseBackendMode && SessionStore.HasToken)
            {
                StartCoroutine(StartRemoteQuiz());
                return;
            }

            QuizEventSystem.StartNewSession();
            parent.SwitchPage(ActivityPanelUI.ActivityPageState.QAContent);
        });
    }

    private IEnumerator RefreshRemote()
    {
        yield return BackendFacade.RefreshQuizCurrent(result =>
        {
            if (!result.Success || result.Data == null || !result.Data.success || result.Data.data == null)
            {
                UIManager.Instance.ShowRemind(
                    "\u95ee\u7b54\u52a0\u8f7d\u5931\u8d25",
                    ButtonAcknowledge,
                    string.IsNullOrEmpty(result.Message) ? "\u65e0\u6cd5\u52a0\u8f7d\u6821\u56ed\u95ee\u7b54\u6570\u636e\u3002" : result.Message);
                return;
            }

            QuizEventSystem.ApplyRemoteCurrent(result.Data.data.config, result.Data.data.state);
            Refresh();
        });
    }

    private IEnumerator StartRemoteQuiz()
    {
        yield return BackendFacade.StartQuiz(result =>
        {
            if (!result.Success || result.Data == null || !result.Data.success || result.Data.data == null)
            {
                UIManager.Instance.ShowRemind(
                    "\u5f00\u59cb\u5931\u8d25",
                    ButtonAcknowledge,
                    string.IsNullOrEmpty(result.Message) ? "\u65e0\u6cd5\u5f00\u59cb\u672c\u6b21\u6821\u56ed\u95ee\u7b54\u3002" : result.Message);
                return;
            }

            QuizEventSystem.StartRemoteSession(result.Data.data.@event, result.Data.data.questions);
            parent.SwitchPage(ActivityPanelUI.ActivityPageState.QAContent);
        });
    }

    private IEnumerator ClaimRemoteWeeklyReward()
    {
        BackendApiResult<BackendQuizWeeklyClaimEnvelope> result = null;
        yield return BackendFacade.ClaimQuizWeeklyReward(value => result = value);

        if (result == null || !result.Success || result.Data == null || !result.Data.success || result.Data.data == null)
        {
            UIManager.Instance.ShowRemind(
                "\u9886\u53d6\u5931\u8d25",
                ButtonAcknowledge,
                result != null && !string.IsNullOrEmpty(result.Message) ? result.Message : "\u65e0\u6cd5\u9886\u53d6\u672c\u5468\u5956\u52b1\u3002");
            yield break;
        }

        BackendQuizWeeklyClaimData claimData = result.Data.data;
        QuizEventSystem.ApplyRemoteWeeklyClaim(claimData.addedCoin, claimData.weeklyScore, claimData.claimed);
        yield return BackendFacade.RefreshHome(null);
        yield return BackendFacade.RefreshGrowth(null);

        List<RewardItem> rewards = claimData.addedCoin > 0 ? BuildCoinRewards(claimData.addedCoin) : null;
        UIManager.Instance.ShowRemind(
            TitleClaimSuccess,
            ButtonAcknowledge,
            $"\u672c\u5468\u5956\u52b1\u5df2\u9886\u53d6\u3002\n\u7eaa\u5ff5\u5e01 +{Mathf.Max(0, claimData.addedCoin)}",
            rewards,
            Refresh);
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
                rewardName = baseItem != null ? baseItem.rewardName : "\u7eaa\u5ff5\u5e01",
                rewardSprite = baseItem != null ? baseItem.rewardSprite : null,
                spritePath = baseItem != null ? baseItem.spritePath : null,
                amount = addedCoin
            }
        };
    }
}
