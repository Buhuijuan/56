using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class AvatarLoader
{
    private const string DefaultAvatarPath = "Sprites/NPCImages/Head2";

    private static readonly Dictionary<string, Sprite> RemoteSpriteCache = new();
    private static readonly HashSet<string> DownloadingUrls = new();

    public static Sprite LoadAvatar(RoleData role)
    {
        if (role == null)
            return LoadDefaultAvatar();

        if (role.useCustomAvatar && !string.IsNullOrEmpty(role.customAvatarPath) && File.Exists(role.customAvatarPath))
            return LoadSpriteFromFile(role.customAvatarPath);

        if (!string.IsNullOrWhiteSpace(role.avatarUrl))
        {
            string resolvedUrl = ResolveAvatarUrl(role.avatarUrl);
            if (string.IsNullOrWhiteSpace(resolvedUrl))
                return LoadDefaultAvatar();

            if (RemoteSpriteCache.TryGetValue(resolvedUrl, out Sprite cached) && cached != null)
                return cached;

            string cachePath = GetRemoteCachePath(resolvedUrl);
            if (File.Exists(cachePath))
            {
                Sprite localCached = LoadSpriteFromFile(cachePath);
                if (localCached != null)
                {
                    RemoteSpriteCache[resolvedUrl] = localCached;
                    return localCached;
                }
            }

            if (!DownloadingUrls.Contains(resolvedUrl))
            {
                DownloadingUrls.Add(resolvedUrl);
                BackendRuntime.Run(DownloadRemoteAvatar(resolvedUrl, cachePath));
            }
        }

        return LoadDefaultAvatar();
    }

    private static Sprite LoadSpriteFromFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;

        byte[] bytes = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        if (!tex.LoadImage(bytes))
            return null;

        return Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f));
    }

    private static IEnumerator DownloadRemoteAvatar(string url, string cachePath)
    {
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        if (SessionStore.HasToken)
            request.SetRequestHeader("Authorization", $"Bearer {SessionStore.Current.token}");
        yield return request.SendWebRequest();

        DownloadingUrls.Remove(url);

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[AvatarLoader] Remote avatar download failed: {url}, error={request.error}, code={(int)request.responseCode}");
            yield break;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);
        if (texture == null)
            yield break;

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f));
        RemoteSpriteCache[url] = sprite;

        try
        {
            string dir = Path.GetDirectoryName(cachePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            byte[] png = texture.EncodeToPNG();
            if (png != null && png.Length > 0)
                File.WriteAllBytes(cachePath, png);
        }
        catch
        {
        }

        HeadAreaUI.RefreshAll();
    }

    private static string GetRemoteCachePath(string url)
    {
        string hash = Hash128.Compute(url).ToString();
        return Path.Combine(Application.persistentDataPath, "avatar_cache", $"{hash}.png");
    }

    private static string ResolveAvatarUrl(string avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl))
            return null;

        string trimmed = avatarUrl.Trim();
        if (trimmed.StartsWith("http://") || trimmed.StartsWith("https://"))
            return trimmed;

        string baseUrl = BackendSettings.BaseUrl;
        if (string.IsNullOrWhiteSpace(baseUrl))
            return trimmed;

        if (!trimmed.StartsWith("/"))
            trimmed = "/" + trimmed;

        return baseUrl.TrimEnd('/') + trimmed;
    }

    private static Sprite LoadDefaultAvatar()
    {
        return Resources.Load<Sprite>(DefaultAvatarPath);
    }
}
