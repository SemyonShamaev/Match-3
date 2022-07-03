using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    private bool drag = false;

    private Camera cam;

    private Vector3 initialTouchPosition;
    private Vector3 initialCameraPosition;

    private int passedLevels;
    private string key;

    public static int levelCount;

    public GameObject[] levels; 
    public Sprite[] levelImages;

    void Start()
    {
        cam = GetComponent<Camera>();
        passedLevels = PlayerPrefs.GetInt("passedLevels");

        foreach(GameObject level in levels)
        {   
            if(passedLevels > 0) level.GetComponent<Image>().sprite = levelImages[2];
            if(passedLevels == 0) level.GetComponent<Image>().sprite = levelImages[1];
            if(passedLevels < 0) break;

            passedLevels--;
            level.GetComponent<Button>().interactable = true;
        }
    }

    void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch touch0 = Input.GetTouch(0);
            if (IsTouching(touch0))
            {
            	if (!drag)
            	{
                    initialTouchPosition = touch0.position;
                    initialCameraPosition = this.transform.position;

                    drag = true;
                }
                else
                {
                    Vector2 delta = cam.ScreenToWorldPoint(touch0.position) - 
                                    cam.ScreenToWorldPoint(initialTouchPosition);

                    Vector3 newPos = initialCameraPosition;
                    newPos.y -= delta.y;

                    newPos.y = (newPos.y < 0) ? 0 : 
                    (newPos.y > 69) ? 69 : newPos.y;      
                    
                    this.transform.position = newPos;
                }
            }
            if (!IsTouching(touch0))
                drag = false;
        }
        else
            drag = false;
    }

    private static bool IsTouching(Touch touch)
    {
        return touch.phase == TouchPhase.Began ||
               touch.phase == TouchPhase.Moved ||
               touch.phase == TouchPhase.Stationary;
    }

    public void levelPointPressed(int btnNumber)
    {
        levelCount = btnNumber;
        SceneManager.LoadScene("Game");
    }
}