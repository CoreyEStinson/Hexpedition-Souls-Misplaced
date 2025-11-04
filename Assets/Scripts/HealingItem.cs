using UnityEngine;

public class HealingItem : MonoBehaviour
{
    [Header("Healing Settings")]
    [SerializeField] private int healAmount = 1;

    [Header("Feedback")]
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private AudioClip pickupSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Check if player is already at max health
                if (playerHealth.GetCurrentHealth() < playerHealth.GetMaxHealth())
                {
                    playerHealth.Heal(healAmount);
                    
                    // Play feedback
                    if (pickupEffect != null)
                    {
                        Instantiate(pickupEffect, transform.position, Quaternion.identity);
                    }
                    if (pickupSound != null)
                    {
                        AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                    }

                    // Destroy the item
                    Destroy(gameObject);
                }
            }
        }
    }
}
