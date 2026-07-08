using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PWDModifyPanelUI : MonoBehaviour
{
    public InputValidator newPWDInput, confirmPWDInput;
    public Button confirmButton, cancelButton;
    public void Show()
    {
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnClickConfirm);
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(OnClickCancel);
        gameObject.SetActive(true);
    }
    public void OnClickConfirm()
    {
        newPWDInput.ClearError();
        confirmPWDInput.ClearError();
        string newPWD = newPWDInput.input.text;
        string confirmPWD = confirmPWDInput.input.text;
        if (string.IsNullOrWhiteSpace(newPWD))
        {
            newPWDInput.SetError("请输入新密码");
            return;
        }
        if (!AccountSystem.IsValidPwd(newPWD))
        {
            newPWDInput.SetError("密码必须包含8~20位数字和字母");
            return;
        }
        if (string.IsNullOrWhiteSpace(confirmPWD))
        {
            confirmPWDInput.SetError("请再次输入新密码");
            return;
        }
        if (newPWD != confirmPWD)
        {
            confirmPWDInput.SetError("两次输入不一致");
            return;
        }
        StartCoroutine(AccountSystem.ModifyPassword(newPWD, (ok, msg) =>
        {
            if (!ok)
            {
                UIManager.Instance.ShowRemind("错误", "知道了", msg);
            }
        }));

        gameObject.SetActive(false);
    }
    public void OnClickCancel()
    {
        gameObject.SetActive(false);
    }
}
