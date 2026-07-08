using System;
using System.Collections.Generic;
using UnityEngine;

public static class CharacterSystem
{
    public static event Action OnCharacterChanged;
    public static PlayerCharacterState characterState => AccountSystem.GetCurrentRole()?.characterState;
    private static readonly HashSet<int> defaultUnlockedForNewPlayer = new HashSet<int> { 1, 2 };
    private static List<CharacterData> characters;

    static CharacterSystem()
    {
        LoadCharacterData();
    }

    private static void LoadCharacterData()
    {
        TextAsset json = Resources.Load<TextAsset>("Jsons/CharacterData");
        characters = JsonUtility.FromJson<CharacterDataListWrapper>(json.text).characters;

        foreach (var character in characters)
        {
            character.LoadAssets();
        }
    }

    [Serializable]
    public class CharacterDataListWrapper
    {
        public List<CharacterData> characters;
    }

    public static List<CharacterData> GetCharacterImageDatas()
    {
        return characters;
    }

    public static CharacterData GetCharacterData(int characterID)
    {
        return characters.Find(i => i.characterID == characterID);
    }

    public static CharacterData GetCharacterForRole(RoleData role)
    {
        if (role == null || role.characterState == null)
            return null;

        int id = role.characterState.currentCharacterID;
        return characters.Find(i => i.characterID == id);
    }

    public static CharacterData GetCurrentCharacter()
    {
        if (characterState == null)
            return null;

        return characters.Find(i => i.characterID == characterState.currentCharacterID);
    }

    public static void SetCurrentCharacter(int characterID)
    {
        if (characterState == null)
            return;

        characterState.currentCharacterID = characterID;
        OnCharacterChanged?.Invoke();
    }

    public static bool IsCharacterUnLocked(int characterID)
    {
        var role = AccountSystem.GetCurrentRole();

        // 没有角色：新用户 → 只能用默认解锁列表
        if (role == null || role.characterState == null)
        {
            return defaultUnlockedForNewPlayer.Contains(characterID);
        }

        // 有角色：用角色自己的解锁状态
        return role.characterState.unlockedCharacters.Contains(characterID);
    }

    public static void UnLockCharacter(int characterID)
    {
        Debug.Log("解锁角色目前仍然本地");
        var role = AccountSystem.GetCurrentRole();
        if (role == null || role.characterState == null)
            return;

        role.characterState.unlockedCharacters.Add(characterID);
    }
    /// <summary>
    /// 获取当前角色的游戏昵称
    /// </summary>
    public static string GetCurrentNickName()
    {
        RoleData currentRole = AccountSystem.GetCurrentRole();
        if (currentRole == null)
        {
            return "未创建角色";
        }

        return string.IsNullOrEmpty(currentRole.nickName) ? "新玩家" : currentRole.nickName;
    }
}
