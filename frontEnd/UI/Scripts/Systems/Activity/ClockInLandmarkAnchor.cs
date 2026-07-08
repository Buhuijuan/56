using UnityEngine;

public class ClockInLandmarkAnchor : MonoBehaviour
{
    public string locationId;
    public string displayName;

    public bool Matches(string targetLocationId)
    {
        return !string.IsNullOrEmpty(locationId) && locationId == targetLocationId;
    }
}
