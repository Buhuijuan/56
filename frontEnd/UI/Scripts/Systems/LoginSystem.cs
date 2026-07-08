using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class LoginSystem
{
    private static AccountData currentAccount;
    private static List<AccountData> fakedata = new List<AccountData>()
    {
        new()
        {
            accountID = "815717",
            mailbox = "123456@example.com",
            password = "12345678",
            roles = new()
            {
            new RoleData
            {
        roleID = "R1",
        campusName = "此间大学",
        nickName = "小一",
        characterState = new()
        {
            unlockedCharacters = new HashSet<int> { 1, 2, 3 },
            currentCharacterID=2
        },
        levelState = new()
        {
            level = 7,
            progress = 0.55f,
            exp = 1500,
            rewardClaimed = new(){1,2,3},
            boxOpened = new(){1,2,3}
        },
        growthState = new()
        {
            stageCompleted = new() { "ST1", "ST2", "ST3" },
            rewardClaimed = new() { "ST1", "ST2", "ST3" },
            taskCompleted = new() { "ST1_GT1", "ST1_GT2", "ST1_GT3", "ST2_GT1", "ST2_GT2", "ST2_GT3", "ST3_GT1", "ST3_GT2", "ST3_GT3", "ST4_GT1", "ST4_GT2" }
        },
        signInState = new()
        {
            todayOnlineMinutes = 50,
            dailySigned = true,
            lastSignInDate = DateTime.Today,
            continuousSignDays = 5,
            totalLoginDays = 12,
            onlineRewardClaimed = new HashSet<int> { 1 },
            dailyRewardClaimed = new HashSet<int> { },
            totalRewardClaimed = new HashSet<int> { 1 }
        },
        quizEventState = new()
        {
            eventId = "quiz_2026_w10",
            weeklyScore = 85,
            lastPlayDate = DateTime.Today.AddDays(-1),
            hasPlayedToday = false
        },
        clockInEventState = new()
        {
            lastCheckInDate = DateTime.Today.AddDays(-1),
            checkedIn = new()
            {
            { "loc_teaching_building_1", false },
            { "loc_library", false }
            }
        },
        storyEventState = new()
        {
            eventId = "story_2026_w10",
            hasFinished = false,
            rewardClaimed = false,
            lastPlayDate = DateTime.MinValue
        },
        taskState = new()
        {
            completedTasks = new HashSet<string> { "CH01" },
            completedGoals = new HashSet<string> { "CH01_G01", "CH01_G02", "CH02_G01", "CH11_G01", "CH11_G02", "CH12_G01", "CH12_G01" }
        },
        titleState = new()
        {
            unlockedTitles = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 11, 13, 17, 18, 21, 22, 23 },
            equippedTitleID = 3
        }
    },
            new RoleData
        {
            roleID="R2",
            campusName = "安心大学",
            nickName = "小二",
            characterState = new()
            {
            unlockedCharacters = new HashSet<int> { 1, 2},
            currentCharacterID=1
            },
            levelState = new()
            {
                level = 5,
                progress = 0.42f,
                exp=235,
                rewardClaimed = new(){1,2,3},
                boxOpened = new(){1,2,3}
            }

        },
            new RoleData
        {
            roleID="R3",
            campusName = "方寸大学",
            nickName = "小三",
            characterState = new()
            {
            unlockedCharacters = new HashSet<int> { 1, 2},
            currentCharacterID=2
            },
            levelState = new()
            {
                level = 3,
                progress = 0.24f,
                exp=130,
                rewardClaimed = new(){1,2,3},
                boxOpened = new(){1,2,3}
            }
        },
            new RoleData
            {
            roleID="R4",
            campusName = "安心大学",
            nickName = "小四",
            characterState = new()
            {
            unlockedCharacters = new HashSet<int> { 1, 2},
            currentCharacterID=1
            },
            levelState = new()
            {
                level = 2,
                progress = 0.15f,
                exp=100,
                rewardClaimed = new(){1,2,3},
                boxOpened = new(){1,2,3}
            }
            }
            }
        },
        new()
        {
            accountID = "815718",
            mailbox = "223456@example.com",
            password = "12345678",
            roles = new List<RoleData>()
        },
        new()
        {
            accountID = "815719",
            mailbox = "323456@example.com",
            password = "12345678",
            roles = new List<RoleData>()
        }
    };
    public static List<AccountData> GetAccountDatas()
    {
        return fakedata;
    }
    public static void SetCurrentAccount(AccountData account)
    {
        currentAccount = account;
    }
    public static AccountData GetCurrentAccount()
    {
        return currentAccount;
    }
    public static bool LoginValidate(string mailbox, string password, out LoginErrorType type, out string errormsg)
    {
        AccountData account = fakedata.Find(i => i.mailbox == mailbox);
        if (account == null)
        {
            type = LoginErrorType.MailboxNotExist;
            errormsg = "账户不存在";
            return false;
        }
        if (account.password != password)
        {
            type = LoginErrorType.PasswordIncorrect;
            errormsg = "密码错误";
            return false;
        }
        type = LoginErrorType.None;
        errormsg = null;
        return true;
    }
    public static bool RegisterValidate(string mailbox, string verification, string password, out LoginErrorType type, out string errormsg, out string newAccountID)
    {
        if (fakedata.Exists(i => i.mailbox == mailbox))
        {
            type = LoginErrorType.MailboxAlreadyRegistered;
            errormsg = "该邮箱已注册";
            newAccountID = null;
            return false;
        }
        if (verification != "111222")
        {
            type = LoginErrorType.VerificationIncorrect;
            errormsg = "验证码错误";
            newAccountID = null;
            return false;
        }
        AccountData newAccount = new AccountData()
        {
            accountID = (int.Parse(fakedata[^1].accountID) + 1).ToString(),
            mailbox = mailbox,
            password = password,
            roles = new()
        };
        RoleData defaultRole = new RoleData()
        {
            roleID = "R" + (newAccount.roles.Count + 1),
            campusName = "默认校园",
            nickName = null,
            characterState = new()
            {
                unlockedCharacters = new HashSet<int> { 1, 2 },
                currentCharacterID = 1
            },
            levelState = new PlayerLevelState(),
            growthState = new PlayerGrowthState(),
            signInState = new PlayerSignInState(),
            quizEventState = new PlayerQuizEventState(),
            clockInEventState = new PlayerClockInEventState(),
            storyEventState = new PlayerStoryEventState(),
            taskState = new PlayerTaskState(),
            titleState = new PlayerTitleState()
        };

        newAccount.roles.Add(defaultRole);
        fakedata.Add(newAccount);
        type = LoginErrorType.None;
        errormsg = null;
        newAccountID = newAccount.accountID;
        return true;
    }
}
