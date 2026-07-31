---
type: design
status: draft
tags:
  - game-design
  - spirits
---

# Spirits

| Spirit | Form | Weapon | Ability status |
|---|---|---|---|
| Fire | Phoenix | Fire bow | Defined |
| Earth | Golem | Stone hammer | Defined; RockFall needs clarification |
| Water | Leviathan | Water trident | Defined |
| Wind | Roc | Chakrams | Partially defined |
| Ice | Yeti | Ice gauntlets | Defined; Ice Crystal upgrades need correction |
| Lightning | Thunder Dragon | Lightning spear | Defined |
| Poison | Scorpion | Poison daggers | Defined |
| Necrotic | Bat | Necrotic katana | Not designed |
| Holy | Biblical Angel | Holy sword | Concepts only |

## Ability index

### Fire

- **Fiery Feathers:** homing fan; later explosions, burning, and fire patches.
- **Fiery Talons:** fire trail with increased size, duration, spread, and explosive finish.
- **Phoenix Dive:** diving attack with multiple dives, fire zones, and a max-level revive.

### Earth

- **Quicksand Domain:** slow field that grows, damages, pulls, and briefly immobilizes elites.
- **Boulder Throw:** bouncing projectile that splits, stuns, and explodes.
- **Stone Spikes:** erupting pillars with more spikes, bleed, and chain eruptions.
- **RockFall:** present in the source design but duplicates Stone Spikes; define its distinct role.

### Water

- **Tidal Wave:** expands from forward-only to four directions.
- **Whirlpool:** pulling damage zone with increased radius and count.
- **Rain Clouds:** enemy-following clouds with increased count, damage, and speed.

### Wind

- **Razor Wind:** radial piercing blades.
- **Tornado:** moving pull effect with increased size, strength, and count.
- Third ability is not defined.

### Ice

- **Frozen Orbs:** orbiting projectiles with increased count, speed, and freeze chance.
- **Avalanche:** growing snowball with increased damage and freeze.
- **Ice Crystal:** delayed growing crystal that explodes; its source upgrades currently duplicate Avalanche and need redesign.

### Lightning

- **Lightning Strike:** strikes random enemies; gains more strikes, damage, and area damage.
- **Chain Lightning Bolt:** jumps between enemies with increasing jump count, damage, and range.
- **Thunder Roar:** expanding electrical pulse that knocks back and stuns.

### Poison

- **Toxic Glob:** exploding blobs that leave longer-lasting pools.
- **Venom Needles:** rapid piercing projectiles.
- **Acid Spray:** frontal cone that grows, damages, breaks armor, and leaves pools.

### Necrotic and Holy

- Necrotic abilities are not designed.
- Holy concepts are healing, shields, and light beams.

## Implementation reference

See `World Of Spirits/Assets/Docs/SPIRIT_ABILITY_SETUP.md` for current Unity components and prefab guidance.
