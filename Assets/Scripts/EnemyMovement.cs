using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Ledge Check")]
    [SerializeField] private bool enableLedgeCheck = true;
    [SerializeField] private Vector2 ledgeCheckOffset = new Vector2(0.5f, 0f);
    [SerializeField] private float ledgeCheckDistance = 1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private Transform playerTransform;

    private Rigidbody2D body;
    private bool facingRight = true;

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
    }

    private void FixedUpdate()
    {
        bool chasingPlayer = IsPlayerWithinRange();
        float desiredSpeed = 0f;

        if (chasingPlayer)
        {
            float direction = Mathf.Sign(playerTransform.position.x - transform.position.x);
            if (direction == 0f)
            {
                direction = facingRight ? 1f : -1f;
            }

            facingRight = direction > 0f;

            if (IsGroundAhead())
            {
                desiredSpeed = chaseSpeed * direction;
            }
        }
        else
        {
            if (!IsGroundAhead())
            {
                Flip();
            }

            if (IsGroundAhead())
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
        if (playerTransform == null)
        {
            return false;
        }

        // Cast rays to the left and right
        Vector2 origin = transform.position;
        RaycastHit2D hitRight = Physics2D.Raycast(origin, Vector2.right, detectionRange, playerLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(origin, Vector2.left, detectionRange, playerLayer);

        return (hitRight.collider != null && hitRight.collider.transform == playerTransform) ||
               (hitLeft.collider != null && hitLeft.collider.transform == playerTransform);
    }

    private bool IsGroundAhead()
    {
        if (!enableLedgeCheck)
        {
            return true;
        }

        Vector2 origin = (Vector2)transform.position;
        origin += new Vector2(facingRight ? ledgeCheckOffset.x : -ledgeCheckOffset.x, ledgeCheckOffset.y);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, ledgeCheckDistance, groundLayer);
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
        Vector3 origin = transform.position + new Vector3(facingRight ? ledgeCheckOffset.x : -ledgeCheckOffset.x, ledgeCheckOffset.y, 0f);
        Gizmos.DrawLine(origin, origin + Vector3.down * ledgeCheckDistance);

        // Draw player detection rays
        Gizmos.color = Color.red;
        Vector3 pos = transform.position;
        Gizmos.DrawLine(pos, pos + Vector3.right * detectionRange);
        Gizmos.DrawLine(pos, pos + Vector3.left * detectionRange);
    }
}
