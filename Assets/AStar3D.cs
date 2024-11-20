// using System.Collections.Generic;
// using UnityEngine;
// using System.Linq;
// using System.Collections;

// public class AStar3D : MonoBehaviour
// {
//     /** ?��?��?��?�� Transform */
//     public Transform startPos, targetPos;

//     /** ?��?��물로 ?��?��?�� ?��?��?�� 마스?�� */
//     public LayerMask obstacleMask;

//     /** �?면으�? ?��?��?�� ?��?��?�� 마스?�� */
//     public LayerMask groundMask;

//     /** 그리?��?�� ?��?�� ?���? */
//     public Vector3 gridWorldSize;

//     /** �? ?��?��?�� 반경 */
//     public float nodeRadius;

//     /** 캐릭?��?�� ?��?�� ?��?�� */
//     public float moveSpeed = 5f;

//     /** 경로�? ?��각화?�� LineRenderer */
//     public LineRenderer lineRenderer;

//     /** 캐릭?���? ?��?��?�� ?�� ?��?���? ?���? */
//     public bool canMove = false;

//     /** 3D 그리?���? ????��?�� 배열 */
//     Node[,,] grid;

//     /** ?��?��?�� 직경 (?��?�� 반경 * 2) */
//     float nodeDiameter;

//     /** 그리?��?�� X, Y, Z ?���? */
//     int gridSizeX, gridSizeY, gridSizeZ;

//     /** 경로�? ????��?��?�� 리스?�� */
//     List<Node> path;

//     /** ?��?�� 경로?��?�� ?��?�� 중인 ?��?��?�� ?��?��?�� */
//     int pathIndex = 0;

//     /** 캐릭?��?�� BoxCollider */
//     BoxCollider boxCollider;

//     /** 경로 초기?�� ?���? */
//     bool isInitialized;

//     ///<summary>
//     /// 초기 ?��?��?�� ?��?�� ?��?��. 그리?�� ?��기�?? ?��?��?�� 직경?�� 계산?���?, BoxCollider�? 초기?��.
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
//     /// �? ?��?��?��마다 경로�? 초기?��?��거나 캐릭?���? ?��?��?��?��?��.
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
//     /// ?��로운 경로�? 초기?��?���? ?��?��?��?��?��. 기존 경로?�� ?��거합?��?��.
//     /// ?��?���? ?��?�� 목표�? ?��?�� 경우?��?�� 경로�? 찾도�? 개선?��.
//     ///</summary>
//     bool InitNewPath()
//     {
//         print("Init New Path");
//         CreateGrid();

//         // 기존 경로�? 존재?��?�� 경우, 경로�? �??���? LineRenderer�? 초기?��?��?��?��.
//         if (path != null && path.Count > 0)
//         {
//             path.Clear();
//         }
//         if (lineRenderer != null)
//         {
//             lineRenderer.positionCount = 0; // 기존 경로 ?��각화 ?���?
//         }

//         // ?��로운 경로�? 계산?���? ?��각화?��?��?��.
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
//     /// ?��?�� 경로�? ?��?�� 캐릭?���? ?��?��?��?��?��?��.
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
//     /// 경로�? ?��각화?���? ?��?�� LineRenderer?�� 경로�? 그립?��?��.
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
//     /// 3D 그리?���? ?��?��?���? ?��?��?�� ?��?�� �??�� ?���?�? ?��?��?��?��?��.
//     /// ?��?��물이 ?��?�� 경우 ?��?��?�� ?��?�� �??�� ?���?�? ?��?��?��?��?��?��?��.
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
//                     // �? 그리?�� ?��?��?��?�� ????�� ?��?�� 좌표 계산
//                     Vector3 worldPoint = new Vector3(
//                         x * nodeDiameter - gridWorldSize.x / 2 + nodeRadius,
//                         y * nodeDiameter - gridWorldSize.y / 2 + nodeRadius,
//                         z * nodeDiameter - gridWorldSize.z / 2 + nodeRadius);

//                     // �?면을 ?��?�� Raycast 발사
//                     RaycastHit hit;
//                     bool walkable = Physics.Raycast(worldPoint + Vector3.up * 100, Vector3.down, out hit, Mathf.Infinity, groundMask);

//                     if (walkable)
//                     {
//                         // Ray�? �?면과 충돌?���? �? ?��치�?? ?��?��?�� worldPosition?���? ?��?��
//                         worldPoint = hit.point + new Vector3(0, nodeRadius, 0);
//                     }

//                     // ?��?��물이 ?��?���? 체크
//                     walkable = walkable && !Physics.CheckSphere(worldPoint, nodeRadius, obstacleMask);

//                     grid[x, y, z] = new Node(walkable, worldPoint, x, y, z);
//                 }
//             }
//         }
//     }

//     ///<summary>
//     /// ?��?�� 좌표?�� ?��?��?��?�� ?��?���? 반환?��?��?��.
//     ///</summary>
//     /// <param name="worldPosition">?��?�� 좌표</param>
//     /// <returns>주어�? ?��?�� 좌표?�� ?��?��?��?�� Node 객체</returns>
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
//     /// A* ?��고리즘을 ?��?��?��?�� ?��?��?��?��?�� 목표?��까�???�� 경로�? 찾습?��?��.
//     /// ?��?��물의 ?��?��?�� 고려?��?�� 경로�? 개선?��?��?��.
//     ///</summary>
//     /// <param name="startPos">경로 ?��?�� ?��?��?��?�� ?��?�� 좌표</param>
//     /// <param name="targetPos">경로 ?��?�� 목표?��?�� ?��?�� 좌표</param>
//     /// <returns>?��?��?��?��?�� 목표?��까�???�� 경로�? ?��?��?��?�� Node 리스?��</returns>
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
//                     neighbor.hCost = GetHeuristic(neighbor, targetNode); // 개선?�� ?��리스?�� ?��?��
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
//     /// ???�? 경로�? 찾기 ?��?�� ?��?��?��?��?��.
//     ///</summary>
//     /// <param name="startNode">경로?�� ?��?�� ?��?��</param>
//     /// <param name="targetNode">경로?�� 목표 ?��?��</param>
//     /// <param name="openSet">?��?�� ?���? ?��?�� 집합</param>
//     /// <param name="closedSet">?��?�� ?��?�� ?��?�� 집합</param>
//     /// <param name="alternativePath">찾�?? ???�? 경로�? 반환?��?�� 리스?��</param>
//     /// <returns>???�? 경로�? 찾았?���? ?���?</returns>
//     bool FindAlternativePath(Node startNode, Node targetNode, SortedSet<Node> openSet, HashSet<Node> closedSet, out List<Node> alternativePath)
//     {
//         alternativePath = new List<Node>();

//         // ???�? 경로 ?��?��?�� ?��?�� ?��?��
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
//     /// 경로�? �??��?���? ?��?�� 불필?��?�� 굽이�? ?��거합?��?��.
//     ///</summary>
//     /// <param name="path">�??��?���? ?�� 경로 리스?��</param>
//     /// <returns>�??��?���? ?�� 경로 리스?��</returns>
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
//     /// ?�� ?��?�� 간의 ?��각적 ?��?���? ?���?�? 체크?��?�� 직선 경로�? ?��?��?��?��?��.
//     ///</summary>
//     /// <param name="startNode">직선 경로 ?��?�� ?��?��</param>
//     /// <param name="endNode">직선 경로 ?�� ?��?��</param>
//     /// <returns>?�� ?��?�� 간의 직선 경로�? ?��?��물에 ?��?�� 차단?��?��?���? ?���?</returns>
//     bool IsLineOfSight(Node startNode, Node endNode)
//     {
//         Vector3 direction = endNode.worldPosition - startNode.worldPosition;
//         return !Physics.Raycast(startNode.worldPosition, direction, direction.magnitude, obstacleMask);
//     }

//     ///<summary>
//     /// ?��?�� ?��?��??? 목표 ?��?�� ?��?��?�� 경로�? ?��추적?��?�� 반환?��?��?��.
//     ///</summary>
//     /// <param name="startNode">경로?�� ?��?�� ?��?��</param>
//     /// <param name="endNode">경로?�� 목표 ?��?��</param>
//     /// <returns>?��?�� ?��?��?��?�� 목표 ?��?��까�???�� 경로�? ?��?��?��?�� Node 리스?��</returns>
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
//     /// ?��?���? ?��?���? 고려?��?�� ?��?�� ?��?���? 반환?��?��?��.
//     ///</summary>
//     /// <param name="node">?��?��?�� 찾을 ?��?��</param>
//     /// <returns>주어�? ?��?��?�� ?��?��?�� ?��?�� ?��?�� 리스?��</returns>
//     List<Node> GetNeighbors(Node node)
//     {
//         List<Node> neighbors = new List<Node>();

//         // 26개의 방향 ????�� 6개의 주요 방향 (?��?��좌우 ?��?��)?���? ?��?�� ?��?���? ?��?��
//         for (int x = -1; x <= 1; x++)
//         {
//             for (int y = -1; y <= 1; y++)
//             {
//                 for (int z = -1; z <= 1; z++)
//                 {
//                     if (x == 0 && y == 0 && z == 0)
//                         continue;

//                     // ?��?�� 방향?���? Raycast�? 발사?��?�� ?��?���? ?��?��
//                     Vector3 direction = new Vector3(x, y, z);
//                     RaycastHit hit;
//                     if (Physics.Raycast(node.worldPosition, direction, out hit, nodeDiameter, obstacleMask))
//                     {
//                         // ?��?��물에 충돌?�� 경우 ?��?��?���? 추�???���? ?��?��
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
//     /// Gizmos�? ?��?��?��?�� 그리?��??? 경로�? ?��각화?��?��?��.
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
//     /// ?�� ?��?�� 간의 ?��?��리드 거리�? 계산?��?��?��.
//     ///</summary>
//     /// <param name="nodeA">거리 계산?�� �? 번째 ?��?��</param>
//     /// <param name="nodeB">거리 계산?�� ?�� 번째 ?��?��</param>
//     /// <returns>?�� ?��?�� 간의 ?��?��리드 거리</returns>
//     int GetDistance(Node nodeA, Node nodeB)
//     {
//         float dstX = Mathf.Abs(nodeA.worldPosition.x - nodeB.worldPosition.x);
//         float dstY = Mathf.Abs(nodeA.worldPosition.y - nodeB.worldPosition.y);
//         float dstZ = Mathf.Abs(nodeA.worldPosition.z - nodeB.worldPosition.z);

//         // ?��?��리드 거리 계산
//         return Mathf.RoundToInt(Mathf.Sqrt(dstX * dstX + dstY * dstY + dstZ * dstZ));
//     }

//     ///<summary>
//     /// ?�� ?��?�� 간의 거리 추정?�� ?��?��?��?��?��.
//     /// ?��?��물의 ?��?��?�� 고려?��?�� 목표 ?��?��까�???�� 거리�? 추정?��?��?��.
//     ///</summary>
//     /// <param name="nodeA">거리 계산?�� �? 번째 ?��?��</param>
//     /// <param name="nodeB">거리 계산?�� ?�� 번째 ?��?��</param>
//     /// <returns>?�� ?��?�� 간의 추정 거리</returns>
//     int GetHeuristic(Node nodeA, Node nodeB)
//     {
//         // 기본 ?��?��리드 거리 계산
//         float dstX = Mathf.Abs(nodeA.worldPosition.x - nodeB.worldPosition.x);
//         float dstY = Mathf.Abs(nodeA.worldPosition.y - nodeB.worldPosition.y);
//         float dstZ = Mathf.Abs(nodeA.worldPosition.z - nodeB.worldPosition.z);
//         float euclideanDistance = Mathf.Sqrt(dstX * dstX + dstY * dstY + dstZ * dstZ);

//         // ?��?��물로 ?��?�� 추�?? 거리 추정
//         float obstaclePenalty = 0f;
//         RaycastHit hit;
//         if (Physics.Raycast(nodeA.worldPosition, (nodeB.worldPosition - nodeA.worldPosition).normalized, out hit, euclideanDistance, obstacleMask))
//         {
//             obstaclePenalty = 1f; // ?��?��물로 ?��?�� 기본 ?��?��?��
//         }

//         // 거리 추정값에 ?��?���? ?��?��?�� 추�??
//         float estimatedDistance = euclideanDistance * (1 + obstaclePenalty);

//         return Mathf.RoundToInt(estimatedDistance);
//     }
// }

// // Node ?��?��?��?�� 기존 코드??? ?��?��

// // Priority Queue?��?�� ?��?���? ?��?��?���? ?��?�� Comparer
// public class NodeComparer : IComparer<Node>
// {
//     ///<summary>
//     /// ?�� ?��?���? 비교?��?�� ?��?��?��?���? 결정?��?��?��.
//     ///</summary>
//     /// <param name="a">비교?�� �? 번째 ?��?��</param>
//     /// <param name="b">비교?�� ?�� 번째 ?��?��</param>
//     /// <returns>?�� ?��?��?�� ?��?��?��?���? 결정?��?�� ?��?�� �?</returns>
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
//     /** ?��?�� �??��?���? ?���? */
//     public bool walkable;

//     /** ?��?�� 좌표 */
//     public Vector3 worldPosition;

//     /** 그리?�� ?�� X 좌표 */
//     public int gridX, gridY, gridZ;

//     /** gCost (?��?�� ?��?��로�???��?�� ?��?�� 비용) */
//     public int gCost;

//     /** hCost (목표 ?��?��까�???�� 추정 비용) */
//     public int hCost;

//     /** ?��?�� ?��?��?�� �?�? ?��?�� */
//     public Node parent;

//     ///<summary>
//     /// ?��?���? 초기?��?��?��?��.
//     ///</summary>
//     /// <param name="walkable">?��?��?�� ?��?�� �??�� ?���?</param>
//     /// <param name="worldPosition">?��?��?�� ?��?�� 좌표</param>
//     /// <param name="gridX">그리?�� ?�� X 좌표</param>
//     /// <param name="gridY">그리?�� ?�� Y 좌표</param>
//     /// <param name="gridZ">그리?�� ?�� Z 좌표</param>
//     public Node(bool walkable, Vector3 worldPosition, int gridX, int gridY, int gridZ)
//     {
//         this.walkable = walkable;
//         this.worldPosition = worldPosition;
//         this.gridX = gridX;
//         this.gridY = gridY;
//         this.gridZ = gridZ;
//     }

//     ///<summary>
//     /// fCost (gCost + hCost)�? 반환?��?��?��.
//     ///</summary>
//     public int fCost
//     {
//         get
//         {
//             return gCost + hCost;
//         }
//     }
// }
