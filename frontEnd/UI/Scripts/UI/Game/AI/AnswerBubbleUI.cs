using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnswerBubbleUI : MonoBehaviour
{
    public TMP_Text answerText;
    private ScrollRect _scroll;

    public void Setup(string content, ScrollRect scroll)
    {
        _scroll = scroll;
        StopAllCoroutines();
        StartCoroutine(Typewriter(content));
    }

    // 外部更新内容（API返回后调用）
    public void UpdateContent(string newContent)
    {
        StopAllCoroutines();
        StartCoroutine(Typewriter(newContent));
    }

    IEnumerator Typewriter(string content)
    {
        answerText.text = content;
        answerText.maxVisibleCharacters = 0;

        for (int i = 0; i <= content.Length; i++)
        {
            answerText.maxVisibleCharacters = i;
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_scroll.content);
            _scroll.verticalNormalizedPosition = 0;
            yield return new WaitForSeconds(0.02f);
        }
    }
}