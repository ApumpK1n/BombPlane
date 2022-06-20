using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class TileMapUtil
{

    public static int centerNum = 3;
    public static int headNum = 2;
    public static int bodyNum = 1;

    public static int[,] testAllyCheckboard = new int[9, 9]
    {
        {0, 0, 2, 0, 0, 0, 0, 1, 0 },
        {1, 1, 3, 1, 1, 1, 0, 1, 0 },
        {0, 0, 1, 0, 0, 1, 1, 3, 2},
        {0, 1, 1, 1, 0, 1, 0, 1, 0 },
        {0, 0, 0, 0, 0, 0, 2, 1, 0},
        {0, 0, 0, 0, 1, 1, 3, 1, 1 },
        {0, 0, 0, 0, 0, 0, 1, 0, 0 },
        {0, 0, 0, 0, 0, 1, 1, 1, 0 },
        {0, 0, 0, 0, 0, 0, 0, 0, 0 },
    };

    public static int[,] testEnemyCheckboard = new int[9, 9]
{
        {0, 0, 2, 0, 0, 0, 0, 1, 0 },
        {1, 1, 3, 1, 1, 1, 0, 1, 0 },
        {0, 0, 1, 0, 0, 1, 1, 3, 2},
        {0, 1, 1, 1, 0, 1, 0, 1, 0 },
        {0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 0, 0, 0, 2, 0, 0 },
        {0, 0, 0, 0, 1, 1, 3, 1, 1 },
        {0, 0, 0, 0, 0, 0, 1, 0, 0 },
        {0, 0, 0, 0, 0, 1, 1, 1, 0 },
};

    public static bool IsHead(int num)
    {
        return num == headNum;
    }

    public static bool IsCenter(int num)
    {
        return num == centerNum;
    }
    public static bool IsBody(int num)
    {
        return num == bodyNum || num == centerNum;
    }

    public static bool IsTileOfType<T>(Tilemap tilemap, Vector3Int position) where T : TileBase
    {
        TileBase targetTile = tilemap.GetTile(position);

        if (targetTile != null && targetTile is T)
        {
            return true;
        }

        return false;
    }

    public static Vector2Int CovertDataPosToTileMapPos(Vector2Int data)
    {
        Vector2Int cellPos = new Vector2Int(0, 0)
        {
            x = data.y,
            y = -data.x
        };
        return cellPos;
    }

    public static Vector2Int CovertTileMapPosToDataPos(Vector2Int cellPos)
    {
        Vector2Int _ = new Vector2Int(0, 0)
        {
            y = cellPos.x,
            x = -cellPos.y
        };
        return _;
    }

    public static void SetPlanePos(Plane plane, Tilemap tileMap)
    {
        Vector2Int cellPos = CovertDataPosToTileMapPos(plane.centerPos);
        Vector3 pos = tileMap.CellToWorld(new Vector3Int(cellPos.x, cellPos.y, 0));

        plane.transform.position = pos;

        plane.FixMovePlanePos();

        SpriteRenderer renderer = plane.GetComponent<SpriteRenderer>();
        if (renderer)
        {
            renderer.sortingOrder = Layers.SortingPlane;
        }
    }

    public static void SortBattleIcon(GameObject icon)
    {
        SpriteRenderer renderer = icon.GetComponent<SpriteRenderer>();
        renderer.sortingOrder = Layers.SortingBattleIcon;
    }


    public static void Draw(Tilemap tileMap, int[,] checkerboard, TileBase planeTile, TileBase wallTile)
    {
        Vector3Int coordinate = new Vector3Int(0, 0, 0);
        Debug.Log("===================");
        for (int i = 0; i < checkerboard.GetLength(0); i++)
        {
            Debug.Log($"{checkerboard[i, 0]} {checkerboard[i, 1]} {checkerboard[i, 2]} {checkerboard[i, 3]} {checkerboard[i, 4]} {checkerboard[i, 5]} {checkerboard[i, 6]} {checkerboard[i, 7]} {checkerboard[i, 8]}");
        }


        for (int i = 0; i < tileMap.size.x; i++)
        {
            for (int j = 0; j < tileMap.size.y; j++)
            {
                Vector2Int pos = CovertDataPosToTileMapPos(new Vector2Int(i, j));
                coordinate.x = pos.x;
                coordinate.y = pos.y;

                TileBase tileBase = tileMap.GetTile(coordinate);
                if (tileBase)
                {
                    if (checkerboard[i, j] != 0)
                    {
                        tileMap.SetTile(coordinate, null);
                        tileMap.SetTile(coordinate, planeTile);
                    }
                    else
                    {
                        tileMap.SetTile(coordinate, null);
                        tileMap.SetTile(coordinate, wallTile);
                    }
                }
            }
        }
    }
}
