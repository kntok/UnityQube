using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    FieldManager Field;
    Cube _Cube;

    [Header("PLAYER PROPERTIES")]
    [SerializeField] int column;
    [SerializeField] int row;
    float moveSpeed = 1000 / 300;
    public bool _canMove;
    public string playerDirection;

    Quaternion to = Quaternion.Euler(0, 0, 0);

    void Awake()
    {
        Field = GameObject.FindGameObjectWithTag("FieldManager").GetComponent<FieldManager>();
    
        column = 0;
        row = 0;
        playerDirection = "";

        Vector3 position;
        position.x = 0;
        position.y = 1;
        position.z = 0;
        transform.position = position;
    }

    #region Properties

    public float MoveSpeed
    {
        get { return moveSpeed; }
    }

    public int X
    {
        get { return column; }
    }

    public int Y
    {
        get { return row; }
    }

    #endregion

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.E))
        //     Field.SetMarker(column, row, "normal");
        
        // if (Input.GetKeyDown(KeyCode.Q))
        //     Field.EraseMarker();

        // if (Input.GetKeyDown(KeyCode.X))
        //     Field.GetCubeAtPosition(column, row).GetComponent<Cube>().Remove();
    }
    
    void FixedUpdate()
    {
        Controller();
    }

    public void SetPosition(float x, float y)
    {
        column = Mathf.FloorToInt(x / 1);
        row = Mathf.FloorToInt(y / 1);

        Vector3 playerPos = transform.position;
        playerPos.x = x;
        playerPos.y = 0.5f;
        playerPos.z = -y + .5f;

        transform.position = playerPos;
    }

    public Vector3 GetPosition()
    {
        Vector3 position;

        position = transform.position;

        return position;
    }

    void Controller()
    {    
        int x2, y2;

        Field.GetFieldPos(transform.position.x, Mathf.Abs(transform.position.z), out x2, out y2);

        column = x2;
        row = y2;

        playerDirection = "";
        
        if (Input.anyKey)
        {
            if (Input.GetKey(KeyCode.W))
                Move("UP");
            else if (Input.GetKey(KeyCode.S))
                Move("DOWN");
            else if (Input.GetKey(KeyCode.A))
                Move("LEFT");
            else if (Input.GetKey(KeyCode.D))
                Move("RIGHT");
        }
    }

    void Move(string direction)
    {
        Vector3 playerPos = transform.position;
        float delta = Time.fixedDeltaTime * moveSpeed;

        UpdateRotation(direction);

        if (CanMove(direction))
        {
            switch (direction)
            {
                case "LEFT":
                    playerPos.x -= delta;
                    break;
                case "UP":
                    playerPos.z += delta;
                    break;
                case "RIGHT":
                    playerPos.x += delta;
                    break;
                case "DOWN":
                    playerPos.z -= delta;
                    break;
            }

            transform.position = playerPos;
            // Debug.Log("動けるよ！");
        }
    }

    bool CanMove(string direction)
    {
        var canMove = true;
        
        var playerPos = transform.position;

        var nextX = transform.position.x;
        var nextY = transform.position.z;
        
        var delta = Time.fixedDeltaTime * moveSpeed;
        
        switch (direction)
        {
            case "LEFT":
                nextX = playerPos.x - delta;
                break;
            case "UP":
                nextY = playerPos.z + delta;
                break;
            case "RIGHT":
                nextX = playerPos.x + delta;
                break;
            case "DOWN":
                nextY = playerPos.z - delta;
                break;
        }

        var floor = Field.GetFloorAtPosition(nextX, Mathf.Abs(nextY));
        var cube = Field.GetCubeAtPosition(nextX, Mathf.Abs(nextY));

        if (!floor || nextY > 0)
            return false;

        if (cube)
        {
            // canMove = false;
        }

        _canMove = canMove;
        
        return canMove;
    }

    // プレイヤーの向き変更
    void UpdateRotation(string direction)
    {
        switch (direction)
        {
            case "LEFT":
                to = Quaternion.Euler(0, 90, 0);
                break;
            case "RIGHT":
                to = Quaternion.Euler(0, -90, 0);
                break;
            case "UP":
                to = Quaternion.Euler(0, -180, 0);
                break;
            case "DOWN":
                to = Quaternion.Euler(0, 0, 0);
                break;
            case "UPPER_LEFT":
                to = Quaternion.Euler(0, -180 + -45, 0);
                break;
            case "UPPER_RIGHT":
                to = Quaternion.Euler(0, -180 + 45, 0);
                break;
            case "LOWER_LEFT":
                to = Quaternion.Euler(0, 45, 0);
                break;
            case "LOWER_RIGHT":
                to = Quaternion.Euler(0, -45, 0);
                break;
            default:
                break;
        }

        // キャラクターの向いている方向を変える
        transform.rotation = Quaternion.Slerp(transform.rotation, to, 0.3f);
    }
}
