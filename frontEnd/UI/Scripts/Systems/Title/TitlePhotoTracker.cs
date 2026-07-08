using UnityEngine;

public class TitlePhotoTracker : MonoBehaviour
{
    public string photoId;

    public void RecordSavedPhoto()
    {
        string resolvedPhotoId = string.IsNullOrWhiteSpace(photoId) ? $"{gameObject.name}_{System.DateTime.UtcNow.Ticks}" : photoId;
        TitleProgressSystem.RecordSavedPhoto(resolvedPhotoId);
    }
}
