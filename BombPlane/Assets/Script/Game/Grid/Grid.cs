using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Linq;

// tileMap坐标系为向右为正，向上为正
// tileMap row和col相反

// 数据坐标系向右为正，向下为正
//A B C 
//B
//C

public class Grid : MonoBehaviour
{
    public SwitchableTile wallTile;
    public PlaneTile planeTile;
    public Tilemap tileMap;
    public Plane objectPlane;

    private readonly int col = 9;
    private readonly int row = 9;

    private int[,] checkerboard;

    private readonly List<Plane> m_planes = new List<Plane>();

    private bool CanMovePlane = false;
    private Plane movePlane;

    public void Awake()
    {
        checkerboard = new int[row, col];

        Plane plane = Instantiate(objectPlane);
        plane.centerPos = new Vector2Int(5, 6);

        Plane plane1 = Instantiate(objectPlane);
        plane1.centerPos = new Vector2Int(1, 2);

        Plane plane2 = Instantiate(objectPlane);
        plane2.centerPos = new Vector2Int(2, 7);
        plane2.toward = PlaneToward.Right;

        m_planes.Add(plane);
        m_planes.Add(plane1);
        m_planes.Add(plane2);
    }

    public void Start()
    {
        foreach (Plane p in m_planes)
        {
            p.RefreshData(checkerboard, new Vector2Int(0, 0));
            TileMapUtil.SetPlanePos(p, tileMap);
        }
        TileMapUtil.Draw(tileMap, checkerboard, planeTile, wallTile);
    }

    public void OnEnable()
    {
        UserInput._OnClick += _OnClick;
        UserInput._OnDrag += _OnDrag;
        UserInput._OnDown += _OnDown;
        UserInput._OnUp += _OnUp;
    }

    public void OnDisable()
    {
        UserInput._OnClick -= _OnClick;
        UserInput._OnDrag -= _OnDrag;
        UserInput._OnDown -= _OnDown;
        UserInput._OnUp -= _OnUp;
    }

    private void _OnClick()
    {
        Collider2D collider = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition), Layers.PlaneMask);
        if (!collider) return;
        
        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (TileMapUtil.IsTileOfType<PlaneTile>(tileMap, tileMap.WorldToCell(world)))
        {
            Plane plane = collider.GetComponent<Plane>();
            plane.Toward();
        }

        foreach (Plane plane in m_planes)
        {
            plane.RefreshData(checkerboard, new Vector2Int(0, 0));
        }
        TileMapUtil.Draw(tileMap, checkerboard, planeTile, wallTile);
    }

    private void _OnDown()
    {
        Collider2D collider = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition), Layers.PlaneMask);
        {
            if (collider)
            {
                CanMovePlane = true;
                movePlane = collider.GetComponent<Plane>();
            }
            else
            {
                CanMovePlane = false;
            }
        }
    }

    private void _OnUp()
    {
        if (CanMovePlane)
        {
            TileMapUtil.SetPlanePos(movePlane, tileMap);
        }
    }

    private void _OnDrag(Vector3 pre, Vector3 touch)
    {
        if (!CanMovePlane) return;

        Vector3 preWorld = Camera.main.ScreenToWorldPoint(pre);
        Vector3 touchWorld = Camera.main.ScreenToWorldPoint(touch);

        movePlane.transform.position = new Vector3(touchWorld.x, touchWorld.y, 0);
        SpriteRenderer renderer = movePlane.GetComponent<SpriteRenderer>();
        if (renderer)
        {
            renderer.sortingOrder = Layers.SortingMovePlane;
        }

        Vector2Int cellPos = TileMapUtil.CovertDataPosToTileMapPos(movePlane.centerPos);
        Vector3 pos = tileMap.CellToWorld(new Vector3Int(cellPos.x, cellPos.y, 0));

        Vector3Int touchCell = tileMap.WorldToCell(touchWorld);
        Vector2Int tempCenter = new Vector2Int(touchCell.x, touchCell.y);
        Vector2Int touchDataPos = TileMapUtil.CovertTileMapPosToDataPos(tempCenter);

        movePlane.tempCenterPos = touchDataPos;
        movePlane.tempToward = movePlane.toward;

        Vector2Int offset = new Vector2Int(0,0);
        if (movePlane.CanSetTempPlane(checkerboard))
        {
            offset = touchDataPos - movePlane.centerPos;
        }
        else
        {
            if ((new Vector2(pos.x, pos.y) - new Vector2(touchWorld.x, touchWorld.y)).magnitude > 2.25f)
            {
                return;
            }
            Vector3Int cell = tileMap.WorldToCell(touchWorld) - tileMap.WorldToCell(preWorld);
            offset = TileMapUtil.CovertTileMapPosToDataPos(new Vector2Int(cell.x, cell.y));


            if (Mathf.Abs(offset.x) > 0 || Mathf.Abs(offset.y) > 0)
            {
                offset.x = Mathf.Min(offset.x, 1);
                offset.y = Mathf.Min(offset.y, 1);
                offset.x = Mathf.Max(offset.x, -1);
                offset.y = Mathf.Max(offset.y, -1);
            }
        }

        if (Mathf.Abs(offset.x) > 0 || Mathf.Abs(offset.y) > 0)
        {
            movePlane.Move(offset);
        }

        foreach (Plane plane in m_planes)
        {
            plane.RefreshData(checkerboard, new Vector2Int(0, 0));
        }

        TileMapUtil.Draw(tileMap, checkerboard, planeTile, wallTile);
    }

    public void RandomPlanePos()
    {
        int[,] doneCheckboard = new int[row, col];
        while (true)
        {
            int doneNum = 0;
            int[,] tempCheckboard = new int[row, col];
            for (int num = 0; num < m_planes.Count; num ++)
            {
                Array values = Enum.GetValues(typeof(PlaneToward));
                System.Random random = new System.Random();
                int indexTo = random.Next(0, values.Length);
                PlaneToward to = (PlaneToward)values.GetValue(indexTo);

                Plane plane = m_planes[num];
                plane.tempToward = to;

                Dictionary<int, List<int>> queue = new Dictionary<int, List<int>>();

                for (int i = 0; i < row; i++)
                {
                    queue[i] = new List<int>();
                    for (int j = 0; j < col; j++)
                    {
                        if (tempCheckboard[i, j] == 0)
                        {
                            queue[i].Add(j);
                        }
                    }
                }
                bool completed = false;

                while (!completed)
                {
                    List<int> keys = queue.Keys.ToList();
                    int x = random.Next(0, keys.Count);
                    int r = keys[x];

                    int y = random.Next(0, queue[r].Count);
                    int c = queue[r][y];


                    queue[r].Remove(c);
                    if (queue[r].Count < 1) queue.Remove(r);

                    if (queue.Count < 1) break;

                    plane.tempCenterPos = new Vector2Int(r, c);

                    if (!plane.CanSetTempPlane(tempCheckboard))
                    {
                        continue;
                    }
                    plane.PreSetData(tempCheckboard);
                    completed = true;
                    doneNum += 1;
                }
            }
            if (doneNum >= 3)
            {
                doneCheckboard = tempCheckboard;
                break;
            }
        }
        foreach(Plane p in m_planes)
        {
            p.RandomDone(doneCheckboard);
            p.RotatePlane();
            TileMapUtil.SetPlanePos(p, tileMap);
        }

        checkerboard = doneCheckboard;

        TileMapUtil.Draw(tileMap, checkerboard, planeTile, wallTile);
    }

    private void Save()
    {

    }
}