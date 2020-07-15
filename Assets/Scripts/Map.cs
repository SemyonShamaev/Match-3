using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    private bool drag = false;

    private Camera cam;
    private Vector3 initialTouchPosition;
    private Vector3 initialCameraPosition;

    void Start()
    {
        cam = GetComponent<Camera>();
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

    public void levelPointPressed(int n)
    {
        Debug.Log("Pressed");
        SceneManager.LoadScene("Game");
    }
}
