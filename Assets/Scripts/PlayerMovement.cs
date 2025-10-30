using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundCheckThreshold = 0.7f; // How vertical the surface needs to be

    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveDirection;
    private PlayerHealth playerHealth;
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
        
        // Ensure Rigidbody2D is set up correctly to prevent sticking
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }

    void Update()
    {
        // Check if player is knocked back - if so, don't allow input
        if (playerHealth != null && playerHealth.IsKnockedBack())
        {
            moveDirection = 0f;
            return;
        }

        // Get input
        float horizontal = Input.GetAxis("Horizontal");

        // Store movement direction
        moveDirection = horizontal;

        // Flip player based on movement direction
        if (horizontal > 0f && !facingRight)
        {
            Flip();
        }
        else if (horizontal < 0f && facingRight)
        {
            Flip();
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    void FixedUpdate()
    {
        // Don't override velocity if player is knocked back
        if (playerHealth != null && playerHealth.IsKnockedBack())
        {
            return;
        }

        // Move the player using physics
        rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if player is on the ground
        if (collision.gameObject.CompareTag("Floor"))
        {
            isGrounded = IsGroundedOnSurface(collision);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Keep grounded while on the floor
        if (collision.gameObject.CompareTag("Floor"))
        {
            isGrounded = IsGroundedOnSurface(collision);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // No longer grounded when leaving the floor
        if (collision.gameObject.CompareTag("Floor"))
        {
            isGrounded = false;
        }
    }

    private bool IsGroundedOnSurface(Collision2D collision)
    {
        // Check if any contact point has a normal pointing upward
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // If the normal's Y component is greater than threshold, we're on top
            if (contact.normal.y > groundCheckThreshold)
            {
                return true;
            }
        }
        return false;
    }

    private void Flip()
    {
        // Toggle facing direction
        facingRight = !facingRight;

        // Flip the player by scaling on the X axis
        // This will flip all children as well
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }
}