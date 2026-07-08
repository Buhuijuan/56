using System;
using System.Collections;
using System.Collections.Generic;

public static class BackendFacade
{
    private static bool _suppressElfPrompt = false;
    public static void SetElfPromptSuppressed(bool suppressed)
    {
        _suppressElfPrompt = suppressed;
    }

    private static void TryShowElfPrompt(BackendElfPrompt prompt)
    {
        if (prompt == null)
            return;

        if (_suppressElfPrompt)
            return;

        if (!prompt.autoPopup)
            return;

        BackendTaskEntry currentMainTask = GameStateStore.Tasks != null ? GameStateStore.Tasks.currentMainTask : null;
        if (currentMainTask != null &&
            currentMainTask.taskCode == prompt.taskCode &&
            currentMainTask.status == "AVAILABLE")
            return;

        if (!string.IsNullOrEmpty(prompt.taskCode))
        {
            BackendTaskEntry task = BackendTaskStore.FindTask(prompt.taskCode);
            if (task != null && task.status == "AVAILABLE")
                return;
        }

        if (TaskEventRuntime.IsIntroductionFlowActive(prompt.taskCode))
        {
            string stage = prompt.stage?.Trim().ToLowerInvariant();
            bool allowDuringIntroFlow = !string.IsNullOrEmpty(stage) &&
                                        (stage.Contains("progress") || stage.Contains("complete"));
            if (!allowDuringIntroFlow)
                return;
        }

        if (TaskPromptSystem.WasPromptShownRecently(prompt, 5f))
            return;

        TaskPromptSystem.EnqueueAutoPrompt(prompt);
    }

    public static void BeginSuppressElfPrompt()
    {
        _suppressElfPrompt = true;
    }

    public static void EndSuppressElfPrompt()
    {
        _suppressElfPrompt = false;
    }

    // -----------------------------
    // Auth
    // -----------------------------
    public static IEnumerator SendCode(string mailbox, Action<BackendApiResult<BackendSendCodeEnvelope>> onComplete)
    {
        AuthSendCodeBody body = new() { mailbox = mailbox };
        yield return ApiClient.Post<BackendSendCodeEnvelope>("/api/auth/send-code", body, onComplete);
    }

    public static IEnumerator Register(string mailbox, string verificationCode, string password,
        Action<BackendApiResult<BackendAuthEnvelope>> onComplete)
    {
        AuthRegisterBody body = new()
        {
            mailbox = mailbox,
            verificationCode = verificationCode,
            password = password
        };
        yield return ApiClient.Post<BackendAuthEnvelope>("/api/auth/register", body, onComplete);
    }

    public static IEnumerator Login(string mailbox, string password, Action<BackendApiResult<BackendAuthEnvelope>> onComplete)
    {
        AuthLoginBody body = new() { mailbox = mailbox, password = password };
        yield return ApiClient.Post<BackendAuthEnvelope>("/api/auth/login", body, result =>
        {
            if (result.Success && result.Data != null && result.Data.success && result.Data.data != null)
            {
                SessionStore.Save(new SessionSnapshot
                {
                    token = result.Data.data.token,
                    accountId = result.Data.data.accountId.ToString(),
                    accountCode = result.Data.data.accountCode,
                    mailbox = result.Data.data.mailbox,
                    currentRoleId = result.Data.data.currentRoleId,
                    hasCurrentRole = result.Data.data.currentRoleId != 0
                });
            }
            onComplete?.Invoke(result);
        });
    }

    public static IEnumerator ResetPassword(string mailbox, string verificationCode, string newPassword,
        Action<BackendApiResult<BackendSendCodeEnvelope>> onComplete)
    {
        AuthResetPasswordBody body = new()
        {
            mailbox = mailbox,
            verificationCode = verificationCode,
            newPassword = newPassword
        };
        yield return ApiClient.Post<BackendSendCodeEnvelope>("/api/auth/reset-password", body, onComplete);
    }

    // -----------------------------
    // Player
    // -----------------------------
    public static IEnumerator RefreshPlayerMe(Action<BackendApiResult<BackendPlayerMeEnvelope>> onComplete)
    {
        yield return ApiClient.Get<BackendPlayerMeEnvelope>("/api/player/me", result =>
        {
            if (result.Success && result.Data != null && result.Data.success)
            {
                GameStateStore.Me = result.Data.data;
                BackendStateMapper.ApplyPlayerMe(result.Data.data);

                if (result.Data.data != null)
                {
                    SessionSnapshot snapshot = SessionStore.Current;
                    snapshot.accountId = result.Data.data.accountId.ToString();
                    snapshot.accountCode = result.Data.data.accountCode;
                    snapshot.mailbox = result.Data.data.mailbox;
                    snapshot.currentRoleId = result.Data.data.currentRoleId;
                    snapshot.hasCurrentRole = result.Data.data.currentRoleId != 0;
                    SessionStore.Save(snapshot);
                }
            }
            onComplete?.Invoke(result);
        });
    }

    public static IEnumerator CreateRole(string campusName, string nickName, int characterId,
        Action<BackendApiResult<BackendRoleEnvelope>> onComplete)
    {
        PlayerCreateRoleBody body = new()
        {
            schoolId = 1,
            nickName = nickName,
            characterId = characterId
        };
        yield return ApiClient.Post<BackendRoleEnvelope>("/api/player/roles", body, result =>
        {
            if (result.Success && result.Data != null && result.Data.success && result.Data.data != null)
            {
                SessionSnapshot snapshot = SessionStore.Current;
                snapshot.currentRoleId = result.Data.data.id;
                snapshot.hasCurrentRole = true;
                SessionStore.Save(snapshot);
            }
            onComplete?.Invoke(result);
        });
    }

    public static IEnumerator SwitchCurrentRole(long roleId, Action<BackendApiResult<BackendSwitchRoleEnvelope>> onComplete)
    {
        PlayerSwitchRoleBody body = new() { roleId = roleId };
        yield return ApiClient.Put<BackendSwitchRoleEnvelope>("/api/player/current-role", body, result =>
        {
            if (result.Success && result.Data != null && result.Data.success && result.Data.data != null)
            {
                SessionSnapshot snapshot = SessionStore.Current;
                snapshot.currentRoleId = result.Data.data.currentRoleId;
                snapshot.hasCurrentRole = result.Data.data.currentRoleId != 0;
                SessionStore.Save(snapshot);
            }
            onComplete?.Invoke(result);
        });
    }

    public static IEnumerator UploadCurrentRoleAvatar(
        byte[] fileBytes,
        string fileName,
        string mimeType,
        Action<BackendApiResult<BackendAvatarUploadEnvelope>> onComplete)
    {
        yield return ApiClient.PostMultipart<BackendAvatarUploadEnvelope>(
            "/api/player/current-role/avatar",
            "file",
            fileBytes,
            fileName,
            mimeType,
            result =>
            {
                if (result.Success && result.Data != null && result.Data.success && result.Data.data != null)
                {
                    RoleData currentRole = AccountSystem.GetCurrentRole();
                    if (currentRole != null && !string.IsNullOrWhiteSpace(result.Data.data.avatarUrl))
                        currentRole.avatarUrl = result.Data.data.avatarUrl;
                }

                onComplete?.Invoke(result);
            });
    }

    public static IEnumerator DeleteRole(long roleId, Action<BackendApiResult<string>> onComplete)
    {
        yield return ApiClient.Delete<string>($"/api/player/roles/{roleId}", onComplete);
    }

    public static IEnumerator DeleteAccount(Action<BackendApiResult<string>> onComplete)
    {
        yield return ApiClient.Delete<string>("/api/player/account", onComplete);
    }

    public static IEnumerator ChangePassword(string newPassword, Action<BackendApiResult<string>> onComplete)
    {
        PlayerChangePasswordBody body = new() { newPassword = newPassword };
        yield return ApiClient.Put<string>("/api/player/profile/password", body, onComplete);
    }

    public static IEnumerator ChangeMailbox(string mailbox, Action<BackendApiResult<string>> onComplete)
    {
        PlayerChangeMailboxBody body = new() { mailbox = mailbox };
        yield return ApiClient.Put<string>("/api/player/profile/mailbox", body, onComplete);
    }

    // -----------------------------
    // Home
    // -----------------------------
    public static IEnumerator RefreshHome(Action<BackendApiResult<BackendGameHomeEnvelope>> onComplete)
    {
        yield return ApiClient.Get<BackendGameHomeEnvelope>("/api/game/home", result =>
        {
            if (result.Success && result.Data != null && result.Data.success)
            {
                GameStateStore.Home = result.Data.data;
                BackendStateMapper.ApplyGameHome(result.Data.data);
            }
            onComplete?.Invoke(result);
        });
    }

    // -----------------------------
    // Tasks
    // -----------------------------
    public static IEnumerator RefreshTasks(Action<BackendApiResult<BackendTaskPageEnvelope>> onComplete, bool autoPrompt = false)
    {
        yield return ApiClient.Get<BackendTaskPageEnvelope>("/api/tasks", result =>
        {
            if (result.Success && result.Data != null && result.Data.success)
            {
                GameStateStore.Tasks = result.Data.data;
                if (result.Data.data != null &&
                    result.Data.data.currentMainTask != null &&
                    result.Data.data.elfPrompt != null &&
                    result.Data.data.elfPrompt.taskCode == result.Data.data.currentMainTask.taskCode)
                {
                    result.Data.data.currentMainTask.elfPrompt = result.Data.data.elfPrompt;
                }
                BackendTaskStore.SetPage(result.Data.data);

                if (autoPrompt)
                    TryShowElfPrompt(result.Data.data.elfPrompt);
            }
            onComplete?.Invoke(result);
        });
    }

    public static IEnumerator RefreshTaskChapters(Action<List<BackendTaskChapterEntry>> onComplete)
    {
        yield return ApiClient.Get<BackendTaskChaptersEnvelope>("/api/tasks/chapters", result =>
        {
            if (result.Success && result.Data != null && result.Data.data != null)
            {
                BackendTaskStore.SetChapters(result.Data.data.chapters);
                onComplete?.Invoke(result.Data.data.chapters);
            }
            else
            {
                onComplete?.Invoke(null);
            }
        });
    }

    public static IEnumerator RefreshCurrentMainTask(Action<BackendApiResult<BackendTaskCurrentMainEnvelope>> onComplete, bool autoPrompt = false)
    {
        yield return ApiClient.Get<BackendTaskCurrentMainEnvelope>("/api/tasks/current/main", result =>
        {
            if (result.Success && result.Data != null && result.Data.success && result.Data.data != null)
            {
                BackendTaskPageData page = GameStateStore.Tasks ?? new BackendTaskPageData();
                page.currentMainTask = result.Data.data.task;
                page.elfPrompt = result.Data.data.elfPrompt;
                if (page.currentMainTask != null &&
                    page.elfPrompt != null &&
                    page.elfPrompt.taskCode == page.currentMainTask.taskCode)
                {
                    page.currentMainTask.elfPrompt = page.elfPrompt;
                }
                GameStateStore.Tasks = page;
                BackendTaskStore.SetPage(page);

                // ⭐ 自动弹出小精灵提示
                if (autoPrompt)
                    TryShowElfPrompt(result.Data.data.elfPrompt);
            }
            onComplete?.Invoke(result);
        });
    }

    public static IEnumerator AcceptTask(string taskCode, Action<BackendApiResult<BackendTaskOperationEnvelope>> onComplete)
    {
        yield return ApiClient.Post<BackendTaskOperationEnvelope>($"/api/tasks/{taskCode}/accept", null, result =>
        {
            if (result.Success && result.Data != null && result.Data.success)
            {
                TryShowElfPrompt(result.Data.data.elfPrompt);
            }
            onComplete?.Invoke(result);
        });
    }

    public static IEnumerator ClaimTask(string taskCode, Action<BackendApiResult<BackendTaskOperationEnvelope>> onComplete)
    {
        yield return ApiClient.Post<BackendTaskOperationEnvelope>($"/api/tasks/{taskCode}/claim", null, result =>
        {
            if (result.Success && result.Data != null && result.Data.success)
            {
                TryShowElfPrompt(result.Data.data.elfPrompt);
            }
            onComplete?.Invoke(result);
        });
    }

    public static IEnumerator SendTaskEvent(TaskEventBody body, Action<BackendApiResult<BackendTaskEventEnvelope>> onComplete, bool autoPrompt = false)
    {
        yield return ApiClient.Post<BackendTaskEventEnvelope>("/api/tasks/events", body, result =>
        {
            if (result.Success && result.Data != null && result.Data.success && result.Data.data != null)
            {
                BackendTaskPageData page = GameStateStore.Tasks ?? new BackendTaskPageData();
                page.currentMainTask = result.Data.data.currentMainTask;
                page.elfPrompt = result.Data.data.elfPrompt;
                if (page.currentMainTask != null &&
                    page.elfPrompt != null &&
                    page.elfPrompt.taskCode == page.currentMainTask.taskCode)
                {
                    page.currentMainTask.elfPrompt = page.elfPrompt;
                }
                GameStateStore.Tasks = page;
                BackendTaskStore.SetPage(page);

                // ⭐ 自动弹出小精灵提示（任务进度 + 任务完成）
                if (autoPrompt)
                {
                    TryShowElfPrompt(result.Data.data.elfPrompt);
                    TryShowElfPrompt(result.Data.data.completedElfPrompt);
                }
            }
            onComplete?.Invoke(result);
        });
    }

    // -----------------------------
    // SignIn / Growth / ClockIn
    // -----------------------------
    public static IEnumerator RefreshSignIn(Action<BackendApiResult<BackendSignInEnvelope>> onComplete)
    {
        yield return ApiClient.Get<BackendSignInEnvelope>("/api/signin", result =>
        {
            if (result.Success && result.Data != null && result.Data.success && result.Data.data != null)
            {
                GameStateStore.SignIn = result.Data.data;
                if (GameStateStore.Home != null)
                    GameStateStore.Home.signIn = result.Data.data.state;

                RoleData currentRole = AccountSystem.GetCurrentRole();
                if (currentRole != null)
                    currentRole.signInState = BackendStateMapper.ToFrontSignIn(result.Data.data.state);
            }
            onComplete?.Invoke(result);
        });
    }

    public static IEnumerator RefreshClockIn(Action<BackendApiResult<BackendClockInEnvelope>> onComplete)
    {
        yield return ApiClient.Get<BackendClockInEnvelope>("/api/activities/clockin/current", result =>
        {
            if (result.Success && result.Data != null && result.Data.success)
                GameStateStore.ClockIn = result.Data.data;

            onComplete?.Invoke(result);
        });
    }

    public static IEnumerator RefreshQuizCurrent(Action<BackendApiResult<BackendQuizCurrentEnvelope>> onComplete)
    {
        yield return ApiClient.Get<BackendQuizCurrentEnvelope>("/api/activities/quiz/current", result =>
        {
            if (result.Success && result.Data != null && result.Data.success)
                GameStateStore.Quiz = result.Data.data;

            onComplete?.Invoke(result);
        });
    }

    public static IEnumerator StartQuiz(Action<BackendApiResult<BackendQuizStartEnvelope>> onComplete)
    {
        yield return ApiClient.Post<BackendQuizStartEnvelope>("/api/activities/quiz/start", null, onComplete);
    }

    public static IEnumerator SubmitQuiz(QuizSubmitBody body, Action<BackendApiResult<BackendQuizSubmitEnvelope>> onComplete)
    {
        yield return ApiClient.Post<BackendQuizSubmitEnvelope>("/api/activities/quiz/submit", body, onComplete);
    }

    public static IEnumerator ClaimQuizWeeklyReward(Action<BackendApiResult<BackendQuizWeeklyClaimEnvelope>> onComplete)
    {
        yield return ApiClient.Post<BackendQuizWeeklyClaimEnvelope>("/api/activities/quiz/claim-weekly-reward", null, onComplete);
    }

    public static IEnumerator CheckClockIn(string locationId, ClockInCheckBody body, Action<BackendApiResult<BackendClockInCheckEnvelope>> onComplete)
    {
        yield return ApiClient.Post<BackendClockInCheckEnvelope>($"/api/activities/clockin/{locationId}/check", body, onComplete);
    }

    public static IEnumerator RefreshGrowth(Action<BackendApiResult<BackendGrowthEnvelope>> onComplete)
    {
        yield return ApiClient.Get<BackendGrowthEnvelope>("/api/growth", result =>
        {
            if (result.Success && result.Data != null && result.Data.success && result.Data.data != null)
                BackendStateMapper.ApplyGrowth(result.Data.data);

            onComplete?.Invoke(result);
        });
    }

    public static IEnumerator EquipTitle(int titleId, Action<BackendApiResult<BackendEquipTitleEnvelope>> onComplete)
    {
        yield return ApiClient.Post<BackendEquipTitleEnvelope>($"/api/titles/{titleId}/equip", null, result =>
        {
            if (result.Success && result.Data != null && result.Data.success)
            {
                RoleData currentRole = AccountSystem.GetCurrentRole();
                if (currentRole != null)
                {
                    currentRole.titleState ??= new PlayerTitleState();
                    currentRole.titleState.equippedTitleID = titleId;
                }

                if (GameStateStore.Home != null && GameStateStore.Home.title != null)
                    GameStateStore.Home.title.equippedTitleId = titleId;
            }

            onComplete?.Invoke(result);
        });
    }

    public static IEnumerator ClaimGrowthReward(string stageId, Action<BackendApiResult<BackendGrowthClaimEnvelope>> onComplete)
    {
        yield return ApiClient.Post<BackendGrowthClaimEnvelope>($"/api/growth/{stageId}/claim", null, onComplete);
    }

    public static IEnumerator ClaimLevelReward(int level, Action<BackendApiResult<BackendLevelRewardClaimEnvelope>> onComplete)
    {
        yield return ApiClient.Post<BackendLevelRewardClaimEnvelope>($"/api/levels/{level}/reward/claim", null, onComplete);
    }

    public static IEnumerator ClaimLevelTitle(int level, Action<BackendApiResult<BackendLevelTitleClaimEnvelope>> onComplete)
    {
        yield return ApiClient.Post<BackendLevelTitleClaimEnvelope>($"/api/levels/{level}/title/claim", null, onComplete);
    }

    public static IEnumerator RefreshGrowthProgress(Action<BackendApiResult<BackendSimpleStatusEnvelope>> onComplete)
    {
        yield return ApiClient.Post<BackendSimpleStatusEnvelope>("/api/growth/refresh", null, onComplete);
    }

    public static IEnumerator DailySign(Action<BackendApiResult<BackendSimpleStatusEnvelope>> onComplete)
    {
        yield return ApiClient.Post<BackendSimpleStatusEnvelope>("/api/signin/daily", null, onComplete);
    }

    public static IEnumerator ClaimOnlineAward(int awardId, Action<BackendApiResult<BackendSimpleStatusEnvelope>> onComplete)
    {
        yield return ApiClient.Post<BackendSimpleStatusEnvelope>($"/api/signin/online/{awardId}/claim", null, onComplete);
    }

    public static IEnumerator ClaimTotalAward(int awardId, Action<BackendApiResult<BackendSimpleStatusEnvelope>> onComplete)
    {
        yield return ApiClient.Post<BackendSimpleStatusEnvelope>($"/api/signin/total/{awardId}/claim", null, onComplete);
    }

    public static IEnumerator Heartbeat(int elapsedSeconds, Action<BackendApiResult<BackendSimpleStatusEnvelope>> onComplete)
    {
        HeartbeatBody body = new() { elapsedSeconds = elapsedSeconds };
        yield return ApiClient.Post<BackendSimpleStatusEnvelope>("/api/player/heartbeat", body, result =>
        {
            if (result.Success && result.Data != null && result.Data.success && result.Data.data != null)
            {
                RoleData currentRole = AccountSystem.GetCurrentRole();
                if (currentRole != null)
                    currentRole.signInState.todayOnlineMinutes = result.Data.data.todayOnlineMinutes;

                if (GameStateStore.Home != null && GameStateStore.Home.signIn != null)
                    GameStateStore.Home.signIn.todayOnlineSeconds = result.Data.data.todayOnlineSeconds;
            }
            onComplete?.Invoke(result);
        });
    }

    // -----------------------------
    // Restore Session
    // -----------------------------
    public static IEnumerator RestoreSession(Action<bool, string> onComplete)
    {
        if (!BackendSettings.UseBackendMode || !SessionStore.HasToken)
        {
            UnityEngine.Debug.LogWarning($"[BackendFacade] RestoreSession aborted. UseBackendMode={BackendSettings.UseBackendMode}, HasToken={SessionStore.HasToken}");
            onComplete?.Invoke(false, "No token");
            yield break;
        }

        UnityEngine.Debug.Log($"[BackendFacade] RestoreSession start. TokenPresent={SessionStore.HasToken}, CurrentRoleId={SessionStore.Current.currentRoleId}, HasCurrentRole={SessionStore.Current.hasCurrentRole}");

        bool meDone = false;
        BackendApiResult<BackendPlayerMeEnvelope> meResult = null;

        yield return RefreshPlayerMe(result =>
        {
            meResult = result;
            meDone = true;
        });

        UnityEngine.Debug.Log(
            $"[BackendFacade] RestoreSession.RefreshPlayerMe done. meDone={meDone}, Success={meResult?.Success}, Status={meResult?.StatusCode}, Message={meResult?.Message}, " +
            $"HasData={meResult?.Data != null}, ApiSuccess={meResult?.Data?.success}, AccountId={meResult?.Data?.data?.accountId}, CurrentRoleId={meResult?.Data?.data?.currentRoleId}, RolesCount={meResult?.Data?.data?.roles?.Count}");

        if (!meDone || meResult == null || !meResult.Success || meResult.Data == null || !meResult.Data.success)
        {
            onComplete?.Invoke(false, meResult != null ? meResult.Message : "Failed to load player");
            yield break;
        }

        if (meResult.Data.data == null || meResult.Data.data.currentRoleId == 0)
        {
            UnityEngine.Debug.Log("[BackendFacade] RestoreSession detected no current role. Skipping RefreshHome and treating session restore as success.");
            GameStateStore.Home = null;
            onComplete?.Invoke(true, null);
            yield break;
        }

        bool homeDone = false;
        BackendApiResult<BackendGameHomeEnvelope> homeResult = null;

        yield return RefreshHome(result =>
        {
            homeResult = result;
            homeDone = true;
        });

        UnityEngine.Debug.Log(
            $"[BackendFacade] RestoreSession.RefreshHome done. homeDone={homeDone}, Success={homeResult?.Success}, Status={homeResult?.StatusCode}, Message={homeResult?.Message}, " +
            $"HasData={homeResult?.Data != null}, ApiSuccess={homeResult?.Data?.success}, RoleId={homeResult?.Data?.data?.role?.id}, RoleNick={homeResult?.Data?.data?.role?.nickName}");

        if (!homeDone || homeResult == null || !homeResult.Success || homeResult.Data == null || !homeResult.Data.success)
        {
            onComplete?.Invoke(false, homeResult != null ? homeResult.Message : "Failed to load home");
            yield break;
        }

        UnityEngine.Debug.Log("[BackendFacade] RestoreSession success.");
        onComplete?.Invoke(true, null);
    }
}
