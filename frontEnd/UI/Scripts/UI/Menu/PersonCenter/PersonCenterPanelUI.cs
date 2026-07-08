using UnityEngine.UI;

public class PersonCenterPanelUI : BaseMultiplePagePanelUI
{
    public Toggle roleToggle, safeTogggle, serviceToggle, developerToggle;
    public PageUI rolePage, safePage, servicePage, developerPage;

    protected override void RegisterPages()
    {
        map.Add(roleToggle, rolePage);
        map.Add(safeTogggle, safePage);
        map.Add(serviceToggle, servicePage);
        map.Add(developerToggle, developerPage);
    }
}
