using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnlineAwardItemUI : MonoBehaviour
{
    public TMP_Text onlineTime;
    public Button button;
    public TMP_Text buttonLabel;
    public Transform rewardRoot;
    public RewardItemUI rewardPrefab;
    private OnlineAwardConfig data;
    private AwardStateType currentState;
    public void Setup(OnlineAwardConfig data, Action getAward)
    {
        this.data = data;
        onlineTime.text = $"在线{data.requiredMinutes}分钟";
        rewardRoot.localScale = new Vector3(0.8f, 0.8f, 1f);
        foreach (var reward in data.rewards)
        {
            RewardItemUI item = Instantiate(rewardPrefab, rewardRoot);
            item.Setup(reward);
        }
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(getAward.Invoke);
    }
    public void UpdateButtonText(int todayOnlineMinutes)
    {
        switch (currentState)
        {
            case AwardStateType.Locked:
                button.interactable = false;
                buttonLabel.text = $"{data.requiredMinutes - todayOnlineMinutes}分钟后";
                break;
            case AwardStateType.UnClaimed:
                button.interactable = true;
                buttonLabel.text = "待领取";
                break;
            case AwardStateType.Claimed:
                button.interactable = false;
                buttonLabel.text = "已领取 √";
                break;
        }
    }
    public void SetCurrentState(AwardStateType newState)
    {
        currentState = newState;
    }
    public AwardStateType GetCurrentState()
    {
        return currentState;
    }
}
