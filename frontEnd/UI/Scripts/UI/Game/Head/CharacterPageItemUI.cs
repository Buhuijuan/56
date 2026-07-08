using UnityEngine.UI;
using UnityEngine;

public class CharacterPageItemUI : MonoBehaviour
{
    public Image characterImage;
    public GameObject mask;
    private CharacterData data;
    private CharactePageUI parent;
    public void Setup(CharacterData data, CharactePageUI parent)
    {
        this.data = data;
        this.parent = parent;
        characterImage.sprite = data.characterImage;
        mask.SetActive(!CharacterSystem.IsCharacterUnLocked(data.characterID));
    }
    public CharacterData GetData()
    {
        return data;
    }
}
