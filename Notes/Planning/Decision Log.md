---
type: decision-log
status: active
tags:
  - planning
  - decisions
---

# Decision log

Record durable design and technical choices here. Link to a detailed note when the reasoning is substantial.

| Date | Decision | Reason | Status |
|---|---|---|---|
| 2026-07-25 | Use the repository root as the Obsidian vault. | Keeps design notes, Unity docs, and version history together. | Accepted |
| 2026-07-25 | Keep implementation files in Unity and project intent in `Notes`. | Avoids duplicating code documentation while preserving design context. | Accepted |
| 2026-08-06 | Scope the demo to Fire, Wind, Water, and Earth spirits. | Four complete elemental kits provide meaningful build variety without requiring the full roster. | Accepted |
| 2026-08-06 | Scope the demo to Burning Plains and an Earth-themed second area. | Earlier two-area demo plan. | Superseded 2026-08-17 |
| 2026-08-06 | Use Earth Golem as the second demo boss. | Earlier two-area demo plan. | Superseded 2026-08-17 |
| 2026-08-17 | Use six sequential planes for Story Mode, starting with Burning Plains and Frozen Wastes. | Matches each story stage with its elemental spirit and guardian. | Supersedes the two-area Earth demo sequence |
| 2026-08-17 | Unlock Infinity Mode after all six Story Mode stages. | Makes the mixed-plane, six-boss run a completion reward. | Accepted |
| 2026-08-17 | Give spirit rotation a one-second cooldown and a three-second elemental buff. | Makes rotation readable and strategically meaningful. | Accepted |
| 2026-08-17 | Use Water's Flow buff to accelerate cooldown recovery by 25%. | Supports Water's control identity without overlapping Earth defense, Fire damage, or Wind speed. | Accepted |
| 2026-08-17 | Add Focused and Empowered stationary weapon charge stages. | Strengthens the risk/reward of stopping in a survival game. | Accepted |
| 2026-08-17 | Give bosses elemental weaknesses, same-element resistance, and control conversion. | Rewards party composition without allowing permanent boss shutdown. | Accepted |
| 2026-08-17 | Remove fusion from the current design. | Keeps production focused on the core spirit, stance, progression, and boss systems. | Accepted |

## Open decisions

- [ ] Long-term campaign role for Wind Roc.
- [ ] Meta-progression model.
- [ ] Rotation buffs for Ice, Lightning, Poison, Necrotic, and Holy.
- [ ] Final status durations, stack caps, weakness, and resistance values.
