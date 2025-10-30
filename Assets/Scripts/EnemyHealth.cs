using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool enableFlashing = true;
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Death")]
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private float deathDelay = 0f;
    
    [Header("Events")]
    public UnityEvent<int> OnDamageTaken;
    public UnityEvent OnDeath;
    
    private bool isDead = false;
    private float flashTimer = 0f;
    private Color originalColor;
    
    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        currentHealth = maxHealth;
    }
    
    private void Update()
    {
        // Handle damage flash
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            
            if (flashTimer <= 0f && spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        OnDamageTaken?.Invoke(damage);
        
        // Visual feedback
        if (enableFlashing && spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            flashTimer = flashDuration;
        }
        
        // Check if dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        if (isDead)
        {
            return;
        }
        
        isDead = true;
        Debug.Log($"{gameObject.name} died!");
        
        OnDeath?.Invoke();
        
        // Spawn death effect
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Destroy enemy
        Destroy(gameObject, deathDelay);
    }
    
    public void Heal(int amount)
    {
        if (isDead)
        {
            return;
        }
        
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"{gameObject.name} healed {amount}. Health: {currentHealth}/{maxHealth}");
    }
    
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    
    public bool IsDead()
    {
        return isDead;
    }
    
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
}
