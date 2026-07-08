using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseMultiplePagePanelUI : MonoBehaviour
{
    protected Dictionary<Toggle, PageUI> map = new Dictionary<Toggle, PageUI>();

    protected virtual void Awake()
    {
        RegisterPages();
        gameObject.SetActive(false);
        foreach (var kv in map)
        {
            kv.Value.Hide();
            kv.Key.onValueChanged.AddListener(OnValueChangedToggle);
        }
    }
    protected abstract void RegisterPages();

    private void OnValueChangedToggle(bool isChecked)
    {
        if (!isChecked) return;
        foreach (var kv in map)
        {
            if (kv.Key.isOn)
                kv.Value.Show();
            else
                kv.Value.Hide();
        }
    }
    public virtual void Open()
    {
        gameObject.SetActive(true);
        SetDefaultPage();
    }
    public virtual void Close()
    {
        gameObject.SetActive(false);
    }
    protected virtual void SetDefaultPage()
    {
        foreach (var kv in map)
        {
            kv.Key.isOn = true;
            kv.Value.Show();
            break;
        }
    }
}
