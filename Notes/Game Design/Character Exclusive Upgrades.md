---
type: upgrade-design
status: draft
created: 2026-10-05
updated: 2026-10-05
tags: [game-design, characters, upgrades]
---

# Character exclusive upgrades

First design pass for Spirit Warrior and Spirit Shepherd: each receives five Common, four Uncommon, three Epic and two Legendary upgrades. Existing general and spirit upgrades remain. These 28 cards are proposals, not Unity assets or implemented effects. Names, numbers and individual effects need review before implementation.

Core character form rules and basic attacks must work without drawing a rare card. Upgrades enhance the playstyle rather than enabling basic play. Start with one purchase per proposed card; decide stacking and numerical values after a short-run prototype.

## Spirit Warrior — 14 proposals

All owned spirits remain weapons. Movement, main-slot weapon pairs and armour are the focus.

| Rarity | Upgrade | Proposed effect |
|---|---|---|
| Common | Running Steel | Weapon attacks deal bonus damage while the player is moving. |
| Common | Agile Guard | Reduce incoming damage while moving; does not grant invulnerability. |
| Common | Quick Draw | The first weapon attack after changing the main spirit comes out faster. It does not reset every weapon cooldown. |
| Common | Relentless Stride | Momentum decays more slowly during brief stops. Offer only if the character has a momentum system. |
| Common | Armour Affinity | Improve the strength of the main spirit's armour power. Offer only when that power exists. |
| Uncommon | Paired Rhythm | Alternating hits from the main spirit's two weapons empower a follow-up hit. Include defensive weapon counters; repeated hits from one weapon do not build the combo. |
| Uncommon | Cleaving Dash | A dash triggers a short attack with the main spirit's weapon. Requires a dash mechanic and a separate proc cooldown. |
| Uncommon | Countermarch | While retreating from a nearby threat, a secondary ranged weapon periodically attacks behind the player. Melee/defensive pairs instead perform a rear guard strike. |
| Uncommon | Elemental Relay | After rotation, the next main-weapon hit gains a technique of the incoming spirit's element. Define each technique separately. |
| Epic | Living Armour | Upgrade the current spirit armour power with an additional elemental movement effect, such as Earth impact protection or a Fire dash burst. Does not turn spirits into companions. |
| Epic | Momentum Finisher | At full momentum, the next eligible main-weapon hit spends the meter for an elemental finisher. Requires momentum; cannot be triggered repeatedly by the same attack. |
| Epic | Perfect Exchange | Rotation performs a brief outgoing weapon strike followed by an incoming weapon strike. These are reduced-strength proc attacks, not full cooldown resets. |
| Legendary | Arsenal Awakening | Periodically activate the paired weapons of every owned spirit for a short coordinated assault. Limit duration, proc rate and spawned attacks. |
| Legendary | Sovereign Armament | Concentrate power in the main spirit: strengthen its weapon pair and armour, while non-main weapons attack less frequently. A specialization alternative to the whole-arsenal build. |

### Warrior build directions

- **Mobile arsenal:** Running Steel, Paired Rhythm, Perfect Exchange and Arsenal Awakening reward movement and rotation.
- **Armoured specialist:** Agile Guard, Armour Affinity, Living Armour and Sovereign Armament concentrate on the current main spirit.
- **Momentum attacker:** Relentless Stride and Momentum Finisher reward moving through combat, if momentum is adopted.

The two legendaries are proposed alternative paths. Decide whether to make them mutually exclusive; do not silently apply both until their combined behavior is defined. Dash, momentum and armour availability remain tied to the unfinished character design.

## Spirit Shepherd — 14 proposals

The newer [[Notes/Game Design/Spirit Shepherd#Combined ability moves|Shepherd-only combined move direction]] combines abilities, never spirits. Decide how it fits the 14-card pool before replacing or adding draft cards.

Spirits stay companions and cannot become weapons. The character has greater party capacity than the Tamer; the final base and maximum capacities are still undecided. Companion attacks continue while stationary.

| Rarity | Upgrade | Proposed effect |
|---|---|---|
| Common | Pack Instinct | A companion deals bonus damage to an enemy recently hit by a different owned spirit. Each spirit is counted once per interaction window. |
| Common | Close Bond | Companions near the player recover ability cooldowns slightly faster. Does not penalize companions outside the radius. |
| Common | Far Sight | Increase valid targeting range for companion abilities that can use range. Does not increase area radius or bypass obstacles. |
| Common | Lingering Presence | Increase the lifetime of compatible companion-created zones and effects, with a duration cap. Does not prolong Freeze or other hard control automatically. |
| Common | Safe Haven | Nearby companion casts periodically grant a small player shield. Use one shared cooldown and a shield cap, rather than one shield per particle or damage tick. |
| Uncommon | Growing Flock | Increase contract capacity by one within the approved Shepherd maximum. Grants a slot, not a free companion; suppress the card at the capacity cap. |
| Uncommon | Guided Hunt | The lead companion's attacks briefly mark a target, giving the other companions a damage bonus against it. Existing targeting remains functional without this upgrade. |
| Uncommon | Elemental Harmony | Hits from two different spirit elements trigger a small coordinated elemental effect. Use a shared cooldown; bosses retain control resistance. |
| Uncommon | Protective Recall | Taking damage periodically recalls nearby spirits for a brief defensive pulse that pushes normal enemies away. Companions keep their casting state; bosses receive reduced control. |
| Epic | Formation Mastery | Strengthen the selected formation: a close formation improves defence, while a spread formation improves attack coverage. Formation selection must exist before offering this card. |
| Epic | Spirit Relay | A normal cast briefly accelerates a different companion's next ability. Each cast can pass the relay once; relay benefits cannot recursively trigger themselves. |
| Epic | Shared Sanctuary | Overlapping zones created by two different spirits periodically grant player protection inside their overlap. Cap the benefit and do not count multiple zones from the same spirit as distinct partners. |
| Legendary | Grand Conductor | Periodically trigger a staggered coordinated cast from all companions. Use eligible offensive abilities, reduced-strength bonus casts and a shared cooldown; no recursive triggers or extra revival charges. |
| Legendary | Guardian Flock | Redirect a limited portion of player damage into a shared spirit ward that recharges as companions cast. Spirits remain active and cannot die from redirected damage; the ward has a strict absorption cap. |

### Shepherd build directions

- **Coordinated offense:** Pack Instinct, Guided Hunt, Spirit Relay and Grand Conductor reward teamwork.
- **Protected flock:** Close Bond, Safe Haven, Shared Sanctuary and Guardian Flock reward staying within the party's protection.
- **Larger party:** Growing Flock adds flexibility within a fixed capacity limit; it must not become an unlimited damage multiplier.

Grand Conductor and Guardian Flock provide offensive and defensive legendary directions. Whether both may be acquired in one run remains an open balance decision.

## Availability and balance rules

- Restrict every proposed card to its matching character.
- Check mechanic prerequisites: dash, momentum, armour, formation, lead companion, relevant weapon pair, or compatible effect.
- Suppress capacity cards at the approved cap and combo cards until enough spirits are owned.
- General Spirit Armor remains the existing armour-stat card; Warrior Living Armour is a distinct proposed ability upgrade.
- General Living Arsenal remains its existing attack-speed legendary; Arsenal Awakening is a distinct proposed Warrior effect.
- Extra attacks and casts need explicit strength, cooldown and trigger rules. They should not recursively generate themselves or reset ordinary cooldowns for free.
- Keep baseline target selection, pool reuse and activation correctness independent of upgrades.
- Establish how exclusive cards share offer slots with existing general and spirit cards before adding all 28 to the runtime catalog.
- Apply limits to shields, damage reduction, control, duration and total simultaneous effects.

## Next design decisions

- [ ] Review or replace the 28 proposed names and effects.
- [ ] Finalize core Warrior dash/momentum/armour rules and Shepherd party/formation/lead rules.
- [ ] Choose numerical values and whether any cards have multiple levels.
- [ ] Decide whether the two legendary paths per character are mutually exclusive.
- [ ] Map every card to its prerequisites and eligible attacks/abilities.
- [ ] Prototype a short run before creating the complete asset set.

## Related

- [[Notes/Game Design/Playable Characters|Playable characters]]
- [[Notes/Game Design/Spirit Warrior|Warrior weapon pairs and movement ideas]]
- [[Notes/Development/Upgrade Catalog Organization|Implemented upgrade groups and character restrictions]]
