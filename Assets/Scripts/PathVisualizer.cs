using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class PathVisualizer : MonoBehaviour
{
    public Tilemap tilemap;

    public Tile walkableTile;
    public Tile obstacleTile;
    public Tile aStarTile;
    public Tile jpsTile;
    public Tile startTile;
    public Tile goalTile;

    public bool[,] map;
    public List<(int x, int y)> aStarPath;
    public List<(int x, int y)> jpsPath;
    public int startX, startY, goalX, goalY;
    
    public void Visualize()
    {
        if (tilemap == null || map == null) return;

        tilemap.ClearAllTiles();

        int width = map.GetLength(0);
        int height = map.GetLength(1);

        // Gambar grid dasar
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile t = map[x, y] ? walkableTile : obstacleTile;
                tilemap.SetTile(new Vector3Int(x, y, 0), t);
            }
        }

        // Gambar path A*
        if (aStarPath != null)
        {
            foreach (var p in aStarPath)
                tilemap.SetTile(new Vector3Int(p.x, p.y, 0), aStarTile);
        }

        // Gambar path JPS
        if (jpsPath != null)
        {
            foreach (var p in jpsPath)
                tilemap.SetTile(new Vector3Int(p.x, p.y, 0), jpsTile);
        }

        // Start & Goal
        tilemap.SetTile(new Vector3Int(startX, startY, 0), startTile);
        tilemap.SetTile(new Vector3Int(goalX, goalY, 0), goalTile);
    }
}
