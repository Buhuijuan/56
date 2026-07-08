using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TaskEventRuntime : MonoBehaviour
{
    private static TaskEventRuntime instance;
    private static readonly Dictionary<string, BackendElfPrompt> LocalNpcTaskProgressPrompts = new()
    {
        ["M_1_2"] = CreateNpcTaskProgressPrompt(
            "M_1_2",
            "请和迎新接待处的志愿者打个招呼吧！",
            "他会帮你确认信息，并告诉你宿舍安排。"),
        ["M_1_3"] = CreateNpcTaskProgressPrompt(
            "M_1_3",
            "和楼管阿姨打个招呼吧。",
            "她会帮你激活门禁，并告诉你入住注意事项。"),
        ["M_1_4"] = CreateNpcTaskProgressPrompt(
            "M_1_4",
            "去和护士确认一下体检流程吧。",
            "她会引导你完成检查，并领取体检结果。")
    };

    private readonly HashSet<string> reportedProximityTasks = new();
    private readonly HashSet<long> landmarksInRange = new();
    private readonly HashSet<string> introCardTasksInProgress = new();
    private readonly HashSet<string> npcDialogueTasksInProgress = new();
    private readonly Dictionary<string, float> ambientNpcDialogueShownAt = new();
    private readonly HashSet<string> npcTaskProgressShown = new();
    private bool bikeStationInRange;
    private bool hasBikePositionSample;
    private bool isSending;
    private float bikeDistanceAccumulator;
    private Vector3 lastBikePositionSample;
    private const string PhotoTaskCode = "B_1_1";
    private const string BikeTaskCode = "B_2_1";
    private const int DefaultPhotoTaskTargetCount = 3;
    private const float LocalNpcDialogueRadius = 5f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot()
    {
        if (instance != null)
            return;

        GameObject go = new GameObject("TaskEventRuntime");
        instance = go.AddComponent<TaskEventRuntime>();
        DontDestroyOnLoad(go);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        landmarksInRange.Clear();
        bikeStationInRange = false;
        reportedProximityTasks.Clear();
        introCardTasksInProgress.Clear();
        npcDialogueTasksInProgress.Clear();
        npcTaskProgressShown.Clear();
        ambientNpcDialogueShownAt.Clear();
        hasBikePositionSample = false;
        bikeDistanceAccumulator = 0f;
        lastBikePositionSample = Vector3.zero;

        if (scene.name == "05_Game" || scene.name == "05_Campus")
            StartCoroutine(RefreshTasksAfterSceneLoad());
    }

    private IEnumerator RefreshTasksAfterSceneLoad()
    {
        if (!BackendSettings.UseBackendMode || !SessionStore.HasToken)
            yield break;

        yield return BackendFacade.RefreshTasks(null);
        yield return BackendFacade.RefreshCurrentMainTask(null);
        yield return BackendFacade.RefreshTaskChapters(null);
    }

    private void Update()
    {
        if (!BackendSettings.UseBackendMode || !SessionStore.HasToken || isSending)
            return;

        TryShowNpcTaskProgressPrompt();
        if (isSending)
            return;

        TryHandleLocalNpcDialogue();
        if (isSending)
            return;

        TryHandleAmbientNpcDialogue();
        if (isSending)
            return;

        TryReportCurrentMainArrival();
        if (isSending)
            return;

        TryReportCurrentMainNpcDialogue();
        if (isSending)
            return;

        TryReportLandmarkVisit();
        if (isSending)
            return;

        TryReportBikeStationVisit();
        if (isSending)
            return;

        TryReportBikeTrialDistance();
    }

    private void TryReportCurrentMainArrival()
    {
        BackendTaskEntry currentMain = BackendTaskStore.GetCurrentMainTask();
        if (currentMain == null || currentMain.status != "IN_PROGRESS" || currentMain.targetType != "arrive_building")
            return;
        if (TaskNpcDialogueSystem.ShouldDeferArrivalToNpcDialogue(currentMain.taskCode))
            return;
        if (reportedProximityTasks.Contains(currentMain.taskCode))
            return;
        if (introCardTasksInProgress.Contains(currentMain.taskCode))
            return;
        if (!TryResolveAnchor(currentMain.targetId, out ClockInLandmarkAnchor anchor))
        {
            Debug.Log($"[TaskEventRuntime] Arrival skipped. task={currentMain.taskCode}, targetId={currentMain.targetId}, anchorResolved=False");
            return;
        }

        Vector3 playerPosition = GetPlayerPosition();
        float distance = GetFlatDistance(playerPosition, anchor.transform.position);
        Debug.Log(
            $"[TaskEventRuntime] Arrival check. task={currentMain.taskCode}, status={currentMain.status}, " +
            $"targetId={currentMain.targetId}, anchor={anchor.locationId}, distance={distance:F2}, " +
            $"radius={ClockInEventSystem.GetCheckRadius():F2}");
        if (distance > ClockInEventSystem.GetCheckRadius())
            return;

        if (TaskIntroductionCardSystem.ShouldUseIntroductionCard(currentMain) &&
            TaskIntroductionCardSystem.TryGetConfig(currentMain.targetId, out IntroductionCardConfig config) &&
            UIManager.Instance != null)
        {
            Debug.Log($"[TaskEventRuntime] Arrival intro flow begin. task={currentMain.taskCode}, targetId={currentMain.targetId}, location={config.locationId}");
            introCardTasksInProgress.Add(currentMain.taskCode);
            StartCoroutine(HandleArrivalWithIntroduction(currentMain.taskCode, currentMain.targetId, config));
            return;
        }

        reportedProximityTasks.Add(currentMain.taskCode);
        isSending = true;

        StartCoroutine(SendEventAndRefresh(new TaskEventBody
        {
            eventType = "ARRIVE_BUILDING",
            targetType = "arrive_building",
            targetId = currentMain.targetId,
            increment = 1,
            currentPosX = playerPosition.x,
            currentPosY = playerPosition.y,
            currentPosZ = playerPosition.z,
            targetPosX = anchor.transform.position.x,
            targetPosY = anchor.transform.position.y,
            targetPosZ = anchor.transform.position.z,
            targetAnchorKey = anchor.locationId
        }, null));
    }

    private IEnumerator CompleteArrivalTaskAfterIntroduction(string taskCode, long targetId)
    {
        Debug.Log($"[TaskEventRuntime] Introduction closed. Try complete arrival. task={taskCode}, targetId={targetId}");

        BackendTaskEntry currentMain = BackendTaskStore.GetCurrentMainTask();
        if (currentMain == null || currentMain.taskCode != taskCode || currentMain.status != "IN_PROGRESS")
        {
            introCardTasksInProgress.Remove(taskCode);
            Debug.Log($"[TaskEventRuntime] Introduction completion aborted. task={taskCode}, currentMain={currentMain?.taskCode}, currentStatus={currentMain?.status}");
            yield break;
        }
        if (reportedProximityTasks.Contains(taskCode))
        {
            introCardTasksInProgress.Remove(taskCode);
            Debug.Log($"[TaskEventRuntime] Introduction completion skipped because task already reported. task={taskCode}");
            yield break;
        }
        Vector3 playerPosition = GetPlayerPosition();

        Debug.Log($"[TaskEventRuntime] Introduction completion sending ARRIVE_BUILDING. task={taskCode}, pos=({playerPosition.x:F2},{playerPosition.y:F2},{playerPosition.z:F2})");
        reportedProximityTasks.Add(taskCode);
        isSending = true;

        yield return SendEventAndRefresh(new TaskEventBody
        {
            eventType = "ARRIVE_BUILDING",
            targetType = "arrive_building",
            targetId = targetId,
            increment = 1,
            currentPosX = playerPosition.x,
            currentPosY = playerPosition.y,
            currentPosZ = playerPosition.z,
            targetPosX = TryGetAnchorPosition(targetId).x,
            targetPosY = TryGetAnchorPosition(targetId).y,
            targetPosZ = TryGetAnchorPosition(targetId).z,
            targetAnchorKey = TryGetAnchorKey(targetId)
        }, null, null, taskCode);

        introCardTasksInProgress.Remove(taskCode);
    }

    private IEnumerator HandleArrivalWithIntroduction(string taskCode, long targetId, IntroductionCardConfig config)
    {
        TaskPromptSystem.RemovePendingPromptsForTask(taskCode);
        BackendFacade.SetElfPromptSuppressed(true);
        Debug.Log($"[TaskEventRuntime] HandleArrivalWithIntroduction refresh current main. task={taskCode}, targetId={targetId}");

        BackendApiResult<BackendTaskCurrentMainEnvelope> currentMainResult = null;
        yield return BackendFacade.RefreshCurrentMainTask(result => currentMainResult = result);

        BackendFacade.SetElfPromptSuppressed(false);

        if (instance == null)
            yield break;

        BackendElfPrompt progressPrompt = ExtractArrivalProgressPrompt(taskCode, currentMainResult);
        if (progressPrompt != null)
        {
            Debug.Log(
                $"[TaskEventRuntime] Arrival progress prompt ready. task={taskCode}, stage={progressPrompt.stage}, " +
                $"lineCount={progressPrompt.contents?.Count ?? 0}, firstLine={(progressPrompt.contents != null && progressPrompt.contents.Count > 0 ? progressPrompt.contents[0] : "<empty>")}");
            TaskPromptSystem.ShowPrompt(progressPrompt, () => ShowIntroductionCardForArrival(taskCode, targetId, config));
            yield break;
        }

        Debug.Log($"[TaskEventRuntime] Arrival progress prompt missing. task={taskCode}, show introduction card directly.");
        ShowIntroductionCardForArrival(taskCode, targetId, config);
    }

    private void ShowIntroductionCardForArrival(string taskCode, long targetId, IntroductionCardConfig config)
    {
        if (UIManager.Instance == null)
        {
            Debug.Log($"[TaskEventRuntime] UIManager missing, skip introduction card UI and complete directly. task={taskCode}");
            if (instance != null)
                instance.StartCoroutine(instance.CompleteArrivalTaskAfterIntroduction(taskCode, targetId));
            return;
        }

        Debug.Log($"[TaskEventRuntime] Show introduction card. task={taskCode}, targetId={targetId}, location={config.locationId}");
        UIManager.Instance.ShowIntroductionCard(config, () =>
        {
            Debug.Log($"[TaskEventRuntime] Introduction card callback invoked. task={taskCode}, targetId={targetId}");
            if (instance != null)
                instance.StartCoroutine(instance.CompleteArrivalTaskAfterIntroduction(taskCode, targetId));
        });
    }

    private static BackendElfPrompt ExtractArrivalProgressPrompt(string taskCode, BackendApiResult<BackendTaskCurrentMainEnvelope> result)
    {
        if (TaskIntroductionCardSystem.TryGetProgressPrompt(taskCode, out BackendElfPrompt localPrompt))
            return localPrompt;

        BackendElfPrompt prompt = result?.Data?.data?.elfPrompt;
        if (prompt == null)
        {
            BackendTaskEntry currentMain = BackendTaskStore.GetCurrentMainTask();
            if (currentMain != null && currentMain.taskCode == taskCode)
                prompt = currentMain.elfPrompt;
        }

        if (prompt == null || prompt.taskCode != taskCode)
            return null;

        string stage = prompt.stage?.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(stage))
        {
            if (stage.Contains("complete"))
                return null;
            if (stage.Contains("progress"))
                return prompt;
            if (stage.Contains("start"))
                return null;
        }

        // Some backend responses may omit stage. In that case, avoid replaying
        // the just-shown start prompt and only use the prompt if it was not
        // displayed very recently.
        if (TaskPromptSystem.WasPromptShownRecently(prompt, 5f))
            return null;

        return prompt;
    }

    private void TryReportCurrentMainNpcDialogue()
    {
        BackendTaskEntry currentMain = BackendTaskStore.GetCurrentMainTask();
        if (currentMain == null || currentMain.status != "IN_PROGRESS" || currentMain.targetType != "npc_dialogue")
            return;
        if (TaskNpcDialogueSystem.ShouldHandleLocally(currentMain.taskCode))
            return;
        if (reportedProximityTasks.Contains(currentMain.taskCode))
            return;
        if (!TryResolveAnchor(currentMain.targetId, out ClockInLandmarkAnchor anchor))
            return;

        Vector3 playerPosition = GetPlayerPosition();
        float distance = GetFlatDistance(playerPosition, anchor.transform.position);
        if (distance > ClockInEventSystem.GetCheckRadius())
            return;

        reportedProximityTasks.Add(currentMain.taskCode);
        TitleEventReporter.ReportNpcDialogue(GetTitleNpcId(currentMain.targetId));
        TaskPromptSystem.ShowPromptForTask(currentMain.taskCode);

        isSending = true;
        StartCoroutine(SendEventAndRefresh(new TaskEventBody
        {
            eventType = "NPC_DIALOGUE",
            targetType = "npc_dialogue",
            targetId = currentMain.targetId,
            increment = 1,
            targetPosX = anchor.transform.position.x,
            targetPosY = anchor.transform.position.y,
            targetPosZ = anchor.transform.position.z,
            targetAnchorKey = anchor.locationId
        }, null));
    }

    private void TryHandleLocalNpcDialogue()
    {
        BackendTaskEntry currentMain = BackendTaskStore.GetCurrentMainTask();
        if (currentMain == null)
            return;
        if (TaskPromptSystem.IsPromptQueuedOrShowing(currentMain.taskCode, "progress"))
            return;
        if (!TaskNpcDialogueSystem.TryGetConfig(currentMain.taskCode, out TaskNpcDialogueConfig config) || config == null)
            return;
        if (!TryResolveAnchor(config.triggerTargetId, out ClockInLandmarkAnchor anchor))
            return;

        string flowKey = $"{currentMain.taskCode}:{config.triggerTargetId}";
        if (npcDialogueTasksInProgress.Contains(flowKey))
            return;

        Vector3 playerPosition = GetPlayerPosition();
        float distance = GetFlatDistance(playerPosition, anchor.transform.position);
        if (distance > LocalNpcDialogueRadius)
            return;

        if (!config.reportTaskEvent)
        {
            if (currentMain.taskCode == "M_1_1")
            {
                if (TaskPromptSystem.IsPromptQueuedOrShowing("M_1_1", "start") ||
                    TaskPromptSystem.IsPromptQueuedOrShowing("M_1_1", "default"))
                    return;
            }

            if (UIManager.Instance != null && UIManager.Instance.IsTaskPromptBlocked)
                return;

            if (ambientNpcDialogueShownAt.ContainsKey(flowKey))
                return;

            ambientNpcDialogueShownAt[flowKey] = Time.unscaledTime;
            npcDialogueTasksInProgress.Add(flowKey);
            Debug.Log($"[TaskEventRuntime] Local NPC ambient dialogue begin. task={currentMain.taskCode}, targetId={config.triggerTargetId}, npc={config.npcId}");
            TaskPromptSystem.ShowPrompt(TaskNpcDialogueSystem.BuildPrompt(config), () =>
            {
                TitleEventReporter.ReportNpcDialogue(config.npcId);
                npcDialogueTasksInProgress.Remove(flowKey);
                Debug.Log($"[TaskEventRuntime] Local NPC ambient dialogue finished. task={currentMain.taskCode}, npc={config.npcId}");
            });
            return;
        }

        if (currentMain.status != "IN_PROGRESS" || reportedProximityTasks.Contains(currentMain.taskCode))
            return;

        if (UIManager.Instance != null && UIManager.Instance.IsTaskPromptBlocked)
            return;

        npcDialogueTasksInProgress.Add(flowKey);
        reportedProximityTasks.Add(currentMain.taskCode);
        TaskPromptSystem.RemovePendingPromptsForTask(currentMain.taskCode);
        Debug.Log($"[TaskEventRuntime] Local NPC task dialogue begin. task={currentMain.taskCode}, triggerTargetId={config.triggerTargetId}, reportEvent={config.eventType}, reportTargetId={config.reportTargetId}");
        TaskPromptSystem.ShowPrompt(TaskNpcDialogueSystem.BuildPrompt(config), () =>
        {
            npcDialogueTasksInProgress.Remove(flowKey);
            TitleEventReporter.ReportNpcDialogue(config.npcId);

            if (instance == null)
                return;

            Vector3 currentPos = GetPlayerPosition();
            long reportTargetId = config.reportTargetId > 0 ? config.reportTargetId : config.triggerTargetId;
            Vector3 targetPos = TryGetAnchorPosition(reportTargetId);
            string targetKey = TryGetAnchorKey(reportTargetId);
            IntroductionCardConfig introCardConfig = null;
            if (config.eventType == "ARRIVE_BUILDING")
                TaskIntroductionCardSystem.TryGetConfig(reportTargetId, out introCardConfig);

            // For NPC-triggered ARRIVE_BUILDING tasks (e.g. M_3_1), the NPC may
            // stand slightly away from the building anchor. Align currentPos to
            // the target anchor so backend arrival validation can progress.
            if (config.eventType == "ARRIVE_BUILDING")
                currentPos = targetPos;

            isSending = true;
            Debug.Log($"[TaskEventRuntime] Local NPC task dialogue sending event. task={currentMain.taskCode}, event={config.eventType}, reportTargetId={reportTargetId}, npc={config.npcId}");
            instance.StartCoroutine(instance.SendEventAndRefresh(new TaskEventBody
            {
                eventType = config.eventType,
                targetType = config.reportTargetType,
                targetId = reportTargetId,
                increment = 1,
                currentPosX = currentPos.x,
                currentPosY = currentPos.y,
                currentPosZ = currentPos.z,
                targetPosX = targetPos.x,
                targetPosY = targetPos.y,
                targetPosZ = targetPos.z,
                targetAnchorKey = targetKey
            }, null, null, currentMain.taskCode, introCardConfig));
        });
    }

    private void TryShowNpcTaskProgressPrompt()
    {
        BackendTaskEntry promptTask = ResolveProgressPromptTask();
        if (promptTask == null || promptTask.status != "IN_PROGRESS")
            return;
        if (!TaskProgressPromptSystem.TryGetConfig(promptTask.taskCode, out TaskProgressPromptConfig config) || config == null)
            return;
        if (npcTaskProgressShown.Contains(promptTask.taskCode))
            return;
        if (TaskPromptSystem.IsPromptQueuedOrShowing(promptTask.taskCode, "progress"))
            return;
        if (TaskPromptSystem.IsPromptQueuedOrShowing(promptTask.taskCode, "npc_local"))
            return;

        BackendElfPrompt progressPrompt = TaskProgressPromptSystem.BuildPrompt(config);
        if (progressPrompt == null)
            return;

        ClockInLandmarkAnchor anchor = Object.FindObjectsOfType<ClockInLandmarkAnchor>(true)
            .FirstOrDefault(item => item != null && item.Matches(config.triggerLocationId));
        if (anchor == null)
            return;

        Vector3 playerPosition = GetPlayerPosition();
        float distance = GetFlatDistance(playerPosition, anchor.transform.position);
        if (distance > ClockInEventSystem.GetCheckRadius())
            return;

        npcTaskProgressShown.Add(promptTask.taskCode);
        TaskPromptSystem.ShowPrompt(progressPrompt);
    }

    private static BackendTaskEntry ResolveProgressPromptTask()
    {
        BackendTaskEntry currentMain = BackendTaskStore.GetCurrentMainTask();
        if (currentMain != null && currentMain.status == "IN_PROGRESS")
            return currentMain;

        BackendTaskEntry bikeTask = BackendTaskStore.FindTask(BikeTaskCode);
        if (bikeTask != null && bikeTask.status == "IN_PROGRESS")
            return bikeTask;

        return null;
    }

    private void TryHandleAmbientNpcDialogue()
    {
        if (UIManager.Instance != null && UIManager.Instance.IsTaskPromptBlocked)
            return;

        BackendTaskEntry currentMain = BackendTaskStore.GetCurrentMainTask();
        Vector3 playerPosition = GetPlayerPosition();

        foreach (AmbientNpcDialogueConfig config in AmbientNpcDialogueSystem.GetAllConfigs())
        {
            if (config == null || string.IsNullOrWhiteSpace(config.locationId))
                continue;

            ClockInLandmarkAnchor anchor = Object.FindObjectsOfType<ClockInLandmarkAnchor>(true)
                .FirstOrDefault(item => item != null && item.Matches(config.locationId));
            if (anchor == null)
                continue;

            // If this NPC is the active task target, let the task-specific flow own it.
            if (currentMain != null &&
                currentMain.status == "IN_PROGRESS" &&
                TaskNpcDialogueSystem.TryGetConfig(currentMain.taskCode, out TaskNpcDialogueConfig taskNpcConfig) &&
                taskNpcConfig != null &&
                taskNpcConfig.triggerTargetId == ResolveTargetIdForAmbientLocation(config.locationId))
            {
                continue;
            }

            float distance = GetFlatDistance(playerPosition, anchor.transform.position);
            if (distance > LocalNpcDialogueRadius)
                continue;

            string flowKey = $"ambient:{config.locationId}";
            if (ambientNpcDialogueShownAt.TryGetValue(flowKey, out float lastShownAt) &&
                Time.unscaledTime - lastShownAt < Mathf.Max(1f, config.cooldownSeconds))
            {
                continue;
            }

            BackendElfPrompt prompt = AmbientNpcDialogueSystem.BuildPrompt(config);
            if (prompt == null)
                continue;

            ambientNpcDialogueShownAt[flowKey] = Time.unscaledTime;
            npcDialogueTasksInProgress.Add(flowKey);
            Debug.Log($"[TaskEventRuntime] Ambient NPC dialogue begin. location={config.locationId}, npc={config.npcId}");
            TaskPromptSystem.ShowPrompt(prompt, () =>
            {
                TitleEventReporter.ReportNpcDialogue(config.npcId);
                npcDialogueTasksInProgress.Remove(flowKey);
                Debug.Log($"[TaskEventRuntime] Ambient NPC dialogue finished. location={config.locationId}, npc={config.npcId}");
            });
            return;
        }
    }

    private static long ResolveTargetIdForAmbientLocation(string locationId)
    {
        return locationId switch
        {
            "loc_welcome_volunteer" => TaskTargetIds.WelcomeVolunteer,
            "loc_reception_volunteer" => TaskTargetIds.ReceptionVolunteer,
            "loc_dorm_manager" => TaskTargetIds.DormManager,
            "loc_nurse" => TaskTargetIds.Nurse,
            "loc_botanical_gardener" => TaskTargetIds.BotanicalGardener,
            "loc_librarian" => TaskTargetIds.Librarian,
            "loc_complex_guard" => TaskTargetIds.ComplexGuard,
            _ => 0
        };
    }

    private static bool TryGetStableNpcTaskProgressPrompt(string taskCode, out BackendElfPrompt prompt)
    {
        prompt = taskCode switch
        {
            "M_1_2" => CreateStableNpcTaskProgressPrompt(
                "M_1_2",
                "请和迎新接待处的志愿者打个招呼吧！",
                "他会帮你确认信息，并告诉你宿舍安排。"),
            "M_1_3" => CreateStableNpcTaskProgressPrompt(
                "M_1_3",
                "和楼管阿姨打个招呼吧。",
                "她会帮你激活门禁，并告诉你入住注意事项。"),
            "M_1_4" => CreateStableNpcTaskProgressPrompt(
                "M_1_4",
                "去和护士确认一下体检流程吧。",
                "她会引导你完成检查，并领取体检结果。"),
            _ => null
        };

        return prompt != null;
    }

    private IEnumerator ShowNpcTaskProgressPrompt(string taskCode)
    {
        if (npcTaskProgressShown.Contains(taskCode))
            yield break;

        npcTaskProgressShown.Add(taskCode);

        BackendFacade.SetElfPromptSuppressed(true);
        BackendApiResult<BackendTaskCurrentMainEnvelope> currentMainResult = null;
        yield return BackendFacade.RefreshCurrentMainTask(result => currentMainResult = result);
        BackendFacade.SetElfPromptSuppressed(false);

        BackendElfPrompt progressPrompt = ExtractNpcProgressPrompt(taskCode, currentMainResult);
        if (progressPrompt != null)
        {
            TaskPromptSystem.ShowPrompt(progressPrompt);
            yield break;
        }

        npcTaskProgressShown.Remove(taskCode);
    }

    private static BackendElfPrompt CreateStableNpcTaskProgressPrompt(string taskCode, params string[] lines)
    {
        return new BackendElfPrompt
        {
            taskCode = taskCode,
            stage = "progress",
            npcName = "AI小精灵",
            avatarKey = "elf_default",
            autoPopup = true,
            contents = lines != null ? new List<string>(lines) : new List<string>()
        };
    }

    private static BackendElfPrompt ExtractNpcProgressPrompt(string taskCode, BackendApiResult<BackendTaskCurrentMainEnvelope> result)
    {
        if (LocalNpcTaskProgressPrompts.TryGetValue(taskCode, out BackendElfPrompt localPrompt))
            return localPrompt;

        BackendElfPrompt prompt = result?.Data?.data?.elfPrompt;
        if (prompt == null)
            prompt = TaskPromptSystem.GetPromptForTask(taskCode);

        if (prompt == null || prompt.taskCode != taskCode)
            return null;

        string stage = prompt.stage?.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(stage))
        {
            if (stage.Contains("complete") || stage.Contains("start") || stage.Contains("npc"))
                return null;
            if (stage.Contains("progress"))
                return prompt;
        }

        if (TaskPromptSystem.WasPromptShownRecently(prompt, 5f))
            return null;

        return prompt;
    }

    private static bool TryGetNpcTaskBuildingTargetId(string taskCode, out long targetId)
    {
        switch (taskCode)
        {
            case "M_1_2":
                targetId = TaskTargetIds.Reception;
                return true;
            case "M_1_3":
                targetId = TaskTargetIds.DormBamboo3;
                return true;
            case "M_1_4":
                targetId = TaskTargetIds.CampusHospital;
                return true;
            default:
                targetId = 0;
                return false;
        }
    }

    private static BackendElfPrompt CreateNpcTaskProgressPrompt(string taskCode, params string[] lines)
    {
        return new BackendElfPrompt
        {
            taskCode = taskCode,
            stage = "progress",
            npcName = "AI小精灵",
            avatarKey = "elf_default",
            autoPopup = true,
            contents = lines != null ? new List<string>(lines) : new List<string>()
        };
    }

    private void TryReportLandmarkVisit()
    {
        Vector3 playerPosition = GetPlayerPosition();
        BackendTaskEntry currentMain = BackendTaskStore.GetCurrentMainTask();

        foreach (long targetId in TaskTargetIds.GetLandmarkIds())
        {
            if (!TryResolveAnchor(targetId, out ClockInLandmarkAnchor anchor))
                continue;

            float distance = GetFlatDistance(playerPosition, anchor.transform.position);
            if (distance > ClockInEventSystem.GetCheckRadius())
            {
                landmarksInRange.Remove(targetId);
                continue;
            }

            if (landmarksInRange.Contains(targetId))
                continue;

            // If the player is currently doing an arrive_building main task for
            // this exact landmark, let the main-task flow own the interaction.
            if (currentMain != null &&
                currentMain.status == "IN_PROGRESS" &&
                currentMain.targetType == "arrive_building" &&
                currentMain.targetId == targetId)
            {
                landmarksInRange.Add(targetId);
                Debug.Log($"[TaskEventRuntime] Skip LANDMARK_VISIT because current main owns the same anchor. task={currentMain.taskCode}, targetId={targetId}, anchor={anchor.locationId}");
                continue;
            }

            landmarksInRange.Add(targetId);
            isSending = true;

            StartCoroutine(SendEventAndRefresh(new TaskEventBody
            {
                eventType = "LANDMARK_VISIT",
                targetType = "arrive_building",
                targetId = targetId,
                increment = 1,
                currentPosX = playerPosition.x,
                currentPosY = playerPosition.y,
                currentPosZ = playerPosition.z,
                targetPosX = anchor.transform.position.x,
                targetPosY = anchor.transform.position.y,
                targetPosZ = anchor.transform.position.z,
                targetAnchorKey = anchor.locationId
            }, null));

            break;
        }
    }

    private void TryReportBikeStationVisit()
    {
        if (!TryResolveAnchor(TaskTargetIds.BikeStation, out ClockInLandmarkAnchor anchor))
            return;

        Vector3 playerPosition = GetPlayerPosition();
        float distance = GetFlatDistance(playerPosition, anchor.transform.position);

        if (distance > ClockInEventSystem.GetCheckRadius())
        {
            bikeStationInRange = false;
            return;
        }

        if (bikeStationInRange)
            return;

        bikeStationInRange = true;
        isSending = true;

        StartCoroutine(SendEventAndRefresh(new TaskEventBody
        {
            eventType = "BIKE_STATION_VISIT",
            targetId = TaskTargetIds.BikeStation,
            increment = 1,
            currentPosX = playerPosition.x,
            currentPosY = playerPosition.y,
            currentPosZ = playerPosition.z,
            targetPosX = anchor.transform.position.x,
            targetPosY = anchor.transform.position.y,
            targetPosZ = anchor.transform.position.z,
            targetAnchorKey = anchor.locationId
        }, null));
    }

    private void TryReportBikeTrialDistance()
    {
        BackendTaskEntry bikeTask = BackendTaskStore.FindTask("B_2_1");
        if (bikeTask == null || bikeTask.status != "IN_PROGRESS")
        {
            ResetBikeDistanceTracking();
            return;
        }

        PlayerAgentMove agent = Object.FindObjectOfType<PlayerAgentMove>();
        if (agent == null || !agent.isBikeMode)
        {
            ResetBikeDistanceTracking();
            return;
        }

        Vector3 currentPosition = agent.transform.position;
        if (!hasBikePositionSample)
        {
            lastBikePositionSample = currentPosition;
            hasBikePositionSample = true;
            return;
        }

        float delta = GetFlatDistance(currentPosition, lastBikePositionSample);
        lastBikePositionSample = currentPosition;

        if (delta <= 0.01f)
            return;

        bikeDistanceAccumulator += delta;
        int wholeMeters = Mathf.FloorToInt(bikeDistanceAccumulator);
        if (wholeMeters <= 0)
            return;

        bikeDistanceAccumulator -= wholeMeters;
        isSending = true;

        StartCoroutine(SendEventAndRefresh(new TaskEventBody
        {
            eventType = "BIKE_TRIAL_DISTANCE",
            targetType = "bike_trial_distance",
            targetId = TaskTargetIds.BikeStation,
            increment = wholeMeters,
            currentPosX = currentPosition.x,
            currentPosY = currentPosition.y,
            currentPosZ = currentPosition.z,
            targetPosX = TryGetAnchorPosition(TaskTargetIds.BikeStation).x,
            targetPosY = TryGetAnchorPosition(TaskTargetIds.BikeStation).y,
            targetPosZ = TryGetAnchorPosition(TaskTargetIds.BikeStation).z,
            targetAnchorKey = TryGetAnchorKey(TaskTargetIds.BikeStation)
        }, null));
    }

    public static IEnumerator SimulateTaskAction(TaskViewData task, System.Action<bool, string> onComplete)
    {
        if (instance == null)
            Boot();

        return instance.RunSimulateTaskAction(task, onComplete);
    }

    public static void ReportNpcDialogue(long targetId)
    {
        if (targetId <= 0 || !BackendSettings.UseBackendMode || !SessionStore.HasToken)
            return;

        BackendTaskEntry currentMain = BackendTaskStore.GetCurrentMainTask();
        if (currentMain != null &&
            currentMain.status == "IN_PROGRESS" &&
            TaskNpcDialogueSystem.ShouldHandleLocally(currentMain.taskCode) &&
            TaskNpcDialogueSystem.TryGetConfig(currentMain.taskCode, out TaskNpcDialogueConfig localConfig) &&
            localConfig != null &&
            localConfig.eventType == "ARRIVE_BUILDING")
        {
            // Tasks such as M_3_1 are handled by local prompt flow and report
            // ARRIVE_BUILDING/NPC events from that callback. Do not upload a
            // generic NPC_DIALOGUE here, or it can pre-empt the intended flow.
            Debug.Log($"[TaskEventRuntime] Skip direct NPC report for local-handled task. task={currentMain.taskCode}, targetId={targetId}");
            return;
        }

        if (instance == null)
            Boot();

        if (instance.isSending)
            return;

        TitleEventReporter.ReportNpcDialogue(GetTitleNpcId(targetId));
        Vector3 currentPos = GetPlayerPosition();
        instance.isSending = true;
        instance.StartCoroutine(instance.SendEventAndRefresh(new TaskEventBody
        {
            eventType = "NPC_DIALOGUE",
            targetType = "npc_dialogue",
            targetId = targetId,
            increment = 1,
            currentPosX = currentPos.x,
            currentPosY = currentPos.y,
            currentPosZ = currentPos.z,
            targetPosX = TryGetAnchorPosition(targetId).x,
            targetPosY = TryGetAnchorPosition(targetId).y,
            targetPosZ = TryGetAnchorPosition(targetId).z,
            targetAnchorKey = TryGetAnchorKey(targetId)
        }, null));
    }

    public static void ReportPhotoCheckinFromCurrentLandmark()
    {
        if (!BackendSettings.UseBackendMode || !SessionStore.HasToken)
            return;

        if (instance == null)
            Boot();

        if (instance.isSending)
            return;

        BackendTaskEntry photoTask = BackendTaskStore.FindTask("B_1_1");
        if (photoTask == null || photoTask.status != "IN_PROGRESS")
            return;

        if (!TryFindInRangeLandmark(out long targetId, out Vector3 playerPosition))
            return;

        instance.isSending = true;
        instance.StartCoroutine(instance.SendEventAndRefresh(new TaskEventBody
        {
            eventType = "PHOTO_CHECKIN",
            targetId = targetId,
            increment = 1,
            currentPosX = playerPosition.x,
            currentPosY = playerPosition.y,
            currentPosZ = playerPosition.z,
            targetPosX = TryGetAnchorPosition(targetId).x,
            targetPosY = TryGetAnchorPosition(targetId).y,
            targetPosZ = TryGetAnchorPosition(targetId).z,
            targetAnchorKey = TryGetAnchorKey(targetId)
        }, null));
    }

    private IEnumerator RunSimulateTaskAction(TaskViewData task, System.Action<bool, string> onComplete)
    {
        if (task == null || !BackendSettings.UseBackendMode || !SessionStore.HasToken)
        {
            onComplete?.Invoke(false, "Backend task mode is not enabled.");
            yield break;
        }

        TaskEventBody body = null;
        string successMessage = null;

        switch (task.taskCode)
        {
            case "M_1_1":
                body = new TaskEventBody { eventType = "AI_DIALOGUE", increment = 1 };
                successMessage = "Simulated the first AI dialogue.";
                break;

            case "M_1_2":
                Vector3 receptionPos = GetPlayerPosition();
                body = new TaskEventBody
                {
                    eventType = "ARRIVE_BUILDING",
                    targetType = "arrive_building",
                    targetId = TaskTargetIds.Reception,
                    increment = 1,
                    currentPosX = receptionPos.x,
                    currentPosY = receptionPos.y,
                    currentPosZ = receptionPos.z,
                    targetPosX = TryGetAnchorPosition(TaskTargetIds.Reception).x,
                    targetPosY = TryGetAnchorPosition(TaskTargetIds.Reception).y,
                    targetPosZ = TryGetAnchorPosition(TaskTargetIds.Reception).z,
                    targetAnchorKey = TryGetAnchorKey(TaskTargetIds.Reception)
                };
                successMessage = "Simulated arriving at the reception point.";
                break;

            case "M_1_3":
                body = new TaskEventBody
                {
                    eventType = "NPC_DIALOGUE",
                    targetType = "npc_dialogue",
                    targetId = TaskTargetIds.DormManager,
                    increment = 1,
                    targetPosX = TryGetAnchorPosition(TaskTargetIds.DormManager).x,
                    targetPosY = TryGetAnchorPosition(TaskTargetIds.DormManager).y,
                    targetPosZ = TryGetAnchorPosition(TaskTargetIds.DormManager).z,
                    targetAnchorKey = TryGetAnchorKey(TaskTargetIds.DormManager)
                };
                successMessage = "Simulated the dorm manager dialogue.";
                break;

            case "M_1_4":
                body = new TaskEventBody
                {
                    eventType = "NPC_DIALOGUE",
                    targetType = "npc_dialogue",
                    targetId = TaskTargetIds.Nurse,
                    increment = 1,
                    targetPosX = TryGetAnchorPosition(TaskTargetIds.Nurse).x,
                    targetPosY = TryGetAnchorPosition(TaskTargetIds.Nurse).y,
                    targetPosZ = TryGetAnchorPosition(TaskTargetIds.Nurse).z,
                    targetAnchorKey = TryGetAnchorKey(TaskTargetIds.Nurse)
                };
                successMessage = "Simulated the nurse dialogue.";
                break;

            case "M_2_1":
                Vector3 jinLakePos = GetPlayerPosition();
                body = new TaskEventBody
                {
                    eventType = "ARRIVE_BUILDING",
                    targetType = "arrive_building",
                    targetId = TaskTargetIds.JinLake,
                    increment = 1,
                    currentPosX = jinLakePos.x,
                    currentPosY = jinLakePos.y,
                    currentPosZ = jinLakePos.z,
                    targetPosX = TryGetAnchorPosition(TaskTargetIds.JinLake).x,
                    targetPosY = TryGetAnchorPosition(TaskTargetIds.JinLake).y,
                    targetPosZ = TryGetAnchorPosition(TaskTargetIds.JinLake).z,
                    targetAnchorKey = TryGetAnchorKey(TaskTargetIds.JinLake)
                };
                successMessage = "Simulated arriving at Jin Lake.";
                break;

            case "M_2_2":
                Vector3 botanicalGardenPos = GetPlayerPosition();
                body = new TaskEventBody
                {
                    eventType = "ARRIVE_BUILDING",
                    targetType = "arrive_building",
                    targetId = TaskTargetIds.BotanicalGarden,
                    increment = 1,
                    currentPosX = botanicalGardenPos.x,
                    currentPosY = botanicalGardenPos.y,
                    currentPosZ = botanicalGardenPos.z,
                    targetPosX = TryGetAnchorPosition(TaskTargetIds.BotanicalGarden).x,
                    targetPosY = TryGetAnchorPosition(TaskTargetIds.BotanicalGarden).y,
                    targetPosZ = TryGetAnchorPosition(TaskTargetIds.BotanicalGarden).z,
                    targetAnchorKey = TryGetAnchorKey(TaskTargetIds.BotanicalGarden)
                };
                successMessage = "Simulated arriving at the botanical garden.";
                break;

            case "M_2_3":
                Vector3 libraryPos = GetPlayerPosition();
                body = new TaskEventBody
                {
                    eventType = "ARRIVE_BUILDING",
                    targetType = "arrive_building",
                    targetId = TaskTargetIds.Library,
                    increment = 1,
                    currentPosX = libraryPos.x,
                    currentPosY = libraryPos.y,
                    currentPosZ = libraryPos.z,
                    targetPosX = TryGetAnchorPosition(TaskTargetIds.Library).x,
                    targetPosY = TryGetAnchorPosition(TaskTargetIds.Library).y,
                    targetPosZ = TryGetAnchorPosition(TaskTargetIds.Library).z,
                    targetAnchorKey = TryGetAnchorKey(TaskTargetIds.Library)
                };
                successMessage = "Simulated arriving at the library.";
                break;

            case "M_3_1":
                Vector3 complexBuildingPos = GetPlayerPosition();
                body = new TaskEventBody
                {
                    eventType = "ARRIVE_BUILDING",
                    targetType = "arrive_building",
                    targetId = TaskTargetIds.ComplexBuilding,
                    increment = 1,
                    currentPosX = complexBuildingPos.x,
                    currentPosY = complexBuildingPos.y,
                    currentPosZ = complexBuildingPos.z,
                    targetPosX = TryGetAnchorPosition(TaskTargetIds.ComplexBuilding).x,
                    targetPosY = TryGetAnchorPosition(TaskTargetIds.ComplexBuilding).y,
                    targetPosZ = TryGetAnchorPosition(TaskTargetIds.ComplexBuilding).z,
                    targetAnchorKey = TryGetAnchorKey(TaskTargetIds.ComplexBuilding)
                };
                successMessage = "Simulated arriving at the complex building.";
                break;

            case "M_3_2":
                Vector3 teachingBuildingPos = GetPlayerPosition();
                body = new TaskEventBody
                {
                    eventType = "ARRIVE_BUILDING",
                    targetType = "arrive_building",
                    targetId = TaskTargetIds.TeachingBuilding1,
                    increment = 1,
                    currentPosX = teachingBuildingPos.x,
                    currentPosY = teachingBuildingPos.y,
                    currentPosZ = teachingBuildingPos.z,
                    targetPosX = TryGetAnchorPosition(TaskTargetIds.TeachingBuilding1).x,
                    targetPosY = TryGetAnchorPosition(TaskTargetIds.TeachingBuilding1).y,
                    targetPosZ = TryGetAnchorPosition(TaskTargetIds.TeachingBuilding1).z,
                    targetAnchorKey = TryGetAnchorKey(TaskTargetIds.TeachingBuilding1)
                };
                successMessage = "Simulated arriving at the first teaching building.";
                break;

            case "M_3_3":
                Vector3 experimentBuildingPos = GetPlayerPosition();
                body = new TaskEventBody
                {
                    eventType = "ARRIVE_BUILDING",
                    targetType = "arrive_building",
                    targetId = TaskTargetIds.ExperimentBuilding1,
                    increment = 1,
                    currentPosX = experimentBuildingPos.x,
                    currentPosY = experimentBuildingPos.y,
                    currentPosZ = experimentBuildingPos.z,
                    targetPosX = TryGetAnchorPosition(TaskTargetIds.ExperimentBuilding1).x,
                    targetPosY = TryGetAnchorPosition(TaskTargetIds.ExperimentBuilding1).y,
                    targetPosZ = TryGetAnchorPosition(TaskTargetIds.ExperimentBuilding1).z,
                    targetAnchorKey = TryGetAnchorKey(TaskTargetIds.ExperimentBuilding1)
                };
                successMessage = "Simulated arriving at the first experiment building.";
                break;

            case "M_3_4":
                Vector3 artBuildingPos = GetPlayerPosition();
                body = new TaskEventBody
                {
                    eventType = "ARRIVE_BUILDING",
                    targetType = "arrive_building",
                    targetId = TaskTargetIds.ArtBuilding,
                    increment = 1,
                    currentPosX = artBuildingPos.x,
                    currentPosY = artBuildingPos.y,
                    currentPosZ = artBuildingPos.z,
                    targetPosX = TryGetAnchorPosition(TaskTargetIds.ArtBuilding).x,
                    targetPosY = TryGetAnchorPosition(TaskTargetIds.ArtBuilding).y,
                    targetPosZ = TryGetAnchorPosition(TaskTargetIds.ArtBuilding).z,
                    targetAnchorKey = TryGetAnchorKey(TaskTargetIds.ArtBuilding)
                };
                successMessage = "Simulated arriving at the art building.";
                break;

            case "B_1_1":
                if (!TryFindInRangeLandmark(out long landmarkId, out Vector3 playerPosition))
                {
                    onComplete?.Invoke(false, "You are not inside any landmark range, so the photo check-in cannot be simulated.");
                    yield break;
                }

                body = new TaskEventBody
                {
                    eventType = "PHOTO_CHECKIN",
                    targetId = landmarkId,
                    increment = 1,
                    currentPosX = playerPosition.x,
                    currentPosY = playerPosition.y,
                    currentPosZ = playerPosition.z,
                    targetPosX = TryGetAnchorPosition(landmarkId).x,
                    targetPosY = TryGetAnchorPosition(landmarkId).y,
                    targetPosZ = TryGetAnchorPosition(landmarkId).z,
                    targetAnchorKey = TryGetAnchorKey(landmarkId)
                };
                successMessage = "Simulated a landmark photo check-in.";
                break;

            case "B_2_1":
                Vector3 bikePos = GetPlayerPosition();
                body = new TaskEventBody
                {
                    eventType = "BIKE_TRIAL_DISTANCE",
                    targetId = TaskTargetIds.BikeStation,
                    increment = 50,
                    currentPosX = bikePos.x,
                    currentPosY = bikePos.y,
                    currentPosZ = bikePos.z,
                    targetPosX = TryGetAnchorPosition(TaskTargetIds.BikeStation).x,
                    targetPosY = TryGetAnchorPosition(TaskTargetIds.BikeStation).y,
                    targetPosZ = TryGetAnchorPosition(TaskTargetIds.BikeStation).z,
                    targetAnchorKey = TryGetAnchorKey(TaskTargetIds.BikeStation)
                };
                successMessage = "Simulated a 50-meter bike trial.";
                break;
        }

        if (body == null)
        {
            onComplete?.Invoke(false, "This task does not require a simulated action.");
            yield break;
        }

        isSending = true;
        yield return SendEventAndRefresh(body, onComplete, successMessage);
    }

    private IEnumerator SendEventAndRefresh(TaskEventBody body, System.Action<bool, string> onComplete, string successMessage = null, string completedPromptTaskCode = null, IntroductionCardConfig completedIntroductionCardConfig = null)
    {
        if (body == null)
            yield break;

        HashSet<int> unlockedTitlesBefore = TaskChapterCompletionPromptSystem.CaptureUnlockedTitleIds();
        string photoStatusBeforeRefresh = BackendTaskStore.FindTask(PhotoTaskCode)?.status;
        string bikeStatusBeforeRefresh = BackendTaskStore.FindTask(BikeTaskCode)?.status;

        BackendApiResult<BackendTaskEventEnvelope> eventResult = null;
        Debug.Log($"[TaskEventRuntime] SendEventAndRefresh begin. EventType={body.eventType}, TargetType={body.targetType}, TargetId={body.targetId}, Increment={body.increment}, Pos=({body.currentPosX:F2},{body.currentPosY:F2},{body.currentPosZ:F2})");
        yield return BackendFacade.SendTaskEvent(body, result => eventResult = result);

        Debug.Log(
            $"[TaskEventRuntime] SendEventAndRefresh response. HttpSuccess={eventResult?.Success}, Status={eventResult?.StatusCode}, " +
            $"Message={eventResult?.Message}, ApiSuccess={eventResult?.Data?.success}, " +
            $"CurrentMain={eventResult?.Data?.data?.currentMainTask?.taskCode}, " +
            $"ProgressedCount={eventResult?.Data?.data?.progressedTasks?.Count ?? 0}");

        if (eventResult == null || !eventResult.Success || eventResult.Data == null || !eventResult.Data.success)
        {
            isSending = false;
            onComplete?.Invoke(false, eventResult != null ? eventResult.Message : "Failed to upload the task event.");
            yield break;
        }

        List<string> completedTaskCodes = eventResult.Data.data.progressedTasks?
            .Where(item => item != null && item.completed && !string.IsNullOrEmpty(item.taskCode))
            .Select(item => item.taskCode)
            .ToList();

        List<string> progressedButIncompleteTaskCodes = eventResult.Data.data.progressedTasks?
            .Where(item => item != null
                           && !item.completed
                           && !string.IsNullOrEmpty(item.taskCode)
                           && item.progressAfter > item.progressBefore)
            .Select(item => item.taskCode)
            .ToList();

        yield return BackendFacade.RefreshTasks(null);
        yield return BackendFacade.RefreshCurrentMainTask(null);
        yield return BackendFacade.RefreshTaskChapters(null);
        yield return TaskChapterCompletionPromptSystem.RefreshGrowthAndQueueChapterPrompts(unlockedTitlesBefore);
        TryQueuePhotoTaskAvailablePrompt(photoStatusBeforeRefresh);
        TryQueueBikeTaskAvailablePrompt(bikeStatusBeforeRefresh);

        TaskRewardPromptSystem.RefreshVisibleTaskUIs();

        Debug.Log(
            $"[TaskEventRuntime] SendEventAndRefresh end. EventType={body.eventType}, " +
            $"CompletedTasks={string.Join(",", completedTaskCodes ?? new List<string>())}, " +
            $"CurrentMainNow={BackendTaskStore.GetCurrentMainTask()?.taskCode}");

        isSending = false;
        onComplete?.Invoke(true, string.IsNullOrEmpty(successMessage) ? "Task progress updated." : successMessage);

        System.Action flushFollowUps = () =>
        {
            TaskPromptSystem.RemovePendingPromptsExceptTasks(eventResult.Data.data.triggeredTasks);
            TaskPromptSystem.EnqueueAutoPromptsForTasks(eventResult.Data.data.triggeredTasks);
            TaskRewardPromptSystem.ShowClaimPromptForCompletedTasks(completedTaskCodes);
        };

        System.Action showCompletedPromptOrFollowUps = () =>
        {
            string completedPromptTargetTaskCode = !string.IsNullOrEmpty(completedPromptTaskCode)
                ? completedPromptTaskCode
                : eventResult.Data?.data?.completedElfPrompt?.taskCode;

            bool shouldShowCompletedPrompt =
                !string.IsNullOrEmpty(completedPromptTargetTaskCode) &&
                completedTaskCodes != null &&
                completedTaskCodes.Contains(completedPromptTargetTaskCode) &&
                eventResult.Data?.data?.completedElfPrompt != null &&
                eventResult.Data.data.completedElfPrompt.taskCode == completedPromptTargetTaskCode;

            if (shouldShowCompletedPrompt)
            {
                TaskPromptSystem.ShowPrompt(eventResult.Data.data.completedElfPrompt, flushFollowUps);
                return;
            }

            flushFollowUps();
        };

        BackendTaskProgressResult photoProgressResult = body.eventType == "PHOTO_CHECKIN"
            ? eventResult.Data.data.progressedTasks?
                .FirstOrDefault(item => item != null &&
                                        item.taskCode == PhotoTaskCode &&
                                        item.progressAfter > item.progressBefore)
            : null;

        if (photoProgressResult != null)
        {
            BackendElfPrompt localPhotoProgressPrompt = BuildPhotoProgressPrompt(body.targetId, photoProgressResult.progressAfter);
            if (localPhotoProgressPrompt != null)
            {
                if (photoProgressResult.completed)
                    TaskPromptSystem.ShowPrompt(localPhotoProgressPrompt, showCompletedPromptOrFollowUps);
                else
                    TaskPromptSystem.ShowPrompt(localPhotoProgressPrompt, flushFollowUps);
                yield break;
            }
        }

        bool shouldShowPhotoProgressPrompt =
            body.eventType == "PHOTO_CHECKIN" &&
            progressedButIncompleteTaskCodes != null &&
            progressedButIncompleteTaskCodes.Count > 0;

        if (shouldShowPhotoProgressPrompt)
        {
            string progressedTaskCode = progressedButIncompleteTaskCodes[0];
            BackendElfPrompt progressedPrompt = TaskPromptSystem.GetPromptForTask(progressedTaskCode);
            if (progressedPrompt != null)
            {
                TaskPromptSystem.ShowPrompt(progressedPrompt, flushFollowUps);
                yield break;
            }
        }

        bool shouldShowIntroductionCardBeforeComplete =
            completedIntroductionCardConfig != null &&
            !string.IsNullOrEmpty(completedPromptTaskCode) &&
            completedTaskCodes != null &&
            completedTaskCodes.Contains(completedPromptTaskCode) &&
            UIManager.Instance != null;

        if (shouldShowIntroductionCardBeforeComplete)
        {
            UIManager.Instance.ShowIntroductionCard(completedIntroductionCardConfig, showCompletedPromptOrFollowUps);
            yield break;
        }

        showCompletedPromptOrFollowUps();
    }

    private static BackendElfPrompt BuildPhotoProgressPrompt(long targetId, int progressAfter)
    {
        int targetCount = GetPhotoTaskTargetCount();
        int remaining = Mathf.Max(0, targetCount - Mathf.Max(0, progressAfter));
        string landmarkName = ResolveLandmarkDisplayName(targetId);

        List<string> contents = new List<string>
        {
            $"\u5df2\u6253\u5361\u5730\u6807\uff1a{landmarkName}\u3002"
        };

        if (remaining > 0)
        {
            contents.Add($"\u518d\u6253\u5361 {remaining} \u4e2a\u4e0d\u540c\u5730\u6807\uff0c\u5c31\u80fd\u5b8c\u6210\u6821\u56ed\u7eaa\u5ff5\u62cd\u6444\u4efb\u52a1\u3002");
            contents.Add("\u8bb0\u5f97\u9009\u4e0d\u540c\u5730\u70b9\u62cd\u7167\uff0c\u8ba9\u4f60\u7684\u6821\u56ed\u76f8\u518c\u66f4\u4e30\u5bcc\u3002");
        }
        else
        {
            contents.Add("\u592a\u68d2\u4e86\uff0c3\u4e2a\u4e0d\u540c\u5730\u6807\u5df2\u5168\u90e8\u6253\u5361\u5b8c\u6210\uff01");
            contents.Add("\u63a5\u4e0b\u6765\u4f1a\u7ed3\u7b97\u4efb\u52a1\u5b8c\u6210\u5956\u52b1\u3002");
        }

        return new BackendElfPrompt
        {
            taskCode = PhotoTaskCode,
            stage = "photo_progress_local",
            npcName = "AI\u5c0f\u7cbe\u7075",
            avatarKey = "elf_default",
            autoPopup = true,
            contents = contents
        };
    }

    private static int GetPhotoTaskTargetCount()
    {
        BackendTaskEntry photoTask = BackendTaskStore.FindTask(PhotoTaskCode);
        if (photoTask != null && photoTask.progressTarget > 0)
            return photoTask.progressTarget;

        return DefaultPhotoTaskTargetCount;
    }

    private static string ResolveLandmarkDisplayName(long targetId)
    {
        if (TryResolveAnchor(targetId, out ClockInLandmarkAnchor anchor))
        {
            if (!string.IsNullOrWhiteSpace(anchor.displayName))
                return anchor.displayName;
            if (!string.IsNullOrWhiteSpace(anchor.locationId))
                return anchor.locationId;
        }

        return $"\u5730\u6807{targetId}";
    }

    private static void TryQueuePhotoTaskAvailablePrompt(string photoStatusBeforeRefresh)
    {
        BackendTaskEntry photoTask = BackendTaskStore.FindTask(PhotoTaskCode);
        if (photoTask == null || photoTask.status != "AVAILABLE")
            return;

        if (photoStatusBeforeRefresh == "AVAILABLE")
            return;

        BackendElfPrompt prompt = new BackendElfPrompt
        {
            taskCode = PhotoTaskCode,
            stage = "photo_available_local",
            npcName = "AI\u5c0f\u7cbe\u7075",
            avatarKey = "elf_default",
            autoPopup = true,
            contents = new List<string>
            {
                "\u4f60\u521a\u624d\u8def\u8fc7\u4e86\u4e00\u4e2a\u6821\u56ed\u6807\u5fd7\u6027\u5730\u6807\uff0c\u6821\u56ed\u6253\u5361\u7eaa\u5ff5\u4efb\u52a1\u5df2\u5f00\u542f\u3002",
                "\u8bb0\u5f97\u53bb\u4efb\u52a1\u9762\u677f\u70b9\u51fb\u5f00\u59cb\u8be5\u4efb\u52a1\uff0c\u7136\u540e\u7528\u76f8\u673a\u62cd\u64443\u4e2a\u4e0d\u540c\u5730\u6807\u3002",
                "\u5b8c\u6210\u540e\u4f60\u5c31\u80fd\u6536\u85cf\u4e00\u5957\u5c5e\u4e8e\u81ea\u5df1\u7684\u6821\u56ed\u7eaa\u5ff5\u7167\u3002"
            }
        };

        TaskPromptSystem.ShowPrompt(prompt);
    }

    private static void TryQueueBikeTaskAvailablePrompt(string bikeStatusBeforeRefresh)
    {
        BackendTaskEntry bikeTask = BackendTaskStore.FindTask(BikeTaskCode);
        if (bikeTask == null || bikeTask.status != "AVAILABLE")
            return;

        if (bikeStatusBeforeRefresh == "AVAILABLE")
            return;

        BackendElfPrompt prompt = new BackendElfPrompt
        {
            taskCode = BikeTaskCode,
            stage = "bike_available_local",
            npcName = "AI\u5c0f\u7cbe\u7075",
            avatarKey = "elf_default",
            autoPopup = true,
            contents = new List<string>
            {
                "\u4f60\u597d\u50cf\u5c31\u5728\u6821\u56ed\u5355\u8f66\u505c\u653e\u70b9\u9644\u8fd1\uff0c\u56db\u5904\u627e\u627e\u5427\u3002",
                "\u627e\u5230\u4e4b\u540e\u5c31\u53ef\u4ee5\u4f7f\u7528\u5355\u8f66\u6309\u94ae\uff0c\u6821\u56ed\u5355\u8f66\u4efb\u52a1\u5df2\u5f00\u542f\u3002",
                "\u8bf7\u53bb\u4efb\u52a1\u9762\u677f\u70b9\u51fb\u5b8c\u6210\u8be5\u4efb\u52a1\u5427\u3002"
            }
        };

        TaskPromptSystem.ShowPrompt(prompt);
    }

    private static bool TryFindInRangeLandmark(out long targetId, out Vector3 playerPosition)
    {
        playerPosition = GetPlayerPosition();

        foreach (long landmarkId in TaskTargetIds.GetLandmarkIds())
        {
            if (!TryResolveAnchor(landmarkId, out ClockInLandmarkAnchor anchor))
                continue;

            float distance = GetFlatDistance(playerPosition, anchor.transform.position);
            if (distance <= ClockInEventSystem.GetCheckRadius())
            {
                targetId = landmarkId;
                return true;
            }
        }

        targetId = 0;
        return false;
    }

    private static bool TryResolveAnchor(long targetId, out ClockInLandmarkAnchor anchor)
    {
        anchor = null;

        if (!TaskTargetIds.TryGetAnchorLocationId(targetId, out string locationId))
            return false;

        anchor = Object.FindObjectsOfType<ClockInLandmarkAnchor>(true)
            .FirstOrDefault(item => item != null && item.Matches(locationId));

        return anchor != null;
    }

    private static Vector3 TryGetAnchorPosition(long targetId)
    {
        return TryResolveAnchor(targetId, out ClockInLandmarkAnchor anchor)
            ? anchor.transform.position
            : Vector3.zero;
    }

    private static string TryGetAnchorKey(long targetId)
    {
        return TryResolveAnchor(targetId, out ClockInLandmarkAnchor anchor)
            ? anchor.locationId
            : null;
    }

    private static Vector3 GetPlayerPosition()
    {
        PlayerAgentMove agent = Object.FindObjectOfType<PlayerAgentMove>();
        if (agent != null)
            return agent.transform.position;

        PlayerMove move = Object.FindObjectOfType<PlayerMove>();
        if (move != null)
            return move.transform.position;

        return Vector3.zero;
    }

    private static float GetFlatDistance(Vector3 playerPosition, Vector3 targetPosition)
    {
        Vector3 flatPlayer = new Vector3(playerPosition.x, 0f, playerPosition.z);
        Vector3 flatTarget = new Vector3(targetPosition.x, 0f, targetPosition.z);
        return Vector3.Distance(flatPlayer, flatTarget);
    }

    private void ResetBikeDistanceTracking()
    {
        hasBikePositionSample = false;
        bikeDistanceAccumulator = 0f;
        lastBikePositionSample = Vector3.zero;
    }

    private static string GetTitleNpcId(long targetId)
    {
        return targetId switch
        {
            TaskTargetIds.WelcomeVolunteer => "npc_welcome_volunteer",
            TaskTargetIds.ReceptionVolunteer => "npc_reception_volunteer",
            TaskTargetIds.DormManager => "npc_dorm_manager",
            TaskTargetIds.Nurse => "npc_nurse",
            _ => $"npc_{targetId}"
        };
    }

    public static bool IsIntroductionFlowActive(string taskCode)
    {
        return instance != null &&
               !string.IsNullOrEmpty(taskCode) &&
               instance.introCardTasksInProgress.Contains(taskCode);
    }
}
