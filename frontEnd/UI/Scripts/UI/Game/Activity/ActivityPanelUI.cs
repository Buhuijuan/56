using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ActivityPanelUI : MonoBehaviour
{
    public enum ActivityPageState
    {
        OverView,
        QADetail,
        ClockInDetail,
        StoryDetail,
        QAContent,
        StoryContent,
        StoryCollection,
        StoryAlbum,
        StoryView
    }

    public GameObject ActivityPanel;
    public TMP_Text Title;
    public GameObject OverViewPanel;
    public GameObject QADetailPanel;
    public GameObject ClockInDetailPanel;
    public GameObject StoryDetailPanel;
    public GameObject QAContentPanel;
    public GameObject StoryContentPanel;
    public GameObject StoryCollectionPanel;
    public GameObject StoryAlbumPanel;
    public GameObject StoryViewPanel;

    private ActivityPageState currentState;
    private ActivityPageState storyViewPreviousPage;

    public class PageInfo
    {
        public string title;
        public GameObject page;

        public PageInfo(string title, GameObject page)
        {
            this.title = title;
            this.page = page;
        }
    }

    public Dictionary<ActivityPageState, PageInfo> pagemap;

    void Awake()
    {
        pagemap = new Dictionary<ActivityPageState, PageInfo>
        {
            { ActivityPageState.OverView, new PageInfo("\u6d3b\u52a8\u5e7f\u573a", OverViewPanel) },
            { ActivityPageState.QADetail, new PageInfo("\u6821\u56ed\u95ee\u7b54", QADetailPanel) },
            { ActivityPageState.ClockInDetail, new PageInfo("\u6668\u5149\u6253\u5361", ClockInDetailPanel) },
            { ActivityPageState.StoryDetail, new PageInfo("\u6545\u4e8b\u63a5\u9f99", StoryDetailPanel) },
            { ActivityPageState.QAContent, new PageInfo("\u7b54\u9898\u9875", QAContentPanel) },
            { ActivityPageState.StoryContent, new PageInfo("\u521b\u4f5c\u7a7a\u95f4", StoryContentPanel) },
            { ActivityPageState.StoryCollection, new PageInfo("\u6545\u4e8b\u96c6", StoryCollectionPanel) },
            { ActivityPageState.StoryAlbum, new PageInfo("\u521b\u4f5c\u518c", StoryAlbumPanel) },
            { ActivityPageState.StoryView, new PageInfo("\u6545\u4e8b\u8be6\u60c5", StoryViewPanel) }
        };

        ActivityPanel.SetActive(false);
        foreach (KeyValuePair<ActivityPageState, PageInfo> kv in pagemap)
        {
            kv.Value.page.SetActive(false);
        }
    }

    public void OpenActivityPopup()
    {
        ActivityPanel.SetActive(true);
        SwitchPage(ActivityPageState.OverView);
    }

    public void SwitchPage(ActivityPageState nextState)
    {
        if (nextState == ActivityPageState.StoryView)
        {
            storyViewPreviousPage = currentState;
        }

        currentState = nextState;
        foreach (KeyValuePair<ActivityPageState, PageInfo> kv in pagemap)
        {
            kv.Value.page.SetActive(false);
        }

        PageInfo nextPage = pagemap[nextState];
        Title.text = nextPage.title;
        nextPage.page.SetActive(true);
    }

    public void OnClickReturn()
    {
        if (currentState == ActivityPageState.OverView)
        {
            ActivityPanel.SetActive(false);
            return;
        }

        if (currentState == ActivityPageState.QADetail ||
            currentState == ActivityPageState.ClockInDetail ||
            currentState == ActivityPageState.StoryDetail)
        {
            SwitchPage(ActivityPageState.OverView);
            return;
        }

        if (currentState == ActivityPageState.QAContent)
        {
            if (QuizEventSystem.Session != null && !QuizEventSystem.Session.isFinished)
            {
                UIManager.Instance.ShowConfirm(
                    "\u786e\u8ba4\u9000\u51fa\u672c\u6b21\u7b54\u9898\u5417\uff1f",
                    "\u8bd5\u5377\u5c1a\u672a\u63d0\u4ea4\uff0c\u5982\u679c\u73b0\u5728\u9000\u51fa\uff1a\n\u672c\u6b21\u7b54\u9898\u8bb0\u5f55\u4e0d\u4f1a\u4fdd\u5b58\uff0c\u5f53\u524d\u79ef\u5206\u4e0d\u4f1a\u589e\u52a0\n\u4eca\u65e5\u7b54\u9898\u673a\u4f1a\u5c06\u89c6\u4e3a\u5df2\u4f7f\u7528\u3002",
                    () =>
                    {
                        QuizEventSystem.MarkPlayedTodayFromAbandon();
                        SwitchPage(ActivityPageState.QADetail);
                    });
            }
            else
            {
                SwitchPage(ActivityPageState.QADetail);
            }

            return;
        }

        if (currentState == ActivityPageState.StoryContent ||
            currentState == ActivityPageState.StoryCollection ||
            currentState == ActivityPageState.StoryAlbum)
        {
            SwitchPage(ActivityPageState.StoryDetail);
            return;
        }

        if (currentState == ActivityPageState.StoryView)
        {
            SwitchPage(storyViewPreviousPage);
        }
    }

    public void OnClickActivityItem(int index)
    {
        switch (index)
        {
            case 0:
                SwitchPage(ActivityPageState.QADetail);
                break;
            case 1:
                SwitchPage(ActivityPageState.ClockInDetail);
                break;
            case 2:
                SwitchPage(ActivityPageState.StoryDetail);
                break;
            case 3:
                SwitchPage(ActivityPageState.StoryCollection);
                break;
            case 4:
                SwitchPage(ActivityPageState.StoryAlbum);
                break;
        }
    }
}
