using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScenReader
{
    public class ScenItem
    {
        public int Bucket;
        public string MapName;
        public int Width;
        public int Height;
        public int StartX;
        public int StartY;
        public int GoalX;
        public int GoalY;
        public float OptimalCost;
    }

    public static List<ScenItem> LoadScen(string path)
    {
        List<ScenItem> items = new List<ScenItem>();

        if (!File.Exists(path))
        {
            Debug.LogError("SCEN file not found: " + path);
            return items;
        }

        var lines = File.ReadAllLines(path);

        // Line 0 = "version 1"
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split('\t');
            if (parts.Length < 9)
            {
                Debug.LogWarning("Invalid SCEN line skipped: " + line);
                continue;
            }

            ScenItem item = new ScenItem
            {
                Bucket = int.Parse(parts[0]),
                MapName = parts[1],
                Width = int.Parse(parts[2]),
                Height = int.Parse(parts[3]),
                StartX = int.Parse(parts[4]),
                StartY = int.Parse(parts[5]),
                GoalX = int.Parse(parts[6]),
                GoalY = int.Parse(parts[7]),
                OptimalCost = float.Parse(parts[8], System.Globalization.CultureInfo.InvariantCulture)
            };

            items.Add(item);
        }

        return items;
    }
}
