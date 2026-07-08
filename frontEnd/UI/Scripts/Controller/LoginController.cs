using System.Collections;
using UnityEngine;

public class LoginController : MonoBehaviour
{
    private LoginPanelUI ui;

    private void Start()
    {
        ui = FindObjectOfType<LoginPanelUI>();
    }

    public void Login(string account, string password)
    {
        BackendRuntime.Run(LoginRoutine(account, password));
    }

    public void Register(string mailbox, string verification, string password)
    {
        BackendRuntime.Run(RegisterRoutine(mailbox, verification, password));
    }

    public void GetVerification(string mailbox)
    {
        Debug.Log($"[LoginController] GetVerification clicked. UseBackendMode={BackendSettings.UseBackendMode}, BaseUrl={BackendSettings.BaseUrl}, Mailbox={mailbox}");
        BackendRuntime.Run(GetVerificationRoutine(mailbox));
    }

    public void FindPassword(string mailbox)
    {
        if (!BackendSettings.UseBackendMode)
        {
            Debug.LogError("Find password logic is not implemented");
            return;
        }

        BackendRuntime.Run(GetVerificationRoutine(
            mailbox,
            "验证码已发送，请前往邮箱查收，然后调用重置密码接口完成密码重置。"
        ));
    }

    private IEnumerator LoginRoutine(string account, string password)
    {
        ui.SetLoginSubmitting(true);

        yield return BackendFacade.Login(account, password, result =>
        {
            Debug.Log($"[LoginController] Login response. Success={result.Success}, Status={result.StatusCode}, Message={result.Message}");
            Debug.Log(
                $"[LoginController] Login payload. HasEnvelope={result.Data != null}, ApiSuccess={result.Data?.success}, " +
                $"AccountId={result.Data?.data?.accountId}, AccountCode={result.Data?.data?.accountCode}, " +
                $"CurrentRoleId={result.Data?.data?.currentRoleId}, TokenEmpty={string.IsNullOrEmpty(result.Data?.data?.token)}");

            if (!result.Success || result.Data == null || !result.Data.success)
            {
                ShowLoginError(ResolveMessage(result, "登录失败，请检查邮箱和密码。"));
                return;
            }

            BackendSettings.AutoLoginEnabled = true;

            Debug.Log(
                $"[LoginController] SessionStore after login. HasToken={SessionStore.HasToken}, " +
                $"CurrentRoleId={SessionStore.Current.currentRoleId}, HasCurrentRole={SessionStore.Current.hasCurrentRole}, Mailbox={SessionStore.Current.mailbox}");

            BackendRuntime.Run(BackendFacade.RestoreSession((success, error) =>
            {
                Debug.Log($"[LoginController] RestoreSession finished. Success={success}, Error={error}");
                Debug.Log(
                    $"[LoginController] State after RestoreSession. HasRoles={GameStateStore.HasRoles}, HasCurrentRole={GameStateStore.HasCurrentRole}, " +
                    $"MeCurrentRoleId={GameStateStore.Me?.currentRoleId}, HomeRoleId={GameStateStore.Home?.role?.id}, HomeNick={GameStateStore.Home?.role?.nickName}");

                if (!success)
                {
                    ui.LaccountInput.SetError(string.IsNullOrEmpty(error) ? "初始化玩家数据失败。" : error);
                    return;
                }

                GameSceneManager.Instance.SwitchScene("03_Menu");
            }));
        });

        ui.SetLoginSubmitting(false);
    }

    private IEnumerator RegisterRoutine(string mailbox, string verification, string password)
    {
        ui.SetRegisterSubmitting(true);

        yield return BackendFacade.Register(mailbox, verification, password, result =>
        {
            if (!result.Success || result.Data == null || !result.Data.success)
            {
                ui.RverifyInput.SetError(ResolveMessage(result, "注册失败，请确认邮箱和验证码是否正确。"));
                return;
            }

            UIManager.Instance.ShowRemind(
                "注册成功",
                "前往登录",
                $"注册成功。\n你的账号 ID 为：{result.Data.data.accountCode}\n现在可以直接登录游戏。",
                null,
                () =>
                {
                    ui.ShowLoginPanel();
                    ui.ClearAllErrors();
                    ui.LaccountInput.input.text = mailbox;
                    ui.LpasswordInput.input.text = password;
                });
        });

        ui.SetRegisterSubmitting(false);
    }

    private IEnumerator GetVerificationRoutine(string mailbox, string successMessage = "验证码已发送，请检查你的邮箱。")
    {
        ui.SetVerificationSending(true);

        Debug.Log($"[LoginController] Sending verification code request to {BackendSettings.BaseUrl}/api/auth/send-code");

        yield return BackendFacade.SendCode(mailbox, result =>
        {
            if (!result.Success || result.Data == null || !result.Data.success)
            {
                Debug.LogError($"[LoginController] Send code failed. Status={result.StatusCode}, Message={result.Message}");
                ui.RmailboxInput.SetError(ResolveMessage(result, "获取验证码失败，请稍后重试。"));
                ui.SetVerificationSending(false);
                return;
            }

            Debug.Log($"[LoginController] Send code succeeded for {mailbox}");

            ui.StartVerificationCooldown(60);

            UIManager.Instance.ShowRemind(
                "验证码已发送",
                "确定",
                successMessage,
                null,
                null);
        });
    }

    private void ShowLoginError(string message)
    {
        string safeMessage = string.IsNullOrWhiteSpace(message) ? "登录失败，请检查邮箱和密码。" : message;
        string lower = safeMessage.ToLowerInvariant();

        if (safeMessage.Contains("密码") || lower.Contains("password"))
        {
            ui.LpasswordInput.SetError(safeMessage);
            return;
        }

        ui.LaccountInput.SetError(safeMessage);
    }

    private static string ResolveMessage<T>(BackendApiResult<T> result, string fallback) where T : class
    {
        if (!string.IsNullOrEmpty(result.Message))
            return result.Message;
        return fallback;
    }
}
