using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardItemUI : MonoBehaviour
{
    public Image rewardImage;
    public TMP_Text rewardAmount;
    private RewardItem data;
    public void Setup(RewardItem data)
    {
        this.data = data;
        rewardImage.sprite = data.rewardSprite;
        rewardAmount.text = data.amount.ToString();
        Debug.Log($"RewardItemUI.Setup 调用成功: {data.rewardId}, sprite={data.rewardSprite}");

    }
}
