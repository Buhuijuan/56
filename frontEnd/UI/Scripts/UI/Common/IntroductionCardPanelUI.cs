using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroductionCardPanelUI : MonoBehaviour
{
    public TMP_Text Name, Introduction;
    public Button returnButton;
    private System.Action onClose;

    public void Show(IntroductionCardConfig config, System.Action closeCallback = null)
    {
        gameObject.SetActive(true);
        Name.text = config.name;
        Introduction.text = config.introduction;
        onClose = closeCallback;
        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(Close);
    }
    public void Close()
    {
        gameObject.SetActive(false);
        System.Action callback = onClose;
        onClose = null;
        callback?.Invoke();
    }

}
