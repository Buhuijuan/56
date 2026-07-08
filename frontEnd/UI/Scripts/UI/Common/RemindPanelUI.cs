using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RemindPanelUI : MonoBehaviour
{
    public TMP_Text titleText, messageText, confirmText;
    public Transform rewardRoot;
    public Button confirmButton;
    public RewardItemUI rewardPrefab;
    public void Show(string title, string buttonText, string message = null, List<RewardItem> rewards = null, Action confirm = null)
    {
        titleText.text = title;
        bool hasRewards = rewards != null && rewards.Count > 0;

        if (!hasRewards)
        {
            messageText.gameObject.SetActive(true);
            rewardRoot.gameObject.SetActive(false);
            messageText.text = message ?? string.Empty;
        }
        confirmText.text = buttonText;
        if (hasRewards)
        {
            messageText.gameObject.SetActive(false);
            rewardRoot.gameObject.SetActive(true);
            foreach (Transform child in rewardRoot)
                Destroy(child.gameObject);
            rewardRoot.localScale = new Vector3(1.2f, 1.2f, 1f);
            RewardItemSystem.EnrichRewards(rewards);
            foreach (var reward in rewards)
            {
                var item = Instantiate(rewardPrefab, rewardRoot);
                item.Setup(reward);
            }
        }
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            confirm?.Invoke();
            Close();
        });
        gameObject.SetActive(true);
    }
    public void Close()
    {
        gameObject.SetActive(false);
        TaskPromptSystem.NotifyUiAvailabilityChanged();
    }
}
