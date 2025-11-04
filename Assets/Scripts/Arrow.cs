using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 10;
    public float lifetime = 5f;
    public Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Try to damage enemy
        EnemyHealth enemyHealth = hitInfo.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
            Debug.Log("Arrow hit enemy for " + damage + " damage");
            
            // Apply knockback
            Rigidbody2D enemyRb = hitInfo.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                Vector2 knockbackDirection = (hitInfo.transform.position - transform.position).normalized;
                enemyRb.linearVelocity = knockbackDirection * 5f; // Knockback force
            }
        }

        // Destroy the arrow on any impact (enemy or wall)
        Destroy(gameObject);
    }
}
