# Spirit ability component setup

Add each component below as a child of the matching spirit prefab. Set its `Ability Index` to 0, 1, or 2 in document order. Assign the required projectile/effect prefab and tune the `Level Scaling` fields in the Inspector.

| Spirit ability | Component | Recommended configuration |
|---|---|---|
| Fiery Feathers | `ProjectilePatternAbility` | Aimed Fan; use a homing `ConfigurableProjectile`, then enable explosion/burn on higher-level prefab variants. |
| Fiery Talons | `SpawnEffectAbility` | At Player; spawn a `PersistentDamageZone` fire-trail prefab. |
| Phoenix Dive | `SpawnEffectAbility` | At Closest Enemy; spawn the dive/fire-zone prefab. |
| Quicksand Domain | `AreaPulseAbility` | Pull Inward; apply Slow; increase radius and status strength by level. |
| Boulder Throw | `ProjectilePatternAbility` | Aimed Fan; use a growing/exploding rock projectile. |
| Stone Spikes | `SpawnEffectAbility` | Random Around Player; spawn spike prefabs with bleed damage. |
| Tidal Wave | `ProjectilePatternAbility` | Change pattern by upgraded prefab: Aimed Fan, Forward And Backward, then Four Directions. |
| Whirlpool | `SpawnEffectAbility` | Random Around Player; spawn a pulling `PersistentDamageZone`. |
| Rain Clouds | `SpawnEffectAbility` | At Closest Enemy; increase spawn count by level. |
| Razor Wind | `ProjectilePatternAbility` | Radial; use a piercing `ConfigurableProjectile`. |
| Tornado | `SpawnEffectAbility` | Random Around Player; spawn a moving/pulling effect prefab. |
| Frozen Orbs | `OrbitingProjectileAbility` | Increase orb count and rotation speed by level. |
| Avalanche | `ProjectilePatternAbility` | Aimed Fan; use a growing projectile that applies Freeze. |
| Ice Crystal | `SpawnEffectAbility` | At Closest Enemy; spawn a delayed exploding crystal prefab. |
| Lightning Strike | `SpawnEffectAbility` | At Closest Enemy; increase spawn count and use an area-damage strike prefab. |
| Chain Lightning Bolt | `ChainLightningAbility` | Increase jumps, range, and damage by level. |
| Thunder Roar | `AreaPulseAbility` | Knockback; apply Stun; increase radius by level. |
| Toxic Glob | `ProjectilePatternAbility` | Aimed Fan; use an exploding poison projectile that leaves a zone. |
| Venom Needles | `ProjectilePatternAbility` | Aimed Fan; use a piercing poison projectile. |
| Acid Spray | `ProjectilePatternAbility` | A wide Aimed Fan; use piercing projectiles with Armor Break. |
| Healing, Shields, Light Beams | Not finalized | The design document still marks Holy Spirit mechanics as incomplete. |
| Necrotic abilities | Not finalized | The design document does not define these abilities yet. |

Weapon prefabs use `AutoProjectileWeapon` for bows, tridents, chakrams, gauntlets, spears, daggers, katana, and sword attacks. The Stone Hammer can continue using `OrbitingMeleeWeapon`. Both read `SpiritProgression.WeaponLevel` automatically.
