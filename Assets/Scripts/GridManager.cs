using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Tilemaps")]
    public Tilemap groundTilemap;
    public Tilemap obstacleTilemap;

    [Header("Tiles")]
    public TileBase[] groundTiles;
    public TileBase[] obstacleTiles;

    [Header("Grid Size")]
    public int width = 100;
    public int height = 100;

    public bool[,] Map { get; private set; }

    void Awake()
    {
        Instance = this;
        GenerateSimpleMap();
        RenderMap();
    }

    // =========================
    // MAP GENERATION (LOGIC)
    // =========================
    void GenerateSimpleMap()
    {
        Map = new bool[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                Map[x, y] = true;

        // tambahkan obstacle random
        for (int i = 0; i < (width * height) / 5; i++)
        {
            int ox = Random.Range(0, width);
            int oy = Random.Range(0, height);
            Map[ox, oy] = false;
        }
    }

    // =========================
    // MAP RENDERING (VISUAL)
    // =========================
    void RenderMap()
    {
        groundTilemap.ClearAllTiles();
        obstacleTilemap.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int ty = height - 1 - y;
                Vector3Int cell = new Vector3Int(x, ty, 0);

                // Ground selalu ada
                groundTilemap.SetTile(cell, GetRandomTile(groundTiles));

                // Obstacle hanya jika tidak walkable
                if (!Map[x, y])
                    obstacleTilemap.SetTile(cell, GetRandomTile(obstacleTiles));
            }
        }
    }

TileBase GetRandomTile(TileBase[] tiles)
{
    if (tiles == null || tiles.Length == 0)
        return null;

    if (tiles.Length == 1)
        return tiles[0];

    float roll = Random.value; // 0..1

    // 70% chance tile index 0
    if (roll < 0.7f)
        return tiles[0];

    // 30% dibagi rata ke sisanya
    int index = Random.Range(1, tiles.Length);
    return tiles[index];
}


    // =========================
    // GRID <-> WORLD
    // =========================
    public Vector3 GridToWorld(int x, int y)
    {
        return groundTilemap.CellToWorld(
            new Vector3Int(x, height - 1 - y, 0)
        ) + new Vector3(0.5f, 0.5f, 0f);
    }

    public bool WorldToGrid(Vector3 world, out int x, out int y)
    {
        Vector3Int cell = groundTilemap.WorldToCell(world);
        x = cell.x;
        y = height - 1 - cell.y;

        return x >= 0 && x < width && y >= 0 && y < height;
    }

    // =========================
    // RANDOM WALKABLE CELL
    // =========================
    public Vector2Int GetRandomWalkableCell()
    {
        List<Vector2Int> walkables = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (Map[x, y])
                    walkables.Add(new Vector2Int(x, y));

        if (walkables.Count == 0)
            return Vector2Int.zero;

        return walkables[Random.Range(0, walkables.Count)];
    }
}
