using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowthPageUI : PageUI
{
    public List<GrowthStageItemUI> stages;

    void OnEnable()
    {
        if (BackendSettings.UseBackendMode && SessionStore.HasToken)
            StartCoroutine(RefreshRemote());
        else
            Refresh();
    }

    private IEnumerator RefreshRemote()
    {
        yield return BackendFacade.RefreshGrowth(result =>
        {
            if (!result.Success || result.Data == null || !result.Data.success || result.Data.data == null)
            {
                UIManager.Instance.ShowRemind("成长加载失败", "知道了", string.IsNullOrEmpty(result.Message) ? "无法加载成长数据。" : result.Message);
                return;
            }

            Refresh();
        });
    }

    public void Refresh()
    {
        GrowthSystem.RefreshProgress();
        List<GrowthStageData> stageDatas = GrowthSystem.GetStageDatas();
        for (int i = 0; i < stages.Count && i < stageDatas.Count; i++)
            stages[i].Setup(stageDatas[i], Refresh);
    }
}
