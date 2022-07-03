using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class Sliders : MonoBehaviour
{
    public Slider slid;
    public AudioMixer masterMixer;
    string key;

    public void setSettings()
    {
        key = slid.name;
        if (PlayerPrefs.HasKey(key))
        { 
            if (key == "MusicSlider")
            {
                slid.value = PlayerPrefs.GetFloat(key);
                masterMixer.SetFloat("VolumeMusic", Mathf.Lerp(-80, 0, slid.value));
            }
            else
            {
                slid.value = PlayerPrefs.GetFloat(key);
                masterMixer.SetFloat("VolumeEffects", Mathf.Lerp(-80, 0, slid.value));
            }
        }
    }

    public void ChangeVolumeEffects()
    {
    	masterMixer.SetFloat("VolumeEffects", Mathf.Lerp(-80, 0, slid.value));
    }

    public void changeVolumeMusic()
    {
    	masterMixer.SetFloat("VolumeMusic",  Mathf.Lerp(-80, 0, slid.value));
    }

    public void Save()
    {
        PlayerPrefs.SetFloat(key, this.slid.value);
        PlayerPrefs.Save();
    }
}