using UnityEngine;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    [Header("Pathfinding")]
    public bool useAStar = false;
    public float moveSpeed = 4f;
    public float repathInterval = 0.3f;

    [Header("Debug")]
#if UNITY_EDITOR
    public bool drawGizmos = true;
#endif

    List<(int x, int y)> path;
    int pathIndex;

    int gx, gy;
    float timer;

    PlayerController player;

    SpriteRenderer sr;
    Vector3 lastPosition;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        player = FindFirstObjectByType<PlayerController>();

        Vector2Int cell = GridManager.Instance.GetRandomWalkableCell();
        gx = cell.x;
        gy = cell.y;

        transform.position = GridManager.Instance.GridToWorld(gx, gy);
        lastPosition = transform.position;
    }

    void Update()
    {
        if (!this || gameObject == null)
            return;

        timer += Time.deltaTime;
        if (timer >= repathInterval)
        {
            timer = 0;
            RecalculatePath();
        }

        MoveAlongPath();
        HandleFlip();
    }

    void RecalculatePath()
    {
        if (player == null) return;

        var (px, py) = player.GridPos;
        bool[,] map = GridManager.Instance.Map;

        (int, int)[] result = useAStar
            ? AStar.FindPath(map, gx, gy, px, py)
            : JumpPointSearch.FindPath(map, gx, gy, px, py);

        if (result == null || result.Length == 0)
            return;

        var rawPath = new List<(int, int)>(result);

        // 🔑 JPS wajib di-expand
        path = useAStar ? rawPath : ExpandPath(rawPath);
        pathIndex = 0;
    }

    void MoveAlongPath()
    {
        if (path == null || pathIndex >= path.Count)
            return;

        int lookAhead = 2;
        int idx = Mathf.Min(pathIndex + lookAhead, path.Count - 1);

        var node = path[idx];
        Vector3 target = GridManager.Instance.GridToWorld(node.x, node.y);

        Vector3 dir = (target - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target) < 0.2f)
        {
            gx = node.x;
            gy = node.y;
            pathIndex++;
        }
    }
    List<(int x, int y)> ExpandPath(List<(int x, int y)> rawPath)
    {
        List<(int, int)> expanded = new();

        for (int i = 0; i < rawPath.Count - 1; i++)
        {
            var (x0, y0) = rawPath[i];
            var (x1, y1) = rawPath[i + 1];

            int dx = Mathf.Clamp(x1 - x0, -1, 1);
            int dy = Mathf.Clamp(y1 - y0, -1, 1);

            int x = x0;
            int y = y0;
            expanded.Add((x, y));

            while (x != x1 || y != y1)
            {
                x += dx;
                y += dy;
                expanded.Add((x, y));
            }
        }

        return expanded;
    }

    void HandleFlip()
    {
        Vector3 delta = transform.position - lastPosition;

        if (Mathf.Abs(delta.x) > 0.001f)
            sr.flipX = delta.x < 0;

        lastPosition = transform.position;
    }


#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        if (gameObject == null) return;
        if (path == null || path.Count == 0) return;
        if (GridManager.Instance == null) return;

        // 🔴 PATH
        Gizmos.color = Color.red;
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 a = GridManager.Instance.GridToWorld(path[i].x, path[i].y);
            Vector3 b = GridManager.Instance.GridToWorld(path[i + 1].x, path[i + 1].y);
            Gizmos.DrawLine(a, b);
        }

        // 🟢 TARGET NODE
        if (pathIndex < path.Count)
        {
            var node = path[pathIndex];
            Vector3 target = GridManager.Instance.GridToWorld(node.x, node.y);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(target, 0.25f);
        }

        // 🔵 ENEMY POSITION
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        // 🟡 GRID CELL
        Gizmos.color = Color.yellow;
        Vector3 cellPos = GridManager.Instance.GridToWorld(gx, gy);
        Gizmos.DrawWireCube(cellPos, Vector3.one * 0.8f);
    }
#endif
}
