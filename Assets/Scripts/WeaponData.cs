using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Identity")]
    public string weaponName;
    public Sprite weaponIcon;
    
    [Header("Damage Settings")]
    public int damage = 1;
    public float attackRange = 1f;
    public float attackCooldown = 0.5f;
    
    [Header("Attack Type")]
    public AttackType attackType = AttackType.Melee;
    public LayerMask targetLayers;
    
    [Header("Attack Visuals")]
    public Vector2 attackOffset = new Vector2(0.5f, 0f);
    public Vector2 attackSize = new Vector2(1f, 1f);
    public GameObject attackEffectPrefab;
    public AudioClip attackSound;
    
    [Header("Animation")]
    public AnimationClip attackAnimation;
    
    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float knockbackUpForce = 2f;
    
    [Header("Projectile Settings (if ranged)")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float projectileLifetime = 3f;
}

public enum AttackType
{
    Melee,
    Ranged,
    Magic
}
