using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterNamePanelUI : MonoBehaviour
{
    public InputValidator nickName;
    public Button chooseImageButton;
    public CharacterImagePanelUI characterImagePanel;
    void Start()
    {
        chooseImageButton.onClick.AddListener(OnClickChooseImage);
        gameObject.SetActive(true);
    }
    //选择形象按钮
    public void OnClickChooseImage()
    {
        nickName.ClearError();
        if (string.IsNullOrWhiteSpace(nickName.input.text))
        {
            nickName.SetError("昵称不能为空");
            return;
        }
        if (nickName.input.text.Length > 5)
        {
            nickName.SetError("昵称不能超过5个字符");
            return;
        }
        CharacterCreationSession.tempNickName = nickName.input.text;
        gameObject.SetActive(false);
        characterImagePanel.gameObject.SetActive(true);
    }

}
