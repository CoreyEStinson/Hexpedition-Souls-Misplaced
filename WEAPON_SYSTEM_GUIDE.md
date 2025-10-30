# Player Weapons System

A comprehensive weapon system for your 2D game that supports melee, ranged, and magic weapons.

## Components Created

### 1. **WeaponData.cs**
A ScriptableObject that defines weapon properties. Create weapon assets in Unity via:
`Right Click → Create → Weapons → Weapon Data`

**Properties:**
- **Weapon Identity**: Name and icon
- **Damage Settings**: Damage amount, attack range, cooldown
- **Attack Type**: Melee, Ranged, or Magic
- **Attack Visuals**: Offset, size, effects, sounds
- **Knockback**: Force applied to enemies
- **Projectile Settings**: For ranged/magic weapons

### 2. **PlayerWeapons.cs**
The main weapon manager attached to the player.

**Features:**
- Weapon unlocking system
- Weapon switching (E/Q keys or mouse wheel)
- Attack handling (left mouse or configurable key)
- Melee attack with hitbox detection
- Ranged attack with projectile spawning
- Visual and audio feedback
- Integration with knockback system

**Inspector Setup:**
1. Add component to Player GameObject
2. Assign available weapons (WeaponData assets)
3. Create an empty GameObject as child called "AttackPoint" for attack origin
4. Optionally assign a SpriteRenderer for weapon visuals
5. Configure attack keys (default: Mouse0, E, Q)

### 3. **Projectile.cs**
Handles projectile behavior for ranged weapons.

**Features:**
- Damage on hit
- Knockback application
- Auto-destroy on impact or lifetime

### 4. **EnemyHealth.cs**
Health system for enemies.

**Features:**
- Health management
- Damage flash effect
- Death handling with effects
- Events for damage and death

### 5. **WeaponPickup.cs** (Already existed)
Allows players to pick up weapons in the world.

## Setup Instructions

### Creating a Weapon

1. **Create WeaponData Asset:**
   - Right-click in Project → Create → Weapons → Weapon Data
   - Name it (e.g., "Sword", "Bow", "FireStaff")

2. **Configure the Weapon:**
   - Set weapon name (must match pickup name)
   - Assign icon sprite
   - Set damage, range, cooldown
   - Choose attack type
   - Configure attack hitbox (offset and size for melee)
   - Add sound effects and visual effects

3. **For Ranged Weapons:**
   - Create a projectile prefab with:
     - SpriteRenderer (projectile visual)
     - Rigidbody2D (Dynamic, no gravity for straight shots)
     - CircleCollider2D or BoxCollider2D (Is Trigger = true)
     - Projectile script
   - Assign to weapon's projectilePrefab field

### Setting Up the Player

1. **Add PlayerWeapons component** to your Player GameObject

2. **Create Attack Point:**
   - Add empty child GameObject to Player
   - Name it "AttackPoint"
   - Position it where attacks should originate (e.g., 0.5 units in front)

3. **Configure PlayerWeapons:**
   - Drag all WeaponData assets to "Available Weapons" list
   - Assign AttackPoint transform
   - Configure keys if desired
   - Optionally add weapon sprite renderer

4. **Layer Setup:**
   - Create "Enemy" layer
   - Assign enemies to Enemy layer
   - In WeaponData, set targetLayers to include Enemy layer

### Setting Up Enemies

1. **Add EnemyHealth component** to enemy GameObjects
2. Configure max health
3. Optionally add death effect prefab
4. Ensure enemy has SpriteRenderer for flash effect

### Creating Weapon Pickups

1. Create an empty GameObject in scene
2. Add WeaponPickup component
3. Add a trigger collider (e.g., BoxCollider2D, Is Trigger = true)
4. Set weapon name to match a WeaponData asset
5. Optionally add a sprite renderer with weapon icon

## Controls

- **Left Mouse / Attack Key**: Attack with current weapon
- **E**: Switch to next weapon
- **Q**: Switch to previous weapon
- **Mouse Wheel**: Scroll through weapons

## Attack Types

### Melee
- Instant hitbox detection
- Uses OverlapBox to detect enemies
- Range and size configurable per weapon

### Ranged
- Spawns projectile prefab
- Travels in facing direction
- Hits enemies and destroys on contact

### Magic
- Currently uses ranged logic
- Can be customized for special effects

## Integration Notes

- Works with existing PlayerHealth knockback system
- Respects knockback state (can't attack while knocked back)
- Enemies knocked back when hit
- Visual feedback with gizmos in editor

## Extending the System

### Adding New Attack Types
Modify `PerformMagicAttack()` in PlayerWeapons.cs for custom behavior

### Custom Projectiles
Add scripts to projectile prefab for special effects (explosions, status effects, etc.)

### Weapon Upgrades
Extend WeaponData with upgrade levels or create separate upgraded weapon assets

## Example Weapons

### Sword (Melee)
```
Damage: 2
Attack Range: 1.5
Cooldown: 0.5s
Attack Size: 1.0 x 1.0
Knockback: 8
```

### Bow (Ranged)
```
Damage: 1
Attack Range: 10
Cooldown: 0.8s
Projectile Speed: 15
Knockback: 3
```

### Staff (Magic)
```
Damage: 3
Attack Range: 8
Cooldown: 1.0s
Projectile Speed: 10
Knockback: 5
```

## Troubleshooting

**Weapons not dealing damage:**
- Check targetLayers in WeaponData
- Ensure enemies are on correct layer
- Verify EnemyHealth component on enemies

**Projectiles not spawning:**
- Ensure projectile prefab is assigned
- Check projectile has Rigidbody2D
- Verify Projectile script is attached

**Can't pick up weapons:**
- Verify weapon name matches WeaponData asset name
- Check Player has PlayerWeapons component
- Ensure pickup has trigger collider

**Visual debugging:**
- Select Player in hierarchy
- Red wireframe box shows melee attack range in Scene view
- Adjust attack offset and size as needed
