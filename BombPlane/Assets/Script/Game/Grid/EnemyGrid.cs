using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class EnemyGrid : MonoBehaviour
{
    public Plane objectPlane;
    public Tilemap tileMap;

    public GameObject Body;
    public GameObject Head;
    public GameObject Missing;

    public AllyGrid allyGrid;

    private int col = 9;
    private int row = 9;
    private int[,] nowCheckboard;
    private bool[,] clickCheckboard;

    private List<Plane> planes = new List<Plane>();
    private void Start()
    {
        nowCheckboard = TileMapUtil.testEnemyCheckboard;
        clickCheckboard = new bool[row, col];

        Plane plane = Instantiate(objectPlane);
        plane.centerPos = new Vector2Int(6, 6);

        Plane plane1 = Instantiate(objectPlane);
        plane1.centerPos = new Vector2Int(1, 2);

        Plane plane2 = Instantiate(objectPlane);
        plane2.centerPos = new Vector2Int(2, 7);
        plane2.toward = PlaneToward.Right;

        planes.Add(plane);
        planes.Add(plane1);
        planes.Add(plane2);

        foreach (Plane p in planes)
        {
            p.HidePlane();
            p.RefreshData(nowCheckboard, new Vector2Int(0, 0));
        }
    }

    private void OnEnable()
    {
        UserInput._OnClick += _OnClick;
    }

    private void OnDisable()
    {
        UserInput._OnClick -= _OnClick;
    }

    private void _OnClick()
    {
        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = tileMap.WorldToCell(world);
        Vector2Int pos = TileMapUtil.CovertTileMapPosToDataPos(new Vector2Int(cellPos.x, cellPos.y));
        if (TileMapUtil.IsTileOfType<SwitchableTile>(tileMap, cellPos) && !clickCheckboard[pos.x, pos.y])
        {
            int num = nowCheckboard[pos.x, pos.y];
            Vector3 worldPos = tileMap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
            clickCheckboard[pos.x, pos.y] = true;
            GameObject icon;
            if (TileMapUtil.IsHead(num))
            {
                GameObject head = Instantiate(Head);
                head.transform.position = worldPos;
                icon = head;
            }
            else if (TileMapUtil.IsBody(num))
            {
                GameObject body = Instantiate(Body);
                body.transform.position = worldPos;
                icon = body;
            }
            else
            {
                GameObject missing = Instantiate(Missing);
                missing.transform.position = worldPos;
                icon = missing;
            }

            TileMapUtil.SortBattleIcon(icon);
            //allyGrid.OnRecevie(pos);

            // 同步点击位置
            Heartbeat.Instance.PlayerClickCheckboard(pos);
        }

       
    }

    public void OnReceiveBattle(int[,] checkboard)
    {
        nowCheckboard = checkboard;

    }
}
