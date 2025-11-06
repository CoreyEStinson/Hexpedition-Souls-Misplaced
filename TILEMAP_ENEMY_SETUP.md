# Enemy Movement with Tilemap Setup Guide

This guide explains how to set up enemy movement to work with Unity's Tilemap and Tilemap Collider 2D.

## Unity Tilemap Setup

### 1. Create Your Tilemap

1. **Create Grid and Tilemap:**
   - Right-click in Hierarchy → 2D Object → Tilemap → Rectangular
   - This creates a Grid with a Tilemap child

2. **Paint Your Level:**
   - Open Tile Palette (Window → 2D → Tile Palette)
   - Create or import tiles
   - Paint your level on the tilemap

### 2. Add Tilemap Collider 2D

1. **Select the Tilemap GameObject** in the Hierarchy

2. **Add Components:**
   - Add Component → Tilemap Collider 2D
   - (Optional but recommended) Add Component → Composite Collider 2D
     - This merges multiple tile colliders into one for better performance

3. **If using Composite Collider:**
   - The Rigidbody2D will be added automatically
   - Set Rigidbody2D Body Type to **Static**
   - On Tilemap Collider 2D, check **"Used By Composite"**
   - On Composite Collider 2D:
     - Set Geometry Type to **Polygons** (smoother edges)
     - Check **"Generation Type: Synchronous"**

### 3. Set Up Layers

1. **Create Layers:**
   - Edit → Project Settings → Tags and Layers
   - Add a new layer called **"Ground"** (or whatever you prefer)
   - Optionally add **"Walls"** for separate wall tilemaps

2. **Assign Layer to Tilemap:**
   - Select your Tilemap GameObject
   - In Inspector, set Layer to **"Ground"**

## Enemy GameObject Setup

### 1. Enemy Components Required

Your enemy should have:
- **Rigidbody2D**
  - Body Type: Dynamic
  - Gravity Scale: 1 (or your preferred value)
  - Collision Detection: Continuous
  - Interpolate: Interpolate
  - Constraints: Freeze Rotation Z (checked)

- **Collider2D** (BoxCollider2D or CapsuleCollider2D)
  - Adjust size to fit your enemy sprite
  - Should NOT be a trigger

- **EnemyMovement Script**

### 2. Configure EnemyMovement Script

In the Inspector, configure these settings:

#### Movement Settings
- **Patrol Speed**: 2 (adjust as needed)
- **Chase Speed**: 4 (adjust as needed)
- **Detection Range**: 5 (how far enemy can see)
- **Chase Linger**: 3 (seconds to keep chasing after losing sight)
- **Player Layer**: Set to "Player" layer
- **Obstacle Layer**: Set to layers that block vision (e.g., "Ground", "Walls")

#### Ledge Check Settings
- **Enable Ledge Check**: ✓ (checked)
- **Ledge Check Offset**: X: 0.5, Y: 0
  - Adjust X based on enemy size (should be at edge of enemy)
- **Ledge Check Distance**: 1.0
  - Should be long enough to reach the ground below
- **Ground Layer**: Select "Ground" (your tilemap layer)

#### Wall Check Settings
- **Enable Wall Check**: ✓ (checked)
- **Wall Check Distance**: 0.5
  - How far ahead to check for walls
- **Wall Check Offset**: X: 0.5, Y: 0.5
  - Adjust based on enemy center and size

#### Tilemap Compatibility
- **Raycast Skin Width**: 0.1
  - Small offset to prevent self-collision issues

## Layer Mask Setup

**Important:** Make sure your Layer Masks include the correct layers:

### Ground Layer Mask
Should include:
- Ground (your tilemap layer)
- Any platform layers

### Obstacle Layer Mask
Should include:
- Ground (for wall detection)
- Walls (if you have a separate wall tilemap)
- Any other obstacles that should block enemy vision

### Player Layer Mask
Should include:
- Player layer only

## Testing and Debugging

### Visual Debug Rays

The script draws debug rays in the Scene view:

- **Yellow Ray (down)**: Ledge detection
  - Green = ground detected
  - Red = no ground (will turn around)

- **Cyan/Blue Ray (forward)**: Wall detection
  - Blue = wall detected (will turn around)
  - Cyan = no wall (can continue)

- **Red Rays (left/right)**: Player detection range

### Common Issues and Solutions

#### Enemy Falls Through Tilemap
- Ensure Tilemap has Tilemap Collider 2D
- Check that Tilemap layer is included in Ground Layer mask
- Verify Rigidbody2D Collision Detection is set to Continuous
- If using Composite Collider, ensure it's set to Static

#### Enemy Doesn't Detect Ledges
- Increase Ledge Check Distance
- Adjust Ledge Check Offset X to be at the edge of the enemy
- Verify Ground Layer mask includes your tilemap layer
- Check that ledge check ray (yellow) is visible in Scene view

#### Enemy Doesn't Turn at Walls
- Increase Wall Check Distance
- Adjust Wall Check Offset Y to be at enemy's center height
- Ensure Ground Layer is included in Obstacle Layer mask
- Check that wall check ray (cyan/blue) is visible in Scene view

#### Enemy Doesn't See Player Through Gaps
- Adjust Detection Range
- Check Obstacle Layer mask includes walls but not empty space
- Ensure Player Layer mask is set correctly
- Position enemy at proper height

#### Enemy Gets Stuck on Tilemap Edges
- Add Physics Material 2D with zero friction to enemy collider
- Ensure Composite Collider is using Polygons geometry type
- Adjust Wall Check Distance and offset
- Consider increasing Raycast Skin Width slightly

## Multiple Tilemap Setup

If you have separate tilemaps for different purposes:

### Ground Tilemap
- Layer: "Ground"
- Used for: Floors, platforms
- Add to Ground Layer mask

### Wall Tilemap
- Layer: "Walls"
- Used for: Vertical walls, obstacles
- Add to Obstacle Layer mask

### Background Tilemap
- Layer: "Background"
- No collider needed
- Don't add to any enemy layer masks

## Physics Material (Optional)

To prevent enemies from sticking to walls:

1. **Create Physics Material 2D:**
   - Right-click in Project → Create → 2D → Physics Material 2D
   - Name it "NoFriction"
   - Set Friction to 0
   - Set Bounciness to 0

2. **Apply to Enemy:**
   - Select enemy GameObject
   - On the Collider2D component, assign the NoFriction material

3. **Apply to Tilemap (optional):**
   - Select Tilemap with Composite Collider 2D
   - Assign NoFriction material to Composite Collider 2D

## Performance Tips

1. **Use Composite Collider:**
   - Combines many tile colliders into fewer colliders
   - Significantly improves performance

2. **Layer Masks:**
   - Only include necessary layers
   - Fewer layers = fewer physics checks

3. **Detection Range:**
   - Don't make it too large
   - Larger range = more raycasts = more CPU usage

4. **Multiple Enemies:**
   - All enemies can share the same configuration
   - Player Transform can be found automatically

## Example Layer Setup

```
Layer 0: Default
Layer 3: Player
Layer 6: Ground (Tilemap)
Layer 7: Walls (Optional separate tilemap)
Layer 8: Enemy
```

**EnemyMovement Configuration:**
- Player Layer: Player (Layer 3)
- Ground Layer: Ground (Layer 6)
- Obstacle Layer: Ground + Walls (Layers 6, 7)

This setup ensures enemies:
- Walk on the tilemap ground
- Turn around at ledges
- Turn around at walls
- Detect and chase the player
- Have vision blocked by walls
