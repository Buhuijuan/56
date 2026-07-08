using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class SignInSystem
{
    public static PlayerSignInState signInState => AccountSystem.GetCurrentRole()?.signInState;

    public static IEnumerator NotifyRoleEntered(Action<bool> callback)
    {
        yield return BackendFacade.RefreshSignIn(result =>
        {
            callback(result.Success && result.Data != null && result.Data.success);
        });
    }

    public static IEnumerator AddOnlineSeconds(int seconds, Action<bool> callback)
    {
        yield return BackendFacade.Heartbeat(seconds, result =>
        {
            callback(result.Success && result.Data != null && result.Data.success);
        });
    }

    private static void EnsureState()
    {
        RoleData role = AccountSystem.GetCurrentRole();
        if (role == null)
            return;

        role.signInState ??= new PlayerSignInState();
        role.signInState.onlineRewardClaimed ??= new HashSet<int>();
        role.signInState.dailyRewardClaimed ??= new HashSet<int>();
        role.signInState.totalRewardClaimed ??= new HashSet<int>();
    }

    public static class OnlineTimeModule
    {
        private static List<OnlineAwardConfig> onlineConfigs;

        static OnlineTimeModule()
        {
            LoadOnlineConfig();
        }

        public static void LoadOnlineConfig()
        {
            TextAsset json = Resources.Load<TextAsset>("Jsons/OnlineAwardConfig");
            if (json == null)
            {
                onlineConfigs = new List<OnlineAwardConfig>();
                return;
            }

            onlineConfigs = JsonUtility.FromJson<OnlineAwardConfigListWrapper>(json.text).onlineAwards;
            foreach (var config in onlineConfigs)
            {
                foreach (var reward in config.rewards)
                {
                    var baseItem = RewardItemSystem.GetRewardItem(reward.rewardId);
                    reward.rewardName = baseItem.rewardName;
                    reward.rewardSprite = baseItem.rewardSprite;
                }
            }
        }

        public static List<OnlineAwardConfig> GetOnlineAwardConfigs()
        {
            return onlineConfigs;
        }

        public static bool IsOnlineAwardClaimed(int awardID)
        {
            return signInState != null && signInState.onlineRewardClaimed.Contains(awardID);
        }

        public static IEnumerator ClaimOnlineAward(int awardID, Action<bool, string> callback)
        {
            bool claimSuccess = false;
            string errorMessage = null;

            yield return BackendFacade.ClaimOnlineAward(awardID, result =>
            {
                if (!result.Success || result.Data == null || !result.Data.success)
                {
                    errorMessage = result.Message ?? "领取失败";
                    return;
                }

                claimSuccess = true;
            });

            if (!claimSuccess)
            {
                callback(false, errorMessage ?? "领取失败");
                yield break;
            }

            bool refreshSuccess = false;
            yield return BackendFacade.RefreshSignIn(result =>
            {
                refreshSuccess = result.Success && result.Data != null && result.Data.success;
                if (!refreshSuccess)
                    errorMessage = result.Message ?? "签到状态刷新失败";
            });

            callback(refreshSuccess, refreshSuccess ? "领取成功" : errorMessage);
        }

        public static IEnumerator ClaimOnlineAward(int awardID, Action<bool, string, int> callback)
        {
            bool claimSuccess = false;
            string errorMessage = null;
            int addedCoin = 0;

            yield return BackendFacade.ClaimOnlineAward(awardID, result =>
            {
                if (!result.Success || result.Data == null || !result.Data.success)
                {
                    errorMessage = result.Message ?? "领取失败";
                    return;
                }

                if (result.Data.data != null)
                    addedCoin = result.Data.data.addedCoin;
                claimSuccess = true;
            });

            if (!claimSuccess)
            {
                callback(false, errorMessage ?? "领取失败", 0);
                yield break;
            }

            bool refreshSuccess = false;
            yield return BackendFacade.RefreshSignIn(result =>
            {
                refreshSuccess = result.Success && result.Data != null && result.Data.success;
                if (!refreshSuccess)
                    errorMessage = result.Message ?? "签到状态刷新失败";
            });

            callback(refreshSuccess, refreshSuccess ? "领取成功" : errorMessage, refreshSuccess ? addedCoin : 0);
        }

        [Serializable]
        public class OnlineAwardConfigListWrapper
        {
            public List<OnlineAwardConfig> onlineAwards;
        }
    }

    public static class DailySignInModule
    {
        private static List<DailyAwardConfig> dailyConfigs;

        static DailySignInModule()
        {
            LoadDailyConfig();
        }

        public static void LoadDailyConfig()
        {
            TextAsset json = Resources.Load<TextAsset>("Jsons/DailyAwardConfig");
            if (json == null)
            {
                dailyConfigs = new List<DailyAwardConfig>();
                return;
            }

            dailyConfigs = JsonUtility.FromJson<DailyAwardConfigListWrapper>(json.text).dailyAwards;
            foreach (var config in dailyConfigs)
            {
                var baseItem = RewardItemSystem.GetRewardItem(config.baseReward.rewardId);
                config.baseReward.rewardName = baseItem.rewardName;
                config.baseReward.rewardSprite = baseItem.rewardSprite;
                if (config.extraReward != null && config.extraReward.rewardId != 0)
                {
                    var extraItem = RewardItemSystem.GetRewardItem(config.extraReward.rewardId);
                    config.extraReward.rewardName = extraItem.rewardName;
                    config.extraReward.rewardSprite = extraItem.rewardSprite;
                }
                else
                {
                    config.extraReward = null;
                }
            }
        }

        public static List<DailyAwardConfig> GetDailyAwardConfigs()
        {
            return dailyConfigs;
        }

        public static bool IsDailyAwardClaimed(int awardID)
        {
            return signInState != null && signInState.dailyRewardClaimed.Contains(awardID);
        }

        public static void ClaimDailyAward(int awardID)
        {
            EnsureState();
            signInState.dailyRewardClaimed.Add(awardID);
            LocalProfileSaveSystem.SaveCurrentAccount();
        }

        public static IEnumerator SignInToday(Action<bool, string> callback)
        {
            bool signSuccess = false;
            string errorMessage = null;

            yield return BackendFacade.DailySign(result =>
            {
                if (!result.Success || result.Data == null || !result.Data.success)
                {
                    errorMessage = result.Message ?? "签到失败";
                    return;
                }

                signSuccess = true;
            });

            if (!signSuccess)
            {
                callback(false, errorMessage ?? "签到失败");
                yield break;
            }

            bool refreshSuccess = false;
            yield return BackendFacade.RefreshSignIn(result =>
            {
                refreshSuccess = result.Success && result.Data != null && result.Data.success;
                if (!refreshSuccess)
                    errorMessage = result.Message ?? "签到状态刷新失败";
            });

            callback(refreshSuccess, refreshSuccess ? "签到成功" : errorMessage);
        }

        public static IEnumerator SignInToday(Action<bool, string, int> callback)
        {
            bool signSuccess = false;
            string errorMessage = null;
            int addedCoin = 0;

            yield return BackendFacade.DailySign(result =>
            {
                if (!result.Success || result.Data == null || !result.Data.success)
                {
                    errorMessage = result.Message ?? "签到失败";
                    return;
                }

                if (result.Data.data != null)
                    addedCoin = result.Data.data.addedCoin;
                signSuccess = true;
            });

            if (!signSuccess)
            {
                callback(false, errorMessage ?? "签到失败", 0);
                yield break;
            }

            bool refreshSuccess = false;
            yield return BackendFacade.RefreshSignIn(result =>
            {
                refreshSuccess = result.Success && result.Data != null && result.Data.success;
                if (!refreshSuccess)
                    errorMessage = result.Message ?? "签到状态刷新失败";
            });

            callback(refreshSuccess, refreshSuccess ? "签到成功" : errorMessage, refreshSuccess ? addedCoin : 0);
        }

        public static int GetTodayDayIndexInWeek()
        {
            int dayOfWeek = (int)DateTime.Today.DayOfWeek;
            return dayOfWeek == 0 ? 7 : dayOfWeek;
        }

        public static int GetWeekIndex(DateTime date)
        {
            var culture = CultureInfo.CurrentCulture;
            return culture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }

        public static void CheckAndResetWeek()
        {
            EnsureState();
            int weekIndex = GetWeekIndex(DateTime.Today);
            if (signInState.currentWeekIndex <= 0)
            {
                signInState.currentWeekIndex = weekIndex;
                LocalProfileSaveSystem.SaveCurrentAccount();
            }
            else if (signInState.currentWeekIndex != weekIndex)
            {
                signInState.currentWeekIndex = weekIndex;
                signInState.dailyRewardClaimed.Clear();
                signInState.continuousSignDays = 0;
                signInState.dailySigned = false;
                LocalProfileSaveSystem.SaveCurrentAccount();
            }
        }

        public static void CheckAndResetDay()
        {
            EnsureState();
            if (signInState.lastDailyResetDate == DateTime.MinValue)
            {
                signInState.lastDailyResetDate = DateTime.Today;
                if (signInState.todayOnlineSeconds <= 0 && signInState.todayOnlineMinutes > 0)
                    signInState.todayOnlineSeconds = signInState.todayOnlineMinutes * 60;
                LocalProfileSaveSystem.SaveCurrentAccount();
            }
            else if (signInState.lastDailyResetDate.Date != DateTime.Today)
            {
                signInState.dailySigned = false;
                signInState.todayOnlineMinutes = 0;
                signInState.todayOnlineSeconds = 0;
                signInState.onlineRewardClaimed.Clear();
                signInState.lastDailyResetDate = DateTime.Today;
                LocalProfileSaveSystem.SaveCurrentAccount();
            }
        }

        [Serializable]
        public class DailyAwardConfigListWrapper
        {
            public List<DailyAwardConfig> dailyAwards;
        }
    }

    public static class TotalLoginModule
    {
        private static List<TotalAwardConfig> totalConfigs;

        static TotalLoginModule()
        {
            LoadTotalConfig();
        }

        public static void LoadTotalConfig()
        {
            TextAsset json = Resources.Load<TextAsset>("Jsons/TotalAwardConfig");
            if (json == null)
            {
                totalConfigs = new List<TotalAwardConfig>();
                return;
            }

            totalConfigs = JsonUtility.FromJson<TotalAwardConfigListWrapper>(json.text).totalAwards;
        }

        public static List<TotalAwardConfig> GetTotalAwardConfigs()
        {
            return totalConfigs;
        }

        public static bool IsTotalAwardClaimed(int awardID)
        {
            return signInState != null && signInState.totalRewardClaimed.Contains(awardID);
        }

        public static IEnumerator ClaimTotalAward(int awardID, Action<bool, string> callback)
        {
            bool claimSuccess = false;
            string errorMessage = null;

            yield return BackendFacade.ClaimTotalAward(awardID, result =>
            {
                if (!result.Success || result.Data == null || !result.Data.success)
                {
                    errorMessage = result.Message ?? "领取失败";
                    return;
                }

                claimSuccess = true;
            });

            if (!claimSuccess)
            {
                callback(false, errorMessage ?? "领取失败");
                yield break;
            }

            bool refreshSuccess = false;
            yield return BackendFacade.RefreshSignIn(result =>
            {
                refreshSuccess = result.Success && result.Data != null && result.Data.success;
                if (!refreshSuccess)
                    errorMessage = result.Message ?? "签到状态刷新失败";
            });

            callback(refreshSuccess, refreshSuccess ? "领取成功" : errorMessage);
        }

        public static IEnumerator ClaimTotalAward(int awardID, Action<bool, string, int> callback)
        {
            bool claimSuccess = false;
            string errorMessage = null;
            int addedCoin = 0;

            yield return BackendFacade.ClaimTotalAward(awardID, result =>
            {
                if (!result.Success || result.Data == null || !result.Data.success)
                {
                    errorMessage = result.Message ?? "领取失败";
                    return;
                }

                if (result.Data.data != null)
                    addedCoin = result.Data.data.addedCoin;
                claimSuccess = true;
            });

            if (!claimSuccess)
            {
                callback(false, errorMessage ?? "领取失败", 0);
                yield break;
            }

            bool refreshSuccess = false;
            yield return BackendFacade.RefreshSignIn(result =>
            {
                refreshSuccess = result.Success && result.Data != null && result.Data.success;
                if (!refreshSuccess)
                    errorMessage = result.Message ?? "签到状态刷新失败";
            });

            callback(refreshSuccess, refreshSuccess ? "领取成功" : errorMessage, refreshSuccess ? addedCoin : 0);
        }

        public static bool CanClaimTotalAward(int awardID)
        {
            TotalAwardConfig config = totalConfigs.Find(i => i.awardID == awardID);
            return config != null && !IsTotalAwardClaimed(awardID) &&
                   signInState != null && signInState.totalLoginDays >= config.requiredDays;
        }

        public static TotalAwardConfig GetTotalAwardConfig(int awardID)
        {
            return totalConfigs.Find(i => i.awardID == awardID);
        }
    }

    [Serializable]
    public class TotalAwardConfigListWrapper
    {
        public List<TotalAwardConfig> totalAwards;
    }
}
