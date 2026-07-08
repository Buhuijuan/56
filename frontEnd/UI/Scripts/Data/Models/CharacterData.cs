using System;
using UnityEngine;
using UnityEngine.Video;

[Serializable]
public class CharacterData
{
    public int characterID;
    public string characterImagePath;
    public string characterPrefabPath;
    public string characterVideoPath;

    [NonSerialized] public Sprite characterImage;
    [NonSerialized] public GameObject characterPrefab;
    [NonSerialized] public VideoClip characterVideo;

    public void LoadAssets()
    {
        characterImage = Resources.Load<Sprite>(characterImagePath);
        characterPrefab = Resources.Load<GameObject>(characterPrefabPath);
        characterVideo = Resources.Load<VideoClip>(characterVideoPath);
    }

}
