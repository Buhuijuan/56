using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryCardItemUI : MonoBehaviour
{
    public TMP_Text titleText, authorText, dateText;
    public Button button;
    public void Setup(StoryRecord record, Action viewStory)
    {
        titleText.text = record.theme;
        authorText.text = $"作者：{record.authorId}";
        DateTime time = record.createdAt;
        dateText.text = $"时间：{time.Year}年{time.Month}月{time.Day}日";
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(viewStory.Invoke);
    }
}
