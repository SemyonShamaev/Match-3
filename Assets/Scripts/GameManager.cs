using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text StopWatch;
    public Text moves;

    public Image[] GoalType;
    public Text[] GoalCount;
    public Sprite[] SweetImages;
    public Sprite[] endImages;

    public GameObject PausePnl;
    public GameObject EndGamePnl;
    public GameObject GameGrid;
    public GameObject SwipeThere;

    public AudioClip WinSound;
    public AudioClip LoseSound;

    public Image endImage;
    
    private int timerCount;
    private int movesCount;

    private int[] goalType;
    private int[] goalCount;

    private int levelCount;
    
    private Requirements[] req;
   
    private GameGrid gameGrid;

    private bool StopTimer;

    void Start()
    {
        gameGrid = GameGrid.GetComponent<GameGrid>();
        levelCount = Map.levelCount - 1;
        addUI();

        StopWatch.text = timerCount/60 + ":" + timerCount % 60;
        if(timerCount % 60 == 0) StopWatch.text += "0";
        StartCoroutine(Timer());
        StartCoroutine(DownTimer());
    }

    private void addUI()
    {
        TextAsset jsonString = (TextAsset) Resources.Load("Levels");
        req = FromJson<Requirements>(jsonString.text);
        timerCount = req[levelCount].time;
        movesCount = req[levelCount].moves;

        goalType = req[levelCount].goalType;
        goalCount = req[levelCount].goalCount;

        for(int i = 0; i < 4; i++)
        {
            GoalType[i].GetComponent<Image>().sprite = SweetImages[goalType[i]];
            GoalCount[i].GetComponent<Text>().text = goalCount[i].ToString();
        }

        moves.text = movesCount.ToString();
    }

    IEnumerator Timer()
    {
		yield return new WaitForSeconds(1);

        timerCount--;
        StopWatch.text = timerCount/60 + ":" + timerCount % 60;
        if(timerCount % 60 == 0) StopWatch.text += "0";
        else if(timerCount % 60 < 10) StopWatch.text = timerCount/60 + ":" + "0" + timerCount % 60;

        if(timerCount > 0) StartCoroutine(Timer());
        else EndGame(false);
    }

    public void addSweet(int sweetType)
    {
        for(int i = 0; i < goalType.Length; i++)
        {
            if(sweetType == goalType[i])
            {
                goalCount[i]--;
                if(goalCount[i] >= 0) GoalCount[i].text = goalCount[i].ToString();
            }
        }
        
        if(goalCount[0] <= 0 && goalCount[1] <= 0 && goalCount[2] <= 0 && goalCount[3] <= 0) EndGame(true);
    }

    public void TakeMove()
    {
        movesCount--;
        moves.text = movesCount.ToString();

        SwipeThere.SetActive(false);
        
        StopTimer = true;

        if(movesCount <= 0) EndGame(false);
    }

    IEnumerator DownTimer()
    {  
        yield return new WaitForSeconds(10);    
        if(!StopTimer)
        {
            SwipeThere.SetActive(true);
            SwipeThere.transform.position = gameGrid.FindPlaceToSwipe();
        }
        else StopTimer = false;
        StartCoroutine(DownTimer());
    }

    private void EndGame(bool win)
    {
        Time.timeScale = 0;
        EndGamePnl.SetActive(true);

        if(win)
        {
            AudioManager.Instance.PlayEffects(WinSound);
            endImage.GetComponent<Image>().sprite = endImages[0];
            if(Map.levelCount >= PlayerPrefs.GetInt("passedLevels"))
            {
            PlayerPrefs.SetInt("passedLevels", Map.levelCount);
            PlayerPrefs.Save();
            }
        } 
        else AudioManager.Instance.PlayEffects(LoseSound);
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void NextGame()
    {
        Time.timeScale = 1;
    }

    public void QuitGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    public void ReGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Game");
    }
    
    public void ToMap()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Map");
    }

    [System.Serializable]
    private class Requirements
    {
        public int time;
        public int moves;
        public int[] goalType;
        public int[] goalCount;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }

    private T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }
}