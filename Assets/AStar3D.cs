// using System.Collections.Generic;
// using UnityEngine;
// using System.Linq;
// using System.Collections;

// public class AStar3D : MonoBehaviour
// {
//     /** ??? ? Transform */
//     public Transform startPos, targetPos;

//     /** ?₯? λ¬Όλ‘ ?Έ??  ? ?΄?΄ λ§μ€?¬ */
//     public LayerMask obstacleMask;

//     /** μ§?λ©΄μΌλ‘? ?Έ??  ? ?΄?΄ λ§μ€?¬ */
//     public LayerMask groundMask;

//     /** κ·Έλ¦¬?? ?? ?¬κΈ? */
//     public Vector3 gridWorldSize;

//     /** κ°? ?Έ?? λ°κ²½ */
//     public float nodeRadius;

//     /** μΊλ¦­?°? ?΄? ?? */
//     public float moveSpeed = 5f;

//     /** κ²½λ‘λ₯? ?κ°ν?  LineRenderer */
//     public LineRenderer lineRenderer;

//     /** μΊλ¦­?°κ°? ?΄??  ? ??μ§? ?¬λΆ? */
//     public bool canMove = false;

//     /** 3D κ·Έλ¦¬?λ₯? ????₯?  λ°°μ΄ */
//     Node[,,] grid;

//     /** ?Έ?? μ§κ²½ (?Έ? λ°κ²½ * 2) */
//     float nodeDiameter;

//     /** κ·Έλ¦¬?? X, Y, Z ?¬κΈ? */
//     int gridSizeX, gridSizeY, gridSizeZ;

//     /** κ²½λ‘λ₯? ????₯?? λ¦¬μ€?Έ */
//     List<Node> path;

//     /** ??¬ κ²½λ‘?? ?΄? μ€μΈ ?Έ?? ?Έ?±?€ */
//     int pathIndex = 0;

//     /** μΊλ¦­?°? BoxCollider */
//     BoxCollider boxCollider;

//     /** κ²½λ‘ μ΄κΈ°? ?¬λΆ? */
//     bool isInitialized;

//     ///<summary>
//     /// μ΄κΈ° ?€? ? ?? ?¨?. κ·Έλ¦¬? ?¬κΈ°μ?? ?Έ?? μ§κ²½? κ³μ°?κ³?, BoxColliderλ₯? μ΄κΈ°?.
//     ///</summary>
//     void Start()
//     {
//         nodeDiameter = nodeRadius * 2;
//         gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
//         gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
//         gridSizeZ = Mathf.RoundToInt(gridWorldSize.z / nodeDiameter);

//         boxCollider = GetComponent<BoxCollider>();

//         canMove = true;
//     }

//     ///<summary>
//     /// λ§? ?? ?λ§λ€ κ²½λ‘λ₯? μ΄κΈ°??κ±°λ μΊλ¦­?°λ₯? ?΄??©??€.
//     ///</summary>
//     void Update()
//     {
//         if (!isInitialized)
//         {
//             if (Physics.CheckBox(transform.position, boxCollider.size, Quaternion.identity, groundMask))
//             {
//                 if (!InitNewPath())
//                 {
//                     StartCoroutine(WaitToInit());
//                 }
//                 isInitialized = true;
//             }
//         }

//         if (path != null && pathIndex < path.Count && canMove)
//         {
//             MoveAlongPath();
//         }
//         else if (path != null && pathIndex >= path.Count)
//         {
//             startPos = transform;
//             isInitialized = false;
//         }
//     }

//     ///<summary>
//     /// ?λ‘μ΄ κ²½λ‘λ₯? μ΄κΈ°??κ³? ??±?©??€. κΈ°μ‘΄ κ²½λ‘? ? κ±°ν©??€.
//     /// ?₯? λ¬? ?€? λͺ©νκ°? ?? κ²½μ°?? κ²½λ‘λ₯? μ°Ύλλ‘? κ°μ ?¨.
//     ///</summary>
//     bool InitNewPath()
//     {
//         print("Init New Path");
//         CreateGrid();

//         // κΈ°μ‘΄ κ²½λ‘κ°? μ‘΄μ¬?? κ²½μ°, κ²½λ‘λ₯? μ§??°κ³? LineRendererλ₯? μ΄κΈ°??©??€.
//         if (path != null && path.Count > 0)
//         {
//             path.Clear();
//         }
//         if (lineRenderer != null)
//         {
//             lineRenderer.positionCount = 0; // κΈ°μ‘΄ κ²½λ‘ ?κ°ν ? κ±?
//         }

//         // ?λ‘μ΄ κ²½λ‘λ₯? κ³μ°?κ³? ?κ°ν?©??€.
//         path = FindPath(startPos.position, targetPos.position);

//         if (path == null || path.Count == 0)
//         {
//             return false;
//         }

//         pathIndex = 0;
//         DrawPath();

//         return true;
//     }

//     IEnumerator WaitToInit()
//     {
//         yield return new WaitForSeconds(3.0f);
//         isInitialized = false;
//     }



//     ///<summary>
//     /// ??¬ κ²½λ‘λ₯? ?°?Ό μΊλ¦­?°λ₯? ?΄???΅??€.
//     ///</summary>
//     void MoveAlongPath()
//     {
//         Node targetNode = path[pathIndex];
//         Vector3 targetPosition = targetNode.worldPosition;
//         Vector3 moveDirection = (targetPosition - transform.position).normalized;

//         if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
//         {
//             pathIndex++;
//         }
//         else
//         {
//             transform.position += moveDirection * moveSpeed * Time.deltaTime;
//         }
//     }

//     ///<summary>
//     /// κ²½λ‘λ₯? ?κ°ν?κΈ? ??΄ LineRenderer? κ²½λ‘λ₯? κ·Έλ¦½??€.
//     ///</summary>
//     void DrawPath()
//     {
//         if (lineRenderer == null) return;

//         lineRenderer.positionCount = path.Count;
//         Vector3[] positions = new Vector3[path.Count];

//         for (int i = 0; i < path.Count; i++)
//         {
//             positions[i] = path[i].worldPosition;
//         }

//         lineRenderer.SetPositions(positions);
//     }

//     ///<summary>
//     /// 3D κ·Έλ¦¬?λ₯? ??±?κ³? ?Έ?? ?΄? κ°??₯ ?¬λΆ?λ₯? ?€? ?©??€.
//     /// ?₯? λ¬Όμ΄ ?? κ²½μ° ?Έ?? ?΄? κ°??₯ ?¬λΆ?λ₯? ??°?΄?Έ?©??€.
//     ///</summary>
//     void CreateGrid()
//     {
//         grid = new Node[gridSizeX, gridSizeY, gridSizeZ];

//         for (int x = 0; x < gridSizeX; x++)
//         {
//             for (int y = 0; y < gridSizeY; y++)
//             {
//                 for (int z = 0; z < gridSizeZ; z++)
//                 {
//                     // κ°? κ·Έλ¦¬? ?¬?Έ?Έ? ???? ?? μ’ν κ³μ°
//                     Vector3 worldPoint = new Vector3(
//                         x * nodeDiameter - gridWorldSize.x / 2 + nodeRadius,
//                         y * nodeDiameter - gridWorldSize.y / 2 + nodeRadius,
//                         z * nodeDiameter - gridWorldSize.z / 2 + nodeRadius);

//                     // μ§?λ©΄μ ?₯?΄ Raycast λ°μ¬
//                     RaycastHit hit;
//                     bool walkable = Physics.Raycast(worldPoint + Vector3.up * 100, Vector3.down, out hit, Mathf.Infinity, groundMask);

//                     if (walkable)
//                     {
//                         // Rayκ°? μ§?λ©΄κ³Ό μΆ©λ?λ©? κ·? ?μΉλ?? ?Έ?? worldPosition?Όλ‘? ?€? 
//                         worldPoint = hit.point + new Vector3(0, nodeRadius, 0);
//                     }

//                     // ?₯? λ¬Όμ΄ ??μ§? μ²΄ν¬
//                     walkable = walkable && !Physics.CheckSphere(worldPoint, nodeRadius, obstacleMask);

//                     grid[x, y, z] = new Node(walkable, worldPoint, x, y, z);
//                 }
//             }
//         }
//     }

//     ///<summary>
//     /// ?? μ’ν? ?΄?Ή?? ?Έ?λ₯? λ°ν?©??€.
//     ///</summary>
//     /// <param name="worldPosition">?? μ’ν</param>
//     /// <returns>μ£Όμ΄μ§? ?? μ’ν? ?΄?Ή?? Node κ°μ²΄</returns>
//     Node NodeFromWorldPoint(Vector3 worldPosition)
//     {
//         float percentX = Mathf.Clamp01((worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x);
//         float percentY = Mathf.Clamp01((worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y);
//         float percentZ = Mathf.Clamp01((worldPosition.z + gridWorldSize.z / 2) / gridWorldSize.z);

//         int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
//         int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
//         int z = Mathf.RoundToInt((gridSizeZ - 1) * percentZ);

//         return grid[x, y, z];
//     }

//     ///<summary>
//     /// A* ?κ³ λ¦¬μ¦μ ?¬?©??¬ ??? ?? λͺ©ν? κΉμ??? κ²½λ‘λ₯? μ°Ύμ΅??€.
//     /// ?₯? λ¬Όμ ??₯? κ³ λ €??¬ κ²½λ‘λ₯? κ°μ ?©??€.
//     ///</summary>
//     /// <param name="startPos">κ²½λ‘ ?? ??? ? ?? μ’ν</param>
//     /// <param name="targetPos">κ²½λ‘ ?? λͺ©ν? ? ?? μ’ν</param>
//     /// <returns>??? ?? λͺ©ν? κΉμ??? κ²½λ‘λ₯? ?¬?¨?? Node λ¦¬μ€?Έ</returns>
//     public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
//     {
//         Node startNode = NodeFromWorldPoint(startPos);
//         Node targetNode = NodeFromWorldPoint(targetPos);

//         if (!startNode.walkable || !targetNode.walkable)
//         {
//             Debug.LogWarning("Start or target node is not walkable.");
//             return null;
//         }

//         SortedSet<Node> openSet = new SortedSet<Node>(new NodeComparer());
//         HashSet<Node> closedSet = new HashSet<Node>();
//         openSet.Add(startNode);

//         int steps = 0;

//         while (openSet.Count > 0)
//         {
//             if (steps >= 300000) break;
//             steps++;
//             Node currentNode = openSet.First();
//             openSet.Remove(currentNode);
//             closedSet.Add(currentNode);

//             if (currentNode == targetNode)
//             {
//                 Debug.Log($"Path found in {steps} steps.");
//                 List<Node> path = RetracePath(startNode, targetNode);
//                 return SmoothPath(path);
//             }

//             foreach (Node neighbor in GetNeighbors(currentNode))
//             {
//                 if (!neighbor.walkable || closedSet.Contains(neighbor))
//                     continue;

//                 int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
//                 if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
//                 {
//                     neighbor.gCost = newMovementCostToNeighbor;
//                     neighbor.hCost = GetHeuristic(neighbor, targetNode); // κ°μ ? ?΄λ¦¬μ€?± ?¬?©
//                     neighbor.parent = currentNode;

//                     if (!openSet.Contains(neighbor))
//                         openSet.Add(neighbor);
//                 }
//             }
//         }

//         Debug.LogError("Can't reach the target. Possible reasons include: \n" +
//                        "- Start or target node is not walkable\n" +
//                        "- Path is completely blocked by obstacles\n" +
//                        "- No valid path exists within the search constraints");
//         return null;
//     }

//     ///<summary>
//     /// ???μ²? κ²½λ‘λ₯? μ°ΎκΈ° ??΄ ???©??€.
//     ///</summary>
//     /// <param name="startNode">κ²½λ‘? ?? ?Έ?</param>
//     /// <param name="targetNode">κ²½λ‘? λͺ©ν ?Έ?</param>
//     /// <param name="openSet">??¬ ?΄λ¦? ?Έ? μ§ν©</param>
//     /// <param name="closedSet">??¬ ?«? ?Έ? μ§ν©</param>
//     /// <param name="alternativePath">μ°Ύμ?? ???μ²? κ²½λ‘λ₯? λ°ν?? λ¦¬μ€?Έ</param>
//     /// <returns>???μ²? κ²½λ‘λ₯? μ°Ύμ?μ§? ?¬λΆ?</returns>
//     bool FindAlternativePath(Node startNode, Node targetNode, SortedSet<Node> openSet, HashSet<Node> closedSet, out List<Node> alternativePath)
//     {
//         alternativePath = new List<Node>();

//         // ???μ²? κ²½λ‘ ??? ?? ?€? 
//         SortedSet<Node> tempOpenSet = new SortedSet<Node>(new NodeComparer());
//         HashSet<Node> tempClosedSet = new HashSet<Node>();
//         tempOpenSet.Add(startNode);

//         int tempSteps = 0;
//         while (tempOpenSet.Count > 0)
//         {
//             if (tempSteps >= 300000) break;
//             tempSteps++;
//             Node currentNode = tempOpenSet.First();
//             tempOpenSet.Remove(currentNode);
//             tempClosedSet.Add(currentNode);

//             if (currentNode == targetNode)
//             {
//                 alternativePath = RetracePath(startNode, targetNode);
//                 return true;
//             }

//             foreach (Node neighbor in GetNeighbors(currentNode))
//             {
//                 if (!neighbor.walkable || tempClosedSet.Contains(neighbor))
//                     continue;

//                 int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
//                 if (newMovementCostToNeighbor < neighbor.gCost || !tempOpenSet.Contains(neighbor))
//                 {
//                     neighbor.gCost = newMovementCostToNeighbor;
//                     neighbor.hCost = GetDistance(neighbor, targetNode);
//                     neighbor.parent = currentNode;

//                     if (!tempOpenSet.Contains(neighbor))
//                         tempOpenSet.Add(neighbor);
//                 }
//             }
//         }

//         Debug.LogWarning("No alternative path found during the alternative path search.");
//         return false;
//     }

//     ///<summary>
//     /// κ²½λ‘λ₯? λΆ???½κ²? ??¬ λΆν?? κ΅½μ΄λ₯? ? κ±°ν©??€.
//     ///</summary>
//     /// <param name="path">λΆ???½κ²? ?  κ²½λ‘ λ¦¬μ€?Έ</param>
//     /// <returns>λΆ???½κ²? ? κ²½λ‘ λ¦¬μ€?Έ</returns>
//     List<Node> SmoothPath(List<Node> path)
//     {
//         if (path == null || path.Count < 3)
//             return path;

//         List<Node> smoothPath = new List<Node> { path[0] };
//         Node prevNode = path[0];
//         Node currentNode = path[1];

//         for (int i = 2; i < path.Count; i++)
//         {
//             Node nextNode = path[i];
//             if (!IsLineOfSight(prevNode, nextNode))
//             {
//                 smoothPath.Add(currentNode);
//                 prevNode = currentNode;
//             }
//             currentNode = nextNode;
//         }

//         smoothPath.Add(currentNode);
//         return smoothPath;
//     }

//     ///<summary>
//     /// ? ?Έ? κ°μ ?κ°μ  ?₯? λ¬? ?¬λΆ?λ₯? μ²΄ν¬??¬ μ§μ  κ²½λ‘λ₯? ??Έ?©??€.
//     ///</summary>
//     /// <param name="startNode">μ§μ  κ²½λ‘ ?? ?Έ?</param>
//     /// <param name="endNode">μ§μ  κ²½λ‘ ? ?Έ?</param>
//     /// <returns>? ?Έ? κ°μ μ§μ  κ²½λ‘κ°? ?₯? λ¬Όμ ??΄ μ°¨λ¨???μ§? ?¬λΆ?</returns>
//     bool IsLineOfSight(Node startNode, Node endNode)
//     {
//         Vector3 direction = endNode.worldPosition - startNode.worldPosition;
//         return !Physics.Raycast(startNode.worldPosition, direction, direction.magnitude, obstacleMask);
//     }

//     ///<summary>
//     /// ?? ?Έ???? λͺ©ν ?Έ? ?¬?΄? κ²½λ‘λ₯? ?­μΆμ ??¬ λ°ν?©??€.
//     ///</summary>
//     /// <param name="startNode">κ²½λ‘? ?? ?Έ?</param>
//     /// <param name="endNode">κ²½λ‘? λͺ©ν ?Έ?</param>
//     /// <returns>?? ?Έ??? λͺ©ν ?Έ?κΉμ??? κ²½λ‘λ₯? ?¬?¨?? Node λ¦¬μ€?Έ</returns>
//     List<Node> RetracePath(Node startNode, Node endNode)
//     {
//         List<Node> path = new List<Node>();
//         Node currentNode = endNode;

//         while (currentNode != startNode)
//         {
//             path.Add(currentNode);
//             currentNode = currentNode.parent;
//         }
//         path.Reverse();
//         return path;
//     }

//     ///<summary>
//     /// ?₯? λ¬? ??Όλ₯? κ³ λ €??¬ ?΄? ?Έ?λ₯? λ°ν?©??€.
//     ///</summary>
//     /// <param name="node">?΄?? μ°Ύμ ?Έ?</param>
//     /// <returns>μ£Όμ΄μ§? ?Έ?? ? ?¨? ?΄? ?Έ? λ¦¬μ€?Έ</returns>
//     List<Node> GetNeighbors(Node node)
//     {
//         List<Node> neighbors = new List<Node>();

//         // 26κ°μ λ°©ν₯ ????  6κ°μ μ£Όμ λ°©ν₯ (??μ’μ° ??€)?Όλ‘? ?΄? ?Έ?λ₯? ??
//         for (int x = -1; x <= 1; x++)
//         {
//             for (int y = -1; y <= 1; y++)
//             {
//                 for (int z = -1; z <= 1; z++)
//                 {
//                     if (x == 0 && y == 0 && z == 0)
//                         continue;

//                     // ??¬ λ°©ν₯?Όλ‘? Raycastλ₯? λ°μ¬??¬ ?₯? λ¬? ??Έ
//                     Vector3 direction = new Vector3(x, y, z);
//                     RaycastHit hit;
//                     if (Physics.Raycast(node.worldPosition, direction, out hit, nodeDiameter, obstacleMask))
//                     {
//                         // ?₯? λ¬Όμ μΆ©λ? κ²½μ° ?΄??Όλ‘? μΆκ???μ§? ??
//                         continue;
//                     }

//                     int checkX = node.gridX + x;
//                     int checkY = node.gridY + y;
//                     int checkZ = node.gridZ + z;

//                     if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY && checkZ >= 0 && checkZ < gridSizeZ)
//                     {
//                         neighbors.Add(grid[checkX, checkY, checkZ]);
//                     }
//                 }
//             }
//         }

//         return neighbors;
//     }


//     ///<summary>
//     /// Gizmosλ₯? ?¬?©??¬ κ·Έλ¦¬???? κ²½λ‘λ₯? ?κ°ν?©??€.
//     ///</summary>
//     void OnDrawGizmos()
//     {
//         Gizmos.color = Color.blue;
//         Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, gridWorldSize.z));

//         if (grid != null)
//         {
//             // foreach (Node node in grid)
//             // {
//             //     Gizmos.color = node.walkable ? Color.blue : Color.red;
//             //     if (!node.walkable)
//             //     {
//             //         Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
//             //     }
//             // }
//         }

//         if (path != null)
//         {

//         }
//     }

//     ///<summary>
//     /// ? ?Έ? κ°μ ? ?΄λ¦¬λ κ±°λ¦¬λ₯? κ³μ°?©??€.
//     ///</summary>
//     /// <param name="nodeA">κ±°λ¦¬ κ³μ°? μ²? λ²μ§Έ ?Έ?</param>
//     /// <param name="nodeB">κ±°λ¦¬ κ³μ°? ? λ²μ§Έ ?Έ?</param>
//     /// <returns>? ?Έ? κ°μ ? ?΄λ¦¬λ κ±°λ¦¬</returns>
//     int GetDistance(Node nodeA, Node nodeB)
//     {
//         float dstX = Mathf.Abs(nodeA.worldPosition.x - nodeB.worldPosition.x);
//         float dstY = Mathf.Abs(nodeA.worldPosition.y - nodeB.worldPosition.y);
//         float dstZ = Mathf.Abs(nodeA.worldPosition.z - nodeB.worldPosition.z);

//         // ? ?΄λ¦¬λ κ±°λ¦¬ κ³μ°
//         return Mathf.RoundToInt(Mathf.Sqrt(dstX * dstX + dstY * dstY + dstZ * dstZ));
//     }

//     ///<summary>
//     /// ? ?Έ? κ°μ κ±°λ¦¬ μΆμ ? ???©??€.
//     /// ?₯? λ¬Όμ ??₯? κ³ λ €??¬ λͺ©ν ?Έ?κΉμ??? κ±°λ¦¬λ₯? μΆμ ?©??€.
//     ///</summary>
//     /// <param name="nodeA">κ±°λ¦¬ κ³μ°? μ²? λ²μ§Έ ?Έ?</param>
//     /// <param name="nodeB">κ±°λ¦¬ κ³μ°? ? λ²μ§Έ ?Έ?</param>
//     /// <returns>? ?Έ? κ°μ μΆμ  κ±°λ¦¬</returns>
//     int GetHeuristic(Node nodeA, Node nodeB)
//     {
//         // κΈ°λ³Έ ? ?΄λ¦¬λ κ±°λ¦¬ κ³μ°
//         float dstX = Mathf.Abs(nodeA.worldPosition.x - nodeB.worldPosition.x);
//         float dstY = Mathf.Abs(nodeA.worldPosition.y - nodeB.worldPosition.y);
//         float dstZ = Mathf.Abs(nodeA.worldPosition.z - nodeB.worldPosition.z);
//         float euclideanDistance = Mathf.Sqrt(dstX * dstX + dstY * dstY + dstZ * dstZ);

//         // ?₯? λ¬Όλ‘ ?Έ? μΆκ?? κ±°λ¦¬ μΆμ 
//         float obstaclePenalty = 0f;
//         RaycastHit hit;
//         if (Physics.Raycast(nodeA.worldPosition, (nodeB.worldPosition - nodeA.worldPosition).normalized, out hit, euclideanDistance, obstacleMask))
//         {
//             obstaclePenalty = 1f; // ?₯? λ¬Όλ‘ ?Έ? κΈ°λ³Έ ?¨??°
//         }

//         // κ±°λ¦¬ μΆμ κ°μ ?₯? λ¬? ?¨??° μΆκ??
//         float estimatedDistance = euclideanDistance * (1 + obstaclePenalty);

//         return Mathf.RoundToInt(estimatedDistance);
//     }
// }

// // Node ?΄??€? κΈ°μ‘΄ μ½λ??? ??Ό

// // Priority Queue?? ?Έ?λ₯? ? ? ¬?κΈ? ?? Comparer
// public class NodeComparer : IComparer<Node>
// {
//     ///<summary>
//     /// ? ?Έ?λ₯? λΉκ΅??¬ ?°? ??λ₯? κ²°μ ?©??€.
//     ///</summary>
//     /// <param name="a">λΉκ΅?  μ²? λ²μ§Έ ?Έ?</param>
//     /// <param name="b">λΉκ΅?  ? λ²μ§Έ ?Έ?</param>
//     /// <returns>? ?Έ?? ?°? ??λ₯? κ²°μ ?? ? ? κ°?</returns>
//     public int Compare(Node a, Node b)
//     {
//         int compare = a.fCost.CompareTo(b.fCost);
//         if (compare == 0)
//         {
//             compare = a.hCost.CompareTo(b.hCost);
//         }
//         return compare;
//     }
// }

// public class Node
// {
//     /** ?΄? κ°??₯?μ§? ?¬λΆ? */
//     public bool walkable;

//     /** ?? μ’ν */
//     public Vector3 worldPosition;

//     /** κ·Έλ¦¬? ?΄ X μ’ν */
//     public int gridX, gridY, gridZ;

//     /** gCost (?? ?Έ?λ‘λ???°? ?΄? λΉμ©) */
//     public int gCost;

//     /** hCost (λͺ©ν ?Έ?κΉμ??? μΆμ  λΉμ©) */
//     public int hCost;

//     /** ??¬ ?Έ?? λΆ?λͺ? ?Έ? */
//     public Node parent;

//     ///<summary>
//     /// ?Έ?λ₯? μ΄κΈ°??©??€.
//     ///</summary>
//     /// <param name="walkable">?Έ?? ?΄? κ°??₯ ?¬λΆ?</param>
//     /// <param name="worldPosition">?Έ?? ?? μ’ν</param>
//     /// <param name="gridX">κ·Έλ¦¬? ?΄ X μ’ν</param>
//     /// <param name="gridY">κ·Έλ¦¬? ?΄ Y μ’ν</param>
//     /// <param name="gridZ">κ·Έλ¦¬? ?΄ Z μ’ν</param>
//     public Node(bool walkable, Vector3 worldPosition, int gridX, int gridY, int gridZ)
//     {
//         this.walkable = walkable;
//         this.worldPosition = worldPosition;
//         this.gridX = gridX;
//         this.gridY = gridY;
//         this.gridZ = gridZ;
//     }

//     ///<summary>
//     /// fCost (gCost + hCost)λ₯? λ°ν?©??€.
//     ///</summary>
//     public int fCost
//     {
//         get
//         {
//             return gCost + hCost;
//         }
//     }
// }
