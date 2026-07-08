using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderValueUI : MonoBehaviour
{
    public Slider slider;
    public TMP_Text valueText;
    // Start is called before the first frame update
    void Start()
    {
        slider.onValueChanged.AddListener(OnSliderValueChanged);
        OnSliderValueChanged(slider.value);
    }

    void OnSliderValueChanged(float value)
    {
        int percent = Mathf.RoundToInt(value * 100f);
        valueText.text = percent + "%";
    }
}
