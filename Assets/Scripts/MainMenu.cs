using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void OpenMapScene()
    {
        SceneManager.LoadScene("Map");
    }

    public void QuitGame()
    {
    	Application.Quit();
    }
}