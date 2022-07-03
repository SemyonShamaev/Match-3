using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameGrid : MonoBehaviour
{
    public enum cellType
    {
        IceCream,
        Candy,
        Cookie,
        Marmalade,
        Chocolate,
        Wafer,
        Empty,
        Deleted
    }

    private enum Direction
    {
        North,
        East,
        South,
        West
    }

    public GameObject gameObjShow;
    public GameObject Grid;
    public GameObject GameManager;

    public AudioClip Vanishing;
    public AudioClip Mix;

    public List<Sweet> sweets = new List<Sweet>();
    public List<GameObject> Objects = new List<GameObject>(); 

    private Level[] lvl;

    private bool drop;

    private const int width = 7, height = 8;
    private Cell[,] cells = new Cell[width, height];

    private Vector2 startPos;
    private Vector2 direction;
    private bool directionChosen;

    private bool swap, isChanged;
    private Direction dir;
    private int posX = - 1, posY;
    private Vector3 pos, _pos;
    private List<Vector2Int> deletePositions = new List<Vector2Int>();

    private GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.GetComponent<GameManager>();
        TextAsset jsonString = (TextAsset) Resources.Load("Levels");
        lvl = FromJson<Level>(jsonString.text);

        FillGrid(Map.levelCount - 1);
        UpdateGrid();
        if(deletePositions.Count == 0 && CheckGrid()) MixGrid(); 
    }  

    void Update()
    {
        if(drop)
        {
            DropCells();
            UpdateDeletePositions();
            if(deletePositions.Count == 0)
            {
                drop = false;
                UpdateGrid();      
                if(deletePositions.Count == 0 && CheckGrid()) MixGrid();
            }
        }

        else if(swap)
        {
            SwapCells();
        }

        else if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startPos = touch.position;
                    directionChosen = false;
                    break;

                case TouchPhase.Moved:
                    direction = touch.position - startPos;
                    break;

                case TouchPhase.Ended:
                    directionChosen = true;
                    break;
            }
        }

        if (directionChosen)
        {
            int xPos = 0, yPos = 0;
            float min = float.MaxValue;
            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    if(min > Vector2.Distance(startPos, cells[i, j].obj.transform.position))
                    {
                        min = Vector2.Distance(startPos, cells[i, j].obj.transform.position);
                        xPos = i; yPos = j;
                    }
                }
            }

            isChanged = false;
            FindDirForSwap(xPos, yPos, startPos, direction);
            directionChosen = false;
        }   
    }

    private void FillGrid(int levelCount)
    {
        for(int i = 0; i < width * height; i++)
        {
            Cell cell = new Cell();
            cell.obj = Objects[i];

            if(lvl[levelCount].grid[i] == 1)
            { 
                int type = Random.Range(0,5); cell.type = (cellType)type;
                cell.obj.GetComponent<Image>().sprite = sweets[type].image;
                cells[i % 7, i / 7] = cell;
            }
            else
            {        
                cell.type = (cellType)6;
                cells[i % 7, i / 7] = cell;
            }

            cells[i % 7, i / 7].xPos = i % 7;
            cells[i % 7, i / 7].yPos = i / 7;
            cells[i % 7, i / 7].pos = cells[i % 7, i / 7].obj.transform.position;
        }

        bool replaced = true;
        while(replaced)
        {
            replaced = false;
            int repeats = 1;
            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < height - 1; j++)
                {
                    if(cells[i, j].type == cells[i, j + 1].type && cells[i, j].type != cellType.Empty && cells[i, j].type != cellType.Deleted)
                    {
                        repeats++;
                        if(repeats > 2) 
                        {
                            int type = Random.Range(0,5);
                            cells[i, j].type = (cellType)type;
                            cells[i, j].obj.GetComponent<Image>().sprite = sweets[type].image;
                            replaced = true;
                            repeats = 1;
                        }
                    }
                    else repeats = 1;
                }
            }

            repeats = 1;
            for(int j = 0; j < height; j++)
            {
                for(int i = 0; i < width - 1; i++)
                {
                    if(cells[i, j].type == cells[i + 1, j].type && cells[i, j].type != cellType.Empty && cells[i, j].type != cellType.Deleted)
                    {
                        repeats++;
                        if(repeats > 2) 
                        {
                            int type = Random.Range(0,5);
                            cells[i, j].type = (cellType)type;
                            cells[i, j].obj.GetComponent<Image>().sprite = sweets[type].image;
                            replaced = true;
                            repeats = 1;
                        }
                    }
                    else repeats = 1;
                }
            }
        }
    }

    private void UpdateGrid()
    {
        int repeats = 1;
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height - 1; j++)
            {
                if(cells[i, j].type == cells[i, j + 1].type && cells[i, j].type != cellType.Empty && cells[i, j].type != cellType.Deleted)
                {
                    repeats++;
                    if(repeats > 2) 
                    {
                        if(j != 7)
                            deletePositions.Add(new Vector2Int(i, j + 1));
                        deletePositions.Add(new Vector2Int(i, j));
                        if(j != 0)
                            deletePositions.Add(new Vector2Int(i, j - 1));
                    }
                }
                else repeats = 1;
            }
        }

        repeats = 1;
        for(int j = 0; j < height; j++)
        {
            for(int i = 0; i < width - 1; i++)
            {
                if(cells[i, j].type == cells[i + 1, j].type && cells[i, j].type != cellType.Empty && cells[i, j].type != cellType.Deleted)
                {
                    repeats++;
                    if(repeats > 2) 
                    {
                        if(i != 6)
                            deletePositions.Add(new Vector2Int(i + 1, j));
                        deletePositions.Add(new Vector2Int(i, j));
                        if(i != 0)
                            deletePositions.Add(new Vector2Int(i - 1, j));
                    }
                }
                else repeats = 1;
            }
        }

        foreach(Vector2Int pos in deletePositions)
        {   
            gameManager.addSweet((int)cells[pos.x, pos.y].type); 
            if(drop == false)  AudioManager.Instance.PlayEffects(Vanishing);
            cells[pos.x, pos.y].obj.GetComponent<Image>().color = new Color(255, 255, 255, 0);
            cells[pos.x, pos.y].type = cellType.Deleted;
            drop = true;
        }
    }

    private void UpdateDeletePositions()
    {        
        deletePositions.Clear();
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(cells[i, j].type == cellType.Deleted) deletePositions.Add(new Vector2Int(i, j));
            }
        }
    }

    public Vector2 FindPlaceToSwipe()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(j - 1 > 0 && cells[i, j].type != cellType.Empty && cells[i, j - 1].type == cells[i, j].type)
                {
                    if(j - 2 > 0 && i - 1 > 0 && i + 1 < 7)
                    {
                        if(cells[i, j].type == cells[i + 1, j - 2].type)
                            return cells[i + 1, j - 2].obj.transform.position - new Vector3(70, 0);
                        if(cells[i, j].type == cells[i - 1, j - 2].type) 
                            return cells[i - 1, j - 2].obj.transform.position - new Vector3(-70, 0);
                    }
                }

                if(j + 1 < 8 && cells[i, j].type != cellType.Empty && cells[i, j + 1].type == cells[i, j].type)
                {
                    if(j + 2 < 8 && i - 1 > 0 && i + 1 < 7)
                    {
                        if(cells[i, j].type == cells[i + 1, j + 2].type)
                            return cells[i + 1, j + 2].obj.transform.position - new Vector3(70, 0);
                        if(cells[i, j].type == cells[i - 1, j + 2].type)
                            return cells[i - 1, j + 2].obj.transform.position - new Vector3(-70, 0);
                    }
                }

                if(i + 1 < 7 && cells[i, j].type != cellType.Empty && cells[i + 1, j].type == cells[i, j].type)
                {
                    if(j - 1 > 0 && i + 2 < 7 && j + 1 < 8)
                    {
                        if(cells[i, j].type == cells[i + 2, j - 1].type)
                            return cells[i + 2, j - 1].obj.transform.position - new Vector3(0, 70);
                        if(cells[i, j].type == cells[i + 2, j + 1].type)
                            return cells[i + 2, j + 1].obj.transform.position - new Vector3(0, -70);
                    }
                }

                if(i - 1 > 0 && cells[i, j].type != cellType.Empty && cells[i - 1, j].type == cells[i, j].type)
                {
                    if(j - 1 > 0 && i - 2 > 0 && j + 1 < 8)
                    {
                        if(cells[i, j].type == cells[i - 2, j - 1].type)
                            return cells[i - 2, j - 1].obj.transform.position - new Vector3(0, 70);
                        if(cells[i, j].type == cells[i - 2, j + 1].type)
                            return cells[i - 2, j + 1].obj.transform.position - new Vector3(0, -70);
                    }
                }

                if(j - 2 > 0 && cells[i, j].type != cellType.Empty && cells[i, j - 2].type == cells[i, j].type)
                {
                    if(j - 1 > 0 && i - 1 > 0 && i + 1 < 7)
                    {
                        if(cells[i, j].type == cells[i + 1, j - 1].type)
                            return cells[i + 1, j - 1].obj.transform.position - new Vector3(70, 0);
                        if(cells[i, j].type == cells[i - 1, j - 1].type)
                            return cells[i - 1, j - 1].obj.transform.position - new Vector3(-70, 0);
                    }
                }

                if(j + 2 < 8 && cells[i, j].type != cellType.Empty && cells[i, j + 2].type == cells[i, j].type)
                {
                    if(j + 1 < 8 && i - 1 > 0 && i + 1 < 7)
                    {
                        if(cells[i, j].type == cells[i + 1, j + 1].type)
                            return cells[i + 1, j + 1].obj.transform.position - new Vector3(70, 0);
                        if(cells[i, j].type == cells[i - 1, j + 1].type)
                            return cells[i - 1, j + 1].obj.transform.position - new Vector3(-70, 0);
                    }
                }

                if(i + 2 < 7 && cells[i, j].type != cellType.Empty && cells[i + 2, j].type == cells[i, j].type)
                {
                    if(j - 1 > 0 && j + 1 < 8 && i + 1 < 7)
                    {
                        if(cells[i, j].type == cells[i + 1, j - 1].type)
                            return cells[i + 1, j - 1].obj.transform.position - new Vector3(0, 70);
                        if(cells[i, j].type == cells[i + 1, j + 1].type) 
                            return cells[i + 1, j + 1].obj.transform.position - new Vector3(0, -70);
                    }
                }

                if(i - 2 > 0 && cells[i, j].type != cellType.Empty && cells[i - 2, j].type == cells[i, j].type)
                {
                    if(j - 1 > 0 && j + 1 < 8 && i - 1 > 0)
                    {
                        if(cells[i, j].type == cells[i - 1, j - 1].type)
                            return cells[i - 1, j - 1].obj.transform.position - new Vector3(0, 70);
                        if(cells[i, j].type == cells[i - 1, j + 1].type) 
                            return cells[i - 1, j + 1].obj.transform.position - new Vector3(0, -70);
                    }
                }
            }
        }

        return transform.position;
    }

    private void DropCells()
    {
        foreach(Vector2Int pos in deletePositions)
        {  
            if(pos.y == 0 && cells[pos.x, pos.y].type != cellType.Empty && cells[pos.x, pos.y].type == cellType.Deleted)
            {
                cells[pos.x, pos.y].obj.GetComponent<Image>().color = new Color(255, 255, 255, 255);
                int type = Random.Range(0,5);
                cells[pos.x, pos.y].type = (cellType)type;
                cells[pos.x, pos.y].obj.GetComponent<Image>().sprite = sweets[type].image;

                cells[pos.x, pos.y].pos =  cells[pos.x, pos.y].obj.transform.position;
            }

            else if(cells[pos.x, pos.y - 1].type != cellType.Deleted &&
            cells[pos.x, pos.y - 1].type != cellType.Empty)
            {   
                cells[pos.x, pos.y - 1].obj.transform.position = Vector2.MoveTowards(cells[pos.x, pos.y - 1].obj.transform.position, cells[pos.x, pos.y].obj.transform.position, 1500 * Time.deltaTime);

                if(cells[pos.x, pos.y - 1].obj.transform.position == cells[pos.x, pos.y].obj.transform.position)
                {
                    cells[pos.x, pos.y].obj.transform.position = cells[pos.x, pos.y - 1].pos;

                    cells[pos.x, pos.y].type = cells[pos.x, pos.y - 1].type;
                    cells[pos.x, pos.y - 1].type = cellType.Deleted;
 
                    GameObject obje = cells[pos.x, pos.y].obj;
                    cells[pos.x, pos.y].obj = cells[pos.x, pos.y - 1].obj;
                    cells[pos.x, pos.y - 1].obj = obje;

                    cells[pos.x, pos.y].pos =  cells[pos.x, pos.y].obj.transform.position;
                    cells[pos.x, pos.y - 1].pos = cells[pos.x, pos.y - 1].obj.transform.position;
                }
            }

            else if(cells[pos.x, pos.y - 1].type == cellType.Empty && cells[pos.x, pos.y].type == cellType.Deleted)
            {
                cells[pos.x, pos.y].obj.GetComponent<Image>().color = new Color(255, 255, 255, 255);
                int type = Random.Range(0,5);
                cells[pos.x, pos.y].type = (cellType)type;
                cells[pos.x, pos.y].obj.GetComponent<Image>().sprite = sweets[type].image;

                cells[pos.x, pos.y].pos =  cells[pos.x, pos.y].obj.transform.position;
            }      
        }
    }

    private void SwapCells()
    {
        if(posX != -1)
        {
            switch (dir)
            { 
                case Direction.North:
                    cells[posX, posY].obj.transform.position = Vector2.MoveTowards(cells[posX, posY].obj.transform.position, _pos, 600 * Time.deltaTime);
                    cells[posX, posY - 1].obj.transform.position = Vector2.MoveTowards(cells[posX, posY - 1].obj.transform.position, pos, 600 * Time.deltaTime);
                    break;
                case Direction.South:
                    cells[posX, posY].obj.transform.position = Vector2.MoveTowards(cells[posX, posY].obj.transform.position, _pos, 600 * Time.deltaTime);
                    cells[posX, posY + 1].obj.transform.position = Vector2.MoveTowards(cells[posX, posY + 1].obj.transform.position, pos, 600 * Time.deltaTime);
                    break;
                case Direction.East:
                    cells[posX, posY].obj.transform.position = Vector2.MoveTowards(cells[posX, posY].obj.transform.position, _pos, 600 * Time.deltaTime);
                    cells[posX + 1, posY].obj.transform.position = Vector2.MoveTowards(cells[posX + 1, posY].obj.transform.position, pos, 600 * Time.deltaTime);
                    break;
                case Direction.West:
                    cells[posX, posY].obj.transform.position = Vector2.MoveTowards(cells[posX, posY].obj.transform.position, _pos, 600 * Time.deltaTime);
                    cells[posX - 1, posY].obj.transform.position = Vector2.MoveTowards(cells[posX - 1, posY].obj.transform.position, pos, 600 * Time.deltaTime);
                    break; 
            }
        }

        if(cells[posX, posY].obj.transform.position == _pos)
        {
            swap = false;

            cellType type = cells[posX, posY].type;
            GameObject obj = cells[posX, posY].obj;

            switch (dir)
            { 
                case Direction.North:
                    cells[posX, posY].obj = cells[posX, posY - 1].obj;
                    cells[posX, posY - 1].obj = obj;
                    cells[posX, posY].type = cells[posX, posY - 1].type;
                    cells[posX, posY - 1].type = type;
                    break;
                case Direction.South:
                    cells[posX, posY].obj = cells[posX, posY + 1].obj;
                    cells[posX, posY + 1].obj = obj;
                    cells[posX, posY].type = cells[posX, posY + 1].type;
                    cells[posX, posY + 1].type = type;
                    break;
                case Direction.East:
                    cells[posX, posY].obj = cells[posX + 1, posY].obj;
                    cells[posX + 1, posY].obj = obj;
                    cells[posX, posY].type = cells[posX + 1, posY].type;
                    cells[posX + 1, posY].type = type;
                    break;
                case Direction.West:
                    cells[posX, posY].obj = cells[posX - 1, posY].obj;
                    cells[posX - 1, posY].obj = obj;
                    cells[posX, posY].type = cells[posX - 1, posY].type;
                    cells[posX - 1, posY].type = type;
                    break;        
            }    

            UpdateGrid();

            if(deletePositions.Count == 0 && !isChanged)
            {
                swap = true;
                isChanged = true;
            }
            else if(deletePositions.Count != 0) gameManager.TakeMove();
        }
    }

    private void FindDirForSwap(int xPos, int yPos, Vector2 startPos, Vector2 direction)
    {
        int _xPos = xPos, _yPos = yPos;
        bool dirFind = true;

        if(direction.y > 10 && Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
        {
            dir = Direction.North;
            _pos = cells[xPos, yPos - 1].obj.transform.position;
            _yPos = yPos - 1;
        }

        else if(direction.x > 10 && Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            dir = Direction.East;
            _pos = cells[xPos + 1, yPos].obj.transform.position;
            _xPos = xPos + 1;
        }

        else if(direction.y < -10 && Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
        {
            dir = Direction.South;
            _pos = cells[xPos, yPos + 1].obj.transform.position;
            _yPos = yPos + 1;
        }

        else if(direction.x < -10 && Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            dir = Direction.West;      
            _pos = cells[xPos - 1, yPos].obj.transform.position;
            _xPos = xPos - 1;
        }
        else dirFind = false;


        if(cells[xPos, yPos].type != cellType.Empty && cells[_xPos, _yPos].type != cellType.Empty && dirFind)
        {
            swap = true;
            posX = xPos;
            posY = yPos; 
            pos = cells[xPos, yPos].obj.transform.position;
        }
    }

    private bool CheckGrid()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(j - 1 > 0 && cells[i, j].type != cellType.Empty && cells[i, j - 1].type == cells[i, j].type)
                {
                    if(j - 2 > 0 && i - 1 > 0 && i + 1 < 7)
                    {
                        if(cells[i, j].type == cells[i + 1, j - 2].type || cells[i, j].type == cells[i - 1, j - 2].type) return false;
                    }
                }

                if(j + 1 < 8 && cells[i, j].type != cellType.Empty && cells[i, j + 1].type == cells[i, j].type)
                {
                    if(j + 2 < 8 && i - 1 > 0 && i + 1 < 7)
                    {
                        if(cells[i, j].type == cells[i + 1, j + 2].type || cells[i, j].type == cells[i - 1, j + 2].type) return false;
                    }
                }

                if(i + 1 < 7 && cells[i, j].type != cellType.Empty && cells[i + 1, j].type == cells[i, j].type)
                {
                    if(j - 1 > 0 && i + 2 < 7 && j + 1 < 8)
                    {
                        if(cells[i, j].type == cells[i + 2, j - 1].type || cells[i, j].type == cells[i + 2, j + 1].type) return false;
                    }
                }

                if(i - 1 > 0 && cells[i, j].type != cellType.Empty && cells[i - 1, j].type == cells[i, j].type)
                {
                    if(j - 1 > 0 && i - 2 > 0 && j + 1 < 8)
                    {
                        if(cells[i, j].type == cells[i - 2, j - 1].type || cells[i, j].type == cells[i - 2, j + 1].type) return false;
                    }
                }

                if(j - 2 > 0 && cells[i, j].type != cellType.Empty && cells[i, j - 2].type == cells[i, j].type)
                {
                    if(j - 1 > 0 && i - 1 > 0 && i + 1 < 7)
                    {
                        if(cells[i, j].type == cells[i + 1, j - 1].type || cells[i, j].type == cells[i - 1, j - 1].type) return false;
                    }
                }

                if(j + 2 < 8 && cells[i, j].type != cellType.Empty && cells[i, j + 2].type == cells[i, j].type)
                {
                    if(j + 1 < 8 && i - 1 > 0 && i + 1 < 7)
                    {
                        if(cells[i, j].type == cells[i + 1, j + 1].type || cells[i, j].type == cells[i - 1, j + 1].type) return false;
                    }
                }

                if(i + 2 < 7 && cells[i, j].type != cellType.Empty && cells[i + 2, j].type == cells[i, j].type)
                {
                    if(j - 1 > 0 && j + 1 < 8 && i + 1 < 7)
                    {
                        if(cells[i, j].type == cells[i + 1, j - 1].type || cells[i, j].type == cells[i + 1, j + 1].type) return false;
                    }
                }

                if(i - 2 > 0 && cells[i, j].type != cellType.Empty && cells[i - 2, j].type == cells[i, j].type)
                {
                    if(j - 1 > 0 && j + 1 < 8 && i - 1 > 0)
                    {
                        if(cells[i, j].type == cells[i - 1, j - 1].type || cells[i, j].type == cells[i - 1, j + 1].type) return false;
                    }
                }
            }
        }

       
        return true;
    }

    private void MixGrid()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(cells[i, j].type != cellType.Empty)
                {
                    int type = Random.Range(0,5);
                    cells[i, j].type = (cellType)type;
                    cells[i, j].obj.GetComponent<Image>().sprite = sweets[type].image;
                }
            }
        }
    
        if(CheckGrid()) MixGrid();
        AudioManager.Instance.PlayEffects(Mix);
        UpdateGrid();
    }

    [System.Serializable]
    private class Cell
    {
        public GameObject obj;
        public cellType type;
        public int xPos, yPos;
        public Vector3 pos;
    }

    [System.Serializable]
    public class Sweet
    {
        public Sprite image;
        public cellType type;
    }

    [System.Serializable]
    private class Level
    {
        public int[] grid;
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