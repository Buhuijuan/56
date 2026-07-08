using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class PhotoCaptureManager : MonoBehaviour
{
    public static PhotoCaptureManager Instance { get; private set; }

    [Header("Capture")]
    public Canvas uiCanvas;
    public GameObject previewPanel;
    public RawImage previewImage;

    [Header("Save")]
    public string albumName = "此间方寸";
    public bool hidePreviewAfterSave = true;

    private Texture2D capturedTexture;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (capturedTexture != null)
        {
            Destroy(capturedTexture);
            capturedTexture = null;
        }
    }

    public void TakePhoto()
    {
        StartCoroutine(CaptureRoutine());
    }

    private IEnumerator CaptureRoutine()
    {
        bool restoreCanvas = uiCanvas != null && uiCanvas.enabled;
        if (uiCanvas != null)
            uiCanvas.enabled = false;

        yield return new WaitForEndOfFrame();

        if (capturedTexture != null)
        {
            Destroy(capturedTexture);
            capturedTexture = null;
        }

        capturedTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        capturedTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        capturedTexture.Apply();

        if (uiCanvas != null)
            uiCanvas.enabled = restoreCanvas;

        if (previewImage != null)
            previewImage.texture = capturedTexture;

        if (previewPanel != null)
            previewPanel.SetActive(true);
    }

    public void SavePhoto()
    {
        if (capturedTexture == null)
        {
            Debug.LogWarning("没有可保存的截图。");
            return;
        }

        StartCoroutine(SaveCapturedPhotoRoutine());
    }

    private IEnumerator SaveCapturedPhotoRoutine()
    {
        byte[] pngBytes = capturedTexture.EncodeToPNG();
        string fileName = "Campus_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";

        bool success = false;
        bool done = false;
        string savedLocation = null;
        string errorMessage = null;

#if UNITY_ANDROID && !UNITY_EDITOR
        bool permissionReady = false;
        bool permissionGranted = false;

        yield return StartCoroutine(RequestLegacyStoragePermission(functionResult =>
        {
            permissionGranted = functionResult;
            permissionReady = true;
        }));

        if (!permissionReady || !permissionGranted)
        {
            Debug.LogWarning("未获得存储权限，取消保存。");
            yield break;
        }

        // Prefer NativeGallery on Android for broader compatibility across devices/ROMs.
        bool nativeDone = false;
        bool nativeOk = false;
        string nativePath = null;
        NativeGallery.SaveImageToGallery(pngBytes, albumName, fileName, (ok, path) =>
        {
            nativeOk = ok;
            nativePath = path;
            nativeDone = true;
        });

        yield return new WaitUntil(() => nativeDone);

        if (nativeOk)
        {
            success = true;
            done = true;
            savedLocation = nativePath;
        }
        else
        {
            // Fallback to original MediaStore save path.
            SavePngToAndroidGallery(pngBytes, fileName, albumName, (ok, location, err) =>
            {
                success = ok;
                savedLocation = location;
                errorMessage = err;
                done = true;
            });

            yield return new WaitUntil(() => done);
        }
#else
        try
        {
            string dir = Path.Combine(Application.persistentDataPath, "CapturePhotos");
            Directory.CreateDirectory(dir);

            savedLocation = Path.Combine(dir, fileName);
            File.WriteAllBytes(savedLocation, pngBytes);

            success = true;
            done = true;
        }
        catch (Exception e)
        {
            success = false;
            done = true;
            errorMessage = e.ToString();
        }
#endif

        if (!success)
        {
            Debug.LogError("保存图片失败: " + errorMessage);
            yield break;
        }

        Debug.Log("图片已保存: " + savedLocation);

        // Android 10+ 下这里通常拿到的是 content:// URI，而不是传统绝对路径。
        try
        {
            TitleEventReporter.ReportSavedPhoto(savedLocation);
        }
        catch (Exception e)
        {
            Debug.LogWarning("TitleEventReporter 调用失败: " + e.Message);
        }

        if (hidePreviewAfterSave && previewPanel != null)
            previewPanel.SetActive(false);

        try
        {
            TaskEventRuntime.ReportPhotoCheckinFromCurrentLandmark();
        }
        catch (Exception e)
        {
            Debug.LogWarning("TaskEventRuntime 调用失败: " + e.Message);
        }
    }

    /// <summary>
    /// 保留原先“静默保存到指定路径”的逻辑。
    /// 这个方法是保存到应用指定路径，不是系统相册。
    /// </summary>
    public void CaptureAndSaveSilently(string absolutePath, Action<Sprite> onComplete = null)
    {
        StartCoroutine(CaptureAndSaveSilentlyRoutine(absolutePath, onComplete));
    }

    private IEnumerator CaptureAndSaveSilentlyRoutine(string absolutePath, Action<Sprite> onComplete)
    {
        if (string.IsNullOrWhiteSpace(absolutePath))
        {
            onComplete?.Invoke(null);
            yield break;
        }

        bool restoreCanvas = uiCanvas != null && uiCanvas.enabled;
        if (uiCanvas != null)
            uiCanvas.enabled = false;

        yield return new WaitForEndOfFrame();

        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        texture.Apply();

        if (uiCanvas != null)
            uiCanvas.enabled = restoreCanvas;

        byte[] bytes = texture.EncodeToPNG();

        string directory = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllBytes(absolutePath, bytes);

        try
        {
            TitleEventReporter.ReportSavedPhoto(absolutePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning("TitleEventReporter 调用失败: " + e.Message);
        }

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

        onComplete?.Invoke(sprite);
    }

    public void CancelPhoto()
    {
        if (previewPanel != null)
            previewPanel.SetActive(false);
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private IEnumerator RequestLegacyStoragePermission(Action<bool> onResult)
    {
        int sdkInt = GetAndroidSdkInt();

        // Android 10+ 走 MediaStore 保存图片，一般不需要 WRITE_EXTERNAL_STORAGE。
        if (sdkInt >= 29)
        {
            onResult?.Invoke(true);
            yield break;
        }

        if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            onResult?.Invoke(true);
            yield break;
        }

        bool finished = false;
        bool granted = false;

        PermissionCallbacks callbacks = new PermissionCallbacks();
        callbacks.PermissionGranted += permissionName =>
        {
            granted = true;
            finished = true;
        };
        callbacks.PermissionDenied += permissionName =>
        {
            granted = false;
            finished = true;
        };
        callbacks.PermissionDeniedAndDontAskAgain += permissionName =>
        {
            granted = false;
            finished = true;
        };

        Permission.RequestUserPermission(Permission.ExternalStorageWrite, callbacks);

        float timeout = 15f;
        while (!finished && timeout > 0f)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        onResult?.Invoke(finished && granted);
    }

    private int GetAndroidSdkInt()
    {
        using (AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            return version.GetStatic<int>("SDK_INT");
        }
    }

    private void SavePngToAndroidGallery(byte[] pngBytes, string fileName, string targetAlbumName, Action<bool, string, string> onComplete)
    {
        try
        {
            int sdkInt = GetAndroidSdkInt();
            const string mimeType = "image/png";

            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject resolver = activity.Call<AndroidJavaObject>("getContentResolver"))
            {
                if (sdkInt >= 29)
                {
                    using (AndroidJavaObject values = new AndroidJavaObject("android.content.ContentValues"))
                    using (AndroidJavaClass mediaClass = new AndroidJavaClass("android.provider.MediaStore$Images$Media"))
                    {
                        values.Call("put", "display_name", fileName);
                        values.Call("put", "mime_type", mimeType);
                        values.Call("put", "relative_path", "Pictures/" + targetAlbumName);
                        values.Call("put", "is_pending", 1);

                        AndroidJavaObject collection = mediaClass.GetStatic<AndroidJavaObject>("EXTERNAL_CONTENT_URI");
                        AndroidJavaObject itemUri = resolver.Call<AndroidJavaObject>("insert", collection, values);

                        if (itemUri == null)
                        {
                            onComplete?.Invoke(false, null, "MediaStore insert 返回 null");
                            return;
                        }

                        using (AndroidJavaObject outputStream = resolver.Call<AndroidJavaObject>("openOutputStream", itemUri))
                        {
                            if (outputStream == null)
                            {
                                onComplete?.Invoke(false, null, "openOutputStream 返回 null");
                                return;
                            }

                            outputStream.Call("write", pngBytes);
                            outputStream.Call("flush");
                            outputStream.Call("close");
                        }

                        using (AndroidJavaObject finishValues = new AndroidJavaObject("android.content.ContentValues"))
                        {
                            finishValues.Call("put", "is_pending", 0);
                            resolver.Call<int>("update", itemUri, finishValues, null, null);
                        }

                        string uriText = itemUri.Call<string>("toString");
                        onComplete?.Invoke(true, uriText, null);
                    }
                }
                else
                {
                    using (AndroidJavaClass env = new AndroidJavaClass("android.os.Environment"))
                    using (AndroidJavaObject picturesDir = env.CallStatic<AndroidJavaObject>(
                        "getExternalStoragePublicDirectory",
                        env.GetStatic<string>("DIRECTORY_PICTURES")))
                    {
                        string picturesPath = picturesDir.Call<string>("getAbsolutePath");
                        string albumPath = Path.Combine(picturesPath, targetAlbumName);
                        Directory.CreateDirectory(albumPath);

                        string fullPath = Path.Combine(albumPath, fileName);
                        File.WriteAllBytes(fullPath, pngBytes);

                        using (AndroidJavaClass mediaScanner = new AndroidJavaClass("android.media.MediaScannerConnection"))
                        {
                            mediaScanner.CallStatic(
                                "scanFile",
                                activity,
                                new string[] { fullPath },
                                new string[] { mimeType },
                                null
                            );
                        }

                        onComplete?.Invoke(true, fullPath, null);
                    }
                }
            }
        }
        catch (Exception e)
        {
            onComplete?.Invoke(false, null, e.ToString());
        }
    }
#endif
}
