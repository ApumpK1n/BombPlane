using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class AllyGrid : MonoBehaviour
{
    public Plane objectPlane;
    public Tilemap tileMap;

    public GameObject Body;
    public GameObject Head;
    public GameObject Missing;

    public Tile planeTile;
    public Tile wallTile;

    private int col = 9;
    private int row = 9;
    private int[,] nowCheckboard;

    private List<Plane> planes = new List<Plane>();
    private void Start()
    {
        nowCheckboard = TileMapUtil.testAllyCheckboard;

        List<Vector2Int> heads = new List<Vector2Int>();
        for(int i=0; i<nowCheckboard.GetLength(0); i++)
        {
            for(int j=0; j<nowCheckboard.GetLength(1); j++)
            {
                if (nowCheckboard[i, j] == TileMapUtil.headNum)
                {
                    heads.Add(new Vector2Int(i, j));
                }
            }
        }

        foreach(Vector2Int head in heads)
        {
            Plane plane = Instantiate(objectPlane);
            plane.CalculateCenterPosAndToward(head, nowCheckboard);
            plane.ScaleParam = 0.5f;
            plane.RotatePlane();
            TileMapUtil.SetPlanePos(plane, tileMap);
            plane.transform.localScale = 0.5f * plane.transform.localScale;

            planes.Add(plane);
        }

        TileMapUtil.Draw(tileMap, nowCheckboard, planeTile, wallTile);
    }

    public void OnRecevie(Vector2Int pos)
    {
        Vector2Int cell =  TileMapUtil.CovertDataPosToTileMapPos(pos);
        Vector3Int cellPos = new Vector3Int(cell.x, cell.y, 0);
        if (TileMapUtil.IsTileOfType<TileBase>(tileMap, cellPos))
        {
            int num = nowCheckboard[pos.x, pos.y];
            Vector3 worldPos = tileMap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0) * 0.5f;

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
            icon.transform.localScale = 0.5f * icon.transform.localScale;
            TileMapUtil.SortBattleIcon(icon);
        }
    }

    public void OnReceiveBattle(int[,] checkboard)
    {
        nowCheckboard = checkboard;

    }
}
