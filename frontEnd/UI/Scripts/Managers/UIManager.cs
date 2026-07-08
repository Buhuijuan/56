using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public LoadingController LoadingPanel;
    public ConfirmPanelUI ConfirmPanel;
    public PWDModifyPanelUI pwdModifyPanel;
    public MailBoxChangePanelUI mailBoxChangePanel;
    public RemindPanelUI remindPanel;
    public IntroductionCardPanelUI introductionCardPanel;
    public AIPromptPanelUI aIPromptPanel;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(transform.root.gameObject);
    }
    public void ShowLoading(string uiScene, string worldScene)
    {
        LoadingPanel.gameObject.SetActive(true);
        StartCoroutine(LoadingPanel.LoadScenes(uiScene, worldScene));
    }
    public void HideLoading()
    {
        LoadingPanel.gameObject.SetActive(false);
        TaskPromptSystem.NotifyUiAvailabilityChanged();
    }
    public void ShowConfirm(string title, string message, Action confirm, Action cancel = null)
    {
        ClearPendingCurrentMainTaskPrompt();
        ConfirmPanel.Show(title, message, confirm, cancel);
    }
    public void ShowPWDModify()
    {
        pwdModifyPanel.Show();
    }
    public void ShowMailBoxChange()
    {
        mailBoxChangePanel.Show();
    }
    public void ShowRemind(string title, string buttonText, string message = null, List<RewardItem> rewards = null, Action confirm = null, bool clearPendingTaskPrompt = true)
    {
        if (clearPendingTaskPrompt)
            ClearPendingCurrentMainTaskPrompt();
        remindPanel.Show(title, buttonText, message, rewards, confirm);
    }
    public void ShowIntroductionCard(IntroductionCardConfig config, Action closeCallback = null)
    {
        introductionCardPanel.Show(config, closeCallback);
    }
    public void ShowAIPrompt(AIPromptConfig config)
    {
        aIPromptPanel.Show(config);
    }

    public bool IsTaskPromptBlocked
    {
        get
        {
            bool aiPromptVisible = aIPromptPanel != null && aIPromptPanel.gameObject.activeInHierarchy;
            bool remindVisible = remindPanel != null && remindPanel.gameObject.activeInHierarchy;
            bool confirmVisible = ConfirmPanel != null && ConfirmPanel.gameObject.activeInHierarchy;
            bool introVisible = introductionCardPanel != null && introductionCardPanel.gameObject.activeInHierarchy;
            bool loadingVisible = false;

            if (LoadingPanel != null && LoadingPanel.gameObject.activeInHierarchy)
                loadingVisible = true;

            if (!loadingVisible)
            {
                LoadingController[] loadingControllers = FindObjectsOfType<LoadingController>(true);
                foreach (LoadingController controller in loadingControllers)
                {
                    if (controller != null && controller.gameObject.activeInHierarchy)
                    {
                        loadingVisible = true;
                        break;
                    }
                }
            }

            return aiPromptVisible || remindVisible || confirmVisible || introVisible || loadingVisible;
        }
    }

    private void ClearPendingCurrentMainTaskPrompt()
    {
        BackendTaskEntry currentMainTask = BackendTaskStore.GetCurrentMainTask();
        if (currentMainTask != null && !string.IsNullOrEmpty(currentMainTask.taskCode))
            TaskPromptSystem.RemovePendingPromptsForTask(currentMainTask.taskCode);
    }
}
