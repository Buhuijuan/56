using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClockInLocationItemUI : MonoBehaviour
{
    public Image photoImage;
    public Sprite defaultSprite;
    public TMP_Text locationName;
    public Button button;

    private ClockInLocation data;

    public void Setup(ClockInLocation data, bool isClocked, Action<ClockInLocation> onClockIn)
    {
        this.data = data;

        if (locationName != null)
        {
            locationName.gameObject.SetActive(true);
            locationName.text = data.name;
            locationName.alignment = TextAlignmentOptions.Center;
            locationName.enableWordWrapping = true;
            locationName.overflowMode = TextOverflowModes.Overflow;
            locationName.enableAutoSizing = true;
            locationName.fontSizeMin = 14;
            locationName.fontSizeMax = 22;
            Color color = locationName.color;
            color.a = 1f;
            locationName.color = color;
        }

        if (photoImage != null)
            photoImage.sprite = isClocked ? LoadPhoto(data.locationId) ?? defaultSprite : defaultSprite;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClockIn?.Invoke(data));

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            string stateText = isClocked ? "已打卡" : "前往打卡";
            buttonText.text = data.name + "\n" + stateText;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.enableWordWrapping = true;
            buttonText.overflowMode = TextOverflowModes.Overflow;
            buttonText.enableAutoSizing = true;
            buttonText.fontSizeMin = 12;
            buttonText.fontSizeMax = 20;
        }
    }

    private Sprite LoadPhoto(string locationId)
    {
        return ClockInPhotoStore.LoadSprite(locationId, DateTime.Today);
    }
}
