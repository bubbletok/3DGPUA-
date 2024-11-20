using UnityEngine;

public class AStartDestinationTest : MonoBehaviour
{
    public GameObject target;

    public LayerMask obstacleLayer;
    public LayerMask groundMask;

    public float minX, maxX;
    public float minZ, maxZ;

    private bool onGround;

    void Awake()
    {
        // minX = -20.0f;
        // maxX = 20.0f;
        // minZ = -20.0f;
        // maxZ = 20.0f;
    }

    void Update()
    {
        if (onGround)
        {
            return;
        }
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, groundMask))
        {
            NewRandomPos();
        }
        else
        {
            transform.position = hit.point + new Vector3(0,0.5f,0);
            onGround = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == target || other.gameObject.layer == obstacleLayer)
        {
            NewRandomPos();
        }
    }

    void NewRandomPos()
    {
        float randX = Random.Range(minX, maxX);
        float randZ = Random.Range(minZ, maxZ);
        transform.position = new Vector3(randX, 0, randZ);
        onGround = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(maxX - minX, 0, maxZ - minZ));
    }
}
