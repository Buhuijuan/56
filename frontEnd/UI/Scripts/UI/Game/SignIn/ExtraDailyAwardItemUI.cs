using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExtraDailyAwardItemUI : MonoBehaviour
{
    public Transform baseRoot;
    public Transform extraRoot;
    public Button getButton;
    public TMP_Text buttonText;
    public TMP_Text label;
    private AwardStateType currentState;
    public RewardItemUI awardPrefab;
    private DailyAwardConfig data;
    public void Setup(DailyAwardConfig data, string label, Action getAward)
    {
        this.data = data;
        RewardItemUI baseReward = Instantiate(awardPrefab, baseRoot);
        baseReward.Setup(data.baseReward);
        RewardItemUI extraReward = Instantiate(awardPrefab, extraRoot);
        extraReward.Setup(data.extraReward);
        this.label.text = label;
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
