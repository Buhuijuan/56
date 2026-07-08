using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyBindingMenuUI : MonoBehaviour
{
    public static KeyBindingMenuUI Instance { get; private set; }

    public GameObject keyPositionPanel;
    public RectTransform parentArea;
    public Button confirmButton, resetButton;
    public TMP_Text saveHintText;
    public Slider sizeSlider;
    public TMP_Text sizeValueText;
    public float hintDuration = 2f;

    public static event Action<bool> OnConflictChanged;
    private static bool currentConflict = false;

    private DraggableKeyItem selectedItem;
    private bool suppressSliderCallback;

    void Awake()
    {
        Instance = this;

        Dictionary<string, Vector2> defaults = new();
        foreach (var item in keyPositionPanel.GetComponentsInChildren<DraggableKeyItem>(true))
        {
            RectTransform rect = item.GetComponent<RectTransform>();
            RectTransform parent = rect.parent.GetComponent<RectTransform>();
            Vector2 norm = DraggableKeyItem.LocalToNormalized(rect.anchoredPosition, parent);
            defaults[item.keyId] = norm;
        }

        KeyBindingSystem.InitializeDefaultsIfNeeded(defaults);

        if (!PlayerPrefs.HasKey("KeyBindingInitialized"))
        {
            KeyBindingSystem.Save();
            PlayerPrefs.SetInt("KeyBindingInitialized", 1);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void NotifyConflictChanged(bool conflict)
    {
        if (conflict == currentConflict)
            return;

        currentConflict = conflict;
        OnConflictChanged?.Invoke(conflict);
    }

    void Start()
    {
        OnConflictChanged += conflict =>
        {
            if (confirmButton != null)
                confirmButton.interactable = !conflict;
        };

        if (saveHintText != null)
            saveHintText.gameObject.SetActive(false);

        foreach (var item in FindObjectsOfType<DraggableKeyItem>(true))
            item.InitPosition();

        if (sizeSlider != null)
        {
            sizeSlider.minValue = KeyBindingSystem.MinScale;
            sizeSlider.maxValue = KeyBindingSystem.MaxScale;
            sizeSlider.wholeNumbers = false;
            sizeSlider.onValueChanged.AddListener(OnSizeSliderChanged);
        }

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirm);
        if (resetButton != null)
            resetButton.onClick.AddListener(OnReset);

        DraggableKeyItem firstItem = keyPositionPanel != null
            ? keyPositionPanel.GetComponentInChildren<DraggableKeyItem>(true)
            : null;
        SelectItem(firstItem);
    }

    public void OnConfirm()
    {
        if (currentConflict)
            return;

        foreach (var item in FindObjectsOfType<DraggableKeyItem>(true))
        {
            RectTransform rect = item.GetComponent<RectTransform>();
            Vector2 norm = DraggableKeyItem.LocalToNormalized(rect.anchoredPosition, parentArea);
            KeyBindingSystem.normalizedPositions[item.keyId] = norm;
            KeyBindingSystem.normalizedScales[item.keyId] = item.GetCurrentScale();
        }

        KeyBindingSystem.Save();
        GameKeyItem.RefreshAllInScene();
        ShowHint("键位已保存...");
    }

    public void OnReset()
    {
        KeyBindingSystem.ResetToDefault();

        foreach (var item in FindObjectsOfType<DraggableKeyItem>(true))
            item.InitPosition();

        DraggableKeyItem firstItem = keyPositionPanel != null
            ? keyPositionPanel.GetComponentInChildren<DraggableKeyItem>(true)
            : null;
        SelectItem(firstItem);

        NotifyConflictChanged(false);
        GameKeyItem.RefreshAllInScene();
        ShowHint("键位已重置...");
    }

    public void SelectItem(DraggableKeyItem item)
    {
        if (selectedItem != null)
            selectedItem.SetSelected(false);

        selectedItem = item;
        if (selectedItem != null)
            selectedItem.SetSelected(true);

        float scale = selectedItem != null ? selectedItem.GetCurrentScale() : KeyBindingSystem.DefaultScale;
        UpdateSizeSlider(scale);
    }

    public void NotifySelectedScaleChanged(DraggableKeyItem item, float scale)
    {
        if (item != selectedItem)
            return;

        UpdateSizeSlider(scale);
    }

    private void OnSizeSliderChanged(float value)
    {
        if (suppressSliderCallback || selectedItem == null)
        {
            UpdateScaleText(value);
            return;
        }

        selectedItem.ApplyScale(value, false);
        UpdateScaleText(value);
    }

    private void UpdateSizeSlider(float scale)
    {
        float clamped = Mathf.Clamp(scale, KeyBindingSystem.MinScale, KeyBindingSystem.MaxScale);

        suppressSliderCallback = true;
        if (sizeSlider != null)
            sizeSlider.value = clamped;
        suppressSliderCallback = false;

        UpdateScaleText(clamped);
    }

    private void UpdateScaleText(float scale)
    {
        if (sizeValueText == null)
            return;

        int percent = Mathf.RoundToInt(scale * 100f);
        sizeValueText.text = $"{percent}%";
    }

    private void ShowHint(string msg)
    {
        if (saveHintText == null)
            return;

        saveHintText.text = msg;
        saveHintText.gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(HideHintAfterDelay());
    }

    private IEnumerator HideHintAfterDelay()
    {
        yield return new WaitForSeconds(hintDuration);
        if (saveHintText != null)
            saveHintText.gameObject.SetActive(false);
    }
}
