---
type: design
status: draft
tags:
  - game-design
  - core
---

# Game overview

## High concept

**Genre:** 2D roguelike survival  
**Theme:** Fantasy adventure  
**Inspirations:** Vampire Survivors, Soulstone Survivors, Halls of Torment

The player is a Spirit Tamer who forms contracts with elemental spirits. Each spirit provides a weapon, abilities, and build options. The player crosses six corrupted regions, purifies their guardians, and restores balance between the Human and Spirit Worlds.

## Core loop

1. Select a spirit.
2. Enter an area.
3. Defeat enemies and collect experience.
4. Level up and choose upgrades or acquire another spirit.
5. Survive for ten minutes.
6. Defeat the area boss.
7. Unlock rewards and proceed.

## Run rules

- A run supports at most three spirits: one starting spirit and up to two acquired spirits.
- The active main spirit can change during play.
- While stationary, the main spirit becomes its elemental weapon and attacks automatically.
- While moving, the main spirit behaves like a support spirit and casts abilities.
- Support spirits continue attacking in both movement states.

## Progression

The Fire Spirit is unlocked by default. Other spirits are earned through summoning challenges opened by defeating guardians.

The meta-progression system is not yet designed. See [[Notes/Planning/Roadmap#Design gaps|Roadmap]].

## Fusion

When two spirits have maximized a specific ability, an upgrade choice may become a fusion ability based on that spirit pairing.

> [!question] Design decision needed
> Specify which ability levels, pairings, and selection rules unlock fusion offers.

## Related

- [[Spirits]]
- [[Areas and Bosses]]
- [[Notes/Development/Technical Overview]]
