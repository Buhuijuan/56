using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StoryViewPageUI : PageUI
{
    public TMP_Text title, fullStory;
    public void Refresh(StoryRecord record)
    {
        title.text = record.theme;
        fullStory.text = record.fullText;
    }
}
