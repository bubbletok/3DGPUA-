using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent; // For thread-safe collections
using System.Threading.Tasks;
using UnityEngine.Events;

public class NewNode : IComparable<NewNode>
{
    public Vector3 Location;
    public Vector3 Size;
    public float G;
    public float H;
    public float F => G + H;
    public NewNode Parent;
    public bool reachable; // Indicates if the node is traversable

    public NewNode(Vector3 l, Vector3 nodeSize)
    {
        this.Location = l;
        this.Size = nodeSize;
        reachable = true; // Default to true, will be set later based on obstacles
    }

    public int CompareTo(NewNode other)
    {
        int compare = F.CompareTo(other.F);
        if (compare == 0)
        {
            compare = H.CompareTo(other.H);
        }
        if (compare == 0)
        {
            compare = Location.GetHashCode().CompareTo(other.Location.GetHashCode());
        }
        return compare;
    }

    public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            return Location.Equals(((NewNode)obj).Location);
        }
    }

    public override int GetHashCode()
    {
        return Location.GetHashCode();
    }
}

public class WorldGrid
{
    public Vector3 worldPos;
    public int Width;
    public int Depth;
    public int Height;

    public Vector3 CellSize;
    public float GridSpacing;
    public NewNode[,,] Grid;

    public Vector3Int[] directions =
    {
        // X-axis movements
        new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
        // Y-axis movements
        new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
        // Z-axis movements
        new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1),

        // Diagonal movements (XY plane)
        new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0),
        new Vector3Int(-1, 1, 0), new Vector3Int(-1, -1, 0),

        // Diagonal movements (XZ plane)
        new Vector3Int(1, 0, 1), new Vector3Int(1, 0, -1),
        new Vector3Int(-1, 0, 1), new Vector3Int(-1, 0, -1),

        // Diagonal movements (YZ plane)
        new Vector3Int(0, 1, 1), new Vector3Int(0, 1, -1),
        new Vector3Int(0, -1, 1), new Vector3Int(0, -1, -1),

        // Diagonal movements in 3D (all axes)
        new Vector3Int(1, 1, 1), new Vector3Int(1, 1, -1),
        new Vector3Int(1, -1, 1), new Vector3Int(1, -1, -1),
        new Vector3Int(-1, 1, 1), new Vector3Int(-1, 1, -1),
        new Vector3Int(-1, -1, 1), new Vector3Int(-1, -1, -1)
    };

    public WorldGrid(Vector3 worldPos, Vector3Int worldSize, Vector3 cellSize, float gridSpacing, LayerMask obstacleLayer, LayerMask groundLayer)
    {
        this.worldPos = worldPos;
        this.Width = worldSize.x;
        this.Depth = worldSize.y;
        this.Height = worldSize.z;
        this.GridSpacing = gridSpacing;
        this.CellSize = cellSize;

        Grid = new NewNode[Width + 1, Depth + 1, Height + 1];
        int halfXSize = Width / 2;
        int halfYSize = Depth / 2;
        int halfZSize = Height / 2;

        for (int x = -halfXSize; x <= halfXSize; x++)
        {
            for (int y = -halfYSize; y <= halfYSize; y++)
            {
                for (int z = -halfZSize; z <= halfZSize; z++)
                {
                    Vector3 offset = new Vector3(x * gridSpacing, y * gridSpacing, z * gridSpacing);
                    NewNode node = new NewNode(worldPos + offset, cellSize);

                    // Check if this node is colliding with an obstacle
                    if (Physics.CheckBox(node.Location, CellSize, Quaternion.identity, obstacleLayer))
                    {
                        node.reachable = false; // Mark as unreachable if overlapping with an obstacle
                    }

                    if (Physics.Raycast(node.Location, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
                    {
                        if (Mathf.Abs(hit.point.y - node.Location.y) > 0.4f) // If the node is not on the ground
                        {
                            node.reachable = false;
                        }
                        else
                        {
                            node.Location = hit.point + new Vector3(0, CellSize.y, 0);
                        }
                    }
                    else
                    {
                        node.reachable = false;
                    }

                    if (Physics.CheckSphere(node.Location, 0.1f, groundLayer))
                    {
                        node.reachable = false;
                    }

                    Grid[x + halfXSize, y + halfYSize, z + halfZSize] = node;
                }
            }
        }
    }

    ~WorldGrid()
    {
        Grid = null;
    }
}

public class NewAStar3D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [Range(0, 5)][SerializeField] private float gridSpacing;
    [SerializeField] private Vector3Int worldSize;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector3 cellSize;

    [Header("Debug Settings")]
    [SerializeField] private bool bDrawWorld;
    [SerializeField] private bool bDrawGrid;
    [SerializeField] private bool bDrawPath;
    [SerializeField] private float drawNodeSize = 0.1f;
    private WorldGrid world;
    private SortedSet<NewNode> openNodes;
    private List<NewNode> closeNodes = new List<NewNode>();
    private NewNode startNode;
    private NewNode goalNode;
    private NewNode lastPos;
    private bool isSearchDone;
    private bool isSearching;
    private List<NewNode> path;

    private int maxSteps;
    private int curSteps;

    private float startTime;
    private float endTime;

    private UnityAction onBeforeSearch;
    private UnityAction onBeginSearch;
    private UnityAction onSearchComplete;

    void Start()
    {
        onBeforeSearch?.Invoke();
    }

    void OnEnable()
    {
        onBeforeSearch += Init;
        onBeginSearch += Clear;
        onSearchComplete += OnSearchComplete;
    }

    void OnDisable()
    {
        onBeforeSearch -= Init;
        onBeginSearch -= Clear;
        onSearchComplete -= OnSearchComplete;
    }

    void Update()
    {
        HandleInput();

        if (isSearching && !isSearchDone)
        {
            SearchParallel(lastPos);
        }
    }

    void HandleInput()
    {
        if (Physics.CheckBox(transform.position, Vector3.one * 0.5f, Quaternion.identity, groundLayer) && Input.GetKeyDown(KeyCode.Space))
        {
            if (!isSearching)
            {
                BeginSearch();
            }

            if (isSearchDone && path != null)
            {
                StartCoroutine(FollowPath());
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            bDrawWorld = !bDrawWorld;
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            bDrawGrid = !bDrawGrid;
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            bDrawPath = !bDrawPath;
        }
    }

    void Init()
    {
        world = new WorldGrid(transform.position, worldSize, cellSize, gridSpacing, obstacleLayer, groundLayer);
        openNodes = new SortedSet<NewNode>();
        closeNodes = new List<NewNode>();
        maxSteps = world.Width * world.Depth * world.Height;
    }

    private void Clear()
    {
        openNodes.Clear();
        closeNodes.Clear();

        startNode = new NewNode(transform.position, world.CellSize);
        goalNode = new NewNode(target.position, world.CellSize);

        path = null;
        isSearchDone = false;
        isSearching = false;
        curSteps = 0;
    }

    void OnSearchComplete()
    {
        path = GetPath();
        endTime = Time.realtimeSinceStartup;
        Debug.Log($"Path Found in {endTime - startTime} seconds");
    }

    private List<NewNode> GetPath()
    {
        NewNode beginNode = lastPos;
        List<NewNode> newPath = new List<NewNode>();
        while (!startNode.Equals(beginNode) && beginNode != null)
        {
            newPath.Add(beginNode);
            beginNode = beginNode.Parent;
        }
        newPath.Add(startNode);
        newPath.Reverse();

        // foreach (Node node in newPath)
        // {
        //     if (Physics.Raycast(node.Location, Vector3.up, out RaycastHit hit, Mathf.Infinity, groundLayer))
        //     {
        //         node.Location = hit.point + Vector3.up * 0.5f;
        //     }
        // }

        return newPath;
    }

    private IEnumerator FollowPath()
    {
        float moveSpeed = 5.0f;
        foreach (NewNode node in path)
        {
            while (Vector3.Distance(transform.position, node.Location) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, node.Location, Time.deltaTime * moveSpeed);
                yield return null;
            }
        }
    }

    private void BeginSearch()
    {
        Debug.Log("Begin Search");

        onBeginSearch?.Invoke();

        openNodes.Add(startNode);
        lastPos = startNode;
        isSearching = true;

        Debug.Log($"Max Steps: {maxSteps}");
        startTime = Time.realtimeSinceStartup;
    }

    private void Search(NewNode thisNode)
    {
        if (thisNode == null)
        {
            Debug.LogError("Node is null");
            isSearchDone = true;
            return;
        }
        if (IsInGoal(thisNode))
        {
            isSearchDone = true;
            onSearchComplete?.Invoke();
            return;
        }

        SearchInDirections(thisNode);

        NewNode node = openNodes.Min;
        closeNodes.Add(node);
        openNodes.Remove(node);

        lastPos = node;
    }

    void SearchInDirections(NewNode thisNode)
    {
        foreach (Vector3Int dir in world.directions)
        {
            if (curSteps >= maxSteps)
            {
                break;
            }
            curSteps++;
            Vector3 neighborPos = thisNode.Location + (Vector3)dir * gridSpacing;
            NewNode neighborNode = GetNodeAtPosition(neighborPos);
            if (neighborNode == null || !neighborNode.reachable) // Skip if unreachable
            {
                continue;
            }
            if (IsInCloseList(neighborNode))
            {
                continue;
            }

            // Height check: ensure the node is not too high or low compared to the current node
            float heightDifference = Mathf.Abs(thisNode.Location.y - neighborNode.Location.y);
            if (heightDifference > 1.5f)
            {
                continue; // Skip the neighbor if the height difference exceeds the allowed stair height
            }

            float G = thisNode.G + Vector3.Distance(thisNode.Location, neighborNode.Location);
            float H = Vector3.Distance(neighborNode.Location, goalNode.Location);
            float F = G + H;

            UpdateNode(neighborNode, thisNode, G, H, F);
        }
    }

    private void SearchParallel(NewNode thisNode)
    {
        if (thisNode == null)
        {
            Debug.LogError("Node is null");
            isSearchDone = true;
            return;
        }

        if (IsInGoal(thisNode))
        {
            isSearchDone = true;
            onSearchComplete?.Invoke();
            return;
        }

        SearchInDirectionsParallel(thisNode);

        // Continue processing the rest of the algorithm
        NewNode node = openNodes.Min;
        closeNodes.Add(node);
        openNodes.Remove(node);

        lastPos = node;
    }

    void SearchInDirectionsParallel(NewNode thisNode)
    {
        ConcurrentBag<NewNode> neighborsToProcess = new ConcurrentBag<NewNode>();

        // Parallel search in all directions
        Parallel.ForEach(world.directions, dir =>
        {
            Vector3 neighborPos = thisNode.Location + (Vector3)dir * gridSpacing;
            NewNode neighborNode = GetNodeAtPosition(neighborPos);

            if (neighborNode == null || !neighborNode.reachable) // Skip if unreachable
            {
                return; // Continue the loop
            }

            if (IsInCloseList(neighborNode))
            {
                return; // Continue the loop
            }

            // Height check: ensure the node is not too high or low compared to the current node
            float heightDifference = Mathf.Abs(thisNode.Location.y - neighborNode.Location.y);
            if (heightDifference > 1.5f)
            {
                return; // Skip the neighbor if the height difference exceeds the allowed stair height
            }

            // Safe to process this neighbor
            neighborsToProcess.Add(neighborNode);
        });

        // After parallel processing, update nodes sequentially (as SortedSet is not thread-safe)
        foreach (NewNode neighborNode in neighborsToProcess)
        {
            float G = thisNode.G + Vector3.Distance(thisNode.Location, neighborNode.Location);
            float H = Vector3.Distance(neighborNode.Location, goalNode.Location);
            float F = G + H;

            // Update the node in the main thread context
            UpdateNode(neighborNode, thisNode, G, H, F);
        }
    }

    private void UpdateNode(NewNode node, NewNode parent, float G, float H, float F)
    {
        if (IsInOpenList(node))
        {
            if (node.F > F)
            {
                openNodes.Remove(node);
                node.G = G;
                node.H = H;
                node.Parent = parent;
                openNodes.Add(node);
            }
        }
        else
        {
            node.G = G;
            node.H = H;
            node.Parent = parent;
            openNodes.Add(node);
        }
    }

    private bool IsInOpenList(NewNode node)
    {
        return openNodes.Contains(node);
    }

    private bool IsInCloseList(NewNode node)
    {
        return closeNodes.Contains(node);
    }

    private bool IsInGoal(NewNode node)
    {
        if (goalNode == null)
        {
            Debug.LogError("Goal node is null");
            return false;
        }

        float diff = Vector3.Distance(node.Location, goalNode.Location);
        return diff < gridSpacing;
    }

    private NewNode GetNodeAtPosition(Vector3 position)
    {
        int halfXSize = world.Width / 2;
        int halfYSize = world.Depth / 2;
        int halfZSize = world.Height / 2;

        int xIndex = Mathf.RoundToInt((position.x - world.worldPos.x) / gridSpacing) + halfXSize;
        int yIndex = Mathf.RoundToInt((position.y - world.worldPos.y) / gridSpacing) + halfYSize;
        int zIndex = Mathf.RoundToInt((position.z - world.worldPos.z) / gridSpacing) + halfZSize;

        if (xIndex >= 0 && xIndex <= world.Width &&
            yIndex >= 0 && yIndex <= world.Depth &&
            zIndex >= 0 && zIndex <= world.Height)
        {
            return world.Grid[xIndex, yIndex, zIndex];
        }

        return null;
    }

    private void OnDrawGizmos()
    {
        if (bDrawWorld)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, (Vector3)worldSize * gridSpacing);
        }

        if (bDrawGrid)
        {
            if (world != null && world.Grid != null)
            {
                for (int x = 0; x < worldSize.x; x++)
                {
                    for (int y = 0; y < worldSize.y; y++)
                    {
                        for (int z = 0; z < worldSize.z; z++)
                        {
                            NewNode node = world.Grid[x, y, z];

                            if (node.reachable == true)
                            {
                                Gizmos.color = Color.white;
                                Gizmos.DrawCube(node.Location, Vector3.one * drawNodeSize);
                                if (IsInCloseList(node))
                                {
                                    Gizmos.color = Color.blue;
                                    Gizmos.DrawCube(node.Location, Vector3.one * drawNodeSize);
                                }
                                else if (IsInOpenList(node))
                                {
                                    Gizmos.color = Color.green;
                                    Gizmos.DrawCube(node.Location, Vector3.one * drawNodeSize);
                                }
                            }
                        }
                    }
                }
            }
        }

        if (path != null && bDrawPath)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(path[i].Location, path[i + 1].Location);
            }
        }
    }
}