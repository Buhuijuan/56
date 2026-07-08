using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterImagePanelUI : MonoBehaviour
{
    public Transform contentRoot;
    public CharacterImageItemUI characterImageItemPrefab;
    public CharacterPreviewController previewController;
    public Button enterCampusButton;
    private List<CharacterImageItemUI> generatedItems = new List<CharacterImageItemUI>();
    void Start()
    {
        enterCampusButton.onClick.AddListener(OnClickEnterCampus);
        gameObject.SetActive(false);
    }
    void OnEnable()
    {
        Refresh();
    }
    public void Refresh()
    {
        Clear();
        List<CharacterData> characterImages = CharacterSystem.GetCharacterImageDatas();
        ToggleGroup group = contentRoot.GetComponent<ToggleGroup>();
        foreach (var image in characterImages)
        {
            CharacterImageItemUI item = Instantiate(characterImageItemPrefab, contentRoot);
            item.Setup(image, this);
            item.toggle.group = group;
            generatedItems.Add(item);
        }
        gameObject.SetActive(true);
    }
    public void Clear()
    {
        foreach (var item in generatedItems)
            Destroy(item.gameObject);
        generatedItems.Clear();
    }
    public void SetCharacterImage(CharacterData data)
    {
        CharacterCreationSession.selectedImageId = data.characterID;
        previewController.ShowCharacter(data);
    }
    public void OnClickEnterCampus()
    {
        StartCoroutine(CreateRoleAndEnterCampus());
    }

    private IEnumerator CreateRoleAndEnterCampus()
    {
        enterCampusButton.interactable = false;

        string campusName = CharacterCreationSession.tempCampusName;
        string nickname = CharacterCreationSession.tempNickName;
        int avatarId = CharacterCreationSession.selectedImageId;

        bool success = false;
        string message = string.Empty;

        // 调用 AccountSystem.CreateRole（内部已经包含 RestoreSession）
        yield return AccountSystem.CreateRole(campusName, nickname, avatarId, (ok, msg) =>
        {
            success = ok;
            message = msg;
        });

        enterCampusButton.interactable = true;

        if (!success)
        {
            UIManager.Instance.ShowRemind("角色创建失败", "知道啦", message);
            yield break;
        }

        CharacterCreationSession.Clear();

        // 进入校园（此时 GameStateStore.Home.role 一定完整）
        GameSceneManager.Instance.SwitchToGame("05_Game", "05_Campus");
    }


}
