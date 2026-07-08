using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputValidator : MonoBehaviour
{
    public TMP_InputField input;
    public Image errorOutline;
    public Text errorText;
    public void SetError(string message)
    {
        errorOutline.gameObject.SetActive(true);
        errorText.gameObject.SetActive(true);
        errorText.text = message;
    }
    public void ClearError()
    {
        errorOutline.gameObject.SetActive(false);
        errorText.gameObject.SetActive(false);
        errorText.text = "";
    }
}
