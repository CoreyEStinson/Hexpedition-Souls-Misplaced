using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float chaseLinger = 3f; // Time to maintain chase speed after losing sight
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer; // Walls and obstacles

    [Header("Ledge Check")]
    [SerializeField] private bool enableLedgeCheck = true;
    [SerializeField] private Vector2 ledgeCheckOffset = new Vector2(0.5f, 0f);
    [SerializeField] private float ledgeCheckDistance = 1f;
    [SerializeField] private LayerMask groundLayer; // Include your Tilemap layer here

    [Header("Wall Check")]
    [SerializeField] private bool enableWallCheck = true;
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private Vector2 wallCheckOffset = new Vector2(0.5f, 0.5f);
    
    [Header("Tilemap Compatibility")]
    [SerializeField] private float raycastSkinWidth = 0.1f; // Small offset to prevent self-collision

    [Header("References")]
    [SerializeField] private Transform playerTransform;

    private Rigidbody2D body;
    private bool facingRight = true;
    private float lastSeenPlayerTime = -100f; // Time when player was last seen

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        if (playerTransform == null)
        {
            GameObject possiblePlayer = GameObject.FindGameObjectWithTag("Player");
            if (possiblePlayer != null)
            {
                playerTransform = possiblePlayer.transform;
            }
        }
        
        // Ensure Rigidbody2D settings are optimal for tilemap interaction
        if (body != null)
        {
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent rotation
        }
    }

    private void FixedUpdate()
    {
        bool canSeePlayer = IsPlayerWithinRange();
        
        // Update last seen time if we can see the player
        if (canSeePlayer)
        {
            lastSeenPlayerTime = Time.time;
        }

        // Check if we should be chasing (can see or recently saw player)
        bool shouldChase = canSeePlayer || (Time.time - lastSeenPlayerTime < chaseLinger);
        
        float desiredSpeed = 0f;

        if (shouldChase && playerTransform != null)
        {
            float direction = Mathf.Sign(playerTransform.position.x - transform.position.x);
            if (Mathf.Abs(direction) < 0.01f)
            {
                direction = facingRight ? 1f : -1f;
            }

            facingRight = direction > 0f;

            if (IsGroundAhead() && !IsWallAhead())
            {
                desiredSpeed = chaseSpeed * direction;
            }
        }
        else
        {
            if (!IsGroundAhead() || IsWallAhead())
            {
                Flip();
            }

            if (IsGroundAhead() && !IsWallAhead())
            {
                desiredSpeed = (facingRight ? 1f : -1f) * patrolSpeed;
            }
        }

        Vector2 currentVelocity = body.linearVelocity;
        currentVelocity.x = desiredSpeed;
        body.linearVelocity = currentVelocity;
        UpdateFacing();
    }

    private bool IsPlayerWithinRange()
    {
        return IsPlayerVisible(Vector2.right) || IsPlayerVisible(Vector2.left);
    }

    private bool IsPlayerVisible(Vector2 direction)
    {
        if (playerTransform == null) return false;

        Vector2 origin = (Vector2)transform.position;
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, detectionRange, playerLayer | obstacleLayer);

        float playerDistance = float.MaxValue;
        float closestObstacleDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.collider.transform == playerTransform)
            {
                playerDistance = hit.distance;
            }
            else if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
            {
                if (hit.distance < closestObstacleDistance)
                {
                    closestObstacleDistance = hit.distance;
                }
            }
        }

        return playerDistance < closestObstacleDistance;
    }

    private bool IsGroundAhead()
    {
        if (!enableLedgeCheck)
        {
            return true;
        }

        Vector2 origin = (Vector2)transform.position;
        origin += new Vector2(facingRight ? ledgeCheckOffset.x : -ledgeCheckOffset.x, ledgeCheckOffset.y);
        
        // Cast ray downward to check for ground (works with Tilemap Collider 2D)
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, ledgeCheckDistance, groundLayer);
        
        // Debug visualization
        Debug.DrawRay(origin, Vector2.down * ledgeCheckDistance, hit.collider != null ? Color.green : Color.red);
        
        return hit.collider != null;
    }

    private bool IsWallAhead()
    {
        if (!enableWallCheck)
        {
            return false;
        }

        Vector2 origin = (Vector2)transform.position;
        origin += new Vector2(facingRight ? wallCheckOffset.x : -wallCheckOffset.x, wallCheckOffset.y);
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        
        // Cast ray forward to check for walls (works with Tilemap Collider 2D)
        // Use both obstacle and ground layers to detect tilemap walls
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, wallCheckDistance, obstacleLayer | groundLayer);
        
        // Debug visualization
        Debug.DrawRay(origin, direction * wallCheckDistance, hit.collider != null ? Color.blue : Color.cyan);
        
        return hit.collider != null;
    }

    private void Flip()
    {
        facingRight = !facingRight;
    }

    private void UpdateFacing()
    {
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * (facingRight ? 1f : -1f);
        transform.localScale = localScale;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw ledge check ray
        Gizmos.color = Color.yellow;
        Vector3 ledgeOrigin = transform.position + new Vector3(facingRight ? ledgeCheckOffset.x : -ledgeCheckOffset.x, ledgeCheckOffset.y, 0f);
        Gizmos.DrawLine(ledgeOrigin, ledgeOrigin + Vector3.down * ledgeCheckDistance);

        // Draw wall check ray
        Gizmos.color = Color.cyan;
        Vector3 wallOrigin = transform.position + new Vector3(facingRight ? wallCheckOffset.x : -wallCheckOffset.x, wallCheckOffset.y, 0f);
        Vector3 wallDirection = facingRight ? Vector3.right : Vector3.left;
        Gizmos.DrawLine(wallOrigin, wallOrigin + wallDirection * wallCheckDistance);

        // Draw player detection rays
        Gizmos.color = Color.red;
        Vector3 pos = transform.position;
        Gizmos.DrawLine(pos, pos + Vector3.right * detectionRange);
        Gizmos.DrawLine(pos, pos + Vector3.left * detectionRange);
    }
}
