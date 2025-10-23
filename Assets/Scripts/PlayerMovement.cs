using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundCheckThreshold = 0.7f; // How vertical the surface needs to be

    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Get input
        float horizontal = Input.GetAxis("Horizontal");

        // Store movement direction
        moveDirection = horizontal;

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    void FixedUpdate()
    {
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
}