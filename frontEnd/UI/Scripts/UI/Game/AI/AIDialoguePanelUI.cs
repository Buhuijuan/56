using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AIDialoguePanelUI : MonoBehaviour
{
    public GameObject welcomeArea, qaArea;
    public Transform contentRoot;
    public ScrollRect scroll;
    public TMP_InputField inputField;
    public Button submitButton;
    public AskBubbleUI askPrefab;
    public AnswerBubbleUI answerPrefab;

    // ======================================================================
    // 【NPC配置】固定为 default，严格按照你的要求
    // ======================================================================
    private const string CURRENT_CATEGORY = "default";
    private string DEFAULT_PLAYER_ID => AccountSystem.GetCurrentAccount()?.accountID ?? "unity_player_001";
    private const string DEFAULT_SCENE = "";

    void Start()
    {
        submitButton.onClick.AddListener(OnSubmit);
        gameObject.SetActive(false);
    }

    void OnSubmit()
    {
        string question = inputField.text.Trim();
        if (string.IsNullOrEmpty(question)) return;

        SwitchToQAArea();
        CreateAskBubble(question);
        inputField.text = "";

        // 调用通用APIManager（和ServicePageUI完全一致）
        RequestAI(question);
    }

    void SwitchToQAArea()
    {
        welcomeArea.SetActive(false);
        qaArea.SetActive(true);
    }

    void CreateAskBubble(string text)
    {
        var bubble = Instantiate(askPrefab, contentRoot);
        bubble.Setup(text);
        StartCoroutine(ScrollToBottom());
    }

    // ======================================================================
    // 核心：完全复用 ServicePageUI 逻辑 + APIManager
    // ======================================================================
    void RequestAI(string question)
    {
        // 先显示加载中
        CreateAnswerBubble("思考中，请稍候...");

        // 调用通用接口，严格使用 default 角色
        APIManager.Instance.RequestQA(
            question: question,
            playerId: DEFAULT_PLAYER_ID,
            category: CURRENT_CATEGORY,
            scene: DEFAULT_SCENE,
            onSuccess: (response) =>
            {
                AnswerBubbleUI last = contentRoot.GetChild(contentRoot.childCount - 1).GetComponent<AnswerBubbleUI>();
                if (last != null) last.UpdateContent(response.data.answer);

                if (!string.IsNullOrWhiteSpace(response?.data?.answer))
                {
                    TitleEventReporter.ReportElfAnswer();
                    BackendRuntime.Run(SendAIDialogueEvent());
                }
            },
            onError: (err) =>
            {
                AnswerBubbleUI last = contentRoot.GetChild(contentRoot.childCount - 1).GetComponent<AnswerBubbleUI>();
                if (last != null) last.UpdateContent("网络异常，请稍后重试~");
                Debug.LogError(err);
            }
        );
    }

    void CreateAnswerBubble(string text)
    {
        var bubble = Instantiate(answerPrefab, contentRoot);
        bubble.Setup(text, scroll);
    }

    IEnumerator ScrollToBottom()
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)contentRoot);
        scroll.verticalNormalizedPosition = 0f;
    }

    private IEnumerator SendAIDialogueEvent()
    {
        if (!BackendSettings.UseBackendMode || !SessionStore.HasToken)
            yield break;

        BackendTaskEntry currentMain = BackendTaskStore.GetCurrentMainTask();
        if (currentMain == null || currentMain.targetType != "ai_dialogue")
            yield break;

        HashSet<int> unlockedTitlesBefore = TaskChapterCompletionPromptSystem.CaptureUnlockedTitleIds();

        TaskEventBody body = new TaskEventBody
        {
            eventType = "AI_DIALOGUE",
            targetType = "ai_dialogue",
            targetId = currentMain.targetId,
            increment = 1
        };

        BackendApiResult<BackendTaskEventEnvelope> eventResult = null;
        yield return BackendFacade.SendTaskEvent(body, result => eventResult = result);

        if (eventResult == null || !eventResult.Success || eventResult.Data == null || !eventResult.Data.success || eventResult.Data.data == null)
            yield break;

        List<string> completedTaskCodes = eventResult.Data.data.progressedTasks?
            .Where(item => item != null && item.completed && !string.IsNullOrEmpty(item.taskCode))
            .Select(item => item.taskCode)
            .ToList();

        yield return BackendFacade.RefreshTasks(null);
        yield return BackendFacade.RefreshCurrentMainTask(null);
        yield return BackendFacade.RefreshTaskChapters(null);
        yield return TaskChapterCompletionPromptSystem.RefreshGrowthAndQueueChapterPrompts(unlockedTitlesBefore);

        TaskRewardPromptSystem.RefreshVisibleTaskUIs();

        bool shouldShowCompletedPrompt =
            completedTaskCodes != null &&
            completedTaskCodes.Contains(currentMain.taskCode) &&
            eventResult.Data?.data?.completedElfPrompt != null &&
            eventResult.Data.data.completedElfPrompt.taskCode == currentMain.taskCode;

        System.Action flushFollowUps = () =>
        {
            TaskPromptSystem.EnqueueAutoPromptsForTasks(eventResult.Data.data.triggeredTasks);
            TaskRewardPromptSystem.ShowClaimPromptForCompletedTasks(completedTaskCodes);
        };

        if (shouldShowCompletedPrompt)
            TaskPromptSystem.ShowPrompt(eventResult.Data.data.completedElfPrompt, flushFollowUps);
        else
            flushFollowUps();

        Debug.Log("AI dialogue task event uploaded and task state refreshed.");
    }
}
