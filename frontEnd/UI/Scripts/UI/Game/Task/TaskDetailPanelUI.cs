using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TaskDetailPanelUI : MonoBehaviour
{
    public TMP_Text introduction;

    public Transform goalRoot;
    public TaskGoalItemUI goalItemPrafab;

    private readonly List<TaskGoalItemUI> generatedItems = new();

    // -------------------------------
    //  纯后端：只保留 TaskViewData
    // -------------------------------
    public void Refresh(TaskViewData data)
    {
        Clear();

        if (data == null)
            return;

        // 任务描述
        if (introduction != null)
            introduction.text = data.description;

        // 任务目标
        foreach (TaskGoalViewData goal in data.goals)
        {
            TaskGoalItemUI item = Instantiate(goalItemPrafab, goalRoot);
            item.Setup(goal, this);
            generatedItems.Add(item);
        }
    }

    public void Clear()
    {
        if (introduction != null)
            introduction.text = string.Empty;

        foreach (TaskGoalItemUI item in generatedItems)
            Destroy(item.gameObject);

        generatedItems.Clear();
    }
}
