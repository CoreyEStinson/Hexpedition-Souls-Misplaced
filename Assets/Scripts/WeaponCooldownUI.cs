using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays weapon cooldown progress using a filled Image UI element.
/// The fill amount starts at 0 (empty) and fills to 1 as the cooldown progresses,
/// then resets to 0 when the weapon is ready to use again.
/// </summary>
public class WeaponCooldownUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerWeapons playerWeapons;
    [SerializeField] private Image cooldownFillImage;
    
    [Header("Visual Settings")]
    [SerializeField] private bool hideWhenReady = false;
    
    [Header("Animation")]
    [SerializeField] private bool smoothFill = true;
    [SerializeField] private float smoothSpeed = 10f;
    
    [Header("Switch Behavior")]
    [Tooltip("If true, disable Animator components on the player (and children) briefly when switching weapons.")]
    [SerializeField] private bool stopAllAnimatorsOnSwitch = false;
    [Tooltip("How long to disable found Animator components when switching weapons (seconds)")]
    [SerializeField] private float animatorDisableDuration = 0.15f;

    [Header("Layout")]
    [Tooltip("Prevent the cooldown UI from visually flipping when a parent (player) flips by keeping its world scale positive.")]
    [SerializeField] private bool preventFlip = true;

    // Cached desired world scale (positive) to maintain regardless of parent flips
    private Vector3 desiredWorldScale;
    
    private float currentCooldownTime = 0f;
    private float maxCooldownTime = 0f;
    private bool isOnCooldown = false;
    private float targetFillAmount = 0f;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        // Auto-find PlayerWeapons if not assigned
        if (playerWeapons == null)
        {
            playerWeapons = FindFirstObjectByType<PlayerWeapons>();
            
            if (playerWeapons == null)
            {
                Debug.LogError("WeaponCooldownUI: No PlayerWeapons found in scene!");
                enabled = false;
                return;
            }
        }
        
        // Validate fill image
        if (cooldownFillImage == null)
        {
            cooldownFillImage = GetComponent<Image>();
            
            if (cooldownFillImage == null)
            {
                Debug.LogError("WeaponCooldownUI: No fill Image assigned or found!");
                enabled = false;
                return;
            }
        }
        
        // Setup fill image
        cooldownFillImage.type = Image.Type.Filled;
        cooldownFillImage.fillMethod = Image.FillMethod.Horizontal;
        cooldownFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        cooldownFillImage.fillAmount = 0f;
        
        // Setup canvas group for fading
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null && hideWhenReady)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Subscribe to events
        if (playerWeapons != null)
        {
            playerWeapons.OnAttack.AddListener(OnWeaponAttack);
            playerWeapons.OnWeaponUnlocked.AddListener(OnWeaponUnlocked);
            playerWeapons.OnWeaponChanged.AddListener(OnWeaponChanged);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (playerWeapons != null)
        {
            playerWeapons.OnAttack.RemoveListener(OnWeaponAttack);
            playerWeapons.OnWeaponUnlocked.RemoveListener(OnWeaponUnlocked);
            playerWeapons.OnWeaponChanged.RemoveListener(OnWeaponChanged);
        }
    }

    private void Update()
    {
        if (isOnCooldown)
        {
            // Update cooldown timer
            currentCooldownTime += Time.deltaTime;
            
            // Calculate fill amount (0 to 1 as cooldown progresses)
            targetFillAmount = Mathf.Clamp01(currentCooldownTime / maxCooldownTime);
            
            // Check if cooldown is complete
            if (currentCooldownTime >= maxCooldownTime)
            {
                CompleteCooldown();
            }
        }
        
        // Smooth or instant fill update
        if (smoothFill)
        {
            cooldownFillImage.fillAmount = Mathf.Lerp(
                cooldownFillImage.fillAmount,
                targetFillAmount,
                Time.deltaTime * smoothSpeed
            );
        }
        else
        {
            cooldownFillImage.fillAmount = targetFillAmount;
        }
        
        // Handle visibility when ready
        if (hideWhenReady && canvasGroup != null)
        {
            float targetAlpha = (isOnCooldown || cooldownFillImage.fillAmount > 0.01f) ? 1f : 0f;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * smoothSpeed);
        }
    }

    /// <summary>
    /// Called when the player attacks with a weapon
    /// </summary>
    private void OnWeaponAttack()
    {
        WeaponData currentWeapon = playerWeapons.GetCurrentWeapon();
        
        if (currentWeapon == null)
        {
            return;
        }
        
        // Start cooldown
        StartCooldown(currentWeapon.attackCooldown);
    }

    /// <summary>
    /// Called when a weapon is unlocked/picked up
    /// </summary>
    private void OnWeaponUnlocked(WeaponData weapon)
    {
        if (weapon == null)
        {
            return;
        }
        
        // Start cooldown when weapon is picked up
        StartCooldown(weapon.attackCooldown);
    }

    /// <summary>
    /// Called when the player switches to a different weapon
    /// </summary>
    private void OnWeaponChanged(WeaponData weapon)
    {
        if (weapon == null)
        {
            return;
        }

        // Stop UI cooldown animation and optionally disable player animators briefly
        StopUiAndPlayerAnimations();

        // Start cooldown animation for the new weapon
        StartCooldown(weapon.attackCooldown);
    }

    private void StopUiAndPlayerAnimations()
    {
        // Immediately stop/reset the cooldown UI animation
        isOnCooldown = false;
        currentCooldownTime = 0f;
        targetFillAmount = 0f;
        if (cooldownFillImage != null)
        {
            cooldownFillImage.fillAmount = 0f;
        }

        // Optionally disable Animator components on the player for a short duration
        if (stopAllAnimatorsOnSwitch && playerWeapons != null)
        {
            Animator[] animators = playerWeapons.GetComponentsInChildren<Animator>(true);
            if (animators != null && animators.Length > 0)
            {
                StartCoroutine(DisableAnimatorsTemporarily(animators, animatorDisableDuration));
            }
            else
            {
                // Try to find a top-level Animator on the player object
                Animator top = playerWeapons.GetComponent<Animator>();
                if (top != null)
                {
                    StartCoroutine(DisableAnimatorsTemporarily(new Animator[] { top }, animatorDisableDuration));
                }
            }
        }
    }

    private System.Collections.IEnumerator DisableAnimatorsTemporarily(Animator[] animators, float duration)
    {
        if (animators == null || animators.Length == 0)
            yield break;

        // Cache enabled states
        bool[] previous = new bool[animators.Length];
        for (int i = 0; i < animators.Length; i++)
        {
            previous[i] = animators[i].enabled;
            animators[i].enabled = false;
        }

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Restore previous enabled state
        for (int i = 0; i < animators.Length; i++)
        {
            if (animators[i] != null)
                animators[i].enabled = previous[i];
        }
    }

    /// <summary>
    /// Start the cooldown timer
    /// </summary>
    private void StartCooldown(float cooldownDuration)
    {
        isOnCooldown = true;
        currentCooldownTime = 0f;
        maxCooldownTime = cooldownDuration;
        targetFillAmount = 0f;
        
        // Show the UI if hidden
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    /// <summary>
    /// Complete the cooldown and reset to ready state
    /// </summary>
    private void CompleteCooldown()
    {
        isOnCooldown = false;
        targetFillAmount = 0f;
        currentCooldownTime = 0f;
    }

    /// <summary>
    /// Manually set the cooldown progress (0 to 1)
    /// </summary>
    public void SetCooldownProgress(float progress)
    {
        targetFillAmount = Mathf.Clamp01(progress);
        
        if (!smoothFill)
        {
            cooldownFillImage.fillAmount = targetFillAmount;
        }
    }

    /// <summary>
    /// Get the current cooldown progress (0 to 1)
    /// </summary>
    public float GetCooldownProgress()
    {
        return targetFillAmount;
    }

    /// <summary>
    /// Check if weapon is currently on cooldown
    /// </summary>
    public bool IsOnCooldown()
    {
        return isOnCooldown;
    }

    /// <summary>
    /// Get remaining cooldown time in seconds
    /// </summary>
    public float GetRemainingCooldownTime()
    {
        if (!isOnCooldown)
        {
            return 0f;
        }
        
        return Mathf.Max(0f, maxCooldownTime - currentCooldownTime);
    }

    /// <summary>
    /// Reset the cooldown immediately
    /// </summary>
    public void ResetCooldown()
    {
        isOnCooldown = false;
        currentCooldownTime = 0f;
        targetFillAmount = 0f;
        cooldownFillImage.fillAmount = 0f;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure fill image is set up correctly in editor
        if (cooldownFillImage != null)
        {
            cooldownFillImage.type = Image.Type.Filled;
            cooldownFillImage.fillMethod = Image.FillMethod.Horizontal;
            cooldownFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            cooldownFillImage.fillAmount = 0f;
        }
    }
#endif
}
