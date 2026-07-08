using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TotalAwardItemUI : MonoBehaviour
{
    public TMP_Text conditionLabel;
    public Image characterReward;
    public AwardBoxItemUI box;
    public TMP_Text boxLabel;
    public TMP_Text waitText;

    private AwardStateType currentState;
    private TotalAwardConfig data;

    public void Setup(TotalAwardConfig data, Action lockCharacter)
    {
        this.data = data;

        ApplyRewardVisualMode();

        if (data != null && data.rewardCharacterID > 0 && characterReward != null)
        {
            CharacterData characterData = CharacterSystem.GetCharacterData(data.rewardCharacterID);
            if (characterData != null)
                characterReward.sprite = characterData.characterImage;
        }

        if (box != null && box.button != null)
        {
            box.button.onClick.RemoveAllListeners();
            box.button.onClick.AddListener(() => lockCharacter?.Invoke());
        }
    }

    public void SetCurrentState(AwardStateType newState)
    {
        currentState = newState;
        ApplyRewardVisualMode();
    }

    public AwardStateType GetCurrentState()
    {
        return currentState;
    }

    public void UpdateCondition(int totalLoginDays)
    {
        ApplyRewardVisualMode();

        switch (currentState)
        {
            case AwardStateType.None:
                if (conditionLabel != null)
                    conditionLabel.text = "？？？";
                if (boxLabel != null)
                    boxLabel.text = "???";
                if (waitText != null)
                    waitText.text = "敬请期待";
                if (box != null)
                {
                    box.SetCurrentState(AwardStateType.Locked);
                    box.UpdateSprite();
                }
                break;

            case AwardStateType.Locked:
                if (conditionLabel != null)
                    conditionLabel.text = $"{data.requiredDays - totalLoginDays}天后";
                if (boxLabel != null)
                    boxLabel.text = $"{data.requiredDays}天";
                if (box != null)
                {
                    box.SetCurrentState(AwardStateType.Locked);
                    box.UpdateSprite();
                }
                break;

            case AwardStateType.UnClaimed:
                if (conditionLabel != null)
                    conditionLabel.text = "待领取";
                if (boxLabel != null)
                    boxLabel.text = $"{data.requiredDays}天";
                if (box != null)
                {
                    box.SetCurrentState(AwardStateType.UnClaimed);
                    box.UpdateSprite();
                }
                break;

            case AwardStateType.Claimed:
                if (conditionLabel != null)
                    conditionLabel.text = "已领取";
                if (boxLabel != null)
                    boxLabel.text = $"{data.requiredDays}天";
                if (box != null)
                {
                    box.SetCurrentState(AwardStateType.Claimed);
                    box.UpdateSprite();
                }
                break;
        }
    }

    private void ApplyRewardVisualMode()
    {
        bool isWait = data == null || data.rewardCharacterID == 0 || currentState == AwardStateType.None;

        if (waitText != null)
        {
            waitText.gameObject.SetActive(isWait);
            if (isWait)
                waitText.transform.SetAsLastSibling();
        }

        if (characterReward != null)
        {
            characterReward.gameObject.SetActive(!isWait);
            characterReward.enabled = !isWait;
            if (isWait)
            {
                characterReward.sprite = null;
                Color color = characterReward.color;
                color.a = 0f;
                characterReward.color = color;
            }
            else
            {
                Color color = characterReward.color;
                color.a = 1f;
                characterReward.color = color;
            }
        }
    }
}
