using UnityEngine;

public class TaskSkinLibrary : MonoBehaviour
{
    public TaskListItemSkin mainListSkin;
    public TaskListItemSkin branchListSkin;
    public TaskChapterItemSkin mainChapterSkin;
    public TaskChapterItemSkin branchChapterSkin;
    public static TaskSkinLibrary Instance { get; private set; }
    void Awake()
    {
        Instance = this;
    }
}
