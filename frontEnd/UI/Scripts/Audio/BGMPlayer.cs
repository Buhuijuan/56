using UnityEngine;

public static class BGMManager
{
    private static AudioSource source;

    public static void Init(AudioSource s)
    {
        source = s;
    }

    public static void Play(AudioClip clip)
    {
        if (clip != null)
        {
            source.clip = clip;
            source.Play();
        }
    }
}
