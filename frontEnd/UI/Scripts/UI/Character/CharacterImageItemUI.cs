using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CharacterImageItemUI : MonoBehaviour
{
    public Image characterImage;
    public GameObject maskImage;
    public Toggle toggle;
    private CharacterImagePanelUI parentPanel;
    private CharacterData data;
    public void Setup(CharacterData data, CharacterImagePanelUI characterImagePanel)
    {
        this.data = data;
        parentPanel = characterImagePanel;
        characterImage.sprite = data.characterImage;
        maskImage.SetActive(!CharacterSystem.IsCharacterUnLocked(data.characterID));
        toggle.interactable = CharacterSystem.IsCharacterUnLocked(data.characterID);
        toggle.isOn = false;
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }
    public void OnToggleValueChanged(bool isSelected)
    {
        if (isSelected)
            parentPanel.SetCharacterImage(data);
    }
}