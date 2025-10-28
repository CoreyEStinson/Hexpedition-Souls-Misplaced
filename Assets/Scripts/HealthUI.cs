using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [Header("Health Sprites")]
    [Tooltip("Sprites representing health states. Index 0 = 0 health, Index 1 = 1 health, etc.")]
    [SerializeField] private Sprite[] healthSprites;

    [Header("Sprite References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    /// <summary>
    /// Updates the health UI to show the corresponding sprite for the current health value.
    /// </summary>
    /// <param name="health">Current health value (0-6)</param>
    public void UpdateHealth(int health)
    {
        if (healthSprites == null || healthSprites.Length == 0)
        {
            Debug.LogWarning("Health sprites array is empty!");
            return;
        }

        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer component is not assigned!");
            return;
        }

        // Clamp health to valid sprite index range
        int spriteIndex = Mathf.Clamp(health, 0, healthSprites.Length - 1);

        // Update the sprite
        if (healthSprites[spriteIndex] != null)
        {
            spriteRenderer.sprite = healthSprites[spriteIndex];
        }
        else
        {
            Debug.LogWarning($"Health sprite at index {spriteIndex} is null!");
        }
    }
}
