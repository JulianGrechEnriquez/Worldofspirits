---
type: implementation-status
status: active
reviewed: 2026-10-04
tags: [development, spirits, abilities]
---

# Spirit ability implementation status

This tracks the five spirits involved in the current gameplay work. It records implementation progress, not production acceptance. Intended five-level designs remain in [[Notes/Game Design/Spirits|Spirits]].

| Spirit | Latest connected work | Remaining known behavior |
|---|---|---|
| Fire | Fiery Talons uses pooled flame particles; movement-dependent effects stop when activation fails. | Feather fire patches; advanced Talons lifetime/damage, spreading and explosions; Phoenix fire zones and revive limitation. |
| Earth | Quicksand renders under characters and slows enemies by 40%; Boulder art/collision damage fix; Stone Spikes art, targeting, burst damage and higher-level Bleed. | Boulder fragments/stun and final-impact explosion behavior; stronger Quicksand slow/elite immobilization; sequential Stone Spikes chains. |
| Water | Rain Clouds uses cloud artwork, rain particles, enemy following and damage ticks; Whirlpool uses rotating inward rings, pull, swirl and higher-level damage. Both have connected upgrade prefabs. | Validate every five-level behavior against the design, including status interactions, target count and main/support roles. |
| Wind | Gale Barrier is connected in the runtime ability list. | Tornado movement and Gale Barrier twin-blast behavior. |
| Ice | Avalanche grows and carries eligible enemies, releasing them on expiry/pool return. | Orbital Snowball freeze chance; full Ice kit remains outside the initial demo completion target. |

## Next ability checks

- [ ] Implement the known missing behaviors above before marking a spirit complete.
- [ ] Validate damage, status application and target counts at all five levels.
- [ ] Check movement activation as main, support one and support two, including mastery exceptions.
- [ ] Check particles, colliders, retained targets and slows after pool reuse.
- [ ] Confirm boss control resistance/conversion for pull, stun and immobilization.
- [ ] Compare tooltip descriptions with actual behavior.
- [ ] Profile crowded runs and extreme upgrade combinations.

Quicksand currently applies damage/pull from level one; its design progression still needs reconciliation. Level-five Stone Spikes currently appear together rather than as a sequential chain. Boulder explosions need final-impact behavior review.

## Evidence

[[Notes/Development/2026-10-04 Gameplay and UI Update|The full update]] records the changes and verification limits. The latest available Windows build succeeded with zero errors; both enabled scenes have zero missing scripts after the final scene fix. These checks do not replace a playthrough of every upgrade combination.

Source update: [GitHub commit 27a5491](https://github.com/JulianGrechEnriquez/Worldofspirits/commit/27a54919ad2b179479b0b1f57b9c73f4ebf58680).
