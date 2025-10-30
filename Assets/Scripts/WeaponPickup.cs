using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private string weaponName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerWeapons playerWeapons = other.GetComponent<PlayerWeapons>();
            if (playerWeapons != null)
            {
                playerWeapons.UnlockWeapon(weaponName);
                Debug.Log($"Player picked up weapon: {weaponName}");
                Destroy(gameObject);
            }
        }
    }
}
