using UnityEngine;
using UnityEngine.AI;
public class NavMeshTest : MonoBehaviour
{
    public Transform target; // The destination
    private NavMeshAgent agent;
    private NavMeshPath path;
    private bool isCalculatingPath = false;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
    }

    void Update()
    {
        if (target != null && !isCalculatingPath)
        {
            CalculatePath();
            // Move the agent to the target
            agent.SetDestination(target.position);
        }
    }

    void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
            }
        }
    }

    void CalculatePath()
    {

        agent.CalculatePath(target.position, path);

        for (int i = 0; i < path.corners.Length; i++)
        {
            Debug.Log("Corner " + i + ": " + path.corners[i]);
        }
        isCalculatingPath = true;

    }
}
