---
type: audit
status: review-required
reviewed: 2026-10-04
tags: [development, cleanup]
---

# Cleanup review

These are review candidates from the cleanup audit. Being outside enabled scene and Resources dependencies does not prove an asset is disposable. Editor tools, runtime loading, source art and future scenes may still use it. This note does not perform deletion.

## Strong candidates to inspect

- Root `tmp`: previews, prompts, helper scripts and backup copies. Preserve anything needed for recovery.
- TextMesh Pro Examples & Extras: demo content, after checking sample dependencies. Keep runtime TMP resources and fonts.
- Legacy Earth Spirt/Earth spike data and the old Earth spike, Fire variant and Ice Ball prefabs.
- `ProgressionInterface.cs`, `SpiritGrantButton.cs` and `DamageProjectile.cs`: no serialized or project-source references found during the audit; confirm runtime loading before removal.
- `OrbitingMeleeWeapon.cs`: the old Ice Ball prefab references it, so review both together before removal.

## Optional development content

- GameTestGround and Lobby scenes, if no longer used for testing or future flow.
- UpgradeTestPanel, StarterSpiritUnlockTestButton, BossEncounterTestButton and CombatDebugHUD, if retiring development controls; remove scene references with them.
- Saved profiling captures, exported builds, logs and IDE output.
- Optional packages such as Multiplayer Center, Version Control integration, Visual Scripting, Timeline and source-art importers, only after checking actual use.
- Unity Library and Temp are generated caches. Close Unity before clearing them; they will rebuild.

## Keep during development

Keep Assets, ProjectSettings, Packages, input, URP, particles, 2D physics, tilemaps, UI/TMP runtime resources, gameplay data/scripts, pools, crowd/combat systems, progression and unlocks. The two EnemyPool classes cooperate. DamageNumber/Emitter are created by EnemyBase at runtime. Editor generators and validators remain useful authoring tools.

The October source update already contains earlier removals of obsolete ability implementations, LegacyCombatCleanup and LavaPoolTilemapBuilder. Its stale Game scene component was removed too. That is separate from the undeleted candidates in this review.

Related: [[Notes/Planning/Project Audit and Backlog|Project audit and backlog]] and [[Notes/Development/2026-10-04 Gameplay and UI Update|October update]].
