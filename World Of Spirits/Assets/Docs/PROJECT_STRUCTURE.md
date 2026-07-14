# World of Spirits Project Structure

This Unity project is organized by gameplay responsibility. Keep scripts in the
most specific existing folder and always move Unity `.meta` files with assets.

## Runtime Scripts

- `Assets/Scripts/Combat/Damage` - living entities, damage contracts, and contact damage.
- `Assets/Scripts/Combat/Projectiles` - projectile bases and projectile implementations.
- `Assets/Scripts/Combat/Weapons` - reusable automatic weapon attacks.
- `Assets/Scripts/Combat` - shared combat targeting and weapon foundations.
- `Assets/Scripts/Enemies/Movement` - normal enemy bases and chase movement.
- `Assets/Scripts/Enemies/Bosses` - boss bases and behavior that does not inherit normal chasing.
- `Assets/Scripts/Player` - player identity and movement.
- `Assets/Scripts/Spirits` - spirit membership and slot rotation management.
- `Assets/Scripts/Spirits` - spirit definitions, membership, progression, and slot management.
- `Assets/Scripts/Spirits/Abilities` - ability foundations, implementations, scaling, and spawned effects.
- `Assets/Scripts/Spirits/Weapons` - weapon behavior such as orbiting melee weapons.
- `Assets/Scripts/UI/Buttons` - interactive gameplay buttons.
- `Assets/Scripts/UI/Debug` - debug HUD and temporary combat feedback.
- `Assets/Scripts/Core` - shared constants and cross-feature systems.
- `Assets/Scripts/Data` - ScriptableObject definitions and serialized data.
- `Assets/Scripts/Progression` - experience, upgrades, unlocks, and pickups.
- `Assets/Scripts/World` - area flow, timers, waves, and spawning.

## Content

- `Assets/Prefabs/Spirits` - Fire, Earth, and future spirit prefabs.
- `Assets/Prefabs/Projectiles` - projectile prefabs.
- `Assets/Prefabs/Enemies` - normal enemy and boss prefabs.
- `Assets/Art/Sprites/Spirits` - source sprites grouped by spirit.
- `Assets/Animation/Spirits` - spirit animation clips and controllers.
- `Assets/Animation/Projectiles` - projectile animations.
- `Assets/ScriptableObjects` - reusable ability, spirit, enemy, and area data.

## Naming Rules

- Use PascalCase for C# files and type names.
- Use correctly spelled singular object names and plural collection folders.
- Use `Spirit`, `Weapon`, `Projectile`, and `Support` consistently.
- Keep base classes beside the concrete implementations that inherit from them.
- Avoid placing scripts directly in broad folders when a specific subfolder exists.

## Current Scene Setup

1. The player owns `PlayerCharacter`, `PlayerMovement`, and `SpiritManager`.
2. Spirit slots are ordered as main, support one, and support two.
3. Normal enemies inherit `EnemyBase`; bosses inherit `BossEnemyBase`.
4. Damageable characters inherit `LivingEntity`.
5. Ranged attacks use `ProjectileBase`; orbiting melee attacks use `OrbitingMeleeWeapon`.

## Design Rules

- The player may own at most three spirits.
- The main spirit changes with `Tab`.
- Support spirits attack in both movement states.
- Bosses are stationary by default and implement explicit movement per attack.
- Areas last ten minutes and end with a boss encounter.
