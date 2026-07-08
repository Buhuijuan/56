using System;
using TMPro;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    public TMP_Text currentServerName;
    private MenuController controller;
    void Start()
    {
        controller = FindObjectOfType<MenuController>();
    }
    public void UpdateServerName(String name)
    {
        currentServerName.text = name;
    }
}
