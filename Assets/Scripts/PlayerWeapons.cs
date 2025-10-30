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
    [SerializeField] private KeyCode attackKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode nextWeaponKey = KeyCode.E;
    [SerializeField] private KeyCode previousWeaponKey = KeyCode.Q;
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer weaponSpriteRenderer;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    
    [Header("Events")]
    public UnityEvent<WeaponData> OnWeaponChanged;
    public UnityEvent<WeaponData> OnWeaponUnlocked;
    public UnityEvent OnAttack;
    
    private float lastAttackTime = -999f;
    private bool facingRight = true;
    private PlayerHealth playerHealth;
    
    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        if (attackPoint == null)
        {
            // Create a default attack point if none is assigned
            GameObject attackPointObj = new GameObject("AttackPoint");
            attackPointObj.transform.SetParent(transform);
            attackPointObj.transform.localPosition = new Vector3(0.5f, 0.5f, 0f);
            attackPoint = attackPointObj.transform;
        }
        
        // Unlock first weapon by default if we have any
        if (availableWeapons.Count > 0 && unlockedWeaponNames.Count == 0)
        {
            UnlockWeapon(availableWeapons[0].weaponName);
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
        
        // Update facing direction based on movement
        UpdateFacingDirection();
        
        // Weapon switching
        if (Input.GetKeyDown(nextWeaponKey))
        {
            SwitchToNextWeapon();
        }
        else if (Input.GetKeyDown(previousWeaponKey))
        {
            SwitchToPreviousWeapon();
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
        
        // Attack
        if (Input.GetKeyDown(attackKey) || Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }
    }
    
    private void UpdateFacingDirection()
    {
        // Check horizontal input to determine facing
        float horizontal = Input.GetAxis("Horizontal");
        if (Mathf.Abs(horizontal) > 0.01f)
        {
            facingRight = horizontal > 0f;
        }
        
        // Update attack point position based on facing
        if (attackPoint != null)
        {
            Vector3 localPos = attackPoint.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * (facingRight ? 1f : -1f);
            attackPoint.localPosition = localPos;
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
        }
        else
        {
            Debug.LogWarning($"Weapon {weaponName} not found in available weapons list!");
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
        
        if (weaponSpriteRenderer != null && currentWeapon != null)
        {
            weaponSpriteRenderer.sprite = currentWeapon.weaponIcon;
            weaponSpriteRenderer.enabled = currentWeapon.weaponIcon != null;
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
        if (attackPoint == null)
        {
            return;
        }
        
        // Calculate attack position and size
        Vector2 attackPosition = attackPoint.position;
        Vector2 attackOffset = new Vector2(
            weapon.attackOffset.x * (facingRight ? 1f : -1f),
            weapon.attackOffset.y
        );
        attackPosition += attackOffset;
        
        // Detect enemies in range
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPosition, weapon.attackSize, 0f, weapon.targetLayers);
        
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
            Vector2 direction = facingRight ? Vector2.right : Vector2.left;
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
        WeaponData currentWeapon = GetCurrentWeapon();
        
        if (currentWeapon != null && attackPoint != null && currentWeapon.attackType == AttackType.Melee)
        {
            Gizmos.color = Color.red;
            
            Vector2 attackPosition = attackPoint.position;
            Vector2 attackOffset = new Vector2(
                currentWeapon.attackOffset.x * (facingRight ? 1f : -1f),
                currentWeapon.attackOffset.y
            );
            attackPosition += attackOffset;
            
            Gizmos.DrawWireCube(attackPosition, currentWeapon.attackSize);
        }
    }
}
