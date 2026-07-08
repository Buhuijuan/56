using TMPro;
using UnityEngine;

public class AskBubbleUI : MonoBehaviour
{
    public TMP_Text askText;

    public void Setup(string text)
    {
        askText.text = text;
    }
}
