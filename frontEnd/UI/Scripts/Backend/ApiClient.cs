using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public static class ApiClient
{
    public static IEnumerator Get<T>(string path, Action<BackendApiResult<T>> onComplete) where T : class
    {
        return Send<T>("GET", path, null, onComplete);
    }

    public static IEnumerator Post<T>(string path, object body, Action<BackendApiResult<T>> onComplete) where T : class
    {
        return Send<T>("POST", path, body, onComplete);
    }

    public static IEnumerator Put<T>(string path, object body, Action<BackendApiResult<T>> onComplete) where T : class
    {
        return Send<T>("PUT", path, body, onComplete);
    }

    public static IEnumerator Delete<T>(string path, Action<BackendApiResult<T>> onComplete) where T : class
    {
        return Send<T>("DELETE", path, null, onComplete);
    }

    public static IEnumerator PostMultipart<T>(
        string path,
        string fieldName,
        byte[] fileBytes,
        string fileName,
        string mimeType,
        Action<BackendApiResult<T>> onComplete) where T : class
    {
        string url = $"{BackendSettings.BaseUrl}{path}";
        List<IMultipartFormSection> formSections = new()
        {
            new MultipartFormFileSection(fieldName, fileBytes, fileName, mimeType)
        };

        using UnityWebRequest request = UnityWebRequest.Post(url, formSections);
        request.downloadHandler = new DownloadHandlerBuffer();

        if (SessionStore.HasToken)
            request.SetRequestHeader("Authorization", $"Bearer {SessionStore.Current.token}");

        yield return request.SendWebRequest();
        onComplete?.Invoke(ParseResponse<T>(request));
    }

    private static IEnumerator Send<T>(string method, string path, object body, Action<BackendApiResult<T>> onComplete) where T : class
    {
        string url = $"{BackendSettings.BaseUrl}{path}";
        using UnityWebRequest request = new UnityWebRequest(url, method);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        if (SessionStore.HasToken)
            request.SetRequestHeader("Authorization", $"Bearer {SessionStore.Current.token}");

        if (body != null)
        {
            string json = JsonUtility.ToJson(body);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        }

        yield return request.SendWebRequest();
        onComplete?.Invoke(ParseResponse<T>(request));
    }

    private static BackendApiResult<T> ParseResponse<T>(UnityWebRequest request) where T : class
    {
        var result = new BackendApiResult<T>
        {
            StatusCode = (int)request.responseCode
        };

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (typeof(T) == typeof(string))
            {
                result.Success = true;
                result.Data = request.downloadHandler.text as T;
            }
            else
            {
                try
                {
                    result.Data = JsonUtility.FromJson<T>(request.downloadHandler.text);
                    result.Success = true;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = $"Failed to parse response: {ex.Message}";
                }
            }
        }
        else
        {
            result.Success = false;
            result.Message = ExtractError(request.downloadHandler != null ? request.downloadHandler.text : null, request.error);
            if (request.responseCode == 401 || request.responseCode == 403)
                BackendRuntime.HandleUnauthorized();
        }

        return result;
    }

    private static string ExtractError(string body, string fallback)
    {
        if (!string.IsNullOrEmpty(body))
        {
            try
            {
                BackendApiErrorEnvelope error = JsonUtility.FromJson<BackendApiErrorEnvelope>(body);
                if (error != null && !string.IsNullOrEmpty(error.message))
                    return error.message;
            }
            catch
            {
            }
        }
        return string.IsNullOrEmpty(fallback) ? "Request failed" : fallback;
    }
}
