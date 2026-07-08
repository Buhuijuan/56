using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SafePageUI : PageUI
{
    public TMP_Text accountID, password, mailbox;
    public Button modifyButton, changebindButton, deleteAccountButton;
    private AccountData data;

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        data = AccountSystem.GetCurrentAccount();

        accountID.text = data != null ? data.accountID : string.Empty;
        password.text = data != null ? data.password : string.Empty;
        mailbox.text = data != null ? data.mailbox : string.Empty;

        modifyButton.onClick.RemoveAllListeners();
        modifyButton.onClick.AddListener(OnClickModify);

        changebindButton.onClick.RemoveAllListeners();
        changebindButton.onClick.AddListener(OnClickChangebind);

        deleteAccountButton.onClick.RemoveAllListeners();
        deleteAccountButton.onClick.AddListener(OnClickDeleteAccount);
    }

    public void OnClickModify()
    {
        UIManager.Instance.ShowPWDModify();
    }

    public void OnClickChangebind()
    {
        UIManager.Instance.ShowMailBoxChange();
    }

    public void OnClickDeleteAccount()
    {
        UIManager.Instance.ShowConfirm(
            "确认注销当前账号吗？",
            "注销后将清空本地缓存。您的个人资料与存储数据将被永久清空，且无法恢复。",
            () =>
            {
                StartCoroutine(AccountSystem.DeleteAccount((ok, msg) =>
                {
                    if (ok)
                    {
                        GameSceneManager.Instance.SwitchScene("02_Login");
                    }
                    else
                    {
                        UIManager.Instance.ShowRemind("错误", "知道了", msg);
                    }
                }));
            }
        );
    }
}
