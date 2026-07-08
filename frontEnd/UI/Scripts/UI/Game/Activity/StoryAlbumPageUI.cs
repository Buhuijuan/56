using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StoryAlbumPageUI : PageUI
{
    public Transform storysRoot;
    public StoryCardItemUI cardPrefab;
    public GameObject emptyRoot;
    public ActivityPanelUI parent;
    public StoryViewPageUI storyViewPage;
    void OnEnable()
    {
        Refresh();
    }
    public void Refresh()
    {
        foreach (Transform child in storysRoot)
            Destroy(child.gameObject);
        var records = StoryEventSystem.myStories;
        if (records.Count == 0)
            emptyRoot.SetActive(true);
        else
        {
            emptyRoot.SetActive(false);
            foreach (var record in records)
            {
                StoryCardItemUI item = Instantiate(cardPrefab, storysRoot);
                item.Setup(record, () =>
                {
                    parent.SwitchPage(ActivityPanelUI.ActivityPageState.StoryView);
                    storyViewPage.Refresh(record);
                });
            }
        }
    }
}
