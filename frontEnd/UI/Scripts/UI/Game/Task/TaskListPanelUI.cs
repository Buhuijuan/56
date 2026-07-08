using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskListPanelUI : MonoBehaviour
{
    public Toggle toggle;
    public Image icon;

    public Sprite expandedSprite;
    public Sprite collapsedSprite;

    public Transform listRoot;
    public TaskListItemUI listItemPrefab;

    private readonly List<TaskListItemUI> generatedItems = new();

    private void Start()
    {
        toggle.onValueChanged.AddListener(OnTaskListToggleValueChanged);
        OnTaskListToggleValueChanged(toggle.isOn);
    }

    private void OnEnable()
    {
        Refresh();
    }

    // -------------------------------
    //  纯后端：只从 TaskDisplaySystem 获取任务
    // -------------------------------
    public void Refresh()
    {
        Clear();

        if (listRoot == null || listItemPrefab == null)
        {
            Debug.LogWarning($"[TaskListPanelUI] Refresh aborted. listRootNull={listRoot == null}, listItemPrefabNull={listItemPrefab == null}, PanelActive={gameObject.activeInHierarchy}");
            return;
        }

        List<TaskViewData> tasks = TaskDisplaySystem.GetTasks();
        Debug.Log($"[TaskListPanelUI] Refresh start. TaskCount={tasks.Count}, PanelActive={gameObject.activeInHierarchy}, Toggle={(toggle != null ? toggle.isOn.ToString() : "null")}");

        foreach (TaskViewData task in tasks)
        {
            Debug.Log($"[TaskListPanelUI] Create item. TaskCode={task.taskCode}, Title={task.title}, GoalCount={task.goals?.Count ?? 0}");
            TaskListItemUI item = Instantiate(listItemPrefab, listRoot);
            item.Setup(task, this);
            generatedItems.Add(item);
        }

        Debug.Log($"[TaskListPanelUI] Refresh end. GeneratedCount={generatedItems.Count}, ChildCount={listRoot.childCount}");
    }

    public void OnTaskListToggleValueChanged(bool isChecked)
    {
        icon.sprite = isChecked ? expandedSprite : collapsedSprite;
        gameObject.SetActive(isChecked);
    }

    public void Clear()
    {
        foreach (TaskListItemUI item in generatedItems)
            Destroy(item.gameObject);

        generatedItems.Clear();
    }
}
