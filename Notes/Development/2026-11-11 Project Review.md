---
type: review
reviewed: 2026-11-11
status: active
---

# Project review — 11 November 2026

## Changes completed

- Consolidated AbilityDefinitionEditor and ProjectileTriggerDrawer under `Scripts/Editor/Inspectors` using Unity AssetDatabase.MoveAsset. Their meta files/GUIDs moved with them. The original empty Editor folder is retained.
- Updated ProjectOrganizer so it creates the Inspectors folder.
- Added the repository README with the Unity entry point, folder ownership, verification instructions and links. Linked this review from Home and updated PROJECT_STRUCTURE.
- Excluded UpgradeTestPanel and CombatDebugHUD implementation from non-development players. Their release components disable themselves; F3/F4 tools remain available in Editor/development builds.
- Excluded StarterSpiritUnlockTestButton implementation from non-development players. Its release component hides its button object, preventing these test buttons from changing persistent unlocks.
- Restricted opt-in health/death and spirit-rotation diagnostic messages to Editor/development runs. Serialized debug preferences remain available for development.
- Preserved the existing uncommitted scene, game-state, pooling, UI, package and tool changes. No gameplay content or packages were deleted.

## Verification performed

- Connected Unity 6000.3.11f1 reported no script compilation failure before or after the changes and asset refresh. Final compilation status was checked after the editor reloaded.
- Scanned all 51 first-party prefabs and all four first-party scenes for missing MonoBehaviour scripts: none found. Scenes loaded for inspection were closed without saving; MainMenu remains active.
- Ran the existing SpiritSystemValidator: 21 content TODOs across five ability definitions, listed below. This validator does not validate every serialized reference or prove ability behavior.
- The enabled build scenes are MainMenu and Game. There are 126 first-party MonoScript assets; no dedicated project test scripts or assembly definitions were found in Assets.
- Checked the moved inspector GUIDs against Git and checked whitespace in the files changed during this review.
- No fresh player build, playthrough, device/controller check or performance capture was run. Release conditional branches still need a player-build check.

## Prioritized follow-up

| Priority | Item | Evidence and next action |
| --- | --- | --- |
| High | Audit development DLL inclusion in a release player | Current PluginImporter settings report AnyPlatform=true for McpPlugin.dll and Microsoft.CodeAnalysis.dll, with Windows64=false; Microsoft.CodeAnalysis.CSharp.dll reports AnyPlatform=false, Editor=true, Windows64=false. These flags alone do not establish final inclusion. Produce a fresh non-development build, inspect its build report and assemblies, then configure development tools through supported package/import settings if necessary. |
| High | Exercise the complete demo loop | Validate MainMenu → character/starter selection → gameplay → pause/resume → boss → results → retry/menu. Check health, upgrades, pools and Time.timeScale reset between runs, plus saved unlock persistence. The current compile/reference checks cannot prove this flow. |
| Medium | Finish or defer five incomplete ability definitions | Holy / Light Beams levels 1–4 and Lightning / Lightning Strike levels 1–4 lack effect prefabs. Poison / Acid Spray levels 1–5, Toxic Glob levels 1–4, and Venom Needles levels 1–4 lack projectile prefabs. These are 21 validator TODOs. Confirm runtime fallback behavior before treating them as broken; author the intended prefabs or explicitly exclude unfinished content from player choices. They are outside the four-spirit demo target. |
| Medium | Add meaningful regression coverage | No dedicated tests were found. Prioritize persistent unlock/reset behavior, pause transitions, retry state reset and pooled-object reuse. Verify player behavior as well as Editor behavior. |
| Medium | Measure performance before optimizing | Capture enemy-heavy gameplay, boss effects, level-up and retry. Compare CPU/frame time, allocations and active object counts. Removing inactive source assets does not establish an FPS gain. |
| Low | Keep planning notes current | Older audit snapshots contain historical counts and build-scene statements. Preserve history, but use this dated review and Unity assets for current evidence. |

## Candidates to remove or archive

These are review candidates, not proven dead content. Check references and desired future use before deleting; move Unity assets through the editor and retain source/license files when needed.

- **TMP Examples & Extras:** remove imported samples after confirming no example fonts/materials/Resources are needed by your UI.
- **Recovery and legacy content:** review `Assets/_Recovery/0.unity`, `Data/Legacy`, old Earth spike and Fire variant prefabs. Archive recoverable originals outside Assets if retired.
- **Old code:** recheck ProgressionInterface, SpiritGrantButton, DamageProjectile and OrbitingMeleeWeapon with serialized and runtime references. Do not delete interfaces/base classes merely because scenes do not reference them.
- **Extra scenes:** retain Lobby and GameTestGround if they remain useful development scenes; both are excluded from the enabled scene list.
- **Optional packages:** review Multiplayer Center, Version Control/Collab, Visual Scripting, Timeline, unused IDE integration and importers. Check package dependencies and run a clean build between removals. Keep the current AI/MCP authoring tools while they are in use.
- **Local disk output:** review old `BulidTestPeformance`, `ProfilerCaptures`, and both tmp locations. The 28 October audit estimated about 511 MiB in old build/capture folders; this is historical, not a new measurement. Keep captures/backups needed for unresolved work.
- **Generated caches:** Library, Temp and IDE state are regenerable. Clear only for a concrete disk/cache reason with the relevant applications closed; normal cleanup does not require deleting them.

The detailed 28 October inventory remains in `tmp/project-cleanup-review.md`; its sizes and dependency findings are historical and should be rechecked before deletion. See also [[Cleanup Review]] and [[../Planning/Demo Scope and Completion Plan]].
