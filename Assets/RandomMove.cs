using UnityEngine;

public class RandomMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float minX = -5f;          // Left boundary
    public float maxX = 5f;           // Right boundary
    public float minSpeed = 2f;       // Minimum move speed
    public float maxSpeed = 5f;       // Maximum move speed
    public float arriveThreshold = 0.1f;  // How close before choosing a new point

    private float targetX;
    private float currentSpeed;
    private int direction = 1;
    private Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;
        PickNewTarget();
    }

    void Update()
    {
        // Move towards target
        Vector3 pos = transform.position;
        pos.x = Mathf.MoveTowards(pos.x, targetX, currentSpeed * Time.deltaTime);
        transform.position = pos;

        // Check if reached target
        if (Mathf.Abs(transform.position.x - targetX) <= arriveThreshold)
            PickNewTarget();
    }

    void PickNewTarget()
    {
        // Pick a new random X point between min and max
        float newTarget = Random.Range(minX, maxX);

        // Determine direction of travel
        int newDirection = newTarget > transform.position.x ? 1 : -1;

        // Flip only if direction changes
        if (newDirection != direction)
        {
            direction = newDirection;
            Vector3 scale = transform.localScale;
            scale.x = -scale.x;
            transform.localScale = scale;
        }

        // Assign new values
        targetX = newTarget;
        currentSpeed = Random.Range(minSpeed, maxSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(minX, transform.position.y, transform.position.z),
                        new Vector3(maxX, transform.position.y, transform.position.z));
        Gizmos.DrawSphere(new Vector3(targetX, transform.position.y, transform.position.z), 0.1f);
    }
}
