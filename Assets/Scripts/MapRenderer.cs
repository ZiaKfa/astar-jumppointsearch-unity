using UnityEngine;
using UnityEngine.Tilemaps;

public class MapRenderer : MonoBehaviour
{
    [Header("Assign From Inspector")]
    public Tilemap groundTilemap;
    public Tilemap obstacleTilemap;
    public TileBase[] tileWalkable;
    public TileBase[] tileBlocked;
    public TileBase tilePath;
    public TileBase openListTile;
    public TileBase closedListTile;
    public TileBase startTile;
    public TileBase endTile;
    public LineRenderer lineRenderer;
    bool lineInitialized = false;
    public float lineWidth = 0.1f;

public void RenderFromArray(bool[,] map)
{
    groundTilemap.ClearAllTiles();
    obstacleTilemap.ClearAllTiles();

    int width = map.GetLength(0);
    int height = map.GetLength(1);

    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            int ty = height - 1 - y;
            Vector3Int pos = new(x, ty, 0);

            // 🔹 GROUND SELALU ADA
            TileBase ground = Random.value < 0.75f
                ? tileWalkable[0]
                : tileWalkable[Random.Range(1, tileWalkable.Length)];

            groundTilemap.SetTile(pos, ground);

            // 🔹 OBSTACLE HANYA JIKA TIDAK WALKABLE
            if (!map[x, y])
            {
                TileBase obstacle = Random.value < 0.75f
                    ? tileBlocked[0]
                    : tileBlocked[Random.Range(1, tileBlocked.Length)];

                obstacleTilemap.SetTile(pos, obstacle);
            }
        }
    }
    
}

    public void RenderPath((int x, int y)[] path, bool[,] map)
    {
        int height = map.GetLength(1);

        for (int i = 0; i < path.Length; i++)
        {
            var (x, y) = path[i];
            int ty = height - 1 - y;
            
            if (i == 0)
                groundTilemap.SetTile(new Vector3Int(x, ty, 0), startTile);
            else if (i == path.Length - 1)
                groundTilemap.SetTile(new Vector3Int(x, ty, 0), endTile);
            else
                groundTilemap.SetTile(new Vector3Int(x, ty, 0), tilePath);
        }

        DrawPathLine(path, map);
    }

    public void RenderOpenList((int x, int y)[] openList, bool[,] map)
    {
        int height = map.GetLength(1);
        
        foreach (var p in openList)
        {
            int ty = height - 1 - p.y;
            groundTilemap.SetTile(new Vector3Int(p.x, ty, 0), openListTile);
        }
    }

    public void RenderClosedList((int x, int y)[] closedList, bool[,] map)
    {
        int height = map.GetLength(1);
        foreach (var p in closedList)
        {
            int ty = height - 1 - p.y;
            groundTilemap.SetTile(new Vector3Int(p.x, ty, 0), closedListTile);
        }
    }

public void ClearPath((int x, int y)[] path, bool[,] map)
{
    int height = map.GetLength(1);

    foreach (var p in path)
    {
        int ty = height - 1 - p.y;
        Vector3Int pos = new(p.x, ty, 0);

        // ground selalu dipulihkan
        TileBase ground = Random.value < 0.75f
            ? tileWalkable[0]
            : tileWalkable[Random.Range(1, tileWalkable.Length)];

        groundTilemap.SetTile(pos, ground);
    }

    lineRenderer.positionCount = 0;
}

    void DrawPathLine((int x, int y)[] path, bool[,] map)
    {
        int height = map.GetLength(1);

        if (!lineInitialized)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.useWorldSpace = true;
            lineInitialized = true;
        }

        lineRenderer.positionCount = path.Length;

        for (int i = 0; i < path.Length; i++)
        {
            int ty = height - 1 - path[i].y;
            Vector3 worldPos = groundTilemap.CellToWorld(new Vector3Int(path[i].x, ty, 0)) + new Vector3(0.5f, 0.5f, 0);
            lineRenderer.SetPosition(i, worldPos);
        }
    }
}
