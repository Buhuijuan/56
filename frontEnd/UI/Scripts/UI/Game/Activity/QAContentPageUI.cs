using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QAContentPage : PageUI
{
    public PagesSwitcher pageSwitcher;
    public Transform pageRoot;
    public QuestionItemUI questionItemPrefab;
    public TMP_Text progressText;
    public Button submitButton;
    public ActivityPanelUI parent;

    private QuizSessionState session;

    private void OnEnable()
    {
        if (QuizEventSystem.Session == null && (!BackendSettings.UseBackendMode || !SessionStore.HasToken))
            QuizEventSystem.StartNewSession();

        session = QuizEventSystem.Session;
        if (session == null || session.questions == null || session.questions.Count == 0)
        {
            parent.SwitchPage(ActivityPanelUI.ActivityPageState.QADetail);
            return;
        }

        GeneratePages();
        UpdateUI();
    }

    private void GeneratePages()
    {
        pageSwitcher.ClearPages();
        foreach (Transform child in pageRoot)
            Destroy(child.gameObject);

        for (int i = 0; i < session.questions.Count; i++)
        {
            QuestionItemUI page = Instantiate(questionItemPrefab, pageRoot);
            page.Setup(i, session.questions[i], OnSelectOption);
        }

        pageSwitcher.SetPages(GetPageList());
    }

    private List<GameObject> GetPageList()
    {
        List<GameObject> pages = new();
        foreach (Transform child in pageRoot)
            pages.Add(child.gameObject);
        return pages;
    }

    private void UpdateUI()
    {
        int index = pageSwitcher.CurrentIndex;
        progressText.text = $"{index + 1}/{session.questions.Count} | \u5df2\u7b54 {QuizEventSystem.GetAnsweredCount()}";

        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(OnSubmit);
    }

    private void OnSelectOption(int questionIndex, int optionIndex)
    {
        session.userAnswers[questionIndex] = optionIndex;

        QuestionItemUI page = pageRoot.GetChild(questionIndex).GetComponent<QuestionItemUI>();
        if (page != null)
            page.ShowExplanation();

        UpdateUI();
    }

    public void OnSubmit()
    {
        if (QuizEventSystem.HasUnansweredQuestions())
        {
            UIManager.Instance.ShowConfirm(
                "\u8fd8\u6709\u672a\u4f5c\u7b54\u9898\u76ee",
                "\u4f60\u8fd8\u6709\u9898\u76ee\u672a\u4f5c\u7b54\u3002\u82e5\u7ee7\u7eed\u63d0\u4ea4\uff0c\u672a\u4f5c\u7b54\u9898\u5c06\u6309\u9519\u8bef\u5904\u7406\u3002",
                SubmitConfirmed);
            return;
        }

        SubmitConfirmed();
    }

    private void SubmitConfirmed()
    {
        int totalQuestions = session != null && session.questions != null ? session.questions.Count : 0;
        int answeredCount = session != null && session.userAnswers != null ? session.userAnswers.Count(answer => answer >= 0) : 0;

        if (BackendSettings.UseBackendMode && SessionStore.HasToken)
        {
            StartCoroutine(SubmitRemoteSession(totalQuestions, answeredCount));
            return;
        }

        int sessionScore = QuizEventSystem.SubmitSessionLocal();
        int correctCount = Mathf.Max(0, sessionScore / 10);

        UIManager.Instance.ShowRemind(
            "\u7b54\u9898\u7ed3\u675f",
            "\u77e5\u9053\u4e86",
            $"\u672c\u6b21\u5171 {totalQuestions} \u9898\uff0c\u5df2\u4f5c\u7b54 {answeredCount} \u9898\uff0c\u7b54\u5bf9 {correctCount} \u9898\u3002\n\u65b0\u589e\u79ef\u5206\uff1a{sessionScore}\n\u672c\u5468\u603b\u79ef\u5206\uff1a{QuizEventSystem.quizEventState.weeklyScore}",
            null,
            () =>
            {
                QuizEventSystem.AbandonSession();
                parent.SwitchPage(ActivityPanelUI.ActivityPageState.QADetail);
            });
    }

    private IEnumerator SubmitRemoteSession(int totalQuestions, int answeredCount)
    {
        QuizSubmitBody body = new()
        {
            answers = new List<int>(session.userAnswers)
        };

        BackendApiResult<BackendQuizSubmitEnvelope> result = null;
        yield return BackendFacade.SubmitQuiz(body, value => result = value);

        if (result == null || !result.Success || result.Data == null || !result.Data.success || result.Data.data == null)
        {
            UIManager.Instance.ShowRemind(
                "\u63d0\u4ea4\u5931\u8d25",
                "\u77e5\u9053\u4e86",
                result != null && !string.IsNullOrEmpty(result.Message) ? result.Message : "\u65e0\u6cd5\u63d0\u4ea4\u672c\u6b21\u95ee\u7b54\u3002");
            yield break;
        }

        BackendQuizSubmitData submitData = result.Data.data;
        QuizEventSystem.ApplyRemoteSubmit(
            submitData.score,
            submitData.weeklyScore,
            submitData.addedCoin,
            submitData.results);

        yield return BackendFacade.RefreshHome(null);
        yield return BackendFacade.RefreshGrowth(null);

        int sessionScore = submitData.score;
        int totalFromResults = submitData.results?.Count ?? totalQuestions;
        int correctCount = submitData.results?.Count(r => r.correct) ?? Mathf.Max(0, sessionScore / 10);

        string message = $"\u672c\u6b21\u5171 {totalFromResults} \u9898\uff0c\u5df2\u4f5c\u7b54 {answeredCount} \u9898\uff0c\u7b54\u5bf9 {correctCount} \u9898\u3002\n\u65b0\u589e\u79ef\u5206\uff1a{sessionScore}\n\u672c\u5468\u603b\u79ef\u5206\uff1a{QuizEventSystem.quizEventState.weeklyScore}";
        message += "\n\u5468\u5956\u52b1\u9700\u5728\u6d3b\u52a8\u89c4\u5219\u5141\u8bb8\u7684\u65f6\u95f4\u9886\u53d6\u3002";

        UIManager.Instance.ShowRemind(
            "\u7b54\u9898\u7ed3\u675f",
            "\u77e5\u9053\u4e86",
            message,
            null,
            () =>
            {
                QuizEventSystem.AbandonSession();
                parent.SwitchPage(ActivityPanelUI.ActivityPageState.QADetail);
            });
    }
}
