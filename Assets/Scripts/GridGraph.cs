// public class GridGraph
// {
//     public int Width;
//     public int Height;

//     public GridNode[,] Nodes;

//     public GridGraph(int width, int height)
//     {
//         Width = width;
//         Height = height;

//         Nodes = new GridNode[Width, Height];

//         GenerateNodes();
//     }

//     private void GenerateNodes()
//     {
//         for (int x = 0; x < Width; x++)
//         {
//             for (int y = 0; y < Height; y++)
//             {
//                 Nodes[x, y] = new GridNode(x, y, true);
//             }
//         }
//     }

//     public bool IsInsideGrid(int x, int y)
//     {
//         return x >= 0 && y >= 0 && x < Width && y < Height;
//     }

//     public GridNode GetNode(int x, int y)
//     {
//         if (!IsInsideGrid(x, y)) return null;
//         return Nodes[x, y];
//     }

//     public GridNode[] GetNeighbors(GridNode node)
//     {
//         int[,] dirs = new int[,]
//         {
//             {  1,  0 },
//             { -1,  0 },
//             {  0,  1 },
//             {  0, -1 }
//         };

//         var list = new System.Collections.Generic.List<GridNode>();

//         for (int i = 0; i < dirs.GetLength(0); i++)
//         {
//             int nx = node.X + dirs[i, 0];
//             int ny = node.Y + dirs[i, 1];

//             if (IsInsideGrid(nx, ny))
//             {
//                 GridNode n = Nodes[nx, ny];
//                 if (n.IsWalkable) list.Add(n);
//             }
//         }

//         return list.ToArray();
//     }
// }
