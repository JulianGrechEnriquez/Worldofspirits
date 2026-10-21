---
type: design-concept
status: discussion
created: 2026-10-05
updated: 2026-10-06
tags: [game-design, characters, shepherd, abilities]
---

# Spirit Shepherd

## Confirmed identity

- Uses four base spirit slots (one lead and three support), compared with the Tamer's three. This is the initial prototype capacity.
- Spirits stay in companion form and cannot become weapons.
- Included in the planned three-character demo roster.
- **Combined ability moves are exclusive to the Shepherd. Spirits themselves never fuse.** Participating spirits keep their own identities and party slots.

The basic Shepherd prototype is implemented: a separate character prefab, character selection, four base slots, and companion-only combat. All unlocked companion abilities can cast while moving or standing still. Weapon upgrade cards are unavailable, and rotation keeps companions visible without form-change animations. Combined moves remain unimplemented.

## Starting spirits (implemented)

The Shepherd chooses two different spirits from the configured starter pool before gameplay begins, including on a fresh profile. The selection shows 0/2 and 1/2 progress, disables the first chosen card, and stays paused until the second spirit is selected. Both starters occupy the existing four-slot party, leaving two slots for later contracts. This selection does not change persistent spirit unlocks. Tamer still chooses one unlocked starter.

## Lead companion (implemented)

The spirit in the main slot is the Shepherd's lead companion. Each of its unlocked abilities gains one temporary effective level, capped at that ability's normal maximum. Locked abilities stay locked. Purchased levels remain unchanged, including when upgrading an ability while it is the leader.

Tab rotates the party and transfers the bonus to the new lead companion. Ability casts use the effective level's damage, count, cooldown, radius and other authored values. This replaces the proposed extra lead-damage bonus and stacks with the existing stationary boost. The HUD identifies the leader; pause-menu ability levels distinguish purchased levels from the temporary bonus. Tamer does not receive this bonus.

## Stationary ability boost (implemented)

Standing still gives the Shepherd +25% companion damage, +20% cast speed and +25% orbit speed. Movement removes these bonuses immediately. The values are adjustable on the Shepherd prefab's UpgradeRuntimeStats component.

The boost stacks with existing spirit damage and cooldown upgrades. Changing movement state preserves cooldown progress, and orbiting effects change speed without jumping to a new angle. A gameplay HUD indicator shows the active bonuses. Tamer stats are unaffected.

## Concentration animation (planned)

- [ ] Add a concentration animation for the Shepherd while standing still and the stationary ability boost is active.
- [ ] Return to movement animations when the player starts moving and the boost ends.

This animation is planned and has not been implemented yet.

## Combined ability moves

Combine compatible abilities into a special move once the player has invested in their progression. Reaching maximum level in a participating ability is the intended unlock direction. Whether both abilities must be maxed, or one must be maxed while the other is unlocked, remains undecided.

| Participating abilities | Proposed move | Proposed behavior |
|---|---|---|
| Water Whirlpool + Wind Tornado | Storm Vortex | A moving waterspout pulls enemies inward, applies Soaked and deals periodic damage. |
| Water Rain Clouds + Lightning Lightning Strike | Thunderstorm | Lightning strikes beneath a rain cloud. |
| Fire Fiery Talons + Wind Tornado | Flame Cyclone | A moving vortex carries flames through enemies. |
| Ice Ice Crystal + Water Whirlpool | Frozen Maelstrom | A chilling vortex gathers enemies before crystals shatter. |

Whirlpool + Tornado is the user's initial example. Move names and detailed effects are proposals. Storm Vortex is a candidate for the first demo prototype because Water and Wind are in the demo spirit pool. Lightning and Ice combinations are later-content examples, not additional demo roster requirements.

## Proposed casting rules

- Require ownership of both participating spirits and their relevant abilities.
- Unlock through an eligible upgrade after the chosen maximum-level prerequisites are met.
- Give the move its own shared cooldown and a cap on active effects.
- Consider one equipped combined move initially; the final limit is open.
- Decide whether a combined cast replaces the two normal casts or is a reduced-frequency additional cast.
- Apply normal elite/boss control resistance and pool cleanup.
- Extra casts from Grand Conductor must not recursively trigger combined moves.

These rules are not all approved. They must be finalized before adding runtime cards or effects.

## Relationship to exclusive upgrades

The Shepherd retains the planned 14-card exclusive pool. Decide whether combined moves are selectable effects within one of those cards, replace draft cards, or belong to a separate conditional combination pool. Do not automatically add extra cards or change the agreed rarity counts.

Elemental Harmony can remain a smaller interaction between elemental hits; a combined move is a specifically authored pairing of named abilities. Grand Conductor coordinates casts rather than merging spirits.

## Open decisions

- [ ] Both abilities maxed, or one maxed plus the other unlocked?
- [ ] Exact upgrade/pool placement and rarity.
- [ ] Maximum equipped combined moves.
- [ ] Cooldown, damage, duration and active-effect limits.
- [ ] Normal cast replacement versus additional combined casts.
- [ ] Which pairings belong in the demo and which follow later.

## Related

- [[Notes/Game Design/Playable Characters|Playable characters]]
- [[Notes/Game Design/Character Exclusive Upgrades|Exclusive upgrade drafts]]
- [[Notes/Game Design/Spirits|Spirit ability designs]]
- [[Notes/Planning/Demo Scope and Completion Plan|Demo scope]]
