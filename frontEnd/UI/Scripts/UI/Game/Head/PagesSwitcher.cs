using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PagesSwitcher : MonoBehaviour
{
    public Button prevButton;
    public Button nextButton;
    public bool loop = true;
    private List<GameObject> pages = new();
    private int currentIndex = 0;
    public int CurrentIndex => currentIndex;
    void Awake()
    {
        prevButton.onClick.AddListener(OnClickPrev);
        nextButton.onClick.AddListener(OnClickNext);
        UpdatePage();
    }
    public void SetPages(List<GameObject> pages)
    {
        this.pages = pages;
        currentIndex = 0;
        UpdatePage();
    }
    public void OnClickNext()
    {
        if (pages.Count == 0) return;

        if (loop)
            currentIndex = (currentIndex + 1) % pages.Count;
        else
            currentIndex = Mathf.Min(currentIndex + 1, pages.Count - 1);

        UpdatePage();
    }

    public void OnClickPrev()
    {
        if (pages.Count == 0) return;

        if (loop)
            currentIndex = (currentIndex - 1 + pages.Count) % pages.Count;
        else
            currentIndex = Mathf.Max(currentIndex - 1, 0);

        UpdatePage();
    }

    public void UpdatePage()
    {
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].SetActive(i == currentIndex);
        }
    }
    public void SetPage(int index)
    {
        if (pages.Count == 0)
        {
            return;
        }
        currentIndex = Math.Clamp(index, 0, pages.Count - 1);
        UpdatePage();
    }
    public void ClearPages()
    {
        pages = new List<GameObject>();
        currentIndex = 0;
    }

}
