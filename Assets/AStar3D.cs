// using System.Collections.Generic;
// using UnityEngine;
// using System.Linq;
// using System.Collections;

// public class AStar3D : MonoBehaviour
// {
//     /** ?‹œ?‘? ?˜ Transform */
//     public Transform startPos, targetPos;

//     /** ?¥?• ë¬¼ë¡œ ?¸?‹?•  ? ˆ?´?–´ ë§ˆìŠ¤?¬ */
//     public LayerMask obstacleMask;

//     /** ì§?ë©´ìœ¼ë¡? ?¸?‹?•  ? ˆ?´?–´ ë§ˆìŠ¤?¬ */
//     public LayerMask groundMask;

//     /** ê·¸ë¦¬?“œ?˜ ?›”?“œ ?¬ê¸? */
//     public Vector3 gridWorldSize;

//     /** ê°? ?…¸?“œ?˜ ë°˜ê²½ */
//     public float nodeRadius;

//     /** ìºë¦­?„°?˜ ?´?™ ?†?„ */
//     public float moveSpeed = 5f;

//     /** ê²½ë¡œë¥? ?‹œê°í™”?•  LineRenderer */
//     public LineRenderer lineRenderer;

//     /** ìºë¦­?„°ê°? ?´?™?•  ?ˆ˜ ?ˆ?Š”ì§? ?—¬ë¶? */
//     public bool canMove = false;

//     /** 3D ê·¸ë¦¬?“œë¥? ????¥?•  ë°°ì—´ */
//     Node[,,] grid;

//     /** ?…¸?“œ?˜ ì§ê²½ (?…¸?“œ ë°˜ê²½ * 2) */
//     float nodeDiameter;

//     /** ê·¸ë¦¬?“œ?˜ X, Y, Z ?¬ê¸? */
//     int gridSizeX, gridSizeY, gridSizeZ;

//     /** ê²½ë¡œë¥? ????¥?•˜?Š” ë¦¬ìŠ¤?Š¸ */
//     List<Node> path;

//     /** ?˜„?¬ ê²½ë¡œ?—?„œ ?´?™ ì¤‘ì¸ ?…¸?“œ?˜ ?¸?±?Š¤ */
//     int pathIndex = 0;

//     /** ìºë¦­?„°?˜ BoxCollider */
//     BoxCollider boxCollider;

//     /** ê²½ë¡œ ì´ˆê¸°?™” ?—¬ë¶? */
//     bool isInitialized;

//     ///<summary>
//     /// ì´ˆê¸° ?„¤? •?„ ?œ„?•œ ?•¨?ˆ˜. ê·¸ë¦¬?“œ ?¬ê¸°ì?? ?…¸?“œ?˜ ì§ê²½?„ ê³„ì‚°?•˜ê³?, BoxColliderë¥? ì´ˆê¸°?™”.
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
//     /// ë§? ?”„? ˆ?„ë§ˆë‹¤ ê²½ë¡œë¥? ì´ˆê¸°?™”?•˜ê±°ë‚˜ ìºë¦­?„°ë¥? ?´?™?•©?‹ˆ?‹¤.
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
//     /// ?ƒˆë¡œìš´ ê²½ë¡œë¥? ì´ˆê¸°?™”?•˜ê³? ?ƒ?„±?•©?‹ˆ?‹¤. ê¸°ì¡´ ê²½ë¡œ?Š” ? œê±°í•©?‹ˆ?‹¤.
//     /// ?¥?• ë¬? ?’¤?— ëª©í‘œê°? ?ˆ?„ ê²½ìš°?—?„ ê²½ë¡œë¥? ì°¾ë„ë¡? ê°œì„ ?¨.
//     ///</summary>
//     bool InitNewPath()
//     {
//         print("Init New Path");
//         CreateGrid();

//         // ê¸°ì¡´ ê²½ë¡œê°? ì¡´ì¬?•˜?Š” ê²½ìš°, ê²½ë¡œë¥? ì§??š°ê³? LineRendererë¥? ì´ˆê¸°?™”?•©?‹ˆ?‹¤.
//         if (path != null && path.Count > 0)
//         {
//             path.Clear();
//         }
//         if (lineRenderer != null)
//         {
//             lineRenderer.positionCount = 0; // ê¸°ì¡´ ê²½ë¡œ ?‹œê°í™” ? œê±?
//         }

//         // ?ƒˆë¡œìš´ ê²½ë¡œë¥? ê³„ì‚°?•˜ê³? ?‹œê°í™”?•©?‹ˆ?‹¤.
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
//     /// ?˜„?¬ ê²½ë¡œë¥? ?”°?¼ ìºë¦­?„°ë¥? ?´?™?‹œ?‚µ?‹ˆ?‹¤.
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
//     /// ê²½ë¡œë¥? ?‹œê°í™”?•˜ê¸? ?œ„?•´ LineRenderer?— ê²½ë¡œë¥? ê·¸ë¦½?‹ˆ?‹¤.
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
//     /// 3D ê·¸ë¦¬?“œë¥? ?ƒ?„±?•˜ê³? ?…¸?“œ?˜ ?´?™ ê°??Š¥ ?—¬ë¶?ë¥? ?„¤? •?•©?‹ˆ?‹¤.
//     /// ?¥?• ë¬¼ì´ ?ˆ?Š” ê²½ìš° ?…¸?“œ?˜ ?´?™ ê°??Š¥ ?—¬ë¶?ë¥? ?—…?°?´?Š¸?•©?‹ˆ?‹¤.
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
//                     // ê°? ê·¸ë¦¬?“œ ?¬?¸?Š¸?— ????•œ ?›”?“œ ì¢Œí‘œ ê³„ì‚°
//                     Vector3 worldPoint = new Vector3(
//                         x * nodeDiameter - gridWorldSize.x / 2 + nodeRadius,
//                         y * nodeDiameter - gridWorldSize.y / 2 + nodeRadius,
//                         z * nodeDiameter - gridWorldSize.z / 2 + nodeRadius);

//                     // ì§?ë©´ì„ ?–¥?•´ Raycast ë°œì‚¬
//                     RaycastHit hit;
//                     bool walkable = Physics.Raycast(worldPoint + Vector3.up * 100, Vector3.down, out hit, Mathf.Infinity, groundMask);

//                     if (walkable)
//                     {
//                         // Rayê°? ì§?ë©´ê³¼ ì¶©ëŒ?•˜ë©? ê·? ?œ„ì¹˜ë?? ?…¸?“œ?˜ worldPosition?œ¼ë¡? ?„¤? •
//                         worldPoint = hit.point + new Vector3(0, nodeRadius, 0);
//                     }

//                     // ?¥?• ë¬¼ì´ ?ˆ?Š”ì§? ì²´í¬
//                     walkable = walkable && !Physics.CheckSphere(worldPoint, nodeRadius, obstacleMask);

//                     grid[x, y, z] = new Node(walkable, worldPoint, x, y, z);
//                 }
//             }
//         }
//     }

//     ///<summary>
//     /// ?›”?“œ ì¢Œí‘œ?— ?•´?‹¹?•˜?Š” ?…¸?“œë¥? ë°˜í™˜?•©?‹ˆ?‹¤.
//     ///</summary>
//     /// <param name="worldPosition">?›”?“œ ì¢Œí‘œ</param>
//     /// <returns>ì£¼ì–´ì§? ?›”?“œ ì¢Œí‘œ?— ?•´?‹¹?•˜?Š” Node ê°ì²´</returns>
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
//     /// A* ?•Œê³ ë¦¬ì¦˜ì„ ?‚¬?š©?•˜?—¬ ?‹œ?‘? ?—?„œ ëª©í‘œ? ê¹Œì???˜ ê²½ë¡œë¥? ì°¾ìŠµ?‹ˆ?‹¤.
//     /// ?¥?• ë¬¼ì˜ ?˜?–¥?„ ê³ ë ¤?•˜?—¬ ê²½ë¡œë¥? ê°œì„ ?•©?‹ˆ?‹¤.
//     ///</summary>
//     /// <param name="startPos">ê²½ë¡œ ?ƒ?ƒ‰ ?‹œ?‘? ?˜ ?›”?“œ ì¢Œí‘œ</param>
//     /// <param name="targetPos">ê²½ë¡œ ?ƒ?ƒ‰ ëª©í‘œ? ?˜ ?›”?“œ ì¢Œí‘œ</param>
//     /// <returns>?‹œ?‘? ?—?„œ ëª©í‘œ? ê¹Œì???˜ ê²½ë¡œë¥? ?¬?•¨?•˜?Š” Node ë¦¬ìŠ¤?Š¸</returns>
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
//                     neighbor.hCost = GetHeuristic(neighbor, targetNode); // ê°œì„ ?œ ?œ´ë¦¬ìŠ¤?‹± ?‚¬?š©
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
//     /// ???ì²? ê²½ë¡œë¥? ì°¾ê¸° ?œ„?•´ ?ƒ?ƒ‰?•©?‹ˆ?‹¤.
//     ///</summary>
//     /// <param name="startNode">ê²½ë¡œ?˜ ?‹œ?‘ ?…¸?“œ</param>
//     /// <param name="targetNode">ê²½ë¡œ?˜ ëª©í‘œ ?…¸?“œ</param>
//     /// <param name="openSet">?˜„?¬ ?—´ë¦? ?…¸?“œ ì§‘í•©</param>
//     /// <param name="closedSet">?˜„?¬ ?‹«?Œ ?…¸?“œ ì§‘í•©</param>
//     /// <param name="alternativePath">ì°¾ì?? ???ì²? ê²½ë¡œë¥? ë°˜í™˜?•˜?Š” ë¦¬ìŠ¤?Š¸</param>
//     /// <returns>???ì²? ê²½ë¡œë¥? ì°¾ì•˜?Š”ì§? ?—¬ë¶?</returns>
//     bool FindAlternativePath(Node startNode, Node targetNode, SortedSet<Node> openSet, HashSet<Node> closedSet, out List<Node> alternativePath)
//     {
//         alternativePath = new List<Node>();

//         // ???ì²? ê²½ë¡œ ?ƒ?ƒ‰?„ ?œ„?•œ ?„¤? •
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
//     /// ê²½ë¡œë¥? ë¶??“œ?Ÿ½ê²? ?•˜?—¬ ë¶ˆí•„?š”?•œ êµ½ì´ë¥? ? œê±°í•©?‹ˆ?‹¤.
//     ///</summary>
//     /// <param name="path">ë¶??“œ?Ÿ½ê²? ?•  ê²½ë¡œ ë¦¬ìŠ¤?Š¸</param>
//     /// <returns>ë¶??“œ?Ÿ½ê²? ?œ ê²½ë¡œ ë¦¬ìŠ¤?Š¸</returns>
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
//     /// ?‘ ?…¸?“œ ê°„ì˜ ?‹œê°ì  ?¥?• ë¬? ?—¬ë¶?ë¥? ì²´í¬?•˜?—¬ ì§ì„  ê²½ë¡œë¥? ?™•?¸?•©?‹ˆ?‹¤.
//     ///</summary>
//     /// <param name="startNode">ì§ì„  ê²½ë¡œ ?‹œ?‘ ?…¸?“œ</param>
//     /// <param name="endNode">ì§ì„  ê²½ë¡œ ? ?…¸?“œ</param>
//     /// <returns>?‘ ?…¸?“œ ê°„ì˜ ì§ì„  ê²½ë¡œê°? ?¥?• ë¬¼ì— ?˜?•´ ì°¨ë‹¨?˜?—ˆ?Š”ì§? ?—¬ë¶?</returns>
//     bool IsLineOfSight(Node startNode, Node endNode)
//     {
//         Vector3 direction = endNode.worldPosition - startNode.worldPosition;
//         return !Physics.Raycast(startNode.worldPosition, direction, direction.magnitude, obstacleMask);
//     }

//     ///<summary>
//     /// ?‹œ?‘ ?…¸?“œ??? ëª©í‘œ ?…¸?“œ ?‚¬?´?˜ ê²½ë¡œë¥? ?—­ì¶”ì ?•˜?—¬ ë°˜í™˜?•©?‹ˆ?‹¤.
//     ///</summary>
//     /// <param name="startNode">ê²½ë¡œ?˜ ?‹œ?‘ ?…¸?“œ</param>
//     /// <param name="endNode">ê²½ë¡œ?˜ ëª©í‘œ ?…¸?“œ</param>
//     /// <returns>?‹œ?‘ ?…¸?“œ?—?„œ ëª©í‘œ ?…¸?“œê¹Œì???˜ ê²½ë¡œë¥? ?¬?•¨?•˜?Š” Node ë¦¬ìŠ¤?Š¸</returns>
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
//     /// ?¥?• ë¬? ?šŒ?”¼ë¥? ê³ ë ¤?•˜?—¬ ?´?›ƒ ?…¸?“œë¥? ë°˜í™˜?•©?‹ˆ?‹¤.
//     ///</summary>
//     /// <param name="node">?´?›ƒ?„ ì°¾ì„ ?…¸?“œ</param>
//     /// <returns>ì£¼ì–´ì§? ?…¸?“œ?˜ ?œ ?š¨?•œ ?´?›ƒ ?…¸?“œ ë¦¬ìŠ¤?Š¸</returns>
//     List<Node> GetNeighbors(Node node)
//     {
//         List<Node> neighbors = new List<Node>();

//         // 26ê°œì˜ ë°©í–¥ ????‹  6ê°œì˜ ì£¼ìš” ë°©í–¥ (?ƒ?•˜ì¢Œìš° ?•?’¤)?œ¼ë¡? ?´?›ƒ ?…¸?“œë¥? ?ƒ?ƒ‰
//         for (int x = -1; x <= 1; x++)
//         {
//             for (int y = -1; y <= 1; y++)
//             {
//                 for (int z = -1; z <= 1; z++)
//                 {
//                     if (x == 0 && y == 0 && z == 0)
//                         continue;

//                     // ?˜„?¬ ë°©í–¥?œ¼ë¡? Raycastë¥? ë°œì‚¬?•˜?—¬ ?¥?• ë¬? ?™•?¸
//                     Vector3 direction = new Vector3(x, y, z);
//                     RaycastHit hit;
//                     if (Physics.Raycast(node.worldPosition, direction, out hit, nodeDiameter, obstacleMask))
//                     {
//                         // ?¥?• ë¬¼ì— ì¶©ëŒ?•œ ê²½ìš° ?´?›ƒ?œ¼ë¡? ì¶”ê???•˜ì§? ?•Š?Œ
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
//     /// Gizmosë¥? ?‚¬?š©?•˜?—¬ ê·¸ë¦¬?“œ??? ê²½ë¡œë¥? ?‹œê°í™”?•©?‹ˆ?‹¤.
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
//     /// ?‘ ?…¸?“œ ê°„ì˜ ?œ ?´ë¦¬ë“œ ê±°ë¦¬ë¥? ê³„ì‚°?•©?‹ˆ?‹¤.
//     ///</summary>
//     /// <param name="nodeA">ê±°ë¦¬ ê³„ì‚°?˜ ì²? ë²ˆì§¸ ?…¸?“œ</param>
//     /// <param name="nodeB">ê±°ë¦¬ ê³„ì‚°?˜ ?‘ ë²ˆì§¸ ?…¸?“œ</param>
//     /// <returns>?‘ ?…¸?“œ ê°„ì˜ ?œ ?´ë¦¬ë“œ ê±°ë¦¬</returns>
//     int GetDistance(Node nodeA, Node nodeB)
//     {
//         float dstX = Mathf.Abs(nodeA.worldPosition.x - nodeB.worldPosition.x);
//         float dstY = Mathf.Abs(nodeA.worldPosition.y - nodeB.worldPosition.y);
//         float dstZ = Mathf.Abs(nodeA.worldPosition.z - nodeB.worldPosition.z);

//         // ?œ ?´ë¦¬ë“œ ê±°ë¦¬ ê³„ì‚°
//         return Mathf.RoundToInt(Mathf.Sqrt(dstX * dstX + dstY * dstY + dstZ * dstZ));
//     }

//     ///<summary>
//     /// ?‘ ?…¸?“œ ê°„ì˜ ê±°ë¦¬ ì¶”ì •?„ ?ˆ˜?–‰?•©?‹ˆ?‹¤.
//     /// ?¥?• ë¬¼ì˜ ?˜?–¥?„ ê³ ë ¤?•˜?—¬ ëª©í‘œ ?…¸?“œê¹Œì???˜ ê±°ë¦¬ë¥? ì¶”ì •?•©?‹ˆ?‹¤.
//     ///</summary>
//     /// <param name="nodeA">ê±°ë¦¬ ê³„ì‚°?˜ ì²? ë²ˆì§¸ ?…¸?“œ</param>
//     /// <param name="nodeB">ê±°ë¦¬ ê³„ì‚°?˜ ?‘ ë²ˆì§¸ ?…¸?“œ</param>
//     /// <returns>?‘ ?…¸?“œ ê°„ì˜ ì¶”ì • ê±°ë¦¬</returns>
//     int GetHeuristic(Node nodeA, Node nodeB)
//     {
//         // ê¸°ë³¸ ?œ ?´ë¦¬ë“œ ê±°ë¦¬ ê³„ì‚°
//         float dstX = Mathf.Abs(nodeA.worldPosition.x - nodeB.worldPosition.x);
//         float dstY = Mathf.Abs(nodeA.worldPosition.y - nodeB.worldPosition.y);
//         float dstZ = Mathf.Abs(nodeA.worldPosition.z - nodeB.worldPosition.z);
//         float euclideanDistance = Mathf.Sqrt(dstX * dstX + dstY * dstY + dstZ * dstZ);

//         // ?¥?• ë¬¼ë¡œ ?¸?•œ ì¶”ê?? ê±°ë¦¬ ì¶”ì •
//         float obstaclePenalty = 0f;
//         RaycastHit hit;
//         if (Physics.Raycast(nodeA.worldPosition, (nodeB.worldPosition - nodeA.worldPosition).normalized, out hit, euclideanDistance, obstacleMask))
//         {
//             obstaclePenalty = 1f; // ?¥?• ë¬¼ë¡œ ?¸?•œ ê¸°ë³¸ ?Œ¨?„?‹°
//         }

//         // ê±°ë¦¬ ì¶”ì •ê°’ì— ?¥?• ë¬? ?Œ¨?„?‹° ì¶”ê??
//         float estimatedDistance = euclideanDistance * (1 + obstaclePenalty);

//         return Mathf.RoundToInt(estimatedDistance);
//     }
// }

// // Node ?´?˜?Š¤?Š” ê¸°ì¡´ ì½”ë“œ??? ?™?¼

// // Priority Queue?—?„œ ?…¸?“œë¥? ? •? ¬?•˜ê¸? ?œ„?•œ Comparer
// public class NodeComparer : IComparer<Node>
// {
//     ///<summary>
//     /// ?‘ ?…¸?“œë¥? ë¹„êµ?•˜?—¬ ?š°?„ ?ˆœ?œ„ë¥? ê²°ì •?•©?‹ˆ?‹¤.
//     ///</summary>
//     /// <param name="a">ë¹„êµ?•  ì²? ë²ˆì§¸ ?…¸?“œ</param>
//     /// <param name="b">ë¹„êµ?•  ?‘ ë²ˆì§¸ ?…¸?“œ</param>
//     /// <returns>?‘ ?…¸?“œ?˜ ?š°?„ ?ˆœ?œ„ë¥? ê²°ì •?•˜?Š” ? •?ˆ˜ ê°?</returns>
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
//     /** ?´?™ ê°??Š¥?•œì§? ?—¬ë¶? */
//     public bool walkable;

//     /** ?›”?“œ ì¢Œí‘œ */
//     public Vector3 worldPosition;

//     /** ê·¸ë¦¬?“œ ?‚´ X ì¢Œí‘œ */
//     public int gridX, gridY, gridZ;

//     /** gCost (?‹œ?‘ ?…¸?“œë¡œë???„°?˜ ?´?™ ë¹„ìš©) */
//     public int gCost;

//     /** hCost (ëª©í‘œ ?…¸?“œê¹Œì???˜ ì¶”ì • ë¹„ìš©) */
//     public int hCost;

//     /** ?˜„?¬ ?…¸?“œ?˜ ë¶?ëª? ?…¸?“œ */
//     public Node parent;

//     ///<summary>
//     /// ?…¸?“œë¥? ì´ˆê¸°?™”?•©?‹ˆ?‹¤.
//     ///</summary>
//     /// <param name="walkable">?…¸?“œ?˜ ?´?™ ê°??Š¥ ?—¬ë¶?</param>
//     /// <param name="worldPosition">?…¸?“œ?˜ ?›”?“œ ì¢Œí‘œ</param>
//     /// <param name="gridX">ê·¸ë¦¬?“œ ?‚´ X ì¢Œí‘œ</param>
//     /// <param name="gridY">ê·¸ë¦¬?“œ ?‚´ Y ì¢Œí‘œ</param>
//     /// <param name="gridZ">ê·¸ë¦¬?“œ ?‚´ Z ì¢Œí‘œ</param>
//     public Node(bool walkable, Vector3 worldPosition, int gridX, int gridY, int gridZ)
//     {
//         this.walkable = walkable;
//         this.worldPosition = worldPosition;
//         this.gridX = gridX;
//         this.gridY = gridY;
//         this.gridZ = gridZ;
//     }

//     ///<summary>
//     /// fCost (gCost + hCost)ë¥? ë°˜í™˜?•©?‹ˆ?‹¤.
//     ///</summary>
//     public int fCost
//     {
//         get
//         {
//             return gCost + hCost;
//         }
//     }
// }
