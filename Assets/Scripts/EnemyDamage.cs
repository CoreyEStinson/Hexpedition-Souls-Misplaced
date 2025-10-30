using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damageAmount = 1;

    [Header("Damage Cooldown")]
    [SerializeField] private float damageCooldown = 1f;

    [Header("Collision Mode")]
    [SerializeField] private bool useTriggerMode = true;
    
    private float lastDamageTime = -999f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!useTriggerMode && collision.gameObject.CompareTag("Player"))
        {
            TryDamagePlayer(collision.gameObject);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!useTriggerMode && collision.gameObject.CompareTag("Player"))
        {
            TryDamagePlayer(collision.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (useTriggerMode && other.CompareTag("Player"))
        {
            TryDamagePlayer(other.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (useTriggerMode && other.CompareTag("Player"))
        {
            TryDamagePlayer(other.gameObject);
        }
    }

    private void TryDamagePlayer(GameObject playerObject)
    {
        // Check cooldown
        if (Time.time - lastDamageTime < damageCooldown)
        {
            return;
        }

        // Get the PlayerHealth component
        PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();
        
        if (playerHealth != null)
        {
            // Deal damage with knockback (pass enemy position for knockback direction)
            playerHealth.TakeDamage(damageAmount, transform.position);

            // Update last damage time
            lastDamageTime = Time.time;

            Debug.Log($"Enemy dealt {damageAmount} damage to player with knockback");
        }
        else
        {
            Debug.LogWarning("Player object does not have a PlayerHealth component!");
        }
    }

    // Optional: Visualize damage range in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
