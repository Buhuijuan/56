using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance;

    private string currentUIScene = "";
    private string currentWorldScene = "";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    // 切换到单场景
    public void SwitchScene(string sceneName)
    {
        StartCoroutine(SwitchSceneFlow(sceneName));
    }

    private IEnumerator SwitchSceneFlow(string sceneName)
    {
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        if (!string.IsNullOrEmpty(currentUIScene))
            yield return SceneManager.UnloadSceneAsync(currentUIScene);

        currentUIScene = sceneName;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
    }

    public void SwitchToGame(string uiScene, string worldScene)
    {
        StartCoroutine(LoadGameFlow(uiScene, worldScene));
    }

    private IEnumerator LoadGameFlow(string uiScene, string worldScene)
    {
        yield return SceneManager.LoadSceneAsync("99_Loading", LoadSceneMode.Additive);

        if (!string.IsNullOrEmpty(currentUIScene))
            yield return SceneManager.UnloadSceneAsync(currentUIScene);

        if (!string.IsNullOrEmpty(currentWorldScene))
            yield return SceneManager.UnloadSceneAsync(currentWorldScene);

        LoadingController loader = FindObjectOfType<LoadingController>();
        loader.mode = LoadingController.LoadingMode.EnterGame;

        yield return loader.LoadScenes(uiScene, worldScene);

        yield return SceneManager.UnloadSceneAsync("99_Loading");
        TaskPromptSystem.NotifyUiAvailabilityChanged();

        currentUIScene = uiScene;
        currentWorldScene = worldScene;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(uiScene));
    }
    public void ReturnScene(string sceneName)
    {
        StartCoroutine(ReturnSceneFlow(sceneName));
    }

    private IEnumerator ReturnSceneFlow(string sceneName)
    {
        yield return SceneManager.LoadSceneAsync("99_Loading", LoadSceneMode.Additive);
        LoadingController loader = FindObjectOfType<LoadingController>();
        loader.mode = LoadingController.LoadingMode.ReturnToMenu;

        if (!string.IsNullOrEmpty(currentUIScene))
            yield return SceneManager.UnloadSceneAsync(currentUIScene);

        if (!string.IsNullOrEmpty(currentWorldScene))
            yield return SceneManager.UnloadSceneAsync(currentWorldScene);

        yield return loader.LoadScenes(sceneName, null);
        yield return SceneManager.UnloadSceneAsync("99_Loading");
        TaskPromptSystem.NotifyUiAvailabilityChanged();

        currentUIScene = sceneName;
        currentWorldScene = "";
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TitleEventReporter.ReportSceneEntered(scene.name);
    }

}
