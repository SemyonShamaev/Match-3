using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public AudioClip backgroundMusic;
    public GameObject SettingsPanel;

    public Slider MusicSlider, EffectsSlider;

    void Start()
    {
        if(GameObject.Find("Music: " + backgroundMusic.name) == null)
            AudioManager.Instance.PlayMusic(backgroundMusic);

        MusicSlider.GetComponent<Sliders>().setSettings(); 
        EffectsSlider.GetComponent<Sliders>().setSettings();
    }

    public void OpenMapScene()
    {
        SceneManager.LoadScene("Map");
    }

    public void openSettingsPanel()
    {
        SettingsPanel.SetActive(!SettingsPanel.activeSelf);
    }

    public void QuitGame()
    {
    	Application.Quit();
    }
}