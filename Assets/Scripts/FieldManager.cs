using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FieldManager : MonoBehaviour
{
    GameController Game;

    [SerializeField] int g_fieldWidth;
    [SerializeField] int g_fieldLength;
    [SerializeField] public int g_fieldHeight;

    [SerializeField] int g_puzzleWidth;
    [SerializeField] int g_questionLength;

    [SerializeField] GameObject qubeNormal;
    [SerializeField] GameObject plate;

    void Awake()
    {
        Game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        qubeNormal = (GameObject)Resources.Load("Prefab/cube", typeof(GameObject));
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Z))
        //     ShrinkLength();

        // if (Input.GetKeyDown(KeyCode.C))
        //     AddLength();

        // if (Input.GetKeyDown(KeyCode.V))
        //     AddHeight();

        // if (Input.GetKeyDown(KeyCode.F))
        //     EraseHeight();

        // if (Input.GetKeyDown(KeyCode.T))
        //     EraseWidth();

        // if (Input.GetKeyDown(KeyCode.Y))
        //     AddWidth();

        if (Input.GetKeyDown(KeyCode.Space))
            RollPuzzleCube(testGroup);
    }

    #region Consts

    const float GRID_SIZE = 1f;
    const float GRID_OFFSET = GRID_SIZE / 2; // 0.5f

    const float DEFAULT_CUBE_SIZE = 1f;

    const int DEFAULT_FIELD_X = 4;

    const int MAX_STAGE_WIDTH = 10;
    const int MAX_STAGE_LENGTH = 40;
    const int MAX_STAGE_HEIGHT = 10;

    const int MIN_STAGE_WIDTH = 1;
    const int MIN_STAGE_LENGTH = 8;
    const int MIN_STAGE_HEIGHT = 1;

    #endregion

    // ---

    public int[,] gridArray; // 消す予定
    public List<GameObject> gridCubes = new List<GameObject>();

    public int[,] aCubeArray;
    public List<GameObject> activeCubes = new List<GameObject>();

    public int[,] qCubeArray;
    public List<GameObject> questionCubes = new List<GameObject>();

    // ---

    public Vector3 GetGridCenter(int x, int z)
    {
        Vector3 origin = Vector3.zero;

        origin.x += (GRID_SIZE * x) + GRID_OFFSET; // Y <=> X
        // origin.y -= (GRID_SIZE * y) + GRID_OFFSET; // Y <=> X
        origin.z -= (GRID_SIZE * z) + GRID_OFFSET; // Y <=> Z

        return origin;
    }

    public Vector3 GetGridCenter2(int x, int y, int z)
    {
        Vector3 origin = Vector3.zero;

        origin.x += (GRID_SIZE * x) + GRID_OFFSET; // Y <=> X
        origin.y -= y; // Y <=> X
        origin.z -= (GRID_SIZE * z) + GRID_OFFSET; // Y <=> Z

        return origin;
    }

    // 0 = normal
    // 1 = advantage
    // 2 = forbiddon
    // 3 = gold
    // 4 = red
    // 9 = null
    GameObject GetCubeResourceObject(int type)
    {
        GameObject obj = null;

        switch (type)
        {
            case 0:
                obj = (GameObject)Resources.Load("Prefab/cubeNormal", typeof(GameObject));
                break;
            case 1:
                obj = (GameObject)Resources.Load("Prefab/cubeAdvantage", typeof(GameObject));
                break;
            case 2:
                obj = (GameObject)Resources.Load("Prefab/cubeForbidden", typeof(GameObject));
                break;
            case 3:
                obj = (GameObject)Resources.Load("Prefab/cubeGoldAdvantage", typeof(GameObject));
                break;
            case 4:
                obj = (GameObject)Resources.Load("Prefab/cubeRedAdvantage", typeof(GameObject));
                break;
        }

        return obj;
    }

    void OnGUI ()
    {
        // Make a background box
        GUI.Box(new Rect(Screen.width - 200,10,200,200), "Array");

        // Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
        // GUI.Button(new Rect(20,40,80,20), "Level 1");

        for (var i = 0; i < aCubeArray.GetLength(0); i++)
        {
            GUILayout.BeginHorizontal("box");

            for (var j = 0; j < aCubeArray.GetLength(1); j++)
            {
                GUI.Label(new Rect((Screen.width - 200) + 20 + (20 * j), 30 + (20 * i), 100, 50), aCubeArray[i, j].ToString());
            }

            GUILayout.EndHorizontal();
        }

        GUI.Label(new Rect((Screen.width - 200) + 20 + (20 * aCubeArray.GetLength(1)), 30 + (20 * aCubeArray.GetLength(0)), 100, 50), $"Q {g_questionWidth} x {g_questionLength}");
    }

    public void GenerateField(int width, int length)
    {
        GameObject parent = new GameObject("Field");

        // 初期化
        gridArray = new int[length, width];
        aCubeArray = new int[length, width];
        markerArray = new int[length, width];

        g_fieldWidth = width;
        g_fieldLength = length;
        g_fieldHeight = 5;

        for (int i = 0; i < length; i++)
        {
            GameObject row = new GameObject("Row " + (i + 1));
            List<GameObject> rowAddBlocks = new List<GameObject>();

            for (int j = 0; j < width; j++)
            {
                gridArray[i, j] = 0;
                markerArray[i, j] = 0;
                aCubeArray[i, j] = -1;

                GameObject obj = Instantiate(qubeNormal, GetGridCenter(j, i), Quaternion.identity) as GameObject;
                obj.transform.SetParent(row.transform);
                obj.transform.localScale = new Vector3(1f, 1f, 1f);
                obj.GetComponent<Grid>().Position(j, i);
                obj.GetComponent<Grid>().SetHeight(g_fieldHeight);

                rowAddBlocks.Add(obj);
            }

            gridCubes.AddRange(rowAddBlocks);
            row.transform.SetParent(parent.transform);
        }

        ResetBoard(width, length);

        new GameObject("Marker");

        Debug.LogFormat("* Generated Field / Width: {0} Length: {1} Height: {2}", g_fieldWidth, g_fieldLength, g_fieldHeight);
    }

    #region 行

    // 行の追加
    public void AddLength()
    {
        var oldLength = g_fieldLength;
        var newLength = oldLength + 1;

        var width = g_fieldWidth;
        var height = g_fieldHeight;

        int[,] newArray = new int[newLength, width];

        // aCubeArray.CopyTo(newArray, 0);
        Array.Copy(gridArray, newArray, gridArray.Length);
        gridArray = new int[newLength, width];

        for (int i = 0; i < newLength; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (aCubeArray.GetLength(0) > i && aCubeArray.GetLength(1) > j)
                    newArray[i, j] = aCubeArray[i, j];
                else
                    newArray[i, j] = -1;
            }
        }

        aCubeArray = new int[newLength, width];
        aCubeArray = newArray;

        markerArray = new int[newLength, width];

        List<GameObject> addBlocks = new List<GameObject>();
        GameObject row = new GameObject("Row " + newLength);

        for (int i = 0; i < g_fieldWidth; i++)
        {
            gridArray[oldLength, i] = 0;

            GameObject obj = Instantiate(qubeNormal, GetGridCenter(i, g_fieldLength), Quaternion.identity) as GameObject;
            obj.transform.localScale = new Vector3(1f, 1f, 1f);
            obj.GetComponent<Grid>().Position(i, g_fieldLength);
            obj.GetComponent<Grid>().SetHeight(g_fieldHeight);
            obj.transform.SetParent(row.transform);

            addBlocks.Add(obj);
        }

        gridCubes.AddRange(addBlocks);
        row.transform.SetParent(GameObject.Find("Field").transform);

        // gridArray = new int[g_fieldWidth, newLength];
        g_fieldLength = newLength;

        AudioManager.Instance.PlaySE(2);

        Debug.Log("* Field Length:" + g_fieldLength);
    }

    // 行の削除
    public void ShrinkLength(int length = 1)
    {
        var oldLength = g_fieldLength;
        var newLength = oldLength - length;

        var width = g_fieldWidth;
        var height = g_fieldHeight;

        int[,] newArray = new int[newLength, width];

        foreach (var grid in gridCubes.ToArray())
        {
            for (int i = 0; i < width; i++)
            {
                if (grid.GetComponent<Grid>().column == i &&
                    grid.GetComponent<Grid>().row == oldLength - 1)
                {
                    grid.GetComponent<Grid>().Erase();
                    gridCubes.Remove(grid);
                }
            }
        }

        gridArray = new int[newLength, width];

        for (int i = 0; i < newLength; i++)
        {
            for (int j = 0; j < width; j++)
            {
                newArray[i, j] = aCubeArray[i, j];
            }
        }

        aCubeArray = new int[newLength, width];
        aCubeArray = newArray;

        markerArray = new int[newLength, width];

        Destroy(GameObject.Find($"Row {oldLength}"));

        g_fieldLength = newLength;

        AudioManager.Instance.PlaySE(1);

        Debug.Log("* SHRINK FIELD! " + g_fieldLength);
        Debug.Log(gridArray.Length);
    }

    #endregion

    #region 高さ

    // 高くする
    public void AddHeight()
    {
        var newH = g_fieldHeight + 1;

        if (MAX_STAGE_HEIGHT >= newH)
        {
            foreach(var grid in gridCubes)
            {
                grid.GetComponent<Grid>().AddHeight(newH);
            }

            g_fieldHeight = newH;
        }
        else
            return;

        Debug.Log("FIELD HEIGHT: " + g_fieldHeight);
    }

    // 低くする
    public void EraseHeight()
    {
        var newH = g_fieldHeight - 1;

        if (newH >= MIN_STAGE_HEIGHT)
        {
            foreach(var grid in gridCubes.ToArray())
            {
                grid.GetComponent<Grid>().CutHeight();
            }

            g_fieldHeight = newH;
        }
        else
            return;

        Debug.Log("FIELD HEIGHT: " + g_fieldHeight);
    }

    #endregion

    #region 横

    // 横を伸ばす
    public void AddWidth()
    {
        var oldWidth = g_fieldWidth;
        var newWidth = oldWidth;

        if (MAX_STAGE_WIDTH > g_fieldWidth)
        {
            newWidth++;

            var length = g_fieldLength;
            var height = g_fieldHeight;

            bool approval = true;

            foreach(var blocks in gridCubes)
            {
                if (blocks.GetComponent<Grid>().column == oldWidth)
                {
                    approval = false;
                }
            }

            if (approval)
            {
                for (int i = 0; i < length; i++)
                {
                    var rowParent = GameObject.Find("Row " + (i + 1));

                    var obj = Instantiate(qubeNormal, GetGridCenter(oldWidth, i), Quaternion.identity) as GameObject;
                    obj.transform.localScale = new Vector3(1f, 1f, 1f);
                    obj.GetComponent<Grid>().Position(oldWidth, i);
                    obj.GetComponent<Grid>().SetHeight(height);
                    obj.transform.SetParent(rowParent.transform);

                    gridCubes.Add(obj);
                }

                g_fieldWidth = newWidth;

                int[,] newArray = new int[length, newWidth];
                gridArray = new int[length, newWidth];

                for (int i = 0; i < length; i++)
                {
                    for (int j = 0; j < newWidth; j++)
                    {
                        if (aCubeArray.GetLength(0) > i && aCubeArray.GetLength(1) > j)
                            newArray[i, j] = aCubeArray[i, j];
                        else
                            newArray[i, j] = -1;
                    }
                }

                aCubeArray = new int[length, newWidth];
                aCubeArray = newArray;

                markerArray = new int[length, newWidth];
            }
        }
        else
            return;

        Debug.Log("FIELD WIDTH: " + g_fieldWidth);
    }

    // 縮める
    public void EraseWidth()
    {
        int oldWidth = g_fieldWidth;
        int newWidth = oldWidth;
        var length = g_fieldLength;

        if (MIN_STAGE_WIDTH < g_fieldWidth)
        {
            newWidth--;

            foreach (var blocks in gridCubes.ToArray())
            {
                if (blocks.GetComponent<Grid>().column == oldWidth - 1 && blocks != null)
                {
                    blocks.GetComponent<Grid>().Erase();
                    gridCubes.Remove(blocks);
                }
            }

            g_fieldWidth = newWidth;

            int[,] newArray = new int[length, newWidth];

            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < newWidth; j++)
                {
                    newArray[i, j] = aCubeArray[i, j];
                }
            }

            aCubeArray = new int[length, newWidth];
            aCubeArray = newArray;

            markerArray = new int[length, newWidth];
        }
        else
            return;

        Debug.Log("FIELD WIDTH: " + g_fieldWidth);
    }

    #endregion

    public void ResetBoard(int width, int length)
    {
        aCubeArray = new int[length, width];

        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < width; j++)
                aCubeArray[i, j] = -1;
        }

        Debug.Log("RESET BOARD!");
    }

    // キューブ作成
    GameObject AddCube(int x, int y, int type)
    {
        var obj = Instantiate(GetCubeResourceObject(type), GetGridCenter2(x, 1, y), Quaternion.identity) as GameObject;
        obj.tag = "Cube";
        obj.name = $"Cube ({y}, {x})";
        // obj.transform.localScale = new Vector3(1f, 1f, 1f);

        obj.GetComponent<Cube>().Position(x, y);
        obj.GetComponent<Cube>().type = type;

        switch (type)
        {
            case 0:
                obj.GetComponent<Cube>().typeString = "normal";
                break;
            case 1:
                obj.GetComponent<Cube>().typeString = "advantage";
                break;
            case 2:
                obj.GetComponent<Cube>().typeString = "forbidden";
                break;
            case 3:
                obj.GetComponent<Cube>().typeString = "gold";
                break;
            case 4:
                obj.GetComponent<Cube>().typeString = "red";
                break;
        }

        aCubeArray[y, x] = type;
        activeCubes.Add(obj);

        return obj;
    }

    #region パズル問題

    // 問題作成
    int[,] questionArray = new int[,]
    {
        {0, 1, 1, 2},
        {0, 1, 1, 2},
        {0, 1, 1, 2},
        {0, 1, 1, 2}
    };

    int g_questionWidth;

    // public List<IList> questionGroup = new List<IList>();
    public List<GameObject> testGroup = new List<GameObject>();

    // パズル作成
    public void CreatePuzzle()
    {
        List<GameObject> puzzle = new List<GameObject>();
        GameObject parent = new GameObject("Question");

        g_questionLength = questionArray.GetLength(0);
        g_questionWidth = questionArray.GetLength(1);

        for (var i = 0; i < questionArray.GetLength(0); i++)
        {
            for (int j = 0; j < questionArray.GetLength(1); j++)
            {
                puzzle.Add(AddCube(j, i, questionArray[i, j]));
            }
        }

        foreach(var obj in puzzle)
        {
            obj.transform.SetParent(parent.transform);
            // obj.GetComponent<Cube>().ActivateRise();
            obj.GetComponent<Cube>().Place();
        }

        // activeCubes.AddRange(puzzle);

        // questionGroup.Add(puzzle);
        testGroup.AddRange(puzzle);
    }

    public List<GameObject> fallCubes = new List<GameObject>();

    public void RollPuzzleCube(List<GameObject> group)
    {
        var qW = aCubeArray.GetLength(1);
        var qL = aCubeArray.GetLength(0);

            // if (cube != null)
            // {
                // List<int> cubeLineAt = new List<int>(); // aCubeArray の行に -1 以外の数値が格納されているのかどの行なのかを格納
                // for (int i = 0; i < qL; i++)
                // {
                //     if (aCubeArray[i, 0] != -1) cubeLineAt.Add(i);
                // }

                // for (var y = 0; y < cubeLineAt.Count; y++)
                //     Debug.Log(cubeLineAt[y]);

                // Debug.Log(cubeLineAt.Count);

                    // int min = cubeLineAt.Min();
                    // int max = cubeLineAt.Max();
                    // int diff = max - min;
                    // Console.WriteLine($"{max} - {min} = {diff}");

                // for (int y = qL - 1; y >= 0; y--)
                // {
                //     for (int x = 0; x < qW; x++)
                //     {
                //         if (y != (max - 1)) // Y が 最後行 - 1 でなければ
                //         {
                //             tmp[x] = aCubeArray[y - 1, x]; // TODO
                //             aCubeArray[y - 1, x] = aCubeArray[y, x];
                //             aCubeArray[y, x] = tmp[x];
                //         }
                //         else
                //         {
                //             tmp[x] = aCubeArray[y - 1, x]; // TODO
                //             aCubeArray[y, x] = tmp[x];
                //         }
                //     }
                // }

                // for (int y = qL - 1; y < qL; y--) // 縦
                // {
                //     for (int x = 0; x < qW; x++) // 横
                //     {
                //         if (y != (max - 1))
                //         {
                //             tmp[x] = aCubeArray[y - 1, x]; // TODO 2
                //             aCubeArray[y - 1, x] = aCubeArray[y, x];
                //             aCubeArray[y, x] = tmp[x];
                //         }
                //         else
                //         {
                //             tmp[x] = aCubeArray[y - 1, x]; // TODO 1
                //             aCubeArray[y, x] = tmp[x];
                //         }
                //     }
                // }
                foreach (var cube in group.ToArray())
                {
                    cube.GetComponent<Cube>().Roll();
                }
                
                for (int y = aCubeArray.GetLength(0) - 1; y >= 0; y--) // 縦
                {
                    int[] tmp = new int[aCubeArray.GetLength(1)];
                    for (int x = 0; x < aCubeArray.GetLength(1); x++) // 横
                    {
                        //if (y == qL - 1)
                        //{
                        //    tmp[x] = array[y - 1, x];
                        //    array[y, x] = tmp[x];
                        //}
                        //else
                        //{
                        //    tmp[x] = array[y + 1, x];
                        //    array[y, x] = tmp[x];
                        //}

                        if (y == 0)
                        {
                            aCubeArray[y + 1, x] = aCubeArray[y, x];
                            aCubeArray[y, x] = -1;
                        }
                        else if (y == aCubeArray.GetLength(0) - 1)
                        {
                            // tmp[x] = array[y , x];
                            aCubeArray[y, x] = aCubeArray[y - 1, x];
                        }
                        else
                        {
                            tmp[x] = aCubeArray[y - 1, x];
                            aCubeArray[y + 1, x] = aCubeArray[y, x];
                            aCubeArray[y, x] = tmp[x];
                        }
                    }
                }
    }

    public void CreateWaitBlocks()
    {
        GameObject parent = new GameObject("Question");

        for (var i = 0; i < questionArray.GetLength(0); i++)
        {
            for (int j = 0; j < questionArray.GetLength(1); j++)
            {
                AddCube(j, i, questionArray[i, j]);
            }
        }
    }

    public void BreakOneStage()
    {
        ShrinkLength();

        Debug.Log("BREAK FIELD!" + g_fieldLength);
    }

    public void AddOneLength()
    {
        AddLength();

        Debug.Log("ADD ONE FIELD!" + g_fieldLength);
    }

    #endregion

    public void GetFieldPos(float x, float z, out int x2, out int z2)
    {
        int boardX = Mathf.FloorToInt(x / 1); // Debug.Log("X2: " + boardX);
        int boardZ = Mathf.FloorToInt(z / 1); // Debug.Log("Z2: " + boardZ);

        x2 = boardX;
        z2 = boardZ;
    }

    public GameObject GetCubeAtPosition(float x, float y)
    {
        int x2, y2;
        GetFieldPos(x, y, out x2, out y2);
        return GetCubeAt(x2, y2);
    }

    public GameObject GetCubeAt(int x, int y)
    {
        // int xx = Mathf.FloorToInt(x);
        // int zz = Mathf.FloorToInt(z);

        GameObject theCube = null;

        if (x < 0 || x >= aCubeArray.GetLength(1) || y < 0 || y >= aCubeArray.GetLength(0))
            return null;

        if (aCubeArray[y, x] == -1)
            return null;
        else
        {
            foreach (var item in activeCubes.ToArray())
            {
                if (item != null && item.GetComponent<Cube>().column == x && item.GetComponent<Cube>().row == y)
                    theCube = item;
            }

            return theCube;
        }
    }

    public bool GetFloorAtPosition(float x, float z)
    {
        int xx = Mathf.FloorToInt(x);
        int zz = Mathf.FloorToInt(z);

        if (xx < 0 || xx >= g_fieldWidth || zz < 0 || zz >= g_fieldLength)
            return false;

        return true;
    }

    float g_TumbleTime = 60f / 47f;
    float g_WaitTime = 60f / 22f;

    public void MatrixMoveCube(int oldX, int oldY, int newX, int newY, int type)
    {
        this.aCubeArray[oldY, oldX] = this.aCubeArray[newY, newX];
        this.aCubeArray[newY, newX] = type;

        // Debug.Log($"MOVED CUBE! ({oldX}, {oldY}) => ({newX}, {newY})");
    }

    public void RemoveCube(GameObject obj)
    {
        if (obj == null)
            return;

        bool eraseForbidden = false;
        bool eraseCube = false;

        // [Z, X]
        if (aCubeArray[obj.GetComponent<Cube>().row, obj.GetComponent<Cube>().column] == -1)
            return;
        else
        {
            foreach (var item in activeCubes.ToArray())
            {
                if (item != null && item == obj)
                {
                    if (item.GetComponent<Cube>().typeString == "forbidden")
                    {
                        eraseForbidden = true;
                        BreakOneStage();
                    }

                    eraseCube = true;

                    if (item.GetComponent<Cube>().typeString == "advantage" ||
                        item.GetComponent<Cube>().typeString == "gold")
                    {
                        SetMarker(obj.GetComponent<Cube>().column, obj.GetComponent<Cube>().row, obj.GetComponent<Cube>().typeString);
                    }

                    item.GetComponent<Cube>().ActivateSink();
                    activeCubes.Remove(item);

                    aCubeArray[item.GetComponent<Cube>().row, item.GetComponent<Cube>().column] = -1;

                    Debug.Log("CUBE REMOVED!");
                }
            }
        }

        if (eraseForbidden)
            AudioManager.Instance.PlaySE(7);
        if (eraseCube)
            AudioManager.Instance.PlaySE(6);
    }

    public void RemoveCube(int x, int z)
    {
        if (x < 0 || x >= aCubeArray.GetLength(1) || z < 0 || z >= aCubeArray.GetLength(0))
            return;

        if (aCubeArray[z, x] == -1)
            return;
        else
        {
            foreach (var item in activeCubes.ToArray())
            {
                if (item != null && item.GetComponent<Cube>().column == z && item.GetComponent<Cube>().row == x
                    && item.GetComponent<Cube>().isRolled)
                {
                    if (item.GetComponent<Cube>().GetTypeString() == "forbidden")
                    {

                    }


                    item.GetComponent<Cube>().Remove();
                    activeCubes.Remove(item);

                    AudioManager.Instance.PlaySE(5);
                    Debug.Log($"CUBE REMOVED! ({z}, {x})");

                    aCubeArray[z, x] = -1;
                }
                else
                {
                    return;
                }
            }
        }
    }

    bool CheckMatrixInCube(int x, int y)
    {
        if (aCubeArray[y, x] != -1)
            return true;

        return false;
    }

    #region Marker

    // マーカー
    public int[,] markerArray;
    public List<GameObject> activeMarker = new List<GameObject>();

    public void SetMarker(int x, int y, string type, bool isAdvantage = false)
    {
        var parent = GameObject.Find("Marker");
        if (parent == null)
            parent = new GameObject("Marker");

        if (markerArray[y, x] == 0)
        {
            markerArray[y, x] = 1;

            GameObject obj = Instantiate(plate, GetGridCenter(x, y), Quaternion.identity) as GameObject;
            obj.GetComponent<Marker>().Position(x, y);
            obj.GetComponent<Marker>().SetType(type);
            obj.GetComponent<Marker>().isAdvantage = isAdvantage;

            obj.name = $"Marker ({x}, {y})";
            obj.transform.SetParent(parent.transform);

            activeMarker.Add(obj);

            Debug.Log($"Marker Set! ({x}, {y})");
        }
    }

    public void EraseMarker(GameObject Marker)
    {
        var parent = GameObject.Find("Marker");
        if (parent == null)
            return;

        if (activeMarker.Count > 0)
        {
            if (markerArray[Marker.GetComponent<Marker>().row, Marker.GetComponent<Marker>().column] == 1)
                markerArray[Marker.GetComponent<Marker>().row, Marker.GetComponent<Marker>().column] = 0;
            else
                return;

            activeMarker.Remove(Marker);
            Marker.GetComponent<Marker>().Erase();

            // AudioManager.Instance.PlaySE(4);
            Debug.Log($"Marker Erased! ({Marker.GetComponent<Marker>().column}, {Marker.GetComponent<Marker>().row})");
        }
    }

    public void AllEraseMarker()
    {
        var parent = GameObject.Find("Marker");
        if (parent == null)
            return;

        // int count = 0;
        if (activeMarker.Count > 0)
        {
            // foreach (var item in activeCubes.ToArray())
            // {
            //     if (markerArray[item.GetComponent<Cube>().row, item.GetComponent<Cube>().column] != 0)
            //     {
            //         RemoveCube(item);
            //         count++;
            //     }
            // }

            // if (count > 0)
            //     AudioManager.Instance.PlaySE(6);

            // for (var i = 0; i < g_fieldLength; i++)
            // {
            //     for (var j = 0; j < g_fieldWidth; j++)
            //     {
            //         if (markerArray[i, j] == 1) markerArray[i, j] = 0;
            //     }
            // }

            foreach (var item in activeMarker.ToArray())
            {
                if (item.GetComponent<Marker>().type != "advantage" &&
                    item.GetComponent<Marker>().type != "gold")
                {
                    if (markerArray[item.GetComponent<Marker>().row, item.GetComponent<Marker>().column] == 1)
                        markerArray[item.GetComponent<Marker>().row, item.GetComponent<Marker>().column] = 0;

                    activeMarker.Remove(item);
                    item.GetComponent<Marker>().Erase();
                }
            }

            Debug.Log("Marker All Erased!");
        }
    }

    #endregion
}
