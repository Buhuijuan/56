using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoginPanelUI : MonoBehaviour
{
    public GameObject LoginPage;
    public GameObject RegisterPage;
    public Toggle loginToggle;
    public Toggle registerToggle;
    public InputValidator LaccountInput;
    public InputValidator LpasswordInput;
    public InputValidator RmailboxInput;
    public InputValidator RverifyInput;
    public InputValidator RpasswordInput;
    public InputValidator RconfirmInput;
    public Button loginButton;
    public Button registerButton;
    public Button verificationButton;
    public TMP_Text verificationButtonText;

    private LoginController controller;
    private Coroutine verificationCooldownCoroutine;
    private string verificationButtonDefaultText = "获取验证码";
    public Dictionary<LoginErrorType, InputValidator> errormap = new();

    private void Start()
    {
        controller = FindObjectOfType<LoginController>();
        errormap.Add(LoginErrorType.None, null);
        errormap.Add(LoginErrorType.MailboxNotExist, LaccountInput);
        errormap.Add(LoginErrorType.PasswordIncorrect, LpasswordInput);
        errormap.Add(LoginErrorType.MailboxAlreadyRegistered, RmailboxInput);
        errormap.Add(LoginErrorType.VerificationIncorrect, RverifyInput);

        if (verificationButtonText != null && !string.IsNullOrWhiteSpace(verificationButtonText.text))
            verificationButtonDefaultText = verificationButtonText.text;

        loginToggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
                ShowLoginPanel();
        });

        registerToggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
                ShowRegisterPanel();
        });

        ShowLoginPanel();
    }

    public void OnClickLogin()
    {
        DismissKeyboard();
        ClearAllErrors();
        string account = LaccountInput.input.text.Trim();
        string password = LpasswordInput.input.text;
        if (string.IsNullOrWhiteSpace(account))
        {
            LaccountInput.SetError("请输入邮箱。");
            return;
        }

        if (!AccountSystem.IsValidEmail(account))
        {
            LaccountInput.SetError("邮箱格式不正确。");
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            LpasswordInput.SetError("请输入密码。");
            return;
        }

        controller.Login(account, password);
    }

    public void OnClickRegister()
    {
        DismissKeyboard();
        ClearAllErrors();
        string mailbox = RmailboxInput.input.text.Trim();
        string verification = RverifyInput.input.text.Trim();
        string password = RpasswordInput.input.text;
        string confirm = RconfirmInput.input.text;

        if (string.IsNullOrWhiteSpace(mailbox))
        {
            RmailboxInput.SetError("请输入邮箱。");
            return;
        }

        if (!AccountSystem.IsValidEmail(mailbox))
        {
            RmailboxInput.SetError("邮箱格式不正确。");
            return;
        }

        if (string.IsNullOrWhiteSpace(verification))
        {
            RverifyInput.SetError("请输入验证码。");
            return;
        }

        if (verification.Length < 4)
        {
            RverifyInput.SetError("验证码格式不正确。");
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            RpasswordInput.SetError("请输入密码。");
            return;
        }

        if (!AccountSystem.IsValidPwd(password))
        {
            RpasswordInput.SetError("密码必须为 8 到 20 位字母和数字组合。");
            return;
        }

        if (string.IsNullOrWhiteSpace(confirm))
        {
            RconfirmInput.SetError("请再次输入密码。");
            return;
        }

        if (password != confirm)
        {
            RconfirmInput.SetError("两次输入的密码不一致。");
            return;
        }

        controller.Register(mailbox, verification, password);
    }

    public void OnClickClickGoToRegister()
    {
        DismissKeyboard();
        registerToggle.isOn = true;
    }

    public void OnClickGetVerification()
    {
        DismissKeyboard();
        ClearRegisterErrors();
        string mailbox = RmailboxInput.input.text.Trim();
        if (string.IsNullOrWhiteSpace(mailbox))
        {
            RmailboxInput.SetError("请输入邮箱。");
            return;
        }

        if (!AccountSystem.IsValidEmail(mailbox))
        {
            RmailboxInput.SetError("邮箱格式不正确。");
            return;
        }

        controller.GetVerification(mailbox);
    }

    public void OnClickFindPassword()
    {
        DismissKeyboard();
        ClearRegisterErrors();
        string mailbox = RmailboxInput.input.text.Trim();
        if (string.IsNullOrWhiteSpace(mailbox))
        {
            RmailboxInput.SetError("请输入邮箱。");
            return;
        }

        if (!AccountSystem.IsValidEmail(mailbox))
        {
            RmailboxInput.SetError("邮箱格式不正确。");
            return;
        }

        controller.FindPassword(mailbox);
    }

    public void ShowLoginPanel()
    {
        DismissKeyboard();
        LoginPage.SetActive(true);
        RegisterPage.SetActive(false);
        loginToggle.isOn = true;
        registerToggle.isOn = false;
    }

    public void ShowRegisterPanel()
    {
        DismissKeyboard();
        LoginPage.SetActive(false);
        RegisterPage.SetActive(true);
        loginToggle.isOn = false;
        registerToggle.isOn = true;
    }

    public void ClearAllErrors()
    {
        LaccountInput.ClearError();
        LpasswordInput.ClearError();
        ClearRegisterErrors();
    }

    public void ClearRegisterErrors()
    {
        RmailboxInput.ClearError();
        RverifyInput.ClearError();
        RpasswordInput.ClearError();
        RconfirmInput.ClearError();
    }

    public void SetLoginSubmitting(bool isSubmitting)
    {
        if (loginButton != null)
            loginButton.interactable = !isSubmitting;
    }

    public void SetRegisterSubmitting(bool isSubmitting)
    {
        if (registerButton != null)
            registerButton.interactable = !isSubmitting;
    }

    public void SetVerificationSending(bool isSending)
    {
        if (verificationButton != null)
            verificationButton.interactable = !isSending;

        if (!isSending)
            SetVerificationButtonText(verificationButtonDefaultText);
        else
            SetVerificationButtonText("发送中...");
    }

    public void StartVerificationCooldown(int seconds)
    {
        if (verificationCooldownCoroutine != null)
            StopCoroutine(verificationCooldownCoroutine);
        verificationCooldownCoroutine = StartCoroutine(VerificationCooldownRoutine(seconds));
    }

    private IEnumerator VerificationCooldownRoutine(int seconds)
    {
        if (verificationButton != null)
            verificationButton.interactable = false;

        int remaining = Mathf.Max(1, seconds);
        while (remaining > 0)
        {
            SetVerificationButtonText($"{remaining}s");
            yield return new WaitForSeconds(1f);
            remaining--;
        }

        if (verificationButton != null)
            verificationButton.interactable = true;
        SetVerificationButtonText(verificationButtonDefaultText);
        verificationCooldownCoroutine = null;
    }

    private void SetVerificationButtonText(string text)
    {
        if (verificationButtonText != null)
            verificationButtonText.text = text;
    }
    private void DismissKeyboard()
    {
        // 让所有 InputField 失去焦点
        EventSystem.current?.SetSelectedGameObject(null);
    }
}