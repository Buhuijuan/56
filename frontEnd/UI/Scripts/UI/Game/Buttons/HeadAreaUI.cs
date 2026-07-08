using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeadAreaUI : MonoBehaviour
{
    public Image headImage;
    public TMP_Text nickName, currentTitle, coinNum;

    void Start()
    {
        Refresh();
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        var role = AccountSystem.GetCurrentRole();
        var title = TitleSystem.GetCurrentTitle();
        if (role == null)
            return;

        headImage.sprite = AvatarLoader.LoadAvatar(role);

        nickName.text = role.nickName;
        if (currentTitle != null)
            currentTitle.text = title != null ? title.titleName : string.Empty;
        if (coinNum != null)
            coinNum.text = ActivityRewardSystem.GetMemorialCoin().ToString();
    }

    public static void RefreshAll()
    {
        foreach (HeadAreaUI headArea in Object.FindObjectsOfType<HeadAreaUI>(true))
        {
            if (headArea != null)
                headArea.Refresh();
        }
    }
}
