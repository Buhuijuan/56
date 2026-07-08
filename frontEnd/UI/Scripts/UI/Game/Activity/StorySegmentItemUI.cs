using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StorySegmentItemUI : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text contentText;
    public TMP_Text labelText;
    public GameObject optionsRoot;
    public Button option1;
    public Button option2;
    public Button option3;
    public Button submitButton;
    public Button claimRewardButton;
    public Button uploadStoryButton;
    public TMP_InputField customInput;
    public GameObject rewardButtonsRoot;

    public void Setup(StorySegment segment, Action<string> onChoose, bool isLastRound)
    {
        // 主题安全显示
        titleText.text = StoryAPIManager.Instance?.currentThemeInfo != null
            ? $"本期主题：{StoryAPIManager.Instance.currentThemeInfo.theme}"
            : "本期主题：加载中...";

        labelText.text = "请选择你的下一步行动";
        optionsRoot.SetActive(false);
        rewardButtonsRoot.SetActive(false);

        // 安全移除监听
        option1.onClick.RemoveAllListeners();
        option2.onClick.RemoveAllListeners();
        option3.onClick.RemoveAllListeners();
        submitButton.onClick.RemoveAllListeners();
        claimRewardButton.onClick.RemoveAllListeners();
        uploadStoryButton.onClick.RemoveAllListeners();

        // 安全获取 ScrollRect
        ScrollRect scroll = GetComponentInParent<ScrollRect>();

        StartCoroutine(TypewriterEffect(contentText, segment.segmentText, scroll, () =>
        {
            optionsRoot.SetActive(!isLastRound);
            rewardButtonsRoot.SetActive(isLastRound); // 👈 必须加这一行！

            if (scroll != null)
                StartCoroutine(RefreshAfterOptions(scroll));
        }));

        if (!isLastRound)
        {
            // 安全设置选项
            if (option1 != null) option1.GetComponentInChildren<TMP_Text>().text = segment.options[0];
            if (option2 != null) option2.GetComponentInChildren<TMP_Text>().text = segment.options[1];
            if (option3 != null) option3.GetComponentInChildren<TMP_Text>().text = segment.options[2];

            // 安全添加点击
            option1?.onClick.AddListener(() => onChoose(segment.options[0]));
            option2?.onClick.AddListener(() => onChoose(segment.options[1]));
            option3?.onClick.AddListener(() => onChoose(segment.options[2]));
            submitButton?.onClick.AddListener(() =>
            {
                if (!string.IsNullOrWhiteSpace(customInput?.text))
                    onChoose(customInput.text.Trim());
            });
            return;
        }

        // ======================================================
        // 👇👇👇 以下是你【完全丢失】的结尾逻辑！全部补回来！
        // ======================================================
        option1.gameObject.SetActive(false);
        option2.gameObject.SetActive(false);
        option3.gameObject.SetActive(false);
        customInput.gameObject.SetActive(false);
        submitButton.gameObject.SetActive(false);
        labelText.gameObject.SetActive(false);

        rewardButtonsRoot.SetActive(true);
        StoryEventSystem.SaveStory();
        claimRewardButton.interactable = StoryEventSystem.CanClaimReward();
        uploadStoryButton.interactable = StoryEventSystem.CanUploadCurrentStory();

        claimRewardButton.onClick.AddListener(() =>
        {
            if (!StoryEventSystem.CanClaimReward())
                return;

            UIManager.Instance.ShowRemind("恭喜获得", "收下啦", null, StoryEventSystem.GetCurrentConfig().rewards, () =>
            {
                if (StoryEventSystem.ClaimReward())
                    claimRewardButton.interactable = false;
            });
        });

        uploadStoryButton.onClick.AddListener(() =>
        {
            if (!StoryEventSystem.CanUploadCurrentStory())
                return;

            if (StoryEventSystem.UploadStory())
            {
                uploadStoryButton.interactable = false;
                UIManager.Instance.ShowRemind("上传成功", "知道了", "故事已加入故事集，其他同学现在也能阅读这篇作品。");
            }
        });
    }

    public void LockInteraction(string chosenText)
    {
        Button[] buttons = { option1, option2, option3 };
        foreach (Button button in buttons)
        {
            if (button == null)
                continue;

            string optionText = button.GetComponentInChildren<TMP_Text>().text;
            button.interactable = false;
            if (optionText != chosenText)
            {
                CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>() ?? button.gameObject.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 0.55f;
            }
        }

        if (customInput != null)
        {
            customInput.interactable = false;
            if (chosenText != customInput.text)
            {
                CanvasGroup canvasGroup = customInput.GetComponent<CanvasGroup>() ?? customInput.gameObject.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 0.55f;
            }
        }

        if (submitButton != null)
            submitButton.interactable = false;
    }

    public IEnumerator TypewriterEffect(TMP_Text text, string content, ScrollRect scroll, Action onComplete)
    {
        if (text == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        text.text = content;
        text.maxVisibleCharacters = 0;

        for (int i = 0; i <= content.Length; i++)
        {
            text.maxVisibleCharacters = i;
            yield return null;

            if (scroll != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)scroll.content);
                scroll.verticalNormalizedPosition = 0f;
            }
            yield return new WaitForSeconds(0.02f);
        }

        onComplete?.Invoke();
    }
    private IEnumerator RefreshAfterOptions(ScrollRect scroll)
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)scroll.content);
        yield return null;
        scroll.verticalNormalizedPosition = 0f;
    }
}
