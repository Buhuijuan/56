using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailBoxChangePanelUI : MonoBehaviour
{
    public InputValidator newMailBoxInput, verifactionInput;
    public Button confirmButton, cancelButton, getVerifactionButton;
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
        newMailBoxInput.ClearError();
        verifactionInput.ClearError();
        string newMailBox = newMailBoxInput.input.text;
        string verification = verifactionInput.input.text;
        if (string.IsNullOrWhiteSpace(newMailBox))
        {
            newMailBoxInput.SetError("请输入新邮箱");
            return;
        }
        if (!AccountSystem.IsValidEmail(newMailBox))
        {
            newMailBoxInput.SetError("邮箱格式不正确");
            return;
        }
        if (string.IsNullOrWhiteSpace(verification))
        {
            verifactionInput.SetError("请输入验证码");
            return;
        }
        if (verification != "111222")
        {
            verifactionInput.SetError("验证码不正确");
            return;
        }
        StartCoroutine(AccountSystem.ChangeMailbox(newMailBox, (ok, msg) =>
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
