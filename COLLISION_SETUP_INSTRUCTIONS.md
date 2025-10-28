# Layer-Based Collision Setup Instructions

## Step 1: Create Layers

1. In Unity, go to **Edit > Project Settings > Tags and Layers**
2. Add these layers:
   - **Player** (e.g., Layer 6)
   - **Enemy** (e.g., Layer 7)
   - **Ground** (e.g., Layer 8)

## Step 2: Assign Layers to GameObjects

1. Select your **Player** GameObject
   - In the Inspector, change the Layer dropdown to "Player"
   - Click "Yes, change children" if prompted

2. Select your **Enemy** GameObject(s)
   - In the Inspector, change the Layer dropdown to "Enemy"
   - Click "Yes, change children" if prompted

3. Select your **Floor/Ground** GameObject(s)
   - In the Inspector, change the Layer dropdown to "Ground"
   - Click "Yes, change children" if prompted

## Step 3: Configure Layer Collision Matrix

1. Go to **Edit > Project Settings > Physics 2D**
2. Scroll down to the **Layer Collision Matrix**
3. Configure these interactions:
   - **Player vs Enemy**: UNCHECK (players can pass through enemies)
   - **Player vs Ground**: CHECK (players collide with ground)
   - **Enemy vs Ground**: CHECK (enemies collide with ground)
   - **Enemy vs Enemy**: CHECK or UNCHECK (depending if enemies should collide with each other)

## Step 4: Update Layer Masks in Scripts

The scripts already use LayerMasks, so update them in the Inspector:

### EnemyMovement.cs
- **Player Layer**: Set to "Player" layer
- **Ground Layer**: Set to "Ground" layer

### EnemyDamage.cs
This script uses collision callbacks, but we need to modify it to use triggers instead.

## Alternative: Use Trigger Colliders for Damage

If you want the enemy to damage the player but not physically block them, use triggers:

1. Add a **second collider** to the enemy:
   - Right-click on Enemy GameObject > Create Empty (child)
   - Name it "DamageZone"
   - Add a **BoxCollider2D** or **CircleCollider2D**
   - Check "Is Trigger"
   - Adjust the size to cover the enemy

2. Move the **EnemyDamage.cs** script to the "DamageZone" child object

3. The enemy's main collider stays as a regular collider (for ground collision)

This way:
- Enemy collides with ground (main collider)
- Enemy damages player via trigger (trigger collider on child)
- Player passes through enemy (layer collision matrix)
