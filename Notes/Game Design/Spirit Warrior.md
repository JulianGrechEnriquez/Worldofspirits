---
type: design-concept
status: discussion
created: 2026-10-05
updated: 2026-10-05
tags: [game-design, characters, warrior, movement, upgrades]
---

# Spirit Warrior

## Established direction

- All owned spirits stay in weapon form and cannot become companions.
- The class should encourage frequent movement.
- Its upgrades should support its weapons and character abilities.

The user proposed two weapon functions for the main-slot spirit and spirit armour that grants abilities. These are the current design direction to explore; exact mechanics are not finalized. Nothing in this note is implemented yet.

## Main-slot weapon pairs

The main spirit could provide two complementary weapons or weapon functions. The following nine pairings were selected as the current concept direction on 5 October 2026. Their interaction examples remain proposals; attack rules, unlocks and balance are not finalized. The list includes all spirits in the design notes, including the future Necrotic concept, rather than only the currently implemented roster.

| Spirit | Existing weapon | Paired weapon | Proposed interaction while moving |
|---|---|---|---|
| Fire — Phoenix | Flame Bow | Talon Blades | Bow attacks at range; blades slash nearby enemies during a dash. |
| Earth — Golem | Stone Hammer | Golem Shield | Shield charge pushes enemies together; hammer follows with a crushing swing. |
| Water — Leviathan | Water Trident | Tide Whip | Whip pulls or sweeps enemies into the trident's path; returning attacks maintain pressure while retreating. |
| Wind — Roc | Chakrams | Gale Fans | Chakrams strike outward and return; fans push enemies or deflect projectiles to open an escape route. |
| Ice — Yeti | Ice Gauntlets | Shard Launcher | Shards slow approaching enemies before the player dashes in with close-range strikes. |
| Lightning — Thunder Dragon | Lightning Spear | Storm Javelins | Spear pierces nearby enemy lines; javelins mark distant targets for a follow-up lightning strike. |
| Poison — Scorpion | Poison Daggers | Venom Needles | Needles poison distant enemies; daggers reward approaching, striking and escaping. |
| Necrotic — Bat | Necrotic Katana | Soul Chains | Chains catch or curse a target; katana finishes weakened enemies. |
| Holy — Angel | Holy Sword | Halo Shield | Sword attacks nearby enemies; shield protects a direction and releases a counterattack. |

These may be two linked functions of one spirit weapon rather than separate unrelated weapons. Other owned spirits remain weapons; their attack frequency, position and number of simultaneous attacks still need decisions.

A possible structure is one normal weapon per spirit, with the main slot activating that spirit's paired weapon and armour power. Another proposal is unlocking the second weapon through a Warrior upgrade. Neither activation/unlock rule is decided yet. Rotation would change the player's fighting style while preserving the weapon-only identity.

Earth and Holy should have different defensive identities: the Golem Shield pushes through danger, while the Halo Shield blocks and counters. Water's earlier returning-spear pairing remains an alternative, but Tide Whip is the current recorded pairing.

## Spirit armour upgrades

Armour could be spirit-created equipment that grants active or automatic player abilities, rather than only defence statistics. It does not turn a spirit into a companion.

Possible branches include Earth **Juggernaut**, focused on charging through enemies, and Earth **Guardian**, focused on blocking and counterattacking. Decide whether armour comes automatically from the main slot, is acquired as an upgrade, or has separate upgrade branches. Also decide what happens to an armour effect when its spirit leaves the main slot.

Earlier armour examples remain proposals: Fire movement builds heat for a flaming dash; Earth movement builds armour for a shield bash; passing close to enemies charges a Water wave; Wind builds speed and releases blades on sharp turns; Ice leaves a slowing frost trail during a dash.

## Movement mechanics discussed

- **Spirit-specific dashes:** Fire rush, Earth shield charge, Water slide/wave and Wind cutting dash. Dash itself is a proposed new mechanic, not an assumed existing feature.
- **Momentum:** movement builds a shared meter that improves attacks; stopping gradually drains it. Tune the rule so it rewards combat movement rather than aimless running.
- **Momentum finishers:** spend the meter for a powerful attack, choosing between sustained bonuses and burst damage.
- **Near-miss rewards:** dodging close to an enemy attack charges armour or empowers the next strike. Define a reliable timing window and prevent repeated rewards from the same attack.
- **Rotation attacks and combos:** switching the main spirit triggers a technique and may preserve momentum. Earth into Wind could create a shockwave; Water into Fire could create a steam burst.
- **Directional techniques:** moving toward enemies favours melee, while moving away favours ranged attacks. Prefer automatic triggers with readable feedback.
- **Escape protection:** secondary weapons attack behind the moving player or briefly intercept projectiles.
- **Movement formations:** weapons spread around the player while moving and gather for focused attacks when stationary, without changing out of weapon form. Check that any stationary bonus does not undermine the class's mobility goal.

## Other Warrior ideas retained

- Weapon formations: defensive circle, forward assault or wide orbit.
- Lead weapon: the selected spirit attacks more frequently or gains a special technique.
- Elemental combinations: Water gathers enemies for Earth; Wind spreads Fire effects.
- Warrior techniques replacing unusable companion upgrades: hammer shockwaves, bow volleys, trident sweeps and chakram ricochets.
- Battle frenzy: a shared meter triggers coordinated empowered weapon attacks.
- Guard and counterattack: recall weapons briefly, then release them outward.
- Weapon mastery: use during a run unlocks a distinctive weapon technique.
- Concentrated assault: temporarily focus weapon attacks on an elite or boss.
- Signature arsenal upgrades: Earth knockback, Wind weapon movement and Water return attacks.

These are alternatives to compare, not a requirement to implement every mechanic. Momentum and battle frenzy may serve the same purpose; choose one before adding multiple meters.

## Candidate first prototype

Prototype Fire and Earth in the same short arena. Compare their main-slot weapon pairs, one spirit-specific dash each, and one armour branch. Keep controls small and add combos only after the basic movement combat feels good. This is a suggested prototype, not a scheduled implementation task.

Example: an Earth Warrior shield-charges through enemies and swings the hammer, then rotates to Wind and throws chakrams while escaping.

## Open decisions

- [ ] How many weapons attack simultaneously, and how do non-main weapons behave?
- [ ] Are the main-slot functions simultaneous, alternating automatically or selected by input?
- [x] Record the initial weapon pairings for all nine design spirits.
- [ ] Define and playtest the interaction of each pair, including defensive and control weapons.
- [ ] How is armour acquired, upgraded and changed during rotation?
- [ ] Does the class gain a dash, and what limits its use?
- [ ] Choose momentum, frenzy or another shared resource; define gain, decay and spending.
- [ ] Keep, replace or remove the Tamer's stationary charge bonuses for this class?
- [ ] Decide whether combinations depend on the owned party or the order of rotation.
- [ ] Balance damage, defence, targeting and visual clarity across the arsenal.

## Implementation considerations for later

Use shared spirit data with character-specific form rules, weapon behavior and upgrade eligibility. The Warrior needs weapon attacks while moving and character-specific activation/cleanup rules. Filter out companion-only cards unless they have an explicit Warrior conversion. Validate armour, dash and weapon state across rotation and pool reuse, and profile simultaneous weapons before increasing their count.

## Related

- [[Notes/Game Design/Character Exclusive Upgrades|Warrior and Shepherd exclusive upgrade draft]]

- [[Notes/Game Design/Playable Characters|Tamer, Warrior and Shepherd concepts]]
- [[Notes/Game Design/Spirits|Existing spirit identities and intended upgrades]]
- [[Notes/Game Design/Combat Rules and Elements|Current Tamer combat rules]]
