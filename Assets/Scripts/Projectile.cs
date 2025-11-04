using UnityEngine;

public class Projectile : MonoBehaviour
{
    private int damage;
    private float knockbackForce;
    private LayerMask targetLayers;
    private bool initialized = false;
    
    public void Initialize(int damage, float knockbackForce, LayerMask targetLayers)
    {
        this.damage = damage;
        this.knockbackForce = knockbackForce;
        this.targetLayers = targetLayers;
        this.initialized = true;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!initialized)
        {
            return;
        }
        
        // Check if the collision is with a target layer
        if (((1 << collision.gameObject.layer) & targetLayers) != 0)
        {
            // Try to damage the target
            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
            
            // Apply knockback
            Rigidbody2D targetRb = collision.GetComponent<Rigidbody2D>();
            if (targetRb != null)
            {
                Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
                Vector2 knockback = knockbackDirection * knockbackForce;
                targetRb.linearVelocity = knockback;
            }
            
            // Destroy projectile on impact
            Destroy(gameObject);
        }
        // Destroy on collision with walls/ground (check if tags exist first)
        else if (!collision.isTrigger)
        {
            // Hit a solid object (not a trigger), destroy the projectile
            Destroy(gameObject);
        }
    }
}
