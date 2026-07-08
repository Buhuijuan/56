using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharactePageUI : PageUI
{
    public Image avatarImage, outAvatarImage;
    public TMP_Text nickName, level, exp, campus, outNickName;
    public Button modifyButton, resetButton, headButton;
    public Transform pageRoot;
    public GameObject pagePrefab;
    public PagesSwitcher pagesSwitcher;
    public BikeToggleUI bikeToggle;
    private List<GameObject> pages = new();

    void Awake()
    {
        pagesSwitcher.SetPages(pages);
        modifyButton.onClick.AddListener(OnClickModify);
        resetButton.onClick.AddListener(OnClickReset);
        headButton.onClick.AddListener(OnClickUploadAvatar);
    }

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        Clear();

        RoleData currentRole = AccountSystem.GetCurrentRole();
        Sprite sprite = AvatarLoader.LoadAvatar(currentRole);
        avatarImage.sprite = sprite;
        outAvatarImage.sprite = sprite;
        nickName.text = currentRole.nickName;
        outNickName.text = currentRole.nickName;
        level.text = "Lv" + currentRole.levelState.level;
        exp.text = currentRole.levelState.exp.ToString();
        campus.text = currentRole.campusName;

        List<CharacterData> characters = CharacterSystem.GetCharacterImageDatas();
        foreach (var character in characters)
        {
            GameObject page = Instantiate(pagePrefab, pageRoot);
            page.GetComponent<CharacterPageItemUI>().Setup(character, this);
            pages.Add(page);
        }

        pagesSwitcher.SetPage(0);
    }

    public void Clear()
    {
        foreach (var page in pages)
            Destroy(page);

        pages.Clear();
    }

    public void OnClickModify()
    {
        int currentIndex = pagesSwitcher.CurrentIndex;
        GameObject currentPage = pages[currentIndex];
        CharacterData currentCharacter = currentPage.GetComponent<CharacterPageItemUI>().GetData();
        if (!CharacterSystem.IsCharacterUnLocked(currentCharacter.characterID))
        {
            Debug.LogError("该形象未解锁，无法修改");
            return;
        }

        CharacterSystem.SetCurrentCharacter(currentCharacter.characterID);
        bikeToggle.bikeToggle.isOn = false;
    }

    public void OnClickReset()
    {
        pagesSwitcher.SetPage(0);
        GameObject defaultPage = pages[0];
        CharacterData defaultCharacter = defaultPage.GetComponent<CharacterPageItemUI>().GetData();
        CharacterSystem.SetCurrentCharacter(defaultCharacter.characterID);
        bikeToggle.bikeToggle.isOn = false;
    }

    public void OnClickUploadAvatar()
    {
        NativeGallery.GetImageFromGallery(path =>
        {
            if (path == null)
                return;

            Texture2D tex = NativeGallery.LoadImageAtPath(path, 1024);
            if (tex == null)
                return;

            Sprite avatarSprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f));

            avatarImage.sprite = avatarSprite;
            outAvatarImage.sprite = avatarSprite;

            string savePath = SaveAvatar(path);

            RoleData role = AccountSystem.GetCurrentRole();
            role.useCustomAvatar = true;
            role.customAvatarPath = savePath;

            if (BackendSettings.UseBackendMode && SessionStore.HasToken)
            {
                byte[] bytes = File.ReadAllBytes(path);
                if (bytes != null && bytes.Length > 0)
                    StartCoroutine(UploadAvatarRemote(bytes, savePath, path));
            }
            else
            {
                LocalProfileSaveSystem.SaveCurrentAccount();
            }

        },
        "选择头像", "image/*");
    }

    private IEnumerator UploadAvatarRemote(byte[] bytes, string savedPath, string sourcePath)
    {
        BackendApiResult<BackendAvatarUploadEnvelope> result = null;
        string extension = Path.GetExtension(sourcePath);
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".jpg";
        string fileName = $"avatar_{AccountSystem.GetCurrentRole()?.roleID ?? "current"}{extension}";
        string mimeType = GetMimeType(extension);

        yield return BackendFacade.UploadCurrentRoleAvatar(bytes, fileName, mimeType, value => result = value);

        if (result == null || !result.Success || result.Data == null || !result.Data.success || result.Data.data == null)
        {
            string message = result != null && !string.IsNullOrEmpty(result.Message) ? result.Message : "头像上传失败，已保留本地头像。";
            UIManager.Instance.ShowRemind("提示", "知道了", message);
            yield break;
        }

        RoleData role = AccountSystem.GetCurrentRole();
        if (role != null)
        {
            role.avatarUrl = result.Data.data.avatarUrl;
            role.useCustomAvatar = true;
            role.customAvatarPath = savedPath;
        }

        LocalProfileSaveSystem.SaveCurrentAccount();
        HeadAreaUI.RefreshAll();
    }

    private string SaveAvatar(string sourcePath)
    {
        string roleId = AccountSystem.GetCurrentRole()?.roleID;
        string extension = Path.GetExtension(sourcePath);
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".jpg";
        string fileName = string.IsNullOrWhiteSpace(roleId) ? $"avatar{extension}" : $"avatar_{roleId}{extension}";
        string savePath = Path.Combine(Application.persistentDataPath, fileName);
        File.Copy(sourcePath, savePath, true);

        return savePath;
    }

    private static string GetMimeType(string extension)
    {
        string ext = extension.ToLowerInvariant();
        if (ext == ".png")
            return "image/png";
        if (ext == ".jpg" || ext == ".jpeg")
            return "image/jpeg";
        if (ext == ".webp")
            return "image/webp";
        return "application/octet-stream";
    }
}
