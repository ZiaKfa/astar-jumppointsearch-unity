// using System.Collections.Generic;
// using UnityEngine;
// public class GridMove : MonoBehaviour
// {
//     public float moveTime = 0.1f;
//     public float moveCooldown = 0.1f;
//     public LayerMask obstacleLayer;
//     public bool isMoving = false;
//     public int targetX;
//     public int targetY;
//     public int startX;
//     public int startY;

//     public bool useJumpPointSearch = true;
//     private AStarDebug aStar;

//     private JumpPointSearch_Debug jpsDebug;

//     private Vector3 targetPosition;
//     private float cooldownTimer = 0f;
//     void Awake()
//     {
//         if (useJumpPointSearch)
//         {
//             if (jpsDebug == null)
//                 jpsDebug = FindFirstObjectByType<JumpPointSearch_Debug>();

//             if (jpsDebug == null || jpsDebug.gridGraph == null)
//             {
//                 Debug.LogError("JumpPointSearch_Debug or its GridGraph NOT FOUND in scene!");
//                 return;
//             }
//         }
//         else
//         {
//             if (aStar == null)
//                 aStar = FindFirstObjectByType<AStarDebug>();

//             if (aStar == null || aStar.gridGraph == null)
//             {
//                 Debug.LogError("AStarDebug or its GridGraph NOT FOUND in scene!");
//                 return;
//             }

//             Node node = aStar.gridGraph.WorldToNode(transform.position);
//             Debug.Log("Player grid position = " + node.x + ", " + node.y);
//             pathfindAStar(node.x, node.y, targetX, targetY);
//         }
//     }

//     void Start()
//     {
//         if (useJumpPointSearch)
//         {
//             Node node = jpsDebug.gridGraph.WorldToNode(transform.position);
//             Debug.Log("Player grid position = " + node.x + ", " + node.y);
//             PathfindJumpPointSearch(node.x, node.y, targetX, targetY);
//         } 
            
//     }
//     void Update()
//     {
//         // decrement cooldown
//         if (cooldownTimer > 0f)
//             cooldownTimer -= Time.deltaTime;

//         // don't start a new move if currently moving or cooling down
//         if (isMoving || cooldownTimer > 0f) return;

//         int x = 0;
//         int y = 0;

//         if (Input.GetKey(KeyCode.W)) y = 1;
//         if (Input.GetKey(KeyCode.S)) y = -1;
//         if (Input.GetKey(KeyCode.A)) x = -1;
//         if (Input.GetKey(KeyCode.D)) x = 1;

//         if (x == 0 && y == 0)
//             return;

//         Vector3 direction = new Vector3(x, y, 0);
//         Vector3 newTargetPosition = transform.position + direction;
//         if (isObstacleAt(newTargetPosition))
//             return;

//         targetPosition = newTargetPosition;
//         StartCoroutine(Move());
//     }

//     public void pathfindAStar(int startX, int startY, int goalX, int goalY)
//     {
//         Debug.Log($"Walkable start= {aStar.gridGraph.IsWalkable(startX, startY)}, target={aStar.gridGraph.IsWalkable(goalX, goalY)}");
//         List<Node> path = aStar.FindPath(startX, startY, goalX, goalY);

//     }


//     public void PathfindJumpPointSearch(int startX, int startY, int goalX, int goalY)
//     {
//      Debug.Log($"Walkable start= {jpsDebug.gridGraph.IsWalkable(startX, startY)}, target={jpsDebug.gridGraph.IsWalkable(goalX, goalY)}");
//         // Ambil node start & goal
//         Node startNode = jpsDebug.gridGraph.GetNode(startX, startY);
//         Node targetNode = jpsDebug.gridGraph.GetNode(goalX, goalY);

//         if (startNode == null || targetNode == null)
//         {
//             Debug.LogWarning("Start atau Target node NULL!");
//             return;
//         }

//         // Konversi ke world position
        
//         // Jalankan JPS
//         List<Node> path = jpsDebug.FindPath(startNode.x, startNode.y, targetNode.x, targetNode.y);

//         // Jika ada path valid → gerakkan ke simpul berikutnya
//         if (path != null && path.Count > 1)
//         {
//             Node nextNode = path[1];

//             Vector3 nextPos = jpsDebug.gridGraph.GridToWorld(nextNode.x, nextNode.y);

//             transform.position = nextPos;
//         }
//     }



//     private bool isObstacleAt(Vector3 position)
//     {
//         Collider2D hit = Physics2D.OverlapCircle(position, 0.1f, obstacleLayer);
//         return hit != null;
//     }
//     private System.Collections.IEnumerator Move()
//     {
//         isMoving = true;
//         Vector3 startPosition = transform.position;
//         float elapsedTime = 0f;

//         while (elapsedTime < moveTime)
//         {
//             transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveTime);
//             elapsedTime += Time.deltaTime;
//             yield return null;
//         }

//         transform.position = targetPosition;
//         isMoving = false;

//         // start cooldown
//         cooldownTimer = moveCooldown;
//     }
// }
