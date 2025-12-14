using UnityEngine;
using System.Diagnostics;   // <- perlu untuk Stopwatch

public class AStarTest : MonoBehaviour
{
    // int startX = 62;
    // int startY = 138;
    // int goalX = 36;
    // int goalY = 14;

    // void Start()
    // {
    //     string path = Application.dataPath + "/Maps/brc000d.map";

    //     bool[,] map = MapLoader.LoadMap(path);
    //     UnityEngine.Debug.Log("Map loaded: " + map.GetLength(0) + " x " + map.GetLength(1));

    //     Stopwatch sw = new Stopwatch();
    //     sw.Start();

    //     var pathResult = AStar.FindPath(
    //         map,
    //         startX, startY,    // start
    //         goalX, goalY       // goal
    //     );

    //     sw.Stop();
        

    //     if (pathResult == null || pathResult.Length == 0)
    //     {
    //         UnityEngine.Debug.Log("No path found by A*.");
    //         return;
    //     }
    //     UnityEngine.Debug.Log($"A* Time Elapsed: {sw.Elapsed.TotalMilliseconds} ms");
    //     UnityEngine.Debug.Log("A* Path length = " + pathResult.Length);

    //     // foreach (var p in pathResult)
    //     // {
    //     //     UnityEngine.Debug.Log($"A* Step: ({p.x}, {p.y})");
    //     // }
    // }
}
