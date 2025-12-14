using System;
using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    private static readonly int[] dxs = { 0, 0, -1, 1, -1, 1, -1, 1 };
    private static readonly int[] dys = { -1, 1, 0, 0, -1, -1, 1, 1 };
    
    // UBAH 1: Costs sekarang Float (1 dan Akar 2)
    private static readonly float Sqrt2 = 1.41421356f;
    private static readonly float[] costs = { 
        1f, 1f, 1f, 1f,        // Lurus
        Sqrt2, Sqrt2, Sqrt2, Sqrt2 // Diagonal
    };

    // UBAH 2: Final Cost jadi float
    public static (int x, int y)[] LastOpenList { get; private set; }
    public static (int x, int y)[] LastClosedList { get; private set; }
    public static float LastFinalCost { get; private set; } = 0f;

    public static (int, int)[] FindPath(bool[,] map, int startX, int startY, int goalX, int goalY)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);
        
        LastFinalCost = 0f;
        LastOpenList = Array.Empty<(int, int)>();
        LastClosedList = Array.Empty<(int, int)>();

        if (startX < 0 || startX >= w || startY < 0 || startY >= h ||
            goalX < 0 || goalX >= w || goalY < 0 || goalY >= h || !map[goalX, goalY])
            return Array.Empty<(int, int)>();

        // UBAH 3: gCost pakai float
        float[,] gCost = new float[w, h]; 
        (int x, int y)[,] parent = new (int x, int y)[w, h];
        bool[,] closed = new bool[w, h];

        var open = new MinHeap(w * h);

        // Init gCost dengan Float.MaxValue
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                gCost[x, y] = float.MaxValue;

        gCost[startX, startY] = 0f;
        parent[startX, startY] = (startX, startY);
        
        // Insert Start Node (Heuristic juga float)
        open.Insert(startY * w + startX, OctileDist(startX, startY, goalX, goalY));

        while (open.Count > 0)
        {
            int cIdx = open.ExtractMin();
            int cx = cIdx % w;
            int cy = cIdx / w;

            if (closed[cx, cy]) continue;
            closed[cx, cy] = true;

            if (cx == goalX && cy == goalY)
            {
                LastFinalCost = gCost[cx, cy];
                LastOpenList = open.ToArray(w);
                LastClosedList = ExtractClosed(closed, w, h);
                return ReconstructPath(parent, startX, startY, goalX, goalY);
            }

            float currentG = gCost[cx, cy];

            for (int i = 0; i < 8; i++)
            {
                int nx = cx + dxs[i];
                int ny = cy + dys[i];

                if (nx < 0 || nx >= w || ny < 0 || ny >= h || !map[nx, ny]) 
                    continue;

                // Corner Cutting Rule (Loose/AND) -> Sama dengan JPS
                if (i >= 4)
                {
                    if (!map[cx, ny] && !map[nx, cy]) continue;
                }

                // UBAH 4: Kalkulasi cost pakai float
                float newG = currentG + costs[i];

                if (newG < gCost[nx, ny])
                {
                    gCost[nx, ny] = newG;
                    parent[nx, ny] = (cx, cy);
                    
                    float hCost = OctileDist(nx, ny, goalX, goalY);
                    open.Insert(ny * w + nx, newG + hCost);
                }
            }
        }

        LastOpenList = open.ToArray(w);
        LastClosedList = ExtractClosed(closed, w, h);

        return Array.Empty<(int, int)>();
    }

    // UBAH 5: Heuristic Octile dengan Float (1 & Sqrt2)
    private static float OctileDist(int x1, int y1, int x2, int y2)
    {
        // Delta tetap int karena koordinat grid
        int dx = (x1 > x2) ? x1 - x2 : x2 - x1;
        int dy = (y1 > y2) ? y1 - y2 : y2 - y1;
        
        // Rumus: (Akar2 * min) + (1 * (max - min))
        // Jika dy lebih kecil, dia sisi diagonalnya.
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

    private static (int, int)[] ExtractClosed(bool[,] closed, int w, int h)
    {
        var list = new List<(int, int)>();
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (closed[x, y]) list.Add((x, y));
            }
        }
        return list.ToArray();
    }

    // UBAH 6: MinHeap Priority jadi Float
    private class MinHeap
    {
        private int[] items;
        private float[] priorities; // Changed int -> float
        public int Count { get; private set; }

        public MinHeap(int cap)
        {
            items = new int[cap];
            priorities = new float[cap];
            Count = 0;
        }

        public void Insert(int item, float prio) // Changed int -> float
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

        public (int, int)[] ToArray(int w)
        {
            var arr = new (int, int)[Count];
            for (int i = 0; i < Count; i++)
            {
                int idx = items[i];
                arr[i] = (idx % w, idx / w);
            }
            return arr;
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
            float tp = priorities[a]; priorities[a] = priorities[b]; priorities[b] = tp; // Float swap
        }
    }
}