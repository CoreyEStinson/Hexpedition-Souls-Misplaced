using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 6;
    [SerializeField] private int currentHealth;

    [Header("UI")]
    [SerializeField] private HealthUI healthUI;

    [Header("Invincibility Settings")]
    [SerializeField] private float invincibilityDuration = 1f;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool enableFlashing = true;
    [SerializeField] private float flashSpeed = 10f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool isInvincible = false;
    private float invincibilityTimer = 0f;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    private void Update()
    {
        // Handle invincibility timer
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            
            // Visual feedback - flashing
            if (enableFlashing && spriteRenderer != null)
            {
                float alpha = Mathf.Abs(Mathf.Sin(Time.time * flashSpeed));
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(0.3f, 1f, alpha);
                spriteRenderer.color = color;
            }

            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                
                // Reset sprite alpha
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = 1f;
                    spriteRenderer.color = color;
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible)
        {
            return;
        }

        // Apply damage
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");

        // Update UI
        UpdateHealthUI();

        // Check if dead
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // Start invincibility
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    private void Die()
    {
        Debug.Log("Player died!");
        // Add death logic here (e.g., play animation, reload scene, etc.)
        // For now, just reset health
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    // Optional: Method to heal the player
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Player healed {amount}. Current health: {currentHealth}");
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthUI != null)
        {
            healthUI.UpdateHealth(currentHealth);
        }
    }
}
