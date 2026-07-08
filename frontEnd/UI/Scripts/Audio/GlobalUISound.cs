using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GlobalUISound : MonoBehaviour
{
    public AudioClip clickSound;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddSoundToAllUI();
    }

    void AddSoundToAllUI()
    {
        // 按钮
        foreach (var btn in FindObjectsOfType<Button>(true))
        {
            btn.onClick.AddListener(() =>
            {
                SFXManager.Play(clickSound);
            });
        }

        // Toggle
        foreach (var toggle in FindObjectsOfType<Toggle>(true))
        {
            toggle.onValueChanged.AddListener((value) =>
            {
                SFXManager.Play(clickSound);
            });
        }
    }
}
