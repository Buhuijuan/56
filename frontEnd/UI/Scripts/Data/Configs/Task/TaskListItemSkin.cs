using UnityEngine;
[CreateAssetMenu(menuName = "Task/TaskListItemSkin")]
public class TaskListItemSkin : ScriptableObject
{
    public Sprite background;
    public string typeLabel;
    public Color labelColor;
    public Sprite sliderBG;

    public Sprite sliderFill;
}
