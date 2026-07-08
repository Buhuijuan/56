using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitlePageUI : PageUI
{
    public Toggle allToggle;
    public Toggle exploreToggle;
    public Toggle socialToggle;
    public Toggle abilityToggle;
    public Toggle collectToggle;
    public Transform titleRoot;
    public TMP_Text currentTitle;
    public TMP_Text outTitle;
    public Button changeButton;
    public TitleToggle titlePrefab;

    private readonly List<TitleToggle> generatedItems = new();
    private Dictionary<TitleType, Toggle> typemap = new();
    private int selectedTitleId;
    private ScrollRect scrollRect;

    private void Start()
    {
        typemap = new()
        {
            { TitleType.Explore, exploreToggle },
            { TitleType.Social, socialToggle },
            { TitleType.Ability, abilityToggle },
            { TitleType.Collect, collectToggle },
        };

        allToggle.onValueChanged.AddListener(OnAllToggleChanged);
        foreach (KeyValuePair<TitleType, Toggle> kv in typemap)
        {
            TitleType capturedType = kv.Key;
            kv.Value.onValueChanged.AddListener(isSelected => OnTypeToggleChanged(isSelected, capturedType));
        }

        changeButton.onClick.AddListener(OnClickChange);
        scrollRect = titleRoot.GetComponentInParent<ScrollRect>();
    }

    private void OnEnable()
    {
        if (BackendSettings.UseBackendMode && SessionStore.HasToken)
        {
            BackendRuntime.Run(RefreshRemote());
            return;
        }

        Refresh();
    }

    private IEnumerator RefreshRemote()
    {
        yield return BackendFacade.RefreshGrowth(null);
        Refresh();
    }

    public void Refresh()
    {
        TitleSystem.RefreshUnlockedTitles();
        Clear();

        List<TitleData> titles = TitleSystem.GetTitleDatas();
        ToggleGroup group = titleRoot.GetComponent<ToggleGroup>();
        TitleData current = TitleSystem.GetCurrentTitle();
        selectedTitleId = current != null ? current.titleID : 1;

        foreach (TitleData title in titles)
        {
            TitleToggle item = Instantiate(titlePrefab, titleRoot);
            item.Setup(title, this);
            item.toggle.group = group;
            item.toggle.isOn = title.titleID == selectedTitleId;
            generatedItems.Add(item);
        }

        ApplyCurrentTitleToView();
        RebuildListLayout();
    }

    public void Clear()
    {
        foreach (TitleToggle item in generatedItems)
            Destroy(item.gameObject);

        generatedItems.Clear();
    }

    public void SelectTitle(int titleId)
    {
        selectedTitleId = titleId;
        TitleData data = TitleSystem.GetTitleDatas().Find(i => i.titleID == titleId);
        outTitle.text = data != null ? data.titleName : currentTitle.text;
    }

    public void OnClickChange()
    {
        if (!TitleSystem.IsTitleUnlocked(selectedTitleId))
        {
            UIManager.Instance.ShowRemind("佩戴失败", "知道了", "该称号尚未解锁");
            return;
        }

        if (BackendSettings.UseBackendMode && SessionStore.HasToken)
        {
            if (!TitleSystem.IsBackendUnlockedTitle(selectedTitleId))
            {
                if (!TitleSystem.SetCurrentTitle(selectedTitleId))
                {
                    UIManager.Instance.ShowRemind("佩戴失败", "知道了", "该称号尚未解锁");
                    return;
                }

                ApplyCurrentTitleToView();
                RefreshHeadArea();
                Refresh();
                return;
            }

            BackendRuntime.Run(EquipSelectedTitleRemote());
            return;
        }

        if (!TitleSystem.SetCurrentTitle(selectedTitleId))
        {
            UIManager.Instance.ShowRemind("佩戴失败", "知道了", "该称号尚未解锁");
            return;
        }

        ApplyCurrentTitleToView();
        RefreshHeadArea();
        Refresh();
    }

    private IEnumerator EquipSelectedTitleRemote()
    {
        BackendApiResult<BackendEquipTitleEnvelope> result = null;
        yield return BackendFacade.EquipTitle(selectedTitleId, value => result = value);

        if (result == null || !result.Success || result.Data == null || !result.Data.success)
        {
            UIManager.Instance.ShowRemind("佩戴失败", "知道了", result != null ? result.Message : "称号佩戴失败。", null);
            yield break;
        }

        yield return BackendFacade.RefreshGrowth(null);
        yield return BackendFacade.RefreshHome(null);

        ApplyCurrentTitleToView();
        RefreshHeadArea();
        Refresh();
    }

    public void OnAllToggleChanged(bool isSelected)
    {
        if (!isSelected)
            return;

        foreach (TitleToggle item in generatedItems)
            item.gameObject.SetActive(true);

        RebuildListLayout();
    }

    public void OnTypeToggleChanged(bool isSelected, TitleType type)
    {
        if (!isSelected)
            return;

        foreach (TitleToggle item in generatedItems)
            item.gameObject.SetActive(item.GetToggleTitleData().type == type);

        RebuildListLayout();
    }

    private void ApplyCurrentTitleToView()
    {
        TitleData current = TitleSystem.GetCurrentTitle();
        currentTitle.text = current != null ? current.titleName : "未使用称号";
        outTitle.text = currentTitle.text;
    }

    private void RefreshHeadArea()
    {
        foreach (HeadAreaUI headArea in Object.FindObjectsOfType<HeadAreaUI>(true))
            headArea.Refresh();
    }

    private void RebuildListLayout()
    {
        Canvas.ForceUpdateCanvases();

        if (titleRoot is RectTransform rectTransform)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }
}
