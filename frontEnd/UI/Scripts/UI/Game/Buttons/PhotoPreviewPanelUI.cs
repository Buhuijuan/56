using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhotoPreviewPanelUI : MonoBehaviour
{
    public Image headImage;
    public TMP_Text nickName;
    void Start()
    {
        gameObject.SetActive(false);
    }
    void OnEnable()
    {
        Refresh();
    }
    public void Refresh()
    {
        var role = AccountSystem.GetCurrentRole();
        headImage.sprite = AvatarLoader.LoadAvatar(role);

        nickName.text = role.nickName;
    }
}
