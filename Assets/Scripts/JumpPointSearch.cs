using System;
using System.Collections.Generic;
using UnityEngine;

public static class JumpPointSearch
{
    private static readonly float Sqrt2 = 1.41421356f;

    // 🔵 DEBUGGING PROPERTIES (Float Cost)
    public static (int x, int y)[] LastOpenList { get; private set; }
    public static (int x, int y)[] LastClosedList { get; private set; }
    public static float LastFinalCost { get; private set; } = 0f;

    public static (int, int)[] FindPath(bool[,] map, int startX, int startY, int goalX, int goalY)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);

        // Reset Debug
        LastFinalCost = 0f;
        LastOpenList = Array.Empty<(int, int)>();
        LastClosedList = Array.Empty<(int, int)>();

        if (startX < 0 || startX >= w || startY < 0 || startY >= h ||
            goalX < 0 || goalX >= w || goalY < 0 || goalY >= h || !map[goalX, goalY])
            return Array.Empty<(int, int)>();

        // 1. STRUKTUR DATA (Sama dengan A*)
        float[,] gCost = new float[w, h];           // Float Cost
        (int x, int y)[,] parent = new (int x, int y)[w, h];
        bool[,] closed = new bool[w, h];

        var open = new MinHeap(w * h);

        // Tracking Visualisasi
        var openListCollector = new HashSet<int>();
        var closedListCollector = new List<(int x, int y)>();

        // Init gCost
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                gCost[x, y] = float.MaxValue;

        // Setup Start Node
        gCost[startX, startY] = 0f;
        parent[startX, startY] = (startX, startY);
        
        int startIdx = startY * w + startX;
        open.Insert(startIdx, OctileDist(startX, startY, goalX, goalY));
        openListCollector.Add(startIdx);

        while (open.Count > 0)
        {
            int cIdx = open.ExtractMin();
            int cx = cIdx % w;
            int cy = cIdx / w;

            // Update Visual Tracking
            openListCollector.Remove(cIdx);

            if (closed[cx, cy]) continue;
            closed[cx, cy] = true;
            closedListCollector.Add((cx, cy));

            if (cx == goalX && cy == goalY)
            {
                LastFinalCost = gCost[cx, cy];
                StoreOpenClosedLists(openListCollector, closedListCollector, w);
                return ReconstructPath(parent, startX, startY, goalX, goalY);
            }

            // --- JPS EXPANSION LOGIC ---
            int px = parent[cx, cy].x;
            int py = parent[cx, cy].y;

            int dx = cx - px;
            int dy = cy - py;
            
            // Normalize Direction (-1, 0, 1)
            dx = (dx > 0) ? 1 : (dx < 0) ? -1 : 0;
            dy = (dy > 0) ? 1 : (dy < 0) ? -1 : 0;

            if (cx == px && cy == py) // Start Node
            {
                // Expand 8 Directions
                Expand(cx, cy, 0, -1); Expand(cx, cy, 0, 1);
                Expand(cx, cy, -1, 0); Expand(cx, cy, 1, 0);
                Expand(cx, cy, -1, -1); Expand(cx, cy, 1, -1);
                Expand(cx, cy, -1, 1); Expand(cx, cy, 1, 1);
            }
            else
            {
                if (dx != 0 && dy != 0) // Diagonal
                {
                    Expand(cx, cy, dx, dy);
                    Expand(cx, cy, dx, 0);
                    Expand(cx, cy, 0, dy);
                    if (!IsWalkable(cx - dx, cy, map, w, h)) Expand(cx, cy, -dx, dy);
                    if (!IsWalkable(cx, cy - dy, map, w, h)) Expand(cx, cy, dx, -dy);
                }
                else if (dx != 0) // Horizontal
                {
                    Expand(cx, cy, dx, 0);
                    if (!IsWalkable(cx, cy - 1, map, w, h)) Expand(cx, cy, dx, -1);
                    if (!IsWalkable(cx, cy + 1, map, w, h)) Expand(cx, cy, dx, 1);
                }
                else // Vertical
                {
                    Expand(cx, cy, 0, dy);
                    if (!IsWalkable(cx - 1, cy, map, w, h)) Expand(cx, cy, -1, dy);
                    if (!IsWalkable(cx + 1, cy, map, w, h)) Expand(cx, cy, 1, dy);
                }
            }
        }

        StoreOpenClosedLists(openListCollector, closedListCollector, w);
        return Array.Empty<(int, int)>();

        // --- LOCAL HELPER ---
        void Expand(int cx, int cy, int dx, int dy)
        {
            TryJump(cx, cy, dx, dy, map, w, h, goalX, goalY, gCost, parent, open, openListCollector);
        }
    }

    private static bool IsWalkable(int x, int y, bool[,] map, int w, int h)
    {
        return x >= 0 && x < w && y >= 0 && y < h && map[x, y];
    }

    private static void StoreOpenClosedLists(HashSet<int> openSet, List<(int, int)> closedSet, int w)
    {
        var openList = new List<(int x, int y)>();
        foreach (int idx in openSet) openList.Add((idx % w, idx / w));
        LastOpenList = openList.ToArray();
        LastClosedList = closedSet.ToArray();
    }

    private static void TryJump(int cx, int cy, int dx, int dy, bool[,] map, int w, int h, int gx, int gy,
        float[,] gCost, (int, int)[,] parent, MinHeap open, HashSet<int> openSet)
    {
        int nx = cx + dx;
        int ny = cy + dy;

        // Validasi dasar
        if (nx < 0 || nx >= w || ny < 0 || ny >= h || !map[nx, ny]) return;
        
        // Corner Cutting Check (Agar sama dengan A*)
        if (dx != 0 && dy != 0) {
            if (!map[cx + dx, cy] && !map[cx, cy + dy]) return; 
        }

        // Lakukan Jump
        long result = Jump(cx, cy, dx, dy, map, gx, gy, w, h);
        if (result == -1) return;

        int jx = (int)(result & 0xFFFFFFFF);
        int jy = (int)(result >> 32);

        // Kalkulasi Cost dengan Float
        float dist = OctileDist(cx, cy, jx, jy);
        float newG = gCost[cx, cy] + dist;

        if (newG < gCost[jx, jy])
        {
            gCost[jx, jy] = newG;
            parent[jx, jy] = (cx, cy);

            int idx = jy * w + jx;
            float hCost = OctileDist(jx, jy, gx, gy);
            
            // Insert Float Priority
            open.Insert(idx, newG + hCost);
            openSet.Add(idx);
        }
    }

    // Fungsi Jump tetap sama (return packed long)
    private static long Jump(int cx, int cy, int dx, int dy, bool[,] map, int gx, int gy, int w, int h)
    {
        int x = cx;
        int y = cy;

        while (true)
        {
            int nx = x + dx;
            int ny = y + dy;

            if (nx < 0 || nx >= w || ny < 0 || ny >= h || !map[nx, ny]) return -1;
            
            // Corner Cutting Logic (Loose/AND)
            if (dx != 0 && dy != 0) {
                if (!map[x + dx, y] && !map[x, y + dy]) return -1;
            }

            x = nx;
            y = ny;

            if (x == gx && y == gy) return ((long)y << 32) | (uint)x;

            // Forced Neighbors
            if (dx != 0 && dy != 0) // Diagonal
            {
                if ((!IsWalkable(x - dx, y, map, w, h) && IsWalkable(x - dx, y + dy, map, w, h)) ||
                    (!IsWalkable(x, y - dy, map, w, h) && IsWalkable(x + dx, y - dy, map, w, h)))
                    return ((long)y << 32) | (uint)x;

                if (Jump(x, y, dx, 0, map, gx, gy, w, h) != -1 || 
                    Jump(x, y, 0, dy, map, gx, gy, w, h) != -1)
                    return ((long)y << 32) | (uint)x;
            }
            else if (dx != 0) // Horizontal
            {
                if ((!IsWalkable(x, y - 1, map, w, h) && IsWalkable(x + dx, y - 1, map, w, h)) ||
                    (!IsWalkable(x, y + 1, map, w, h) && IsWalkable(x + dx, y + 1, map, w, h)))
                    return ((long)y << 32) | (uint)x;
            }
            else // Vertical
            {
                if ((!IsWalkable(x - 1, y, map, w, h) && IsWalkable(x - 1, y + dy, map, w, h)) ||
                    (!IsWalkable(x + 1, y, map, w, h) && IsWalkable(x + 1, y + dy, map, w, h)))
                    return ((long)y << 32) | (uint)x;
            }
        }
    }

    // Heuristic dengan Float (1 & Sqrt2)
    private static float OctileDist(int x1, int y1, int x2, int y2)
    {
        int dx = (x1 > x2) ? x1 - x2 : x2 - x1;
        int dy = (y1 > y2) ? y1 - y2 : y2 - y1;
        if (dx > dy)
            return (Sqrt2 * dy) + (1f * (dx - dy));
        else
            return (Sqrt2 * dx) + (1f * (dy - dx));
    }

    private static (int, int)[] ReconstructPath((int x, int y)[,] parent, int sx, int sy, int gx, int gy)
    {
        var path = new List<(int, int)>();
        int x = gx;
        int y = gy;

        while (!(x == sx && y == sy))
        {
            path.Add((x, y));
            var p = parent[x, y];
            x = p.x;
            y = p.y;
        }

        path.Add((sx, sy));
        path.Reverse();
        return path.ToArray();
    }

    // MinHeap dengan Prioritas Float
    private class MinHeap
    {
        private int[] items;
        private float[] priorities; // Float
        public int Count { get; private set; }

        public MinHeap(int cap)
        {
            items = new int[cap];
            priorities = new float[cap];
            Count = 0;
        }

        public void Insert(int item, float prio)
        {
            if (Count == items.Length)
            {
                Array.Resize(ref items, Count * 2);
                Array.Resize(ref priorities, Count * 2);
            }
            items[Count] = item;
            priorities[Count] = prio;
            SiftUp(Count++);
        }

        public int ExtractMin()
        {
            int item = items[0];
            Count--;
            items[0] = items[Count];
            priorities[0] = priorities[Count];
            SiftDown(0);
            return item;
        }

        private void SiftUp(int idx)
        {
            while (idx > 0)
            {
                int p = (idx - 1) / 2;
                if (priorities[idx] >= priorities[p]) break;
                Swap(idx, p);
                idx = p;
            }
        }

        private void SiftDown(int idx)
        {
            while (true)
            {
                int l = idx * 2 + 1, r = l + 1, s = idx;
                if (l < Count && priorities[l] < priorities[s]) s = l;
                if (r < Count && priorities[r] < priorities[s]) s = r;
                if (s == idx) break;
                Swap(idx, s);
                idx = s;
            }
        }

        private void Swap(int a, int b)
        {
            int ti = items[a]; items[a] = items[b]; items[b] = ti;
            float tp = priorities[a]; priorities[a] = priorities[b]; priorities[b] = tp;
        }
    }
}