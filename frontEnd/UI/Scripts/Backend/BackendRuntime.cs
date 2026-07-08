using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackendRuntime : MonoBehaviour
{
    private static BackendRuntime instance;
    private Coroutine heartbeatRoutine;
    private Coroutine sceneSyncRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot()
    {
        if (instance != null)
            return;

        GameObject go = new GameObject("BackendRuntime");
        instance = go.AddComponent<BackendRuntime>();
        DontDestroyOnLoad(go);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public static void Run(IEnumerator routine)
    {
        if (instance == null)
            Boot();

        instance.StartCoroutine(routine);
    }

    public static void HandleUnauthorized()
    {
        SessionStore.Clear();
        GameStateStore.Clear();
        TaskPromptSystem.ClearPending();

        if (SceneManager.GetActiveScene().name != "02_Login")
            SceneManager.LoadScene("02_Login");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool inGame = scene.name == "05_Game" || scene.name == "05_Campus";
        if (inGame && BackendSettings.UseBackendMode && SessionStore.HasToken)
        {
            if (heartbeatRoutine == null)
                heartbeatRoutine = StartCoroutine(HeartbeatLoop());

            if (sceneSyncRoutine == null)
                sceneSyncRoutine = StartCoroutine(SyncGameSceneState());
        }
        else
        {
            if (heartbeatRoutine != null)
            {
                StopCoroutine(heartbeatRoutine);
                heartbeatRoutine = null;
            }

            if (sceneSyncRoutine != null)
            {
                StopCoroutine(sceneSyncRoutine);
                sceneSyncRoutine = null;
            }
        }
    }

    private IEnumerator SyncGameSceneState()
    {
        if (!SessionStore.Current.hasCurrentRole)
        {
            sceneSyncRoutine = null;
            yield break;
        }

        TaskPromptSystem.ClearPending();
        BackendFacade.BeginSuppressElfPrompt();

        yield return BackendFacade.RefreshHome(null);

        yield return BackendFacade.RefreshTasks(null);
        yield return BackendFacade.RefreshSignIn(null);
        yield return BackendFacade.RefreshCurrentMainTask(null);
        yield return BackendFacade.RefreshTaskChapters(null);
        yield return BackendFacade.RefreshClockIn(null);
        yield return BackendFacade.RefreshGrowth(null);

        BackendFacade.EndSuppressElfPrompt();

        // Re-fetch the current main task once after the loading scene is gone so
        // only the latest in-progress task can enqueue its prompt.
        yield return BackendFacade.RefreshCurrentMainTask(null, true);
        TaskRewardPromptSystem.RefreshVisibleTaskUIs();

        sceneSyncRoutine = null;
    }

    private IEnumerator HeartbeatLoop()
    {
        while (BackendSettings.UseBackendMode && SessionStore.HasToken)
        {
            yield return new WaitForSeconds(30f);

            bool shouldSend = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                string sceneName = SceneManager.GetSceneAt(i).name;
                if (sceneName == "05_Game" || sceneName == "05_Campus")
                {
                    shouldSend = true;
                    break;
                }
            }

            if (!shouldSend)
                continue;

            yield return BackendFacade.Heartbeat(30, null);
        }

        heartbeatRoutine = null;
    }
}
