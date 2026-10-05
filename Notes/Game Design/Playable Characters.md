---
type: design-concept
status: discussion
created: 2026-10-04
updated: 2026-10-05
tags: [game-design, characters, upgrades]
---

# Playable characters

## Confirmed concept

Add playable characters with different relationships to their contracted spirits and different upgrade options.

- **Spirit Warrior:** all owned spirits remain in weapon form and cannot return to companion form. The desired playstyle involves frequent movement, with a different upgrade pool.
- **Spirit Shepherd:** spirits remain in companion form, cannot transform into weapons, and the character can own more spirits than the current three-spirit party. Companion-only behavior was confirmed on 4 October 2026.
- **Spirit Tamer:** the existing movement-based character is the comparison point: companion abilities while moving, main-spirit weapon while stationary.
- **Spirit Beastmaster:** spirits fight as independently moving creatures with their own health bars. At zero health they disappear, recover and return automatically. The name and recovery direction were chosen on 5 October 2026; see [[Notes/Game Design/Spirit Beastmaster|Spirit Beastmaster]].

The planned demo roster is **Spirit Tamer, Spirit Warrior and Spirit Shepherd**, recorded on 5 October 2026. **Spirit Beastmaster is planned for later development.** See [[Notes/Planning/Demo Scope and Completion Plan|demo scope]]. The new characters are not implemented; exact limits, balance values and unlock rules remain open.

## Proposed character identities

| Character | Combat focus | Form rules | Upgrade direction |
|---|---|---|---|
| Spirit Tamer | Movement, stance changes and spirit rotation | Existing companion/weapon switching | Existing weapon and ability upgrades |
| Spirit Warrior | Spirit weapons and weapon techniques | Weapon forms only | Weapon patterns, reach, attack speed, impacts and elemental techniques |
| Spirit Shepherd | A larger spirit party and ability combinations | Companion forms only | Ability behavior, spirit capacity, formation and coordination |
| Spirit Beastmaster | Independent damageable spirit creatures | Creature combat; weapon/form rules undecided | Creature attacks, durability, healing and recovery; specific cards undecided |

## Spirit Warrior — proposals

Let the equipped spirit weapon attack while the player moves. Otherwise this character would spend too much time without an attack because its spirits cannot enter companion form.

The latest direction explores **two weapon functions for the main-slot spirit**, such as melee plus ranged or melee plus defence, together with spirit armour upgrades that grant player abilities. Exact simultaneous attack rules remain open. See [[Notes/Game Design/Spirit Warrior|Spirit Warrior]] for the 5 October discussion.

Initial pairings for all nine design spirits are recorded in [[Notes/Game Design/Spirit Warrior#Main-slot weapon pairs|the Warrior weapon table]]. These are selected concepts, not implemented weapons; detailed interactions and unlock rules remain open.

Companion-only abilities should not appear as unusable upgrades. Spirit identity could instead appear through character-specific weapon techniques:

- Fire: burning impacts, piercing volleys or a charged flame shot.
- Earth: crushing impacts, shockwaves or a defensive stance.
- Water: returning thrusts, water arcs or a pull on impact.
- Wind: ricocheting blades, piercing throws or a burst after catching a weapon.

These are examples, not approved replacement kits. Decide whether stationary Focused/Empowered charging fits the movement-focused Warrior, and whether rotation buffs retain their current values.

## Spirit Shepherd — proposals

Only the Shepherd can unlock [[Notes/Game Design/Spirit Shepherd#Combined ability moves|combined ability moves]], such as Whirlpool + Tornado. Spirits do not merge or change identity. Maximum-level ability investment is the intended unlock direction; exact prerequisites are still open.

All contracted spirits use companion abilities. Stopping movement should not deactivate the entire party; its activation rules must allow companion casting while standing still.

Prototype **four total spirits**, then compare five if the party needs more variety. Neither limit is approved yet. Start with the same number of spirits as the other characters and acquire extra companions through contracts during the run, rather than granting a full party immediately.

Possible upgrade examples:

- An additional contract slot, with a defined maximum.
- Ability-specific cooldown, radius or duration upgrades.
- Formation choices that change where spirits cast from.
- Coordinated casts or elemental interactions between companions.

More spirits already add damage, control and screen effects. Measure those gains before adding broad damage bonuses or choosing a damage penalty. Define formation and target distribution so companions remain readable and avoid wasting all their attacks on one enemy.

Rotation could become selecting a lead companion for formation or a buff. Whether this uses current rotation buffs, a new command, or no rotation action remains open.

## Upgrade rules to design

[[Notes/Game Design/Character Exclusive Upgrades|The first Warrior and Shepherd upgrade draft]] proposes 14 cards for each class, with prerequisites and offensive/defensive legendary paths. These designs are not implemented yet.

The agreed pool size is **5 Common, 4 Uncommon, 3 Epic and 2 Legendary character-exclusive upgrades per character** (14 total), alongside the existing shared and spirit upgrades. Exact card designs remain open. Existing Rare upgrades are kept; the exclusive pool has no Rare tier. Spirit Master is now Tamer-only in the actual catalog. See [[Notes/Development/Upgrade Catalog Organization|catalog organization]] for the implemented classification and eligibility rules.

- Shared upgrades may include health, movement speed, experience and collection range, where those systems already exist.
- Warrior upgrades should modify usable weapons or approved weapon techniques.
- Shepherd upgrades should modify companion abilities, contracts or approved commands.
- Filter upgrades by character and usable spirit/form. Avoid offering a weapon-only card to the Shepherd or a companion-only card to the Warrior without an explicit conversion.
- Write separate descriptions when an elemental upgrade behaves differently for each character.
- Decide how character selection interacts with Story Mode's required starting spirit. A possible approach is choosing the character first, while the stage still determines the starting spirit.

## Decisions still needed

- [ ] Warrior: define how all weapon-form spirits attack and how the main slot's second weapon function works.
- [ ] Warrior: choose armour powers, dash rules and movement rewards.
- [ ] Warrior: retain stationary charge bonuses?
- [ ] Warrior: weapon-only upgrades, or a full set of weapon techniques replacing companion abilities?
- [ ] Shepherd: four or five maximum spirits, and how are extra slots acquired?
- [ ] Shepherd: lead-spirit command, rotation buffs, or another control?
- [ ] Character selection and unlock requirements.
- [ ] Initial spirit, party size and progression rules per character.
- [x] Choose Tamer, Warrior and Shepherd for the demo; defer Beastmaster.
- [ ] Order Warrior and Shepherd prototypes within the Burning Plains demo work.

## Future implementation checklist

- [ ] Define character data for capacity, allowed forms and upgrade eligibility.
- [ ] Replace assumptions that every character has exactly three slots or movement-based transformation.
- [ ] Apply activation rules per character, including standing still and pooled effect cleanup.
- [ ] Adapt selection UI, party HUD, formation and upgrade descriptions.
- [ ] Validate pooling, targeting, boss control and performance with the larger party.
- [ ] Playtest each character through the same short Burning Plains run.

## Related

- [[Notes/Game Design/Game Overview|Game overview]]
- [[Notes/Game Design/Spirits|Spirit kits and intended upgrades]]
- [[Notes/Game Design/Combat Rules and Elements|Current Tamer combat rules]]
- [[Notes/Development/Spirit Ability Implementation Status|Current ability implementation]]
- [[Notes/Planning/Demo Scope and Completion Plan|Current demo scope]]
