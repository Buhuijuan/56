using UnityEngine;

public static class SFXManager
{
    private static AudioSource source;

    public static void Init(AudioSource s)
    {
        source = s;
    }

    public static void Play(AudioClip clip)
    {
        if (clip != null)
            source.PlayOneShot(clip);
    }
}
