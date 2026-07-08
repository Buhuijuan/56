using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PwdVisibleToggle : MonoBehaviour
{
    private Image icon;
    public Sprite showSprite;
    public Sprite hideSprite;
    public TMP_InputField text;
    public Toggle toggle;

    // Start is called before the first frame update
    void Start()
    {
        icon = GetComponent<Image>();
        toggle.onValueChanged.AddListener(OnValueChangedPwdVisible);
        OnValueChangedPwdVisible(toggle.isOn);
    }
    public void OnValueChangedPwdVisible(bool isChecked)
    {
        if (isChecked)
        {
            icon.sprite = showSprite;
            text.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            icon.sprite = hideSprite;
            text.contentType = TMP_InputField.ContentType.Password;
        }
        text.ForceLabelUpdate();
    }

}
