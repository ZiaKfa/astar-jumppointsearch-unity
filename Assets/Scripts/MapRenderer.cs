using UnityEngine;
using UnityEngine.Tilemaps;

public class MapRenderer : MonoBehaviour
{
    [Header("Assign From Inspector")]
    public Tilemap tilemap;
    public TileBase tileWalkable;
    public TileBase tileBlocked;
    public TileBase tilePath;
    public TileBase openListTile;
    public TileBase closedListTile;
    public LineRenderer lineRenderer;   // <= drag dari inspector
    bool lineInitialized = false;
    public float lineWidth = 0.1f;
    /// <summary>
    /// Render map secara benar pada orientasi grid normal.
    /// (0,0) tetap di kiri bawah, tidak invert lagi
    /// </summary>
    public void RenderFromArray(bool[,] map)
    {
        tilemap.ClearAllTiles();

        int width  = map.GetLength(0);
        int height = map.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // fix utama — balik sumbu Y dengan benar
                int ty = (height - 1 - y);

                Vector3Int pos = new Vector3Int(x, ty, 0);

                tilemap.SetTile(pos, map[x,y] ? tileWalkable : tileBlocked);
            }
        }
    }

    /// <summary>
    /// Render jalur path sesuai orientasi map fix.
    /// </summary>
public void RenderPath((int x, int y)[] path, bool[,] map)
{
    int height = map.GetLength(1);

    // Render tile path
    foreach (var p in path)
    {
        int ty = (height - 1 - p.y);
        tilemap.SetTile(new Vector3Int(p.x, ty, 0), tilePath);
    }

    DrawPathLine(path, map);
}

public void RenderOpenList((int x, int y)[] openList, bool[,] map)
{
    int height = map.GetLength(1);
    
    foreach (var p in openList)
    {
        int ty = (height - 1 - p.y);
        tilemap.SetTile(new Vector3Int(p.x, ty, 0), openListTile);
    }
}

public void RenderClosedList((int x, int y)[] closedList, bool[,] map)
 {
    int height = map.GetLength(1);
    foreach (var p in closedList)
    {
        int ty = (height - 1 - p.y);
        tilemap.SetTile(new Vector3Int(p.x, ty, 0), closedListTile);
    }
}

    /// <summary>
    /// Reset/clear path berdasarkan map asli.
    /// </summary>
public void ClearPath((int x, int y)[] path, bool[,] map)
{
    int height = map.GetLength(1);

    foreach (var p in path)
    {
        int ty = (height - 1 - p.y);
        tilemap.SetTile(new Vector3Int(p.x, ty, 0),
            map[p.x,p.y] ? tileWalkable : tileBlocked);
    }

    lineRenderer.positionCount = 0;   // 🔥 garis terhapus juga
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
        int ty = (height - 1 - path[i].y);

        // 🎯 garis langsung di tengah tile (bukan pojokan)
        Vector3 worldPos = tilemap.CellToWorld(new Vector3Int(path[i].x, ty, 0)) + 
                           new Vector3(0.5f, 0.5f, 0);

        lineRenderer.SetPosition(i, worldPos);
    }
}

}
