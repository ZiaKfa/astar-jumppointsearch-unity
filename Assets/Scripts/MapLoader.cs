using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class MapLoader
{
    public static bool[,] LoadMap(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("Map file not found: " + filePath);
            return null;
        }

        string[] lines = File.ReadAllLines(filePath);

        int width = 0;
        int height = 0;

        List<string> mapLines = new List<string>();

        bool startRead = false;

        foreach (string line in lines)
        {
            string trimmed = line.Trim();

            if (trimmed.StartsWith("width"))
            {
                width = int.Parse(trimmed.Split(' ')[1]);
            }
            else if (trimmed.StartsWith("height"))
            {
                height = int.Parse(trimmed.Split(' ')[1]);
            }
            else if (trimmed.StartsWith("map"))
            {
                startRead = true;
            }
            else if (startRead)
            {
                mapLines.Add(trimmed);
            }
        }

        bool[,] walkable = new bool[width, height];

        for (int y = 0; y < height; y++)
        {
            string row = mapLines[y];

            for (int x = 0; x < width; x++)
            {
                char c = row[x];

                walkable[x, y] = c switch
                {
                    '.' => true,
                    'G' => true,
                    'S' => true,

                    '#' => false,
                    '@' => false,
                    'T' => false,
                    _   => false
                };
            }
        }

        return walkable;
    }
}
