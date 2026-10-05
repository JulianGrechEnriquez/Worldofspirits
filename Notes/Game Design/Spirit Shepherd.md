---
type: design-concept
status: discussion
created: 2026-10-05
updated: 2026-10-05
tags: [game-design, characters, shepherd, abilities]
---

# Spirit Shepherd

## Confirmed identity

- Owns more companion spirits than the standard Tamer; exact capacity remains undecided.
- Spirits stay in companion form and cannot become weapons.
- Included in the planned three-character demo roster.
- **Combined ability moves are exclusive to the Shepherd. Spirits themselves never fuse.** Participating spirits keep their own identities and party slots.

These are design rules. The Shepherd and combined move system are not implemented yet.

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
