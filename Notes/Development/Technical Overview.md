---
type: technical
status: active
tags:
  - development
  - unity
---

# Technical overview

## Project

- Engine: Unity
- Solution: `World Of Spirits/World of Spirits.slnx`
- Main source: `World Of Spirits/Assets/Scripts`
- Project organization reference: `World Of Spirits/Assets/Docs/PROJECT_STRUCTURE.md`

## Runtime architecture

- **Player:** identity, movement, and `SpiritManager`.
- **Spirits:** definitions, progression, membership, slots, abilities, and weapons.
- **Combat:** living entities, damage, targeting, projectiles, and automatic weapons.
- **Enemies:** regular enemy and boss foundations with separate movement behavior.
- **Progression:** experience, upgrades, unlocks, and pickups.
- **World:** area flow, timers, waves, and spawning.
- **UI:** gameplay controls and temporary debug feedback.

## Current invariants

- The player owns no more than three spirits.
- Spirit slots are main, support one, and support two.
- Normal enemies inherit `EnemyBase`.
- Bosses inherit `BossEnemyBase`.
- Damageable characters inherit `LivingEntity`.
- Ranged attacks use `ProjectileBase`.
- Orbiting melee attacks use `OrbitingMeleeWeapon`.
- Areas last ten minutes and finish with a boss encounter.

## Definition of done for a feature

- [ ] Design intent is written and linked.
- [ ] Runtime behavior is implemented.
- [ ] Inspector/prefab configuration is complete.
- [ ] A representative scene or test validates the behavior.
- [ ] Edge cases and tuning notes are captured.
- [ ] Documentation matches the implementation.
