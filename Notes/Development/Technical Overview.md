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
- Main source: `World Of Spirits/Assets/_WorldOfSpirits/Scripts`
- Project organization reference: `World Of Spirits/Assets/_WorldOfSpirits/Documentation/PROJECT_STRUCTURE.md`

## Runtime architecture

- **Player:** identity, movement, and `SpiritManager`.
- **Spirits:** definitions, progression, membership, slots, abilities, and weapons.
- **Combat:** living entities, damage, targeting, projectiles, and automatic weapons.
- **Enemies:** regular enemy and boss foundations with separate movement behavior.
- **Progression:** experience, upgrades, unlocks, and pickups.
- **World:** area flow, timers, waves, and spawning.
- **UI:** gameplay controls and temporary debug feedback.

## Architecture strengths

- ScriptableObject definitions separate content from runtime state.
- Combat and crowd simulations centralize high-volume work.
- Spatial indexes avoid full enemy scans for common queries.
- Enemies, projectiles, pickups, effects, and damage numbers use pooling.
- Interfaces define damage, status, reward, and enemy-classification boundaries.
- Biome spawning, abilities, weapons, and upgrades are data-driven.

## Current technical priorities

- Complete and profile one full Burning Plains run before expanding content breadth.
- Exclude development-only NuGet, MCP, and compiler DLLs from player builds.
- Add Runtime, Editor, and Tests assembly definitions.
- Add automated Edit Mode and Play Mode coverage.
- Split oversized classes only where responsibility mixing is slowing work or profiling shows a cost.
- Reduce per-object update work for high-volume pickups and combat UI.
- Remove or centralize forced physics synchronization in melee attacks.
- Use stable content IDs rather than GameObject names before implementing saves.

See [[Notes/Planning/Project Audit and Backlog|Project audit and backlog]] for the full audit and checklist.

## Current invariants

- The player owns no more than three spirits.
- Spirit slots are main, support one, and support two.
- Spirit rotation has a one-second cooldown.
- Selecting Fire, Earth, Water, or Wind as the main spirit grants its three-second rotation buff.
- Stationary weapon form begins after a 0.25-second transition and charges through Focused and Empowered states.
- Normal enemies inherit `EnemyBase`.
- Bosses inherit `BossEnemyBase`.
- Bosses use phase gates, elemental weaknesses and resistances, and reduced hard-control effects.
- Damageable characters inherit `LivingEntity`.
- Ranged attacks use `ProjectileBase`.
- Orbiting melee attacks use `OrbitingMeleeWeapon`.
- Story Mode stages last ten minutes and finish with a guardian encounter.
- Infinity Mode unlocks after all six Story Mode stages and runs the six guardians sequentially before endless scaling.

## Definition of done for a feature

- [ ] Design intent is written and linked.
- [ ] Runtime behavior is implemented.
- [ ] Inspector/prefab configuration is complete.
- [ ] A representative scene or test validates the behavior.
- [ ] Edge cases and tuning notes are captured.
- [ ] Documentation matches the implementation.
