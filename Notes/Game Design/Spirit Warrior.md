---
type: design-concept
status: playable-prototype
created: 2026-10-05
updated: 2026-10-18
tags: [game-design, characters, warrior, movement, upgrades]
---

# Spirit Warrior

## Established direction

- All owned spirits stay in weapon form and cannot become companions.
- The class should encourage frequent movement.
- Its upgrades should support its weapons and character abilities.

The weapon-only character, main-slot paired weapons, spirit armour, elemental dash and momentum now have a playable first implementation for the five existing spirit prefabs. Values are initial tuning choices; the future-spirit kits and additional upgrade branches below remain design proposals.

## Basic character implementation — 18 October 2026

- Select Spirit Warrior from the character selection screen. It has a separate prefab using the current player artwork.
- Choose one starter spirit, with three base spirit slots.
- All owned spirits stay in weapon form and attack while moving or stationary, including support slots.
- Companion visuals and companion ability casting are disabled. Companion ability upgrade cards are excluded.
- Rotation keeps the weapon-only form and skips companion transformation animations.
- The main spirit grants its paired weapon and armour automatically, without drawing an upgrade. Non-main spirits retain their normal weapons and their own weapon levels.
- Space or Left Shift dashes in the movement direction, or the last facing direction when stationary. Dash lasts 0.2 seconds at 3× movement speed and has a 3-second cooldown, scaled by cooldown reduction. Terrain collision and incoming damage remain active.
- Moving within 8 units of an enemy builds momentum over 4 seconds. It grants up to +20% weapon damage; stopping or leaving combat drains it at 35% per second.
- Tab rotates the main spirit. The incoming spirit supplies the pair and armour; paired-weapon cooldowns remain attached to each spirit so rotation cannot reset them. An ongoing dash retains its starting element, and its temporary Water shield can last its remaining 2 seconds after rotation.
- Pausing freezes dash movement, cooldowns and attacks. Stun/freeze prevents dash movement and damage. Disabling the character clears dash, momentum, pair cooldowns and technique visuals.
- HUD shows the main pair, armour, dash readiness and momentum. Starter cards explain the Warrior kit instead of companion abilities.
- The five armament assets under `Data/Warrior` expose names, descriptions, colours, paired damage, cooldown, range and armour reduction for tuning. The Warrior prefab holds dash settings and material references.

### Implemented pair and armour effects

| Spirit | Main-slot paired weapon | Armour and dash |
|---|---|---|
| Fire | Talon Blades: two physical claws alternate extend/hit/return lunges, like Ice Gauntlets. Level one: 12 damage, 0.5-second attack cooldown, 2.8-unit thrust, 3.2-unit targeting range; burns for 1.5 seconds. | Ember Armour: 10% damage reduction. Dash hits and burns nearby enemies. |
| Earth | Golem Shield: a physical shield automatically lunges and bashes nearby enemies; 18 damage every 0.9 seconds, 2.8-unit thrust, 3.2-unit targeting range. Hits briefly stun and apply 4.5 base knockback through crowd steering. No incoming hit is required. | Juggernaut Armour: 15% baseline reduction; frontal reduction rises to 60% during dash. Dash stuns struck enemies for 0.4 seconds. |
| Water | Tide Whip: 14-damage sweep every 1.2 seconds, 3.5-unit range; pulls and slows enemies. | Tide Armour: 10% reduction. Dash grants a 12-point shield for 2 seconds and pushes/slows enemies. |
| Wind | Gale Fans: 14-damage sweep every 1.2 seconds; pushes and slows enemies. | Gale Armour: 10% reduction and 12% movement speed. Dash pushes/slows nearby enemies. Projectile deflection remains a future option. |
| Ice | Shard Launcher: three travelling shards every 1.8 seconds, 6 damage each, 9-unit range; slows for 1 second. | Frost Armour: 10% reduction. Dash slows nearby enemies by 35% for 2 seconds. |

Each paired weapon scales damage by +20% per purchased weapon level after level one. General attack damage, attack speed, area, duration and elemental upgrades apply where relevant. Ice supports extra projectiles (capped at five per volley), projectile speed and size; its eight reusable shot slots cap simultaneous shards. A dash deals 12 base damage scaled by momentum before normal weapon damage modifiers and hits each enemy once. Wind paired melee range starts at 2.7 units. Fire and Earth use separate five-level WeaponDefinition assets with physical hitboxes and the same ThrustMeleeWeaponBase attack loop as Ice Gauntlets; the main spirit weapon level controls their level. Attack speed and melee echo damage upgrades apply. Each physical swing deduplicates its own targets. Their cooldowns persist across main-slot changes. Water/Wind forces use the existing kinematic crowd steering system.

Shared companion-only modifier cards are also excluded: 36 existing cards were rejected, including ability cards and cards that only improve companion damage. The proposed 14 Warrior-exclusive upgrades remain a separate design draft and are not implemented in this pass.

Validation in Unity play mode: all five paired kits dealt damage or spawned travelling shards; shield frontal reduction, Water shielding, pair cooldowns, dash cooldowns and hit deduplication passed. All five normal weapons stayed active while moving/stationary with companion renderers hidden and 20 distinct melee anchors. Tamer/Shepherd form-state regression checks passed. A live Ice shard hit a stationary target four units away. Sixty passive-ability movement/slot checks passed and no companion orbital snowballs spawned. A live dash moved three units; pausing froze movement and cooldown, and live rotation switched the main pair/armour. Final live motion/rotation results are recorded in `tmp/warrior-motion-validation.txt`.

Presentation uses five generated transparent paired-weapon sprites under `Art/Sprites/Warrior/PairedWeapons`: Fire Talon, Earth Shield, Tide Whip, Gale Fan and Shard Launcher. Art and full generation prompts are saved together in that folder; the built-in image_gen tool was used. The current player sprite, magical armour outlines and attack effects remain. Dedicated Warrior character artwork, balance playtesting, the four future spirits and additional armour/upgrade branches are still outstanding.

Physical weapon validation: live Fire attacks alternated both claws through Idle/Extending/Returning and dealt damage; Earth automatically bashed, dealt damage, and produced crowd knockback. Main-slot change hid/disabled the outgoing shield. A paused physical weapon did not advance its phase. See `tmp/warrior-physical-weapons-validation.txt`.

## Main-slot weapon pairs

The main spirit could provide two complementary weapons or weapon functions. The following nine pairings were selected as the current concept direction on 5 October 2026. The first five pairs have the playable behaviour listed above; the remaining four and additional interaction examples remain proposals. Balance is not finalized. The list includes all spirits in the design notes, including the future Necrotic concept, rather than only the currently implemented roster.

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

The current implementation gives every owned spirit its normal weapon and the main spirit one additional technique. All normal weapons can attack simultaneously and use separate resting anchors.

The current baseline grants the main-slot pair and armour automatically. Unlocking extra functions through upgrades remains a possible later extension.

Earth and Holy should have different defensive identities: the Golem Shield pushes through danger, while the Halo Shield blocks and counters. Water's earlier returning-spear pairing remains an alternative, but Tide Whip is the current recorded pairing.

## Additional spirit armour upgrade proposals

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

- [x] All owned normal weapons can attack simultaneously; only the main slot activates a pair and armour.
- [x] The base weapon and automatic paired technique work simultaneously; Earth performs automatic physical shield bashes while its armour reduces frontal damage.
- [x] Record the initial weapon pairings for all nine design spirits.
- [ ] Define and playtest the interaction of each pair, including defensive and control weapons.
- [x] Baseline armour follows the main slot automatically. Upgrade branches remain future work.
- [x] Add a 0.2-second dash with a 3-second base cooldown and one hit per target.
- [x] Add combat movement momentum with gain/decay and a passive weapon damage bonus. Spending/finishers remain future work.
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
