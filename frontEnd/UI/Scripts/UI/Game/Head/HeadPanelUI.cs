using System.ComponentModel;
using UnityEngine.UI;

public class HeadPanelUI : BaseMultiplePagePanelUI
{
    public Toggle characterToggle, titleToggle, levelToggle, growthToggle;
    public PageUI characterPage, titlePage, levelPage, growthPage;
    protected override void RegisterPages()
    {
        map.Add(characterToggle, characterPage);
        map.Add(titleToggle, titlePage);
        map.Add(levelToggle, levelPage);
        map.Add(growthToggle, growthPage);
    }
}
