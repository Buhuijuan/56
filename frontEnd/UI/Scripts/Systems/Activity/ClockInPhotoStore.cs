using System;
using System.IO;
using UnityEngine;

public static class ClockInPhotoStore
{
    private const string FolderName = "ClockInPhotos";

    public static string BuildPhotoPath(string locationId, DateTime date)
    {
        string fileName = $"{GetRoleSegment()}_{date:yyyyMMdd}_{locationId}.png";
        return Path.Combine(Application.persistentDataPath, FolderName, fileName);
    }

    public static Sprite LoadSprite(string locationId, DateTime date)
    {
        string path = BuildPhotoPath(locationId, date);
        if (!File.Exists(path))
            return null;

        byte[] bytes = File.ReadAllBytes(path);
        if (bytes == null || bytes.Length == 0)
            return null;

        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(bytes))
            return null;

        texture.name = Path.GetFileNameWithoutExtension(path);
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private static string GetRoleSegment()
    {
        string roleId = AccountSystem.GetCurrentRole()?.roleID;
        return string.IsNullOrWhiteSpace(roleId) ? "unknown_role" : Sanitize(roleId);
    }

    private static string Sanitize(string value)
    {
        string result = value;
        foreach (char c in Path.GetInvalidFileNameChars())
            result = result.Replace(c, '_');
        return result;
    }
}
