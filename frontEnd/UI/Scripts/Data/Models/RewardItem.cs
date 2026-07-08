using System;
using UnityEngine;

[Serializable]
public class RewardItem
{
    public int rewardId;
    public string rewardName;
    public string spritePath;
    public int amount;
    [NonSerialized] public Sprite rewardSprite;
}
