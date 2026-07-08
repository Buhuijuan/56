using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class PersistentBootstrap : MonoBehaviour
{
    public AudioMixer mixer;
    public AudioSource SFXPlayerAudioSource;
    public AudioSource BGMPlayerAudioSource;
    private void Start()
    {
        SettingSystem.Instance.Load();
        PictureManager.Init();
        AudioManager.Init(mixer);
        SFXManager.Init(SFXPlayerAudioSource);
        BGMManager.Init(BGMPlayerAudioSource);

        GameSceneManager.Instance.SwitchScene("01_Launch");
    }
}
