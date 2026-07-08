using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CharacterPreviewController : MonoBehaviour
{
    public RawImage previewImage;
    public VideoPlayer videoPlayer;

    public void ShowCharacter(CharacterData data)
    {
        if (data.characterVideo == null)
        {
            Debug.LogWarning("角色没有视频：" + data.characterID);
            return;
        }

        videoPlayer.clip = data.characterVideo;
        videoPlayer.isLooping = true;
        videoPlayer.Play();
    }
}
