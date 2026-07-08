using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToggleStyleUI : MonoBehaviour
{
    [Header("文字引用")]
    public TMP_Text labelText;

    [Header("选中样式")]
    public Color selectedColor;
    public int selectedFontSize;
    public TMP_FontAsset selectedFontAsset;

    [Header("未选中样式")]
    public Color normalColor;
    public int normalFontSize;
    public TMP_FontAsset normalFontAsset;

    [Header("阴影效果")]
    public Color underlayColor = new Color(0, 0, 0, 0.5f);
    public Vector2 underlayOffset = new Vector2(1.0f, -1.0f);
    public float underlaySoftness = 0.7f;
    public float underlayDilate = 1.0f;
    [Header("是否移至顶部")]
    public bool isTop = false;

    private Toggle toggle;
    private Material runtimeMaterial;


    void Start()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnTabValueChanged);

        runtimeMaterial = Instantiate(labelText.fontMaterial);
        runtimeMaterial.shader = Shader.Find("TextMeshPro/Distance Field");
        runtimeMaterial.EnableKeyword("UNDERLAY_ON");
        runtimeMaterial.SetColor("_UnderlayColor", underlayColor);
        runtimeMaterial.SetFloat("_UnderlayOffsetX", underlayOffset.x);
        runtimeMaterial.SetFloat("_UnderlayOffsetY", underlayOffset.y);
        runtimeMaterial.SetFloat("_UnderlaySoftness", underlaySoftness);
        runtimeMaterial.SetFloat("_UnderlayDilate", underlayDilate);
        labelText.fontMaterial = runtimeMaterial;

        ApplyStyle(toggle.isOn);
    }

    public void OnTabValueChanged(bool isOn)
    {
        ApplyStyle(isOn);
    }

    void ApplyStyle(bool isSelected)
    {
        if (labelText == null) return;
        if (isSelected)
        {
            labelText.color = selectedColor;
            labelText.fontSize = selectedFontSize;
            labelText.font = selectedFontAsset;
            EnableUnderlay();
            if (isTop)
                transform.SetAsLastSibling();
        }
        else
        {
            labelText.color = normalColor;
            labelText.fontSize = normalFontSize;
            labelText.font = normalFontAsset;
            DisableUnderlay();

        }
    }

    void EnableUnderlay()
    {
        runtimeMaterial.SetColor("_UnderlayColor", underlayColor);
        runtimeMaterial.SetFloat("_UnderlayOffsetX", underlayOffset.x);
        runtimeMaterial.SetFloat("_UnderlayOffsetY", underlayOffset.y);
        runtimeMaterial.SetFloat("_UnderlaySoftness", underlaySoftness);
        runtimeMaterial.SetFloat("_UnderlayDilate", underlayDilate);
    }
    void DisableUnderlay()
    {
        runtimeMaterial.SetColor("_UnderlayColor", new Color(0, 0, 0, 0));
        runtimeMaterial.SetFloat("_UnderlayOffsetX", 0);
        runtimeMaterial.SetFloat("_UnderlayOffsetY", 0);
        runtimeMaterial.SetFloat("_UnderlaySoftness", 0);
        runtimeMaterial.SetFloat("_UnderlayDilate", 0);
    }

}