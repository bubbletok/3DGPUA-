using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using System;

struct Node
{
    public Vector3 Location;
    public Vector3 Size;
    public float G;
    public float H;
    public float F;
    public int id;
    public int parentIdx;
    public int Reachable;
};

struct INode : IComparable<INode>
{
    public Vector3 Location;
    public float F;
    public float H;
    public int Id;

    public INode(Vector3 l, float f, float h, int id)
    {
        Location = l;
        F = f;
        H = h;
        Id = id;
    }

    public int CompareTo(INode other)
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
}

public class PriorityQueue<T>
{
    private List<(T item, float priority)> elements = new List<(T, float)>();

    public int Count => elements.Count;

    public void Enqueue(T item, float priority)
    {
        elements.Add((item, priority));
        int ci = elements.Count - 1;
        while (ci > 0)
        {
            int pi = (ci - 1) / 2;
            if (elements[ci].priority >= elements[pi].priority) break;
            var tmp = elements[ci];
            elements[ci] = elements[pi];
            elements[pi] = tmp;
            ci = pi;
        }
    }

    public T Dequeue()
    {
        int li = elements.Count - 1;
        T frontItem = elements[0].item;
        elements[0] = elements[li];
        elements.RemoveAt(li);

        if (elements.Count > 0)
        {
            int pi = 0;
            while (true)
            {
                int ci = pi * 2 + 1;
                if (ci >= elements.Count) break;
                int rc = ci + 1;
                if (rc < elements.Count && elements[rc].priority < elements[ci].priority)
                    ci = rc;
                if (elements[pi].priority <= elements[ci].priority) break;
                var tmp = elements[ci];
                elements[ci] = elements[pi];
                elements[pi] = tmp;
                pi = ci;
            }
        }
        return frontItem;
    }

    public bool Contains(T item)
    {
        return elements.Any(e => e.item.Equals(item));
    }

    public void Remove(T item)
    {
        int index = elements.FindIndex(e => e.item.Equals(item));
        if (index >= 0)
        {
            elements.RemoveAt(index);
            for (int i = elements.Count / 2 - 1; i >= 0; i--)
            {
                int pi = i;
                while (true)
                {
                    int ci = pi * 2 + 1;
                    if (ci >= elements.Count) break;
                    int rc = ci + 1;
                    if (rc < elements.Count && elements[rc].priority < elements[ci].priority)
                        ci = rc;
                    if (elements[pi].priority <= elements[ci].priority) break;
                    var tmp = elements[ci];
                    elements[ci] = elements[pi];
                    elements[pi] = tmp;
                    pi = ci;
                }
            }
        }
    }
}

public class AStarCompute : MonoBehaviour
{
    [SerializeField] private ComputeShader computeShader;
    [SerializeField] private Transform target;

    [Header("Grid Settings")]
    [Range(0, 5)][SerializeField] private float gridSpacing;
    [SerializeField] private Vector3Int worldSize;
    [SerializeField] private Vector3 cellSize;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask groundLayer;

    [Space, Header("Debug Settings")]
    [SerializeField] private bool bDrawWorld;
    [SerializeField] private bool bDrawGrid;
    [SerializeField] private bool bDrawPath;
    [Range(0, 2)][SerializeField] private float drawNodeSize = 0.1f;

    [Space, Header("Compute Buffers")]
    private ComputeBuffer nodeBuffer;

    [Space, Header("Kernel Index")]
    private int InitGridKernel;
    private int UpdateGridValueKernel;

    private bool isValueUpdated;
    private bool isPathFound;
    private Node[,,] grid;
    private Vector3 startPos;
    private Vector3 goalPos;
    private List<Node> path;


    [Header("Path Finding Properties")]
    private Node startNode;
    private PriorityQueue<INode> openList = new PriorityQueue<INode>();
    private HashSet<int> closedListIds = new HashSet<int>();
    List<Node> goalNodes;
    private int maxSteps = 100000; // Max steps to prevent infinite loop
    private int totalNodes;
    private int threadGroudModifier;
    private Node[] flatGrid;
    private int worldSizeX;
    private int worldSizeY;
    private int worldSizeZ;

    Vector3Int[] directions =
    {
        new Vector3Int(-1, -1, -1), new Vector3Int(-1, -1, 0), new Vector3Int(-1, -1, 1),
        new Vector3Int(-1, 0, -1),  new Vector3Int(-1, 0, 0),  new Vector3Int(-1, 0, 1),
        new Vector3Int(-1, 1, -1),  new Vector3Int(-1, 1, 0),  new Vector3Int(-1, 1, 1),
        new Vector3Int(0, -1, -1),  new Vector3Int(0, -1, 0),  new Vector3Int(0, -1, 1),
        new Vector3Int(0, 0, -1),                            new Vector3Int(0, 0, 1),
        new Vector3Int(0, 1, -1),   new Vector3Int(0, 1, 0),   new Vector3Int(0, 1, 1),
        new Vector3Int(1, -1, -1),  new Vector3Int(1, -1, 0),  new Vector3Int(1, -1, 1),
        new Vector3Int(1, 0, -1),   new Vector3Int(1, 0, 0),   new Vector3Int(1, 0, 1),
        new Vector3Int(1, 1, -1),   new Vector3Int(1, 1, 0),   new Vector3Int(1, 1, 1)
    };

    void Start()
    {
        Init();
    }

    void Init()
    {
        worldSizeX = worldSize.x;
        worldSizeY = worldSize.y;
        worldSizeZ = worldSize.z;

        grid = new Node[worldSizeX, worldSizeY, worldSizeZ];
        threadGroudModifier = Mathf.Max(worldSizeX, worldSizeY, worldSizeZ) * 2;

        startPos = transform.position;
        goalPos = target.position;

        path = new List<Node>();

        flatGrid = new Node[worldSizeX * worldSizeY * worldSizeZ];

        totalNodes = worldSizeX * worldSizeY * worldSizeZ;

        FindKernel();
        CreateInitialBuffers();
        SetBuffers();
        SetProperty();
        InitGrid();
    }

    void FindKernel()
    {
        InitGridKernel = computeShader.FindKernel("InitGrid");
        UpdateGridValueKernel = computeShader.FindKernel("UpdateGridValue");
    }

    void CreateInitialBuffers()
    {
        int size = worldSizeX * worldSizeY * worldSizeZ;
        int nodeSize = sizeof(float) * 9 + sizeof(int) * 3;
        nodeBuffer = new ComputeBuffer(size, nodeSize);
    }

    void SetBuffers()
    {
        SetComputeShaderBuffers(computeShader, nodeBuffer, "grid", InitGridKernel, UpdateGridValueKernel);
    }

    void SetComputeShaderBuffers(ComputeShader computeShader, ComputeBuffer buffer, string id, params int[] kernels)
    {
        for (int i = 0; i < kernels.Length; i++)
        {
            computeShader.SetBuffer(kernels[i], id, buffer);
        }
    }

    void SetProperty()
    {
        computeShader.SetInts("worldSize", worldSizeX, worldSizeY, worldSizeZ);
        computeShader.SetVector("worldPos", startPos);
        computeShader.SetFloat("gridSpacing", gridSpacing);
        computeShader.SetVector("cellSize", cellSize);
        computeShader.SetVector("targetPos", goalPos);

    }

    void InitGrid()
    {
        int threadGroupSize = totalNodes / threadGroudModifier; //Mathf.Min(worldSize.x, worldSize.y, worldSize.z);

        computeShader.Dispatch(InitGridKernel, threadGroupSize, 1, 1);
        UpdateGrid();
        UpdateNodeReachability();
        SetGridInComputeShader();
        UpdateGrid();
    }

    void UpdateGrid()
    {
        nodeBuffer.GetData(flatGrid);
        for (int x = 0; x < worldSizeX; ++x)
        {
            for (int y = 0; y < worldSizeY; ++y)
            {
                for (int z = 0; z < worldSizeZ; ++z)
                {
                    int index = x + y * worldSize.x + z * worldSize.x * worldSize.y;
                    grid[x, y, z] = flatGrid[index];
                }
            }
        }
    }

    void UpdateNodeReachability()
    {
        for (int x = 0; x < worldSizeX; ++x)
        {
            for (int y = 0; y < worldSizeY; ++y)
            {
                for (int z = 0; z < worldSizeZ; ++z)
                {
                    Node node = grid[x, y, z];
                    if (Physics.CheckBox(node.Location, cellSize, Quaternion.identity, obstacleLayer))
                    {
                        grid[x, y, z].Reachable = 0;
                    }
                    if (Physics.Raycast(node.Location, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
                    {
                        if (Mathf.Abs(hit.point.y - node.Location.y) > 0.5f)
                        {
                            grid[x, y, z].Reachable = 0;
                        }
                        else
                        {
                            grid[x, y, z].Location = hit.point + new Vector3(0, cellSize.y, 0);
                        }
                    }
                    else
                    {
                        grid[x, y, z].Reachable = 0;
                    }

                    if (Physics.CheckSphere(node.Location, 0.1f, groundLayer))
                    {
                        grid[x, y, z].Reachable = 0;
                    }
                }
            }
        }
    }

    void SetGridInComputeShader()
    {
        Parallel.For(0, totalNodes - 1, idx =>
        {
            int x = idx % worldSize.x;
            int y = (idx / worldSize.x) % worldSize.y;
            int z = idx / (worldSize.x * worldSize.y);
            flatGrid[idx] = grid[x, y, z];
        });

        nodeBuffer.SetData(flatGrid);
    }


    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        if (Physics.CheckBox(transform.position, Vector3.one * 0.5f, Quaternion.identity, groundLayer) && Input.GetKeyDown(KeyCode.Space))
        {
            if (!isValueUpdated)
            {
                isValueUpdated = true;
                UpdateGridValue();
                UpdateGrid();
            }
            else if (!isPathFound)
            {
                path = GetPath();
                if (path != null)
                {
                    isPathFound = true;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            isValueUpdated = false;
            isPathFound = false;
            Init();
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

    void UpdateGridValue()
    {
        Debug.Log("Update Grid Value");
        int threadGroupSize = totalNodes / threadGroudModifier;
        computeShader.Dispatch(UpdateGridValueKernel, threadGroupSize, 1, 1);
    }

    Node GetStartNode()
    {
        for (int x = 0; x < worldSizeX; ++x)
        {
            for (int y = 0; y < worldSizeY; ++y)
            {
                for (int z = 0; z < worldSizeZ; ++z)
                {
                    Node node = grid[x, y, z];
                    if (node.Reachable == 0)
                    {
                        continue;
                    }
                    if (Vector3.Distance(node.Location, startPos) < gridSpacing * 2)
                    {
                        return node;
                    }
                }
            }
        }
        return new Node();
    }

    List<Node> GetNodesNearbyTarget()
    {
        List<Node> nodes = new List<Node>();
        Parallel.For(0, totalNodes - 1, idx =>
        {
            int x = idx % worldSize.x;
            int y = (idx / worldSize.x) % worldSize.y;
            int z = idx / (worldSize.x * worldSize.y);
            Node node = grid[x, y, z];
            if (node.Reachable == 0)
            {
                return;
            }
            else if (Vector3.Distance(node.Location, goalPos) < gridSpacing * 2)
            {
                nodes.Add(node);
            }
        });
        return nodes;
    }

    List<Node> GetPath()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        nodeBuffer.GetData(flatGrid);

        startNode = GetStartNode();
        goalNodes = GetNodesNearbyTarget();

        closedListIds.Clear();

        openList.Enqueue(new INode(startNode.Location, startNode.F, startNode.H, startNode.id), startNode.F); ;

        int steps = 0;
        int debugCounter = 0;

        while (openList.Count > 0)
        {
            ++debugCounter;
            // if (debugCounter % 100 == 0)
            // {
            //     Debug.Log($"Step {debugCounter}: Open List Size = {openList.Count}, Closed List Size = {closedListIds.Count}");
            // }

            if (steps++ > maxSteps)
            {
                Debug.LogWarning("Max Steps Reached - Path finding terminated");
                break;
            }

            INode currentINode = openList.Dequeue();
            Node currentNode = flatGrid[currentINode.Id];

            if (goalNodes.Any(goal => Vector3.Distance(goal.Location, currentNode.Location) <= 0.1f))
            {
                stopwatch.Stop();
                Debug.Log($"Path found in {debugCounter} steps, took {stopwatch.ElapsedMilliseconds}ms");
                return ReconstructPath(currentNode, flatGrid);
            }

            closedListIds.Add(currentNode.id);

            ProcessNeighboringNodes(currentNode, flatGrid);
        }

        stopwatch.Stop();
        Debug.LogWarning($"No Path Found after {debugCounter} steps, took {stopwatch.ElapsedMilliseconds}ms");
        return null;
    }


    private List<Node> ReconstructPath(Node endNode, Node[] flatGrid)
    {
        Stack<Node> pathStack = new Stack<Node>(1000);
        Node currentNode = endNode;
        int maxIterations = 1000;

        do
        {
            pathStack.Push(currentNode);
            currentNode = flatGrid[currentNode.parentIdx];
        }
        while (currentNode.id != startNode.id && --maxIterations > 0);

        if (maxIterations <= 0)
        {
            Debug.LogError("Path reconstruction exceeded maximum iterations");
            return null;
        }

        pathStack.Push(startNode);
        return new List<Node>(pathStack);
    }

    private bool IsWalkable(int x, int y, int z)
    {
        if (x < 0 || x >= worldSize.x || y < 0 || y >= worldSize.y || z < 0 || z >= worldSize.z)
            return false;

        int index = x + y * worldSize.x + z * worldSize.x * worldSize.y;
        return flatGrid[index].Reachable == 1;
    }

    private (int x, int y, int z) Jump(int x, int y, int z, Vector3Int direction)
    {
        int nextX = x + direction.x;
        int nextY = y + direction.y;
        int nextZ = z + direction.z;

        // 경계 체크 또는 장애물 체크
        if (!IsWalkable(nextX, nextY, nextZ))
            return (-1, -1, -1);

        int currentIdx = nextX + nextY * worldSize.x + nextZ * worldSize.x * worldSize.y;
        Node currentNode = flatGrid[currentIdx];

        if (goalNodes.Any(goal => Vector3.Distance(goal.Location, currentNode.Location) <= 0.1f))
            return (nextX, nextY, nextZ);

        

        return Jump(nextX, nextY, nextZ, direction);
    }

    private void ProcessNeighboringNodes(Node currentNode, Node[] flatGrid)
    {
        int x = currentNode.id % worldSize.x;
        int y = (currentNode.id / worldSize.x) % worldSize.y;
        int z = currentNode.id / (worldSize.x * worldSize.y);

        int len = directions.Length;
        for (int i = 0; i < len; ++i)
        {
            var direction = directions[i];
            // var jumpPoint = Jump(x, y, z, direction);

            // if (jumpPoint == (-1, -1, -1))
            //     continue;

            // int newX = jumpPoint.x;
            // int newY = jumpPoint.y;
            // int newZ = jumpPoint.z;

            int newX = x + direction.x;
            int newY = y + direction.y;
            int newZ = z + direction.z;

            if (newX < 0 || newX >= worldSize.x ||
                newY < 0 || newY >= worldSize.y ||
                newZ < 0 || newZ >= worldSize.z)
                continue;

            int neighborId = newX + newY * worldSize.x + newZ * worldSize.x * worldSize.y;

            Node neighbor = flatGrid[neighborId];

            if (neighbor.Reachable == 0 || closedListIds.Contains(neighbor.id))
                continue;

            INode neighborINode = new INode(neighbor.Location, neighbor.F, neighbor.H, neighbor.id);

            float moveCost = Vector3.Distance(currentNode.Location, neighbor.Location);
            float newG = currentNode.G + moveCost;
            
            if (!openList.Contains(neighborINode) || newG < neighbor.G)
            {
                flatGrid[neighborId].G = newG;
                flatGrid[neighborId].F = newG + neighbor.H;
                flatGrid[neighborId].parentIdx = currentNode.id;
                
                INode newINode = new INode(
                    flatGrid[neighborId].Location, 
                    flatGrid[neighborId].F, 
                    flatGrid[neighborId].H, 
                    flatGrid[neighborId].id
                );

                if (!openList.Contains(neighborINode))
                {
                    openList.Enqueue(newINode, newINode.F);
                }
                else
                {
                    openList.Remove(neighborINode);
                    openList.Enqueue(newINode, newINode.F);
                }
            }
            // INode newINode = new INode(flatGrid[neighborId].Location, flatGrid[neighborId].F, flatGrid[neighborId].H, flatGrid[neighborId].id);

            // if (!openList.Contains(neighborINode))
            // {
            //     flatGrid[neighborId].parentIdx = currentNode.id;
            //     openList.Enqueue(newINode, newINode.F);
            // }
            // else if (currentNode.F > neighbor.F)
            // {
            //     openList.Remove(neighborINode);
            //     flatGrid[neighborId].parentIdx = currentNode.id;
            //     openList.Enqueue(newINode, newINode.F);
            // }
        }
    }

    void OnDrawGizmos()
    {
        if (bDrawWorld)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, (Vector3)worldSize * gridSpacing);
        }
        if (bDrawGrid)
        {
            if (grid != null)
            {
                for (int i = 0; i < flatGrid.Length; i++)
                {
                    Node node = flatGrid[i];
                    if (node.Reachable == 1)
                    {
                        Gizmos.color = Color.gray;
                        Gizmos.DrawCube(node.Location, Vector3.one * drawNodeSize * (5 / ((node.F) == 0 ? 5 : node.F)));
                        Gizmos.color = Color.cyan;
                        Node parent = flatGrid[node.parentIdx];
                        Gizmos.DrawLine(node.Location, parent.Location);
                    }
                }
            }
        }

        if (path != null && bDrawPath)
        {
            for (int i = 0; i < path.Count; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(path[i].Location, drawNodeSize);
            }
        }
    }
}