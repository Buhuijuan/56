using UnityEngine;

public class TitlePetInteractionTracker : MonoBehaviour
{
    public void RecordInteraction()
    {
        TitleProgressSystem.RecordPetInteraction();
    }

    private void OnMouseDown()
    {
        RecordInteraction();
    }
}
