using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PlaneToward
{
    Top,
    Right,
    Bottom,
    Left,
}

public class Plane : MonoBehaviour
{
    //2:机头 1:机翼 3:中心 影响旋转
    public int[,] Top = new int[4, 5]
    {
        { 0,0,2,0,0},
        { 1,1,3,1,1},
        { 0,0,1,0,0},
        { 0,1,1,1,0},
    };

    public int[,] Bottom = new int[4, 5]
    {
        { 0,1,1,1,0},
        { 0,0,1,0,0},
        { 1,1,3,1,1},
        { 0,0,2,0,0},
    };

    public int[,] Right = new int[5, 4]
    {
        { 0,0,1,0},
        { 1,0,1,0},
        { 1,1,3,2},
        { 1,0,1,0},
        { 0,0,1,0},
    };

    public int[,] Left = new int[5, 4]
    {
        { 0,1,0,0},
        { 0,1,0,1},
        { 2,3,1,1},
        { 0,1,0,1},
        { 0,1,0,0},
    };

    public Vector2Int centerPos = new Vector2Int(5, 5);
    public PlaneToward toward = PlaneToward.Top;


    public Vector2Int tempCenterPos;
    public PlaneToward tempToward;
    public float ScaleParam = 1f;

    private int[,] checkerboard;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public void HidePlane()
    {
        boxCollider.enabled = false;
        spriteRenderer.enabled = false;
    }

    private int[,] GetPlaneData()
    {
        int[,] Data = Top;

        switch (toward)
        {
            case PlaneToward.Top:
                Data = Top;
                break;
            case PlaneToward.Bottom:
                Data = Bottom;
                break;
            case PlaneToward.Right:
                Data = Right;
                break;
            case PlaneToward.Left:
                Data = Left;
                break;
        }
        return Data;
    }

    private Vector2Int GetOffset()
    {
        Vector2Int offset = new Vector2Int(0, 0); // (0, 0)相对于头部的偏移
        switch (toward)
        {
            case PlaneToward.Top:
                offset = new Vector2Int(-1, -2);
                break;
            case PlaneToward.Bottom:
                offset = new Vector2Int(-2, -2);
                break;
            case PlaneToward.Right:
                offset = new Vector2Int(-2, -2);
                break;
            case PlaneToward.Left:
                offset = new Vector2Int(-2, -1);
                break;
        }
        return offset;
    }

    public bool CanSetPlane(int[,] checkerboard, Vector2Int extraOffset)
    {
        int[,] Data = GetPlaneData();
        Vector2Int offset = GetOffset();
        offset += extraOffset;

        for (int i = 0; i < Data.GetLength(0); i++)
        {
            for (int j = 0; j < Data.GetLength(1); j++)
            {
                Vector2Int pos = centerPos + offset + new Vector2Int(i, j);
                if (pos.x < 0 || pos.x >= checkerboard.GetLength(0) || pos.y < 0 || pos.y >= checkerboard.GetLength(1))
                {
                    return false;
                }
                bool cantAdd = (Data[i, j] != 0  && checkerboard[pos.x, pos.y] != 0); 
                if (cantAdd) 
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void SetData(int[,] checkerboard, Vector2Int extraOffset)
    {
        SetCheckboard(checkerboard);
        Vector2Int temp = centerPos;
        int[,] Data = GetPlaneData();
        Vector2Int offset = GetOffset();
        offset += extraOffset;
        for (int i = 0; i < Data.GetLength(0); i++)
        {
            for (int j = 0; j < Data.GetLength(1); j++)
            {
                Vector2Int pos = centerPos + offset + new Vector2Int(i, j);
                checkerboard[pos.x, pos.y] += Data[i, j];

                if (TileMapUtil.IsCenter(checkerboard[pos.x, pos.y]))
                {
                    temp = pos;
                }
            }
        }
        centerPos = temp;
    }

    public void ResetData()
    {
        Vector2Int offset = GetOffset();
        int[,] Data = GetPlaneData();

        for (int i = 0; i < Data.GetLength(0); i++)
        {
            for (int j = 0; j < Data.GetLength(1); j++)
            {
                Vector2Int pos = centerPos + offset + new Vector2Int(i, j);
                if (Data[i, j] != 0 && checkerboard[pos.x, pos.y] != 0)
                {
                    checkerboard[pos.x, pos.y] = 0;
                }
                
            }
        }
    }

    public void SetCheckboard(int[,] checkerboard)
    {
        this.checkerboard = checkerboard;
    }

    public bool RefreshData(int[,] checkerboard, Vector2Int extraOffset)
    {
        if (!CanSetPlane(checkerboard, extraOffset)) return false;
        SetData(checkerboard, extraOffset);
        RotatePlane();
        return true;
    }

    public void Toward()
    {
        int[,] copy = (int[,])checkerboard.Clone();
        PlaneToward preToward = toward;
        ResetData();

        int t = (int)toward + 1;

        t %= Enum.GetValues(typeof(PlaneToward)).Length;
        toward = (PlaneToward)t;

        Vector2Int extraOffset = new Vector2Int(0, 0);
        if (!CanSetPlane(checkerboard, extraOffset))
        {
            checkerboard = copy;
            toward = preToward;
            return;
        }
        SetData(checkerboard, extraOffset);

        RotatePlane();
    }

    public void Move(Vector2Int offset)
    {
        int[,] copy = (int[,])checkerboard.Clone();
        ResetData();

        if (!CanSetPlane(checkerboard, offset)) 
        {
            checkerboard = copy;
            return;
        } 

        SetData(checkerboard, offset);
    }

    public void RotatePlane()
    {
        Vector3 vector = (int)toward * -Vector3.forward * 90;
        transform.rotation = Quaternion.Euler(vector);

        FixRotatePlanePos();
    }

    private void FixRotatePlanePos()
    {
        switch (toward)
        {
            case PlaneToward.Top:
                transform.position += new Vector3(-0.5f, -0.5f, 0) * ScaleParam;
                break;
            case PlaneToward.Bottom:
                transform.position += new Vector3(0.5f, 0.5f, 0) * ScaleParam;
                break;
            case PlaneToward.Right:
                transform.position += new Vector3(-0.5f, 0.5f, 0) * ScaleParam;
                break;
            case PlaneToward.Left:
                transform.position += new Vector3(0.5f, -0.5f, 0) * ScaleParam;
                break;
        }
    }

    public void FixMovePlanePos()
    {
        switch (toward)
        {
            case PlaneToward.Top:
                transform.position += new Vector3(0.5f, 0, 0) * ScaleParam;
                break;
            case PlaneToward.Bottom:
                transform.position += new Vector3(0.5f, 1f, 0) * ScaleParam;
                break;
            case PlaneToward.Right:
                transform.position += new Vector3(0, 0.5f, 0) * ScaleParam;
                break;
            case PlaneToward.Left:
                transform.position += new Vector3(1f, 0.5f, 0) * ScaleParam;
                break;
        }
    }

    public void PreSetData(int[,] tmpCheckboard)
    {
        int[,] Data = GetTempPlaneData();
        Vector2Int offset = GetTempOffset();
        for (int i = 0; i < Data.GetLength(0); i++)
        {
            for (int j = 0; j < Data.GetLength(1); j++)
            {
                Vector2Int pos = tempCenterPos + offset + new Vector2Int(i, j);
                tmpCheckboard[pos.x, pos.y] += Data[i, j];
            }
        }
    }


    public void RandomDone(int[,] tempCheckboard)
    {
        checkerboard = tempCheckboard;
        centerPos = tempCenterPos;
        toward = tempToward;
    }

    public bool CanSetTempPlane(int[,] checkerboard)
    {
        int[,] Data = GetTempPlaneData();
        Vector2Int offset = GetTempOffset();

        for (int i = 0; i < Data.GetLength(0); i++)
        {
            for (int j = 0; j < Data.GetLength(1); j++)
            {
                Vector2Int pos = tempCenterPos + offset + new Vector2Int(i, j);
                if (pos.x < 0 || pos.x >= checkerboard.GetLength(0) || pos.y < 0 || pos.y >= checkerboard.GetLength(1))
                {
                    return false;
                }
                bool cantAdd = (Data[i, j] != 0 && checkerboard[pos.x, pos.y] != 0);
                if (cantAdd)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private int[,] GetTempPlaneData()
    {
        int[,] Data = Top;

        switch (tempToward)
        {
            case PlaneToward.Top:
                Data = Top;
                break;
            case PlaneToward.Bottom:
                Data = Bottom;
                break;
            case PlaneToward.Right:
                Data = Right;
                break;
            case PlaneToward.Left:
                Data = Left;
                break;
        }
        return Data;
    }

    private Vector2Int GetTempOffset()
    {
        Vector2Int offset = new Vector2Int(0, 0); // (0, 0)相对于头部的偏移
        switch (tempToward)
        {
            case PlaneToward.Top:
                offset = new Vector2Int(-1, -2);
                break;
            case PlaneToward.Bottom:
                offset = new Vector2Int(-2, -2);
                break;
            case PlaneToward.Right:
                offset = new Vector2Int(-2, -2);
                break;
            case PlaneToward.Left:
                offset = new Vector2Int(-2, -1);
                break;
        }
        return offset;
    }

    public void CalculateCenterPosAndToward(Vector2Int headPos, int[,] checkboard)
    {
        int row = checkboard.GetLength(0);
        int col = checkboard.GetLength(1);

        if (headPos.x + 1 < row && checkboard[headPos.x + 1, headPos.y] == TileMapUtil.centerNum)
        {
            toward = PlaneToward.Top;
            centerPos = new Vector2Int(headPos.x + 1, headPos.y);
        }
        else if (headPos.y - 1 >= 0 && checkboard[headPos.x, headPos.y - 1] == TileMapUtil.centerNum)
        {
            toward = PlaneToward.Right;
            centerPos = new Vector2Int(headPos.x, headPos.y - 1);
        }
        else if (headPos.x - 1 >= 0 && checkboard[headPos.x - 1, headPos.y] == TileMapUtil.centerNum)
        {
            toward = PlaneToward.Bottom;
            centerPos = new Vector2Int(headPos.x - 1, headPos.y);
        }
        else if (headPos.y + 1 < col && checkboard[headPos.x, headPos.y + 1] == TileMapUtil.centerNum)
        {
            toward = PlaneToward.Left;
            centerPos = new Vector2Int(headPos.x, headPos.y + 1);
        }
    }
}
