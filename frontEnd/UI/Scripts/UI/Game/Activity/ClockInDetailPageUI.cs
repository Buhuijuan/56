using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClockInDetailPageUI : PageUI
{
    public Transform locationRoot;
    public ClockInLocationItemUI locationItemPrefab;
    public TMP_Text clockProgress;

    private void OnEnable()
    {
        if (BackendSettings.UseBackendMode && SessionStore.HasToken)
        {
            BackendRuntime.Run(RefreshRemote());
            return;
        }

        Refresh();
    }

    public void Refresh()
    {
        if (BackendSettings.UseBackendMode && SessionStore.HasToken)
        {
            RefreshRemoteView();
            return;
        }

        ClockInEventSystem.CheckAndRefresh();
        List<ClockInLocation> todayLocations = ClockInEventSystem.GetTodayLocations();
        Populate(todayLocations, location => ClockInEventSystem.IsChecked(location.locationId), OnClickLocalClockIn);
    }

    private IEnumerator RefreshRemote()
    {
        yield return BackendFacade.RefreshClockIn(null);
        yield return BackendFacade.RefreshHome(null);
        yield return BackendFacade.RefreshGrowth(null);
        RefreshRemoteView();
    }

    private void RefreshRemoteView()
    {
        List<ClockInLocation> todayLocations = GameStateStore.ClockIn?.config?.locations ?? new List<ClockInLocation>();
        Populate(todayLocations, IsRemoteChecked, OnClickRemoteClockIn);
    }

    private void Populate(IEnumerable<ClockInLocation> locations, System.Func<ClockInLocation, bool> isChecked, System.Action<ClockInLocation> onClick)
    {
        ClearItems();

        List<ClockInLocation> locationList = locations != null ? locations.Take(2).ToList() : new List<ClockInLocation>();
        UpdateProgress(locationList.Count(location => isChecked(location)), locationList.Count);

        foreach (ClockInLocation location in locationList)
        {
            ClockInLocationItemUI item = Instantiate(locationItemPrefab, locationRoot);
            item.Setup(location, isChecked(location), onClick);
            RectTransform itemRect = item.transform as RectTransform;
            if (itemRect == null)
                continue;

            itemRect.anchorMin = new Vector2(0.5f, 0.5f);
            itemRect.anchorMax = new Vector2(0.5f, 0.5f);
            itemRect.pivot = new Vector2(0.5f, 0.5f);
            itemRect.localScale = Vector3.one;
        }
    }

    private void OnClickLocalClockIn(ClockInLocation location)
    {
        Vector3 playerPosition = GetPlayerPosition();
        ClockInAttemptResult result = ClockInEventSystem.TryCheckIn(playerPosition, location.locationId);

        UIManager.Instance.ShowRemind(
            result.success ? "打卡成功" : (result.alreadyChecked ? "今日已打卡" : "打卡失败"),
            "知道了",
            result.message,
            result.success ? result.rewards : null);

        Refresh();
    }

    private void OnClickRemoteClockIn(ClockInLocation location)
    {
        Vector3 playerPosition = GetPlayerPosition();
        BackendRuntime.Run(CheckRemoteClockIn(location, playerPosition));
    }

    private IEnumerator CheckRemoteClockIn(ClockInLocation location, Vector3 playerPosition)
    {
        ClockInCheckBody body = new ClockInCheckBody
        {
            currentPosX = playerPosition.x,
            currentPosY = playerPosition.y,
            currentPosZ = playerPosition.z
        };

        BackendApiResult<BackendClockInCheckEnvelope> result = null;
        yield return BackendFacade.CheckClockIn(location.locationId, body, value => result = value);

        if (result == null || !result.Success || result.Data == null || !result.Data.success)
        {
            UIManager.Instance.ShowRemind("打卡失败", "知道了", result != null ? result.Message : "打卡提交失败。", null);
            yield break;
        }

        yield return CaptureClockInPhoto(location.locationId);
        UpdateLocalClockInProgress(location.locationId, location.name);
        yield return BackendFacade.RefreshClockIn(null);
        yield return BackendFacade.RefreshHome(null);

        string message = $"{location.name}打卡成功。";
        if (result.Data.data != null)
            message += $"\n纪念币 +{result.Data.data.addedCoin}";

        UIManager.Instance.ShowRemind("打卡成功", "知道了", message, BuildRemoteRewards(result.Data.data));
        RefreshRemoteView();
    }

    private List<RewardItem> BuildRemoteRewards(BackendClockInCheckData checkData)
    {
        if (checkData == null || checkData.addedCoin <= 0)
            return new List<RewardItem>();

        RewardItem baseItem = RewardItemSystem.GetRewardItem(1);
        RewardItem reward = new RewardItem
        {
            rewardId = 1,
            amount = checkData.addedCoin,
            rewardName = baseItem != null ? baseItem.rewardName : "纪念币",
            rewardSprite = baseItem != null ? baseItem.rewardSprite : null,
            spritePath = baseItem != null ? baseItem.spritePath : null
        };

        return new List<RewardItem> { reward };
    }

    private bool IsRemoteChecked(ClockInLocation location)
    {
        List<string> checkedIds = GameStateStore.ClockIn?.state?.checkedLocationIds;
        return checkedIds != null && checkedIds.Contains(location.locationId);
    }

    private void ClearItems()
    {
        foreach (Transform child in locationRoot)
            Destroy(child.gameObject);
    }

    private void UpdateProgress(int checkedCount, int totalCount)
    {
        if (clockProgress == null)
            return;

        clockProgress.gameObject.SetActive(true);
        clockProgress.text = $"{checkedCount}/{totalCount}";
    }

    private IEnumerator CaptureClockInPhoto(string locationId)
    {
        PhotoCaptureManager manager = PhotoCaptureManager.Instance;
        if (manager == null)
            manager = UnityEngine.Object.FindObjectOfType<PhotoCaptureManager>();

        if (manager == null || string.IsNullOrEmpty(locationId))
            yield break;

        bool finished = false;
        string photoPath = ClockInPhotoStore.BuildPhotoPath(locationId, System.DateTime.Today);
        manager.CaptureAndSaveSilently(photoPath, _ => finished = true);

        while (!finished)
            yield return null;
    }

    private void UpdateLocalClockInProgress(string locationId, string locationName)
    {
        TitleEventReporter.ReportClockInSuccess(locationId, locationName);
    }

    private Vector3 GetPlayerPosition()
    {
        PlayerAgentMove playerAgentMove = Object.FindObjectOfType<PlayerAgentMove>();
        if (playerAgentMove != null)
            return playerAgentMove.transform.position;

        PlayerMove playerMove = Object.FindObjectOfType<PlayerMove>();
        if (playerMove != null)
            return playerMove.transform.position;

        return Vector3.zero;
    }
}
