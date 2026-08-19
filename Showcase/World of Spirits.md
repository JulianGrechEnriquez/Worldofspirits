---
type: showcase
status: draft
project: World of Spirits
updated: 2026-08-17
tags: [portfolio, game-development, unity]
---

# World of Spirits

> A 2D roguelike survival game where movement changes how your elemental companions fight.

## Project summary

World of Spirits is a solo-developed Unity game about a Spirit Tamer restoring balance across six corrupted elemental planes. The player forms a party of up to three spirits, rotates their active companion during combat, and builds a different combination of weapons and support abilities each run.

## Distinctive mechanic

The main spirit changes role according to player movement:

- While moving, it acts as a support companion and automatically casts abilities.
- While stationary, it transforms into an elemental weapon that charges through Focused and Empowered states.
- Rotating the party changes the weapon and grants a short elemental buff.

This creates a constant choice between mobility, automatic support, weapon commitment, and rotation timing.

## Planned modes

- **Story Mode:** six ten-minute stages, each with a required starting spirit and guardian boss.
- **Infinity Mode:** unlockable mixed-plane survival with all six guardians in one continuous run, followed by endless scaling.

## Current playable focus

The active vertical slice is Burning Plains: survive enemy waves, develop a three-spirit build, and defeat the Fire Phoenix across multiple phases and a one-time Rebirth.

The initial production spirits are Fire, Earth, Water, and Wind. Each has one five-level weapon, three five-level abilities, and a defined combat identity.

## Technical highlights

- Unity 6 and the Universal Render Pipeline for 2D.
- ScriptableObject-driven spirits, weapons, abilities, enemies, upgrades, bosses, and biomes.
- Pooled enemies, projectiles, effects, pickups, and combat text.
- Spatial combat queries and centralized crowd simulation for high enemy counts.
- Modular boss attacks with phase gates, telegraphs, recovery windows, and one-time death prevention.
- Keyboard/controller-ready input and data-driven upgrade progression.

## Current status

**In development.** The immediate goal is a complete Burning Plains run from menu to reward screen, followed by external playtesting and performance validation.

## Portfolio asset checklist

- [ ] Hero gameplay screenshot.
- [ ] Short clip showing moving support mode and stationary weapon mode.
- [ ] Spirit rotation and buff clip.
- [ ] Fire Phoenix telegraph and Rebirth clip.
- [ ] Upgrade selection screenshot.
- [ ] Performance capture with a large enemy wave.
- [ ] Download link or playable web/build link.
- [ ] Concise development retrospective.

## Internal references

- [[Notes/Game Design/Game Overview|Game overview]]
- [[Notes/Game Design/Combat Rules and Elements|Combat rules]]
- [[Notes/Game Design/Spirits|Spirit kits]]
- [[Notes/Game Design/Areas and Bosses|Areas and bosses]]
- [[Notes/Development/Technical Overview|Technical overview]]

