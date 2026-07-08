using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class RoleData
{
    public string roleID;
    public string campusName;
    public string nickName;
    public string avatarUrl;
    public bool useCustomAvatar = false;
    public string customAvatarPath;
    public PlayerCharacterState characterState = new();
    public PlayerLevelState levelState = new();
    public PlayerGrowthState growthState = new();
    public PlayerSignInState signInState = new();
    public PlayerQuizEventState quizEventState = new();
    public PlayerClockInEventState clockInEventState = new();
    public PlayerStoryEventState storyEventState = new();
    public PlayerTaskState taskState = new();
    public PlayerTitleState titleState = new();
    public PlayerRewardState rewardState = new();
}
