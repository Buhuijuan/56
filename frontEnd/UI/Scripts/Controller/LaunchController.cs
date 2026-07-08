using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LaunchController : MonoBehaviour
{
    public float T = 1.5f;
    public float AutoLoginDecisionSeconds = 2.0f;
    public GameObject autoBg;
    public TMP_Text lastAccount;
    public Button changeButton;

    private bool cancelAutoLogin;
    private Coroutine loadingTextRoutine;

    private void Start()
    {
        ResolveAutoLoginUI();
        SetAutoBgVisible(false);
        if (changeButton != null)
        {
            changeButton.onClick.RemoveListener(OnClickChangeAccount);
            changeButton.onClick.AddListener(OnClickChangeAccount);
        }

        StartCoroutine(BootstrapFlow());
    }

    private IEnumerator BootstrapFlow()
    {
        yield return new WaitForSeconds(T);

        if (!BackendSettings.UseBackendMode || !BackendSettings.AutoLoginEnabled || !SessionStore.HasToken)
        {
            SetAutoBgVisible(false);
            GameSceneManager.Instance.SwitchScene("02_Login");
            yield break;
        }

        cancelAutoLogin = false;
        SetAutoBgVisible(true);
        StartLoadingTextAnimation();

        float waitSeconds = Mathf.Max(0f, AutoLoginDecisionSeconds);
        float elapsed = 0f;
        while (elapsed < waitSeconds)
        {
            if (cancelAutoLogin)
                yield break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (cancelAutoLogin)
            yield break;

        bool completed = false;
        bool ok = false;
        yield return BackendFacade.RestoreSession((success, _) =>
        {
            completed = true;
            ok = success;
        });

        StopLoadingTextAnimation();

        if (cancelAutoLogin)
            yield break;

        if (!completed || !ok)
        {
            SessionStore.Clear();
            GameStateStore.Clear();
            SetAutoBgVisible(false);
            GameSceneManager.Instance.SwitchScene("02_Login");
            yield break;
        }

        SetAutoBgVisible(false);
        GameSceneManager.Instance.SwitchScene("03_Menu");
    }

    private void OnDestroy()
    {
        if (changeButton != null)
            changeButton.onClick.RemoveListener(OnClickChangeAccount);
        StopLoadingTextAnimation();
    }

    private void OnClickChangeAccount()
    {
        cancelAutoLogin = true;
        StopLoadingTextAnimation();
        SetAutoBgVisible(false);
        GameSceneManager.Instance.SwitchScene("02_Login");
    }

    private void ResolveAutoLoginUI()
    {
        if (autoBg == null)
            autoBg = GameObject.Find("AutoBg");

        if (autoBg == null)
            return;

        if (lastAccount == null)
        {
            Transform t = autoBg.transform.Find("LastAccount");
            if (t != null)
                lastAccount = t.GetComponent<TMP_Text>();
        }

        if (changeButton == null)
        {
            Transform t = autoBg.transform.Find("ChangeButton");
            if (t != null)
                changeButton = t.GetComponent<Button>();
        }
    }

    private void SetAutoBgVisible(bool visible)
    {
        if (autoBg != null)
            autoBg.SetActive(visible);
    }

    private void StartLoadingTextAnimation()
    {
        if (lastAccount == null)
            return;

        StopLoadingTextAnimation();
        loadingTextRoutine = StartCoroutine(AnimateLoadingText());
    }

    private void StopLoadingTextAnimation()
    {
        if (loadingTextRoutine != null)
        {
            StopCoroutine(loadingTextRoutine);
            loadingTextRoutine = null;
        }
    }

    private IEnumerator AnimateLoadingText()
    {
        string account = SessionStore.Current != null ? SessionStore.Current.mailbox : null;
        if (string.IsNullOrWhiteSpace(account))
            account = !string.IsNullOrWhiteSpace(SessionStore.Current?.accountCode)
                ? SessionStore.Current.accountCode
                : "\u4e0a\u6b21\u8d26\u53f7";

        int dots = 0;
        while (true)
        {
            dots = (dots + 1) % 4;
            string suffix = new string('.', dots);
            lastAccount.text = $"{account}\uff0c\u767b\u5f55\u4e2d{suffix}";
            yield return new WaitForSeconds(0.35f);
        }
    }
}
