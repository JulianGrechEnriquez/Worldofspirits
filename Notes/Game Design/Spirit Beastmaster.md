---
type: design-concept
status: discussion
created: 2026-10-05
updated: 2026-10-05
tags: [game-design, characters, companions, beastmaster]
---

# Spirit Beastmaster

## Confirmed concept

The character is named **Spirit Beastmaster**. Owned spirits become independent fighting creatures with their own movement, attacks and health bars. Enemies can attack and damage them. For example, the Fire Phoenix moves toward enemies and fights alongside the player as a creature.

When a creature's health reaches zero, it disappears and enters recovery, then returns automatically. Manual revival is not the chosen direction.

This is a character concept, not an implemented system. The number of active creatures, detailed controls and weapon/form restrictions are not decided.

The 5 October demo roster is Tamer, Warrior and Shepherd. Beastmaster is deferred to later development; see [[Notes/Planning/Demo Scope and Completion Plan|demo scope]].

## Difference from the Shepherd

The Shepherd focuses on a larger party of ability-casting companions. The Beastmaster's defining feature is independently moving, damageable spirit creatures that can temporarily be defeated. Do not assume the Shepherd also gains creature health and recovery.

## Proposed recovery behavior

- Disappear into spirit energy at zero health.
- Stop attacking and become untargetable during recovery.
- Show recovery progress on the spirit's party portrait.
- Reappear near the player after recovery.
- Consider full health and brief protection on return to prevent immediate defeat.
- A 15–20 second recovery is an initial tuning suggestion, not an approved value.

A returning creature needs a valid spawn position. Clear enemy targets when it disappears and reset its health/combat state when it returns. Define what happens to existing projectiles and persistent effects, and ensure creature defeat does not grant enemy-kill rewards.

## Possible creature identities

- Fire Phoenix: circles targets and dives at enemies.
- Earth Golem: close-range fighter that blocks space.
- Water Leviathan: sweeps through groups and pushes enemies.
- Wind Roc: fast harassment of ranged threats.
- Ice Yeti: charges and slows groups.

These behaviors are proposals. The only confirmed example is that a Phoenix can independently attack enemies and has its own health.

## Possible upgrades

Explore creature health, healing, attack techniques, recovery speed and coordinated player/creature attacks. No exclusive Beastmaster cards have been designed or added to the catalog. Whether this character adopts the same 14-card structure remains to be decided.

## Open decisions

- [ ] Active creature count and owned-spirit capacity.
- [ ] Automatic targeting, follow distance, leash and optional commands.
- [ ] Whether spirits ever enter weapon form for this class.
- [ ] The player's own attacks while creatures fight or recover.
- [ ] Recovery duration, return health and return protection.
- [ ] Creature resistance to hazards, area attacks and boss mechanics.
- [ ] Upgrade pool and interactions with existing spirit cards.
- [ ] Creature navigation, friendly collision and performance budget.

## Related

- [[Notes/Game Design/Playable Characters|Playable character concepts]]
- [[Notes/Game Design/Spirits|Spirit identities]]
- [[Notes/Game Design/Character Exclusive Upgrades|Warrior and Shepherd upgrade drafts]]
