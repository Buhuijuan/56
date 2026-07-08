using System;
using UnityEngine;

[Serializable]
public class SessionSnapshot
{
    public string token;
    public string accountId;
    public string accountCode;
    public string mailbox;
    public long currentRoleId;
    public bool hasCurrentRole;
}

public static class SessionStore
{
    private const string SessionKey = "backend.session";
    private static SessionSnapshot current;

    public static SessionSnapshot Current
    {
        get
        {
            if (current == null)
                Load();
            return current;
        }
    }

    public static bool HasToken => Current != null && !string.IsNullOrEmpty(Current.token);

    public static void Save(SessionSnapshot snapshot)
    {
        current = snapshot;
        PlayerPrefs.SetString(SessionKey, JsonUtility.ToJson(snapshot));
        PlayerPrefs.Save();
    }

    public static void Clear()
    {
        current = new SessionSnapshot();
        PlayerPrefs.DeleteKey(SessionKey);
        PlayerPrefs.Save();
    }

    private static void Load()
    {
        if (!PlayerPrefs.HasKey(SessionKey))
        {
            current = new SessionSnapshot();
            return;
        }
        current = JsonUtility.FromJson<SessionSnapshot>(PlayerPrefs.GetString(SessionKey));
        if (current == null)
            current = new SessionSnapshot();
    }
}
