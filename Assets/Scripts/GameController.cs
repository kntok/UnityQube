using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] int g_fieldWidth;
    [SerializeField] int g_fieldLength;
    [SerializeField] int g_fieldHeight;

    [SerializeField] float g_RollTime = 60f / 47f;
    [SerializeField] float g_WaitTime = 60f / 25f;

    [SerializeField] int g_penalty;
    [SerializeField] int g_penaltyMax;
    [SerializeField] int g_penaltyQueue;

    FieldManager F;
    PlayerController P;

    // TODO:
    // ステージ毎にテクスチャが変更するようにする。
    // 難易度毎の速さ設定
    // カメラ
    // パズル用の配列 {1, 1, 1, 1} => { 1111 }
    // 崩れる時とかのアニメーション追加する

    [SerializeField] bool rotating;
    [SerializeField] bool deleting;
    [SerializeField] bool markOn;
    [SerializeField] bool breaking;

    void Awake()
    {
        F = GameObject.FindGameObjectWithTag("FieldManager").GetComponent<FieldManager>();
        P = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        g_fieldWidth = 4;
        g_fieldLength = 16; // 16
        g_fieldHeight = 5;

        deleting = false;
        markOn = false;
        breaking = false;
        rotating = false;
    }

    void Start()
    {
        F.GenerateField(g_fieldWidth, g_fieldLength); // 横, 行
        F.CreatePuzzle();
        
        P.SetPosition(2, 3);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            SetMarker();

        if (Input.GetKeyDown(KeyCode.F))
            ActivateAdvantage();
        
        if (Input.GetKeyDown(KeyCode.Space))
            ActivateGoldAdvantage();
        
        // RotateProcess();
        // DeleteProcess();
        // BreakLineProcess();
    }

    // マーカーを設置/解除
    void SetMarker()
    {
        if (markOn)
        {
            foreach (var item in F.activeMarker)
            {
                item.GetComponent<Marker>().SetType("red");
            }

            markOn = false;
            AudioManager.Instance.PlaySE(4);

            // F.AllEraseMarker(); // <- TODO

            if (!rotating)
                CheckMarker();
        }
        else
        {
            var posX = P.X;
            var posY = P.Y;
            
            if (F.markerArray[posY, posX] == 0)
            {
                F.SetMarker(posX, posY, "normal");
                markOn = true;
                AudioManager.Instance.PlaySE(3);
            }
        }
    }

    void CheckMarker()
    {
        bool deleted = false;
        int advCount = 0;

        foreach (var m in F.activeMarker.ToArray())
        {
            if (m.GetComponent<Marker>().type == "red")
            {
                var cube = F.GetCubeAt(m.GetComponent<Marker>().column, m.GetComponent<Marker>().row);

                if (cube != null)
                {
                    if (m.GetComponent<Marker>().isAdvantage)
                        advCount++;
                    
                    F.RemoveCube(cube);
                    
                    if (cube.GetComponent<Cube>().typeString == "advantage")
                        m.GetComponent<Marker>().SetType("advantage");
                    else if (!m.GetComponent<Marker>().isAdvantage)
                        F.EraseMarker(m);

                    deleted = true;
                }
            }
        }

        if (deleted)
        {
            deleting = true;
            
            if (advCount > 0)
            {
                foreach (var item in F.activeMarker.ToArray())
                {
                    if ((item.GetComponent<Marker>().type == "red" || item.GetComponent<Marker>().isAdvantage) &&
                        item.GetComponent<Marker>().type == "gold"
                        )
                    {
                        F.EraseMarker(item);
                    }
                }
            }
        }
    }

    // アドバンテージキューブの爆破
    void ActivateAdvantage()
    {
        bool activate = false;

        foreach (var item in F.activeMarker.ToArray())
        {
            var blueX = -1;
            var blueZ = -1;

            if (item.GetComponent<Marker>().type == "normal")
            {
                blueX = item.GetComponent<Marker>().column;
                blueZ = item.GetComponent<Marker>().row;
            }

            if (item.GetComponent<Marker>().type == "advantage")
            {
                item.GetComponent<Marker>().SetType("red");
                activate = true;

                int posX = item.GetComponent<Marker>().column;
                int posY = item.GetComponent<Marker>().row;

                int areaXstart = posX - 1;
                int areaXend = posX + 1;

                int areaYstart = posY - 1;
                int areaYend = posY + 1;

                if (areaXstart < 0)
                    areaXstart = 0;
                if (areaXend > g_fieldLength)
                    areaXend = g_fieldLength;

                if (areaYstart < 0)
                    areaYstart = 0;
                if (areaYend > g_fieldLength)
                    areaYend = g_fieldLength;

                for (int i = areaXstart; i <= areaXend; i++)
                {
                    for (int j = areaYstart; j <= areaYend; j++)
                    {
                        if (i != blueX && j != blueZ)
                            F.SetMarker(i, j, "red", true);
                    }
                }
            }
        }

        if (activate)
        {
            AudioManager.Instance.PlaySE(8);
            if (!rotating) CheckMarker();
        }
    }

    // レッドアドバンテージキューブの爆破
    void ActivateRedAdvantage()
    {
        bool activate = false;

        foreach (var item in F.activeMarker.ToArray())
        {
            if (item.GetComponent<Marker>().type == "gold")
            {
                item.GetComponent<Marker>().SetType("red");
                activate = true;

                int posX = item.GetComponent<Marker>().column;
                int posY = item.GetComponent<Marker>().row;

                int areaXstart = posX - 2;
                int areaXend = posX + 2;

                int areaYstart = posY - 2;
                int areaYend = posY + 2;

                if (areaXstart < 0)
                    areaXstart = 0;
                if (areaXend > g_fieldLength)
                    areaXend = g_fieldLength;

                if (areaYstart < 0)
                    areaYstart = 0;
                if (areaYend > g_fieldLength)
                    areaYend = g_fieldLength;

                for (int i = areaXstart; i <= areaXend; i++)
                {
                    for (int j = areaYstart; j <= areaYend; j++)
                    {
                        F.SetMarker(i, j, "red", true);
                    }
                }
            }
        }

        if (activate)
        {
            AudioManager.Instance.PlaySE(8);
            CheckMarker();
        }
    }

    // ゴールドアドバンテージキューブの爆破
    void ActivateGoldAdvantage()
    {
        bool activate = false;

        foreach (var item in F.activeMarker.ToArray())
        {
            if (item.GetComponent<Marker>().type == "gold")
            {
                item.GetComponent<Marker>().SetType("red");
                activate = true;

                for (int i = 0; i < g_fieldLength; i++)
                {
                    for (int j = 0; j < g_fieldWidth; j++)
                    {
                        F.SetMarker(j, i, "red", true);
                    }
                }
            }
        }

        if (activate)
        {
            AudioManager.Instance.PlaySE(8);
            if (!rotating) CheckMarker();
        }
    }

    void RotateProcess()
    {

    }

    Stack<GameObject> deleteCubes = new Stack<GameObject>();
    void DeleteProcess()
    {
        if (deleting)
            return;
        
        bool eraseForbidden = false;
        bool eraseCube = false;

        // // [Z, X]
        // if (aCubeArray[obj.GetComponent<Cube>().row, obj.GetComponent<Cube>().column] == -1)
        //     return;
        // else
        // {
        //     if (!cube.deleteStarted)
        //     {
        //         cube.deleteStarted = true;
        //     }

        //     foreach (var item in activeCubes.ToArray())
        //     {
        //         if (item != null && item == obj)
        //         {
        //             if (item.GetComponent<Cube>().GetTypeString() == "forbidden")
        //             {
        //                 eraseForbidden = true;
        //                 F.BreakOneStage();
        //             }

        //             eraseCube = true;

        //             if (item.GetComponent<Cube>().GetTypeString() == "advantage")
        //             {
        //                 SetMarker(obj.GetComponent<Cube>().column, obj.GetComponent<Cube>().row, obj.GetComponent<Cube>().typeString);
        //             }

        //             item.GetComponent<Cube>().ActivateSink();
        //             activeCubes.Remove(item);
                    
        //             aCubeArray[item.GetComponent<Cube>().row, item.GetComponent<Cube>().column] = -1;

        //         }
        //     }
        // }

        if (eraseForbidden)
            AudioManager.Instance.PlaySE(7);
        if (eraseCube)
            AudioManager.Instance.PlaySE(6);
        
        deleting = false;
    }

    void BreakLineProcess()
    {
        if (!breaking)
            return;

        F.BreakOneStage();
        
        breaking = false;
    }
}
