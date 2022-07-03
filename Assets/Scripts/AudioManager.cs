using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private AudioMixer mixer;
    private static AudioManager _instance;

    public static AudioManager Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            return new GameObject("(singleton) SoundManager").AddComponent<AudioManager>();
        }
    }

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        mixer =  Resources.Load<AudioMixer>("Mixer");
    }

    public void PlayEffects(AudioClip clip)
    { 
        GameObject effect = new GameObject("Effect: " + clip.name);
        AudioSource effectsSource = effect.AddComponent<AudioSource>();

        effect.transform.SetParent(transform);
        effectsSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Master/Effects")[0];
        effectsSource.loop = false;
        effectsSource.priority = 0;
        effectsSource.playOnAwake = false;
        effectsSource.ignoreListenerPause = true;
        effectsSource.clip = clip;
        effectsSource.Play();
    }

    public void PlayMusic(AudioClip clip)
    { 
        GameObject music = new GameObject("Music: " + clip.name);
        AudioSource musicSource = music.AddComponent<AudioSource>();

        music.transform.SetParent(transform);

        musicSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Master/Music")[0];
        musicSource.loop = true;
        musicSource.priority = 0;
        musicSource.playOnAwake = false;
        musicSource.ignoreListenerPause = true;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PauseMusic()
    {
        AudioListener.pause = true;
    }

    public void RestoreMusic()
    {
        AudioListener.pause = false;
    }
}
