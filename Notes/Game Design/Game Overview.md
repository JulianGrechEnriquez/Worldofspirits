---
type: design
status: active
updated: 2026-08-17
tags: [game-design, core]
---

# Game overview

## High concept

**Genre:** 2D roguelike survival  
**Theme:** Fantasy adventure  
**Inspirations:** Vampire Survivors, Soulstone Survivors, Halls of Torment

The player is a Spirit Tamer who forms contracts with elemental spirits. Every spirit can become an elemental weapon or act as a support companion. The player crosses six corrupted planes, defeats their guardians, and restores balance between the Human and Spirit Worlds.

## Player-facing hook

World of Spirits turns movement into a combat stance:

- **Moving:** the main spirit remains in companion form and automatically casts support abilities.
- **Stationary:** after a short transformation, the main spirit becomes its weapon and attacks automatically.
- **Rotating spirits:** changes the active weapon and grants a temporary elemental buff.

The player chooses between mobility, support casting, weapon charging, and timed spirit rotation rather than only steering around enemies.

## Core loop

1. Select a mode and stage.
2. Begin with the spirit associated with that plane.
3. Defeat enemies and collect experience.
4. Choose upgrades or contract another spirit.
5. Rotate a party of up to three spirits.
6. Survive for ten minutes in Story Mode.
7. Defeat the guardian and unlock the next stage.

## Game modes

### Story Mode

Story Mode contains six separate ten-minute stages. Each stage ends with a guardian boss. Defeating that boss completes the stage and unlocks the next plane.

| Order | Plane | Starting spirit | Guardian |
|---:|---|---|---|
| 1 | Burning Plains | Fire | Fire Phoenix |
| 2 | Frozen Wastes | Ice | Ice Yeti |
| 3 | Thunder Peaks | Lightning | Storm Dragon |
| 4 | Poison Marsh | Poison | Giant Scorpion |
| 5 | Shadow Realm | Necrotic | Necrotic Bat King |
| 6 | Celestial Temple | Holy | Fallen Angel |

The stage determines the starting spirit. Up to two additional spirits can be contracted during the run.

### Infinity Mode

Infinity Mode unlocks after all six Story Mode stages are complete. It combines enemies and hazards from every plane in one continuous run. All six Story Mode bosses appear sequentially, and play continues after the final boss for as long as the player survives.

## Run rules

- A party contains no more than three spirits: main, support one, and support two.
- Rotation manually moves the next spirit into the main slot and has a one-second cooldown.
- Selecting a new main spirit grants a three-second elemental buff.
- Existing projectiles and persistent effects remain active when their spirit changes role.
- Story Mode stages last ten minutes and finish with a guardian encounter.

## Current production focus

The immediate milestone remains a polished Burning Plains vertical slice:

> Menu → Burning Plains → Fire Phoenix → reward → stage completion → menu

The full Story Mode and Infinity Mode are the long-term structure. See [[Notes/Planning/Roadmap|Roadmap]].

## Related

- [[Spirits]]
- [[Combat Rules and Elements]]
- [[Areas and Bosses]]
- [[Notes/Development/Technical Overview]]

