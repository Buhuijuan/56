using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseDailyAwardItemUI : MonoBehaviour
{
    public TMP_Text dayText;
    public Button getButton;
    public TMP_Text buttonText;
    public Transform baserewardRoot;
    public RewardItemUI rewardItemPrefab;
    private AwardStateType currentState;
    private DailyAwardConfig data;
    public void Setup(DailyAwardConfig data, Action getAward)
    {
        this.data = data;
        dayText.text = $"<size=27>DAY </size=27><size=39>{data.dayIndex}</size=39>";
        RewardItemUI item = Instantiate(rewardItemPrefab, baserewardRoot);
        item.Setup(data.baseReward);
        getButton.onClick.RemoveAllListeners();
        getButton.onClick.AddListener(getAward.Invoke);
    }
    public void UpdateButtonState()
    {
        switch (currentState)
        {
            case AwardStateType.Locked:
                getButton.interactable = false;
                buttonText.text = "待领取";
                break;
            case AwardStateType.UnClaimed:
                getButton.interactable = true;
                buttonText.text = "签到";
                break;
            case AwardStateType.Claimed:
                getButton.interactable = false;
                buttonText.text = "已领取";
                break;
            case AwardStateType.Expired:
                getButton.interactable = false;
                buttonText.text = "已过期";
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
