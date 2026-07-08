using UnityEngine.UI;

public class SignInPanelUI : BaseMultiplePagePanelUI
{
    public Toggle onlineToggle;
    public Toggle dailyToogle;
    public Toggle totalToggle;
    public PageUI onlinePage;
    public PageUI dailyPage;
    public PageUI totalPage;

    protected override void RegisterPages()
    {
        map.Add(onlineToggle, onlinePage);
        map.Add(dailyToogle, dailyPage);
        map.Add(totalToggle, totalPage);
    }
}
