using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionItemUI : MonoBehaviour
{
    public TMP_Text questionText;
    public Transform optionsRoot;
    public Toggle optionTogglePrefab;
    public Transform explanationRoot;
    public TMP_Text correctAnswerText;
    public TMP_Text explanationText;

    private int questionIndex;
    private QuizQuestion data;
    private Action<int, int> onSelect;

    public void Setup(int index, QuizQuestion data, Action<int, int> onSelect)
    {
        questionIndex = index;
        this.data = data;
        this.onSelect = onSelect;

        questionText.text = $"{index + 1}.{data.questionText}";

        foreach (Transform child in optionsRoot)
            Destroy(child.gameObject);

        var group = optionsRoot.GetComponent<ToggleGroup>();
        if (group == null)
            group = optionsRoot.gameObject.AddComponent<ToggleGroup>();
        group.allowSwitchOff = true;

        for (int i = 0; i < data.options.Count; i++)
        {
            int optionIndex = i;

            var toggle = Instantiate(optionTogglePrefab, optionsRoot);
            toggle.group = group;
            toggle.isOn = false;
            toggle.GetComponentInChildren<Text>().text = data.options[i];

            toggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    onSelect(questionIndex, optionIndex);
                }
            });
        }

        explanationRoot.gameObject.SetActive(false);
    }

    public void ShowExplanation()
    {
        if (data == null || data.options == null || data.correctIndex < 0 || data.correctIndex >= data.options.Count)
            return;

        explanationRoot.gameObject.SetActive(true);
        correctAnswerText.text = $"{data.options[data.correctIndex]}";
        // 优先使用 data.explanation，如果为空则使用默认文本
        explanationText.text = string.IsNullOrEmpty(data.explanation)
            ? "正确答案如上所示"
            : data.explanation;

        foreach (Transform child in optionsRoot)
        {
            var toggle = child.GetComponent<Toggle>();
            if (toggle != null)
                toggle.interactable = false;
        }
    }
}
