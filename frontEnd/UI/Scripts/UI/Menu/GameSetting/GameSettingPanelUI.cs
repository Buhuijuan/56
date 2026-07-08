using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingPanelUI : BaseMultiplePagePanelUI
{
    public Toggle pictureToggle, audioToggle, controlToggle;
    public PageUI picturePage, audioPage, controlPage;

    protected override void RegisterPages()
    {
        map.Add(pictureToggle, picturePage);
        map.Add(audioToggle, audioPage);
        map.Add(controlToggle, controlPage);
    }
}
