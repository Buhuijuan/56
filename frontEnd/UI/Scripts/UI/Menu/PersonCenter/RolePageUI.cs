using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class RolePageUI : PageUI
{
    public Transform contentRoot;
    public GameObject emptyRoot;
    public RoleItemUI rolePrefab;
    private List<RoleItemUI> generatedItems = new List<RoleItemUI>();
    private bool isRefreshingFromBackend;

    void OnEnable()
    {
        if (BackendSettings.UseBackendMode && SessionStore.HasToken && SessionStore.Current.hasCurrentRole && !isRefreshingFromBackend)
            StartCoroutine(RefreshWithBackendTasks());
        else
            Refresh();
    }

    private IEnumerator RefreshWithBackendTasks()
    {
        isRefreshingFromBackend = true;
        yield return BackendFacade.RefreshTasks(null);
        isRefreshingFromBackend = false;
        Refresh();
    }

    public void Refresh()
    {
        Clear();
        List<RoleData> roles = AccountSystem.GetRoles();
        if (roles.Count == 0)
        {
            emptyRoot.SetActive(true);
            contentRoot.gameObject.SetActive(false);
        }
        else
        {
            emptyRoot.SetActive(false);
            contentRoot.gameObject.SetActive(true);
            foreach (var role in roles)
            {
                RoleItemUI roleItem = Instantiate(rolePrefab, contentRoot);
                roleItem.Setup(role, this);
                generatedItems.Add(roleItem);
            }
        }
    }
    public void Clear()
    {
        foreach (var item in generatedItems)
        {
            Destroy(item.gameObject);
        }
        generatedItems.Clear();
    }
}
