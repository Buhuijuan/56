using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerChangeUI : MonoBehaviour
{
    private MenuController controller;
    private MenuUI menuUI;
    public GameObject targetObject;

    void Start()
    {
        controller = FindObjectOfType<MenuController>();
        menuUI = FindObjectOfType<MenuUI>();
    }
    public void OnClickServerChange(string name)
    {
        controller.SetServer(name);
        menuUI.UpdateServerName(name);
    }

    public void OnButtonClick()
    {
        // 显示对象
        targetObject.SetActive(true);

        // 启动协程，5秒后隐藏
        StartCoroutine(HideAfterDelay(2f));
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        targetObject.SetActive(false);
    }
}
