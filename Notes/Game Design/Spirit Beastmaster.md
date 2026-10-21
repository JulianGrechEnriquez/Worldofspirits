---
type: design-concept
status: discussion
created: 2026-10-05
updated: 2026-10-19
tags: [game-design, characters, companions, beastmaster]
---

# Spirit Beastmaster

## Confirmed concept

The character is named **Spirit Beastmaster**. Owned spirits become independent fighting creatures with their own movement, attacks and health bars. Enemies can attack and damage them. For example, the Fire Phoenix moves toward enemies and fights alongside the player as a creature.

When a creature's health reaches zero, it disappears and enters recovery, then returns automatically. Manual revival is not the chosen direction.

## Playable prototype — 18 October 2026

The Beastmaster is now available as a fourth option in the saved character selection screen. It starts with one spirit and uses the Tamer's three contract slots. Spirits become independent damageable creatures, with basic automatic close-range elemental attacks and their unlocked spirit abilities. The first ability starts at level one; other abilities can be unlocked and upgraded through spirit cards. Abilities cast from the creature and pause during recovery. Weapon forms remain disabled.

| Input | Order |
|---|---|
| Left-click an enemy | Focus that enemy, then resume the previous order when it dies or despawns. |
| Left-click the ground | Move to that location and fight nearby enemies. |
| Right-click the ground | Guard that location, fighting within four world units and returning when threats leave. |
| Q | Cancel focus, return to the player and defend while following. |
| Hold Space | Fire spirit bolts toward the cursor, including while all creatures recover. |

Commands apply to all owned creatures. World markers show the destination or focused enemy. Each creature has a health bar; the command panel displays health and recovery countdowns. Orders ignore UI clicks and paused gameplay.

The player has a separate spirit bolt: 6 base physical damage, a 0.35-second fire interval, speed 14, lifetime 1.2 seconds and a small knockback. It uses the projectile pool and shared player damage modifiers, respects pause and player health, and does not require a living spirit.

Initial tuning: 100 creature health, 12 base attack damage, 0.8-second attack interval, 18-second recovery and 1.5 seconds of protection after returning at full health. Shared damage modifiers apply to creature attacks. These values are prototype defaults, not final balance.

Enemies can pursue nearby creatures and crowd enemies can deal contact damage to them. Defeated creatures disappear, lose their hitbox, stop attacking and cannot be targeted; they return near the player automatically. Creature defeats grant no enemy rewards. Spirit-ability cards are available; weapon cards remain excluded.

Unity Play Mode checks passed for selection and card layout, starter creation, independent position, actual Input System mouse/Q bindings, guard/attack-move/recall orders, focus preservation and cleanup, enemy contact damage, creature attacks, defeat and recovery, and owner disable/reactivation. Spirit-specific creature techniques, terrain navigation, dedicated art, exclusive upgrades and full-run balancing remain future work.

19 October Play Mode verification also passed for the Space input binding, bolt cooldown and pause checks, firing during creature recovery, actual projectile collision and damage, knockback, basic ability casting from the creature, and ability disable/restore on defeat/recovery.

The 5 October demo plan deferred Beastmaster; work on this playable prototype began on 18 October. The earlier production scope remains historical context; see [[Notes/Planning/Demo Scope and Completion Plan|demo scope]].

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
