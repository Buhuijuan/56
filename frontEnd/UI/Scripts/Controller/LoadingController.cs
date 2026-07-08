using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LoadingController : MonoBehaviour
{
    public List<string> slogans = new()
    {
        "方寸之间，心安于此",
        "心之所安，即是此间",
        "世界很大，先从此间出发",
        "在此间，遇见更好的自己",
        "你的故事，从此间开始",
        "方寸之间，自有天地"
    };

    public enum LoadingMode
    {
        EnterGame,
        ReturnToMenu
    }
    public LoadingMode mode = LoadingMode.EnterGame;
    public TMP_Text sloganText;
    public Slider slider;
    public TMP_Text value;
    public TMP_Text tipText;
    private bool tipsRunning = true;

    private readonly string[] enterGameTips = new string[]
    {
    "正在加载校园环境，请稍候…",
    "小贴士：你可以在设置中调整音量",
    "正在初始化场景，请耐心等待…",
    "正在加载建筑模型…",
    "正在准备交互系统…",
    "正在优化渲染资源…"
    };

    private readonly string[] returnMenuTips = new string[]
    {
    "正在保存当前进度…",
    "正在清理场景资源…",
    "正在返回主菜单…",
    "小贴士：你可以在菜单中更换角色形象",
    "正在卸载校园场景…"
    };

    private void Start()
    {
        if (slogans != null && slogans.Count > 0)
        {
            int index = Random.Range(0, slogans.Count);
            sloganText.text = slogans[index];
        }
        StartCoroutine(TipLoop());
    }

    private IEnumerator TipLoop()
    {
        int index = 0;
        string[] tips = mode == LoadingMode.EnterGame ? enterGameTips : returnMenuTips;

        while (tipsRunning)
        {
            tipText.text = tips[index];
            index = (index + 1) % tips.Length;

            yield return new WaitForSeconds(1f);
        }
    }


    public IEnumerator LoadScenes(string uiScene, string worldScene)
    {
        AsyncOperation opUI = SceneManager.LoadSceneAsync(uiScene, LoadSceneMode.Additive);
        AsyncOperation opWorld = null;

        if (!string.IsNullOrEmpty(worldScene))
            opWorld = SceneManager.LoadSceneAsync(worldScene, LoadSceneMode.Additive);

        opUI.allowSceneActivation = true;
        if (opWorld != null) opWorld.allowSceneActivation = true;

        float fakeProgress = 0f;
        float targetProgress = 0f;

        while (fakeProgress < 0.3f)
        {
            fakeProgress += Time.deltaTime * 0.2f;
            slider.value = fakeProgress;
            value.text = (fakeProgress * 100f).ToString("F0") + "%";
            yield return null;
        }

        while (!opUI.isDone || (opWorld != null && !opWorld.isDone))
        {
            float realProgress = opUI.progress;
            if (opWorld != null) realProgress = (opUI.progress + opWorld.progress) * 0.5f;

            targetProgress = 0.3f + realProgress / 0.9f * 0.6f;

            fakeProgress = Mathf.MoveTowards(fakeProgress, targetProgress, Time.deltaTime * 0.5f);

            slider.value = fakeProgress;
            value.text = (fakeProgress * 100f).ToString("F0") + "%";

            yield return null;
        }

        while (fakeProgress < 1f)
        {
            fakeProgress = Mathf.MoveTowards(fakeProgress, 1f, Time.deltaTime * 0.5f);
            slider.value = fakeProgress;
            value.text = (fakeProgress * 100f).ToString("F0") + "%";
            yield return null;
        }

        value.text = "点击屏幕以继续";
        tipsRunning = false;
        tipText.text = "";

        yield return new WaitUntil(() => Input.anyKeyDown);

        UIManager.Instance.HideLoading();
    }

}
