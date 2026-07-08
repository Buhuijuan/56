using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AIPromptPanelUI : MonoBehaviour
{
    public Image NPCImage;
    public TMP_Text NPCName, NPCContent;
    public Button nextButton;
    private int currentRound = 0;
    private List<string> currentContents;
    private Coroutine typingCoroutine;
    void Awake()
    {
        nextButton.onClick.AddListener(OnNextClicked);
    }

    public void Show(AIPromptConfig config)
    {
        gameObject.SetActive(true);
        NPCImage.sprite = config.npcAvatar;
        NPCName.text = config.npcName;
        currentContents = config.contents;
        currentRound = 0;
        PlayCurrentRound();
    }
    public void OnNextClicked()
    {
        currentRound++;
        if (currentRound >= currentContents.Count)
        {
            Close();
            return;
        }
        PlayCurrentRound();

    }
    public void PlayCurrentRound()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        NPCContent.maxVisibleCharacters = 0;
        NPCContent.text = currentContents[currentRound];

        typingCoroutine = StartCoroutine(TypewriterEffect(NPCContent, currentContents[currentRound]));
    }
    public IEnumerator TypewriterEffect(TMP_Text text, string content)
    {
        text.maxVisibleCharacters = 0;
        for (int i = 0; i <= content.Length; i++)
        {
            text.maxVisibleCharacters = i;
            yield return new WaitForSeconds(0.02f);
        }

    }

    private void Close()
    {
        gameObject.SetActive(false);
        TaskPromptSystem.NotifyPromptClosed();
    }
}
