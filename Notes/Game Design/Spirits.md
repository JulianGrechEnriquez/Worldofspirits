---
type: design
status: active
updated: 2026-08-17
tags: [game-design, spirits]
---

# Spirits

## Shared structure

Every production-ready spirit requires one five-level weapon, exactly three five-level support abilities, a distinct combat identity, all three party roles, a rotation buff, and complete presentation.

Fire, Earth, Water, and Wind are the first standardized kits. Ice, Lightning, Poison, Necrotic, and Holy remain later production work.

## Rotation buffs

Rotation has a one-second cooldown. Selecting a new main spirit grants a three-second buff. A repeated buff refreshes but does not stack.

| Spirit | Buff | Effect |
|---|---|---|
| Fire | Blazing Resolve | +20% damage dealt |
| Earth | Stoneguard | 25% damage reduction |
| Wind | Tailwind | +20% movement speed |
| Water | Flow | Cooldowns recover 25% faster |

## Standardized spirit kits

### Fire — Phoenix

**Identity:** aggressive damage, Burn, explosions, and offensive momentum.  
**Weapon:** Flame Bow — homing burning feathers; later levels add multishot, piercing, death explosions, and fire patches.

- **Fiery Feathers:** homing fan → improved tracking → piercing Burn → explosions → Ashen Flock.
- **Fiery Talons:** fire trail → wider/longer trail → bonus against burning enemies → spreading flames → Phoenix Footsteps.
- **Phoenix Dive:** line attack → greater width → burning path → second dive → Rebirth Dive and one revive per run.

### Earth — Golem

**Identity:** durability, stun, Bleed, and space control.  
**Weapon:** Stone Hammer — orbiting sweep; later levels add reach, knockback, shockwaves, and a second hammer.

- **Quicksand Domain:** slow → larger/stronger field → damage → inward pull → Sinking Kingdom.
- **Boulder Throw:** bouncing boulder → more bounces → fragments → stun → Continental Breaker.
- **Stone Spikes:** targeted eruptions → more spikes and Bleed → chained fault lines → two waves and cracked ground → Worldspine.

> [!note]
> Rockfall is not part of the Earth Spirit's three-ability kit. It remains an Earth Golem or challenge-boss attack.

### Water — Leviathan

**Identity:** push, pull, grouping, Soaked, and ability flow.  
**Weapon:** Water Trident — piercing outward-and-return throw; later levels add Soaked, multishot, and waves.

- **Tidal Wave:** front wave → front/back → wider and Soaked → four directions → High Tide.
- **Whirlpool:** one pull zone → larger/stronger → two zones → damage and Soaked → Maelstrom.
- **Rain Clouds:** following cloud → two clouds → stronger rain and Soaked → downpours → Endless Monsoon.

### Wind — Roc

**Identity:** mobility, displacement, projectile control, and shielding.  
**Weapon:** Chakrams — damage outward and on return; later levels add piercing, multishot, and catch bursts.

- **Razor Wind:** two blades → four blades → more speed/range → piercing → Thousand Cuts.
- **Tornado:** moving pull zone → larger/longer → stronger pull → two tornadoes → Eye of the Storm.
- **Gale Barrier:** shield and push → larger shield → projectile destruction → second pulse → Sanctuary of Wind.

## Later spirit concepts

| Spirit | Form | Weapon | Intended identity | Status |
|---|---|---|---|---|
| Ice | Yeti | Ice gauntlets | Freeze, defensive traps, enemy gathering | Partial redesign complete |
| Lightning | Thunder Dragon | Lightning spear | Rapid hits, chaining, Shock | Concept defined |
| Poison | Scorpion | Poison daggers | Stacking damage and weakening | Concept defined |
| Necrotic | Bat | Necrotic katana | Execution, curses, life steal | Abilities not designed |
| Holy | Biblical Angel | Holy sword | Healing, shielding, anti-corruption | Concepts only |

## Ice ability distinction

- **Avalanche:** growing directional snowball that carries normal enemies, leaves a frozen wake, and finishes with radial ice shards.
- **Ice Crystal:** defensive crystals around the player that damage nearby enemies, shatter on proximity or expiry, and apply Freeze.

Avalanche is moving crowd control; Ice Crystal protects immediate space.

## Implementation reference

See the spirit scripts under `World Of Spirits/Assets/_WorldOfSpirits/Scripts/Spirits` for implementation details.

