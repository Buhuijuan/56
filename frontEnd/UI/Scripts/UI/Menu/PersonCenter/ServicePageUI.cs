using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServicePageUI : PageUI
{
    public GameObject questionsArea, qaArea;
    public Transform contentRoot;
    public ScrollRect scroll;
    public TMP_InputField inputField;
    public Button submitButton;
    public List<Button> qButtons = new();
    public AskBubbleUI askPrefab;
    public AnswerBubbleUI answerPrefab;

    // ======================================================================
    // 【当前客服NPC配置】在这里设置，每个NPC面板都可以不一样
    // ======================================================================
    private const string CURRENT_CATEGORY = "echo";   // 客服用echo
    // 替换成这行
    private string DEFAULT_PLAYER_ID => AccountSystem.GetCurrentAccount()?.accountID ?? "test123";
    private const string DEFAULT_SCENE = "";          // 场景可空

    void Start()
    {
        foreach (var qButton in qButtons)
            qButton.onClick.AddListener(() => 
                OnPresetQuestion(qButton.GetComponentInChildren<TMP_Text>().text));
        
        submitButton.onClick.AddListener(OnSubmit);
    }

    void OnPresetQuestion(string question)
    {
        SwitchToQAArea();
        CreateAskBubble(question);
        RequestAI(question); // 统一调用
    }

    void OnSubmit()
    {
        string question = inputField.text.Trim();
        if (string.IsNullOrEmpty(question)) return;

        SwitchToQAArea();
        CreateAskBubble(question);
        inputField.text = "";

        RequestAI(question);
    }

    void SwitchToQAArea()
    {
        questionsArea.SetActive(false);
        qaArea.SetActive(true);
    }

    void CreateAskBubble(string text)
    {
        var bubble = Instantiate(askPrefab, contentRoot);
        bubble.Setup(text);
        StartCoroutine(ScrollToBottom());
    }

    // ======================================================================
    // 核心：调用通用APIManager，传入当前NPC的category
    // ======================================================================
    void RequestAI(string question)
    {
        // 先显示加载中
        CreateAnswerBubble("思考中，请稍候...");

        // 调用通用接口，每个NPC自己传参数
        APIManager.Instance.RequestQA(
            question: question,
            playerId: DEFAULT_PLAYER_ID,  // 可后续动态获取
            category: CURRENT_CATEGORY,   // 客服=echo
            scene: DEFAULT_SCENE,         // 可空
            onSuccess: (response) =>
            {
                AnswerBubbleUI last = contentRoot.GetChild(contentRoot.childCount - 1).GetComponent<AnswerBubbleUI>();
                if (last != null) last.UpdateContent(response.data.answer);
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
}