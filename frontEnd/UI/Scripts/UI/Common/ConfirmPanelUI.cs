using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPanelUI : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text message;
    public Button confirmButton;
    public Button cancelButton;
    private Action confirmAction;
    private Action cancelAction;
    public void Show(string title, string message, Action confirm, Action cancel = null)
    {
        this.title.text = title;
        this.message.text = message;
        confirmAction = confirm;
        cancelAction = cancel;
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            confirm?.Invoke();
            Close();
        }
        );
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() =>
        {
            cancel?.Invoke();
            Close();
        }
        );
        gameObject.SetActive(true);
    }
    public void Close()
    {
        gameObject.SetActive(false);
        TaskPromptSystem.NotifyUiAvailabilityChanged();
    }
}
