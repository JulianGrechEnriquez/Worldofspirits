---
type: design
status: active
updated: 2026-08-17
tags: [game-design, combat, balance]
---

# Combat rules and elements

## Movement states

### Moving — support state

- The main spirit remains in companion form and uses support abilities.
- Both support spirits continue casting.
- Starting to move immediately ends stationary weapon charge.
- Existing projectiles and persistent effects continue normally.

### Stationary — weapon state

- After 0.25 seconds without movement, the main spirit transforms into its weapon.
- The weapon attacks automatically after transformation.
- After one stationary second, **Focused** grants +15% weapon damage and +10% attack speed.
- After two stationary seconds, **Empowered** grants +30% weapon damage and +20% attack speed.
- Empowered is the maximum charge stage.

The player, spirit, weapon, and HUD must communicate transforming, Focused, Empowered, and rotation-cooldown states.

## Spirit rotation

- Rotation cycles the next owned spirit into the main slot.
- Cooldown: one second.
- The new main spirit grants its elemental buff for three seconds.
- Duration can refresh, but strength cannot stack with itself.

See [[Spirits#Rotation buffs|Spirits]] for the buff table.

## Status effects

| Status | Rule |
|---|---|
| Burn | Fire damage over time; applications refresh and can stack to a tuned limit. |
| Soaked | Slight slow; increases damage from the next Lightning hit. |
| Freeze | Builds frost; normal enemies freeze, while elites and bosses receive a shorter slow. |
| Shock | Lightning damage that can arc to nearby enemies. |
| Poison | Stacking damage over time that rewards repeated application. |
| Bleed | Physical damage over time from spikes, blades, and crushing attacks. |

Pull, push, pin, freeze, and stun have reduced strength and duration against elites and bosses.

## Boss elemental rules

- Bosses take **25% additional damage** from their weakness.
- Bosses take **25% less damage** from their own element.
- Hard control becomes a short slow, movement reduction, or interrupt instead of a full disable.

| Boss | Weak to | Resists |
|---|---|---|
| Fire Phoenix | Water | Fire |
| Ice Yeti | Fire | Ice |
| Storm Dragon | Earth | Lightning |
| Giant Scorpion | Wind | Poison |
| Necrotic Bat King | Holy | Necrotic |
| Fallen Angel | Necrotic | Holy |
| Earth Golem | Water | Earth |
| Water Leviathan | Lightning | Water |
| Wind Roc | Ice | Wind |

These percentages are starting values and must be tuned through playtesting.

