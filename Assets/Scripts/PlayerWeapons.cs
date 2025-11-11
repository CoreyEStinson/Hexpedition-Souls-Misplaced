using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerWeapons : MonoBehaviour
{
    [Header("Weapon Setup")]
    [SerializeField] private List<WeaponData> availableWeapons = new List<WeaponData>();
    [SerializeField] private List<string> unlockedWeaponNames = new List<string>();
    [SerializeField] private int currentWeaponIndex = 0;
    
    [Header("Attack Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private KeyCode attackKey = KeyCode.E;
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer weaponSpriteRenderer;
    [SerializeField] private Animator animator;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    
    [Header("Events")]
    public UnityEvent<WeaponData> OnWeaponChanged;
    public UnityEvent<WeaponData> OnWeaponUnlocked;
    public UnityEvent OnAttack;
    
    private float lastAttackTime = -999f;
    private PlayerHealth playerHealth;
    private Collider2D attackCollider;
    
    // Property to get facing direction from player's scale
    private bool FacingRight
    {
        get { return transform.localScale.x > 0f; }
    }
    
    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (attackPoint == null)
        {
            // Create a default attack point if none is assigned
            GameObject attackPointObj = new GameObject("AttackPoint");
            attackPointObj.transform.SetParent(transform);
            attackPointObj.transform.localPosition = new Vector3(0.5f, 0.5f, 0f);
            attackPoint = attackPointObj.transform;
        }

        if (attackPoint != null)
        {
            attackCollider = attackPoint.GetComponent<Collider2D>();
            if (attackCollider == null)
            {
                Debug.LogWarning("Attack point does not have a Collider2D. Melee attacks may not work correctly. Please add a Collider2D to the attack point object.");
            }
        }
        
        UpdateWeaponVisual();
    }
    
    private void Update()
    {
        // Check if player can act (not knocked back)
        bool canAct = playerHealth == null || !playerHealth.IsKnockedBack();
        
        if (!canAct)
        {
            return;
        }
        
        // Weapon switching with number keys (1-9)
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SwitchToWeaponByIndex(i);
                break;
            }
        }
        
        // Mouse wheel weapon switching
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            SwitchToNextWeapon();
        }
        else if (scroll < 0f)
        {
            SwitchToPreviousWeapon();
        }
        
        // Attack with E key or Mouse Button 1 (right click)
        if (Input.GetKeyDown(attackKey) || Input.GetMouseButtonDown(1))
        {
            TryAttack();
        }
    }
    
    public void UnlockWeapon(string weaponName)
    {
        if (unlockedWeaponNames.Contains(weaponName))
        {
            Debug.Log($"Weapon {weaponName} already unlocked!");
            return;
        }
        
        // Find the weapon in available weapons
        WeaponData weapon = availableWeapons.Find(w => w.weaponName == weaponName);
        if (weapon != null)
        {
            unlockedWeaponNames.Add(weaponName);
            Debug.Log($"Unlocked weapon: {weaponName}");
            OnWeaponUnlocked?.Invoke(weapon);
            
            // If this is the first weapon, equip it
            if (unlockedWeaponNames.Count == 1)
            {
                currentWeaponIndex = 0;
                UpdateWeaponVisual();
                OnWeaponChanged?.Invoke(weapon);
            }
            else
            {
                // Switch to the newly unlocked weapon if not the first
                currentWeaponIndex = unlockedWeaponNames.Count - 1;
                UpdateWeaponVisual();
                OnWeaponChanged?.Invoke(weapon);
            }
        }
        else
        {
            Debug.LogWarning($"Weapon {weaponName} not found in available weapons list!");
        }
    }
    
    public void SwitchToWeaponByIndex(int index)
    {
        if (unlockedWeaponNames.Count == 0)
        {
            return;
        }
        
        // Make sure the index is within bounds
        if (index >= 0 && index < unlockedWeaponNames.Count)
        {
            currentWeaponIndex = index;
            UpdateWeaponVisual();
            
            WeaponData currentWeapon = GetCurrentWeapon();
            if (currentWeapon != null)
            {
                Debug.Log($"Switched to: {currentWeapon.weaponName}");
                OnWeaponChanged?.Invoke(currentWeapon);
            }
        }
    }
    
    public void SwitchToNextWeapon()
    {
        if (unlockedWeaponNames.Count <= 1)
        {
            return;
        }
        
        currentWeaponIndex = (currentWeaponIndex + 1) % unlockedWeaponNames.Count;
        UpdateWeaponVisual();
        
        WeaponData currentWeapon = GetCurrentWeapon();
        if (currentWeapon != null)
        {
            Debug.Log($"Switched to: {currentWeapon.weaponName}");
            OnWeaponChanged?.Invoke(currentWeapon);
        }
    }
    
    public void SwitchToPreviousWeapon()
    {
        if (unlockedWeaponNames.Count <= 1)
        {
            return;
        }
        
        currentWeaponIndex--;
        if (currentWeaponIndex < 0)
        {
            currentWeaponIndex = unlockedWeaponNames.Count - 1;
        }
        
        UpdateWeaponVisual();
        
        WeaponData currentWeapon = GetCurrentWeapon();
        if (currentWeapon != null)
        {
            Debug.Log($"Switched to: {currentWeapon.weaponName}");
            OnWeaponChanged?.Invoke(currentWeapon);
        }
    }
    
    private void UpdateWeaponVisual()
    {
        WeaponData currentWeapon = GetCurrentWeapon();
        
        if (weaponSpriteRenderer != null)
        {
            if (currentWeapon != null)
            {
                weaponSpriteRenderer.sprite = currentWeapon.weaponIcon;
                weaponSpriteRenderer.enabled = currentWeapon.weaponIcon != null;
            }
            else
            {
                // No weapon equipped, disable sprite renderer
                weaponSpriteRenderer.enabled = false;
            }
        }
    }
    
    private void TryAttack()
    {
        WeaponData currentWeapon = GetCurrentWeapon();
        
        if (currentWeapon == null)
        {
            Debug.Log("No weapon equipped!");
            return;
        }
        
        // Check cooldown
        if (Time.time - lastAttackTime < currentWeapon.attackCooldown)
        {
            return;
        }
        
        lastAttackTime = Time.time;
        
        // Play attack animation
        if (animator != null && currentWeapon.attackAnimation != null)
        {
            Debug.Log("Playing attack animation: " + currentWeapon.attackAnimation.name);
            animator.Play(currentWeapon.attackAnimation.name, -1, 0f);
        }
        
        // Perform attack based on weapon type
        switch (currentWeapon.attackType)
        {
            case AttackType.Melee:
                PerformMeleeAttack(currentWeapon);
                break;
            case AttackType.Ranged:
                PerformRangedAttack(currentWeapon);
                break;
            case AttackType.Magic:
                PerformMagicAttack(currentWeapon);
                break;
        }
        
        // Play attack sound
        if (audioSource != null && currentWeapon.attackSound != null)
        {
            audioSource.PlayOneShot(currentWeapon.attackSound);
        }
        
        // Spawn attack effect
        if (currentWeapon.attackEffectPrefab != null && attackPoint != null)
        {
            GameObject effect = Instantiate(currentWeapon.attackEffectPrefab, attackPoint.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
        
        OnAttack?.Invoke();
        Debug.Log($"Attacked with {currentWeapon.weaponName}!");
    }
    
    private void PerformMeleeAttack(WeaponData weapon)
    {
        if (attackCollider == null)
        {
            Debug.LogWarning("No attack collider found on the attack point. Cannot perform melee attack.");
            return;
        }

        // Use the attack collider to detect enemies
        List<Collider2D> hitEnemies = new List<Collider2D>();
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(weapon.targetLayers);
        contactFilter.useTriggers = true;
        
        int hitCount = Physics2D.OverlapCollider(attackCollider, contactFilter, hitEnemies);

        if (hitCount > 0)
        {
            foreach (Collider2D enemy in hitEnemies)
            {
                // Apply damage
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(weapon.damage);
                }
                
                // Apply knockback
                Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                    Vector2 knockback = new Vector2(
                        knockbackDirection.x * weapon.knockbackForce,
                        weapon.knockbackUpForce
                    );
                    enemyRb.linearVelocity = knockback;
                }
                
                Debug.Log($"Hit enemy: {enemy.name}");
            }
        }
    }
    
    private void PerformRangedAttack(WeaponData weapon)
    {
        if (weapon.projectilePrefab == null || attackPoint == null)
        {
            Debug.LogWarning("Ranged weapon missing projectile prefab!");
            return;
        }
        
        // Spawn projectile
        GameObject projectile = Instantiate(weapon.projectilePrefab, attackPoint.position, Quaternion.identity);
        
        // Set projectile direction and speed
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb != null)
        {
            Vector2 direction = FacingRight ? Vector2.right : Vector2.left;
            projectileRb.linearVelocity = direction * weapon.projectileSpeed;
        }
        
        // Setup projectile damage
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(weapon.damage, weapon.knockbackForce, weapon.targetLayers);
        }
        
        // Destroy projectile after lifetime
        Destroy(projectile, weapon.projectileLifetime);
    }
    
    private void PerformMagicAttack(WeaponData weapon)
    {
        // Similar to ranged but could have different effects
        // For now, use the same logic as ranged
        PerformRangedAttack(weapon);
    }
    
    public WeaponData GetCurrentWeapon()
    {
        if (unlockedWeaponNames.Count == 0 || currentWeaponIndex >= unlockedWeaponNames.Count)
        {
            return null;
        }
        
        string currentWeaponName = unlockedWeaponNames[currentWeaponIndex];
        return availableWeapons.Find(w => w.weaponName == currentWeaponName);
    }
    
    public List<string> GetUnlockedWeapons()
    {
        return new List<string>(unlockedWeaponNames);
    }
    
    public bool HasWeapon(string weaponName)
    {
        return unlockedWeaponNames.Contains(weaponName);
    }
    
    // Visual debugging
    private void OnDrawGizmosSelected()
    {
        // Gizmos are now handled by the collider on the attack point
    }
}
