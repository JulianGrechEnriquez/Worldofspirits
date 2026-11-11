# Project cleanup review — 28 October 2026

No game assets, scripts, packages, or saved scenes were modified or deleted during this review. Only audit files were written under the repository's tmp folder.

## Scope and evidence

- Inventoried repository folders, Assets, Packages, ProjectSettings, local build output and profiling captures.
- Checked enabled build scenes: MainMenu and Game. Lobby and GameTestGround are not enabled.
- Queried Unity's asset dependency graph using enabled scenes, Resources assets and asset GUIDs found in ProjectSettings as roots.
- Scanned serialized GUID references across Assets and ProjectSettings and checked C# references for selected candidate scripts.
- Inspected development components in Game through an additive scene load, then closed it without saving.
- Found 370 files outside the dependency graph. This is NOT a deletion list: interfaces, base classes, runtime-created components, editor tooling, shader includes and source artwork can legitimately appear here. Reflection, dynamic loading, package internals and future content are not fully proved by this scan.
- Sizes below are source/local disk sizes, not measured player-build or RAM savings.

## 1. Remove from release gameplay first

| Item | Evidence | Recommendation |
| --- | --- | --- |
| UpgradeTestPanel | Attached to GameManager in Game; OnGUI draws an F4 button even when its window is closed. No release-build guard in the script. | Exclude from release builds, or remove the component from the release scene. Keep in development if useful. Do not delete GameManager itself. |
| StarterSpiritUnlockTestButton | Unlock All Spirits and Reset Unlocks buttons in Game. No release-build guard. They call persistent unlock/reset operations. | Exclude both buttons from release builds. Keep normal starter selection and saved unlock progression. |
| BossEncounterTestButton | TEST BOSS button in Game. Already disables itself outside Editor/development builds. | Existing release guard is useful; keep development-only or strip the object from release builds. Keep BossEncounterController and normal boss events. |
| CombatDebugHUD | Component is disabled in the inspected Game scene. Can scan active entities and draw an IMGUI overlay when enabled. | Safe development-only candidate; current disabled component is not an active performance problem. |
| Verbose gameplay logs | All four player prefabs have logHealthChanges and logRotation enabled. | Turn these flags off for release, or compile verbose diagnostics only for development. Keep useful errors. |

These changes are the most directly relevant to removing features outside normal gameplay. Measure frame-time improvement; do not expect large FPS gains from deleting inactive files.

## 2. Strong asset and code cleanup candidates

| Item | Finding | Action |
| --- | --- | --- |
| Assets/TextMesh Pro/Examples & Extras | About 5.58 MiB. No external serialized GUID references found. Includes example Resources assets. | Remove the examples folder and its meta after a clean build/UI check. Keep TMP Resources, fonts, materials and shaders required by UI. |
| Assets/_Recovery/0.unity | About 1.68 MiB. Recovery copy; no external serialized references found; not an enabled build scene. | Archive outside Assets or remove if no recovery content is needed. |
| Assets/_WorldOfSpirits/Data/Legacy | Earth Spirt and Earth spike assets; no external serialized references found. | Archive/remove if the legacy implementation is retired. |
| ProgressionInterface.cs | No serialized references or other project C# references found. | Candidate for removal after compile and progression UI check. Keep active PlayerProgressionHud, UpgradeScreen and related systems. |
| SpiritGrantButton.cs | No serialized references or other project C# references found. | Candidate for removing an unused debug helper. |
| DamageProjectile.cs | No serialized references or other project C# references found. | Candidate after checking projectile behavior in a fresh build. Keep active projectile implementations. |
| OrbitingMeleeWeapon.cs | No serialized references or other project C# references found in the current tree. | Candidate if its old orbiting weapon implementation is retired. |
| Prefabs/Projectiles/Earth/spike.prefab | No inbound serialized references found. | Candidate if the old spike is retired; review associated EarthSpike animation and Spike controller together. Keep the current Stone Spikes effect. |
| Prefabs/Spirits/Variants/Fire/fire.prefab and fire 1.prefab | No inbound serialized references found. | Archive/remove old variants after confirming the current Fire Spirit still works. |
| Settings/Lit2DSceneTemplate.scenetemplate | About 3.76 MiB; outside current player dependency roots. | Optional editor template. Remove only if it is no longer used when creating scenes. No expected gameplay FPS gain. |

Use Unity asset operations for cleanup, including associated meta files, and compile/test between groups. No assets in the candidate list were deleted.

## 3. Optional development and source content

- Lobby.unity and GameTestGround.unity: not enabled in the build. Keep if used for testing or planned flow; otherwise archive. They are already outside the normal scene list.
- Documentation, Notes, Showcase, Home.md and design DOCX files: development material. Keep in the repository or move documentation out of Assets; deleting these is not a gameplay optimization.
- Unreferenced attack sheets, tile variants, GIF previews and generated reference images: review individually. Exported sprites and their source files are not automatically interchangeable.
- The two Ice VFX Aseprite source files are outside the current dependency graph. Archive outside Assets if only exported PNGs are needed at runtime, while retaining editable originals for art work.
- Keep license and attribution files when retaining the associated art/fonts. Lack of a Unity GUID reference does not make a license disposable.

## 4. Packages and third-party tooling

The manifest contains development features with no project gameplay usage found for several of them. Package dependency resolution and a clean build are still required before removal.

- com.unity.multiplayer.center: candidate if no multiplayer development is planned.
- com.unity.collab-proxy: candidate if Git is the only version control workflow.
- com.unity.visualscripting: no project source usage or gameplay graphs identified; review removal.
- com.unity.timeline: no playable timeline assets or project source usage identified; review removal.
- com.unity.ai.inference: no inference model assets or project source use identified. Check whether AI authoring packages depend on it before removing.
- com.unity.2d.spriteshape and com.unity.2d.animation: no project SpriteShapeController/SpriteSkin source use found; serialized/package dependencies still require a check.
- IDE integrations: keep the IDE integration actually used; the other may be optional.
- PSD importer: the PSD files found are TMP examples. Reassess after deleting examples. Aseprite importer remains useful while the Ice source assets remain in Assets.
- AI Assistant and AI Game Developer/MCP are development tools, not normal gameplay features. Keep them if they are part of the authoring workflow; audit release inclusion separately.

Assets/Plugins/NuGet totals about 17.66 MiB. Microsoft.CodeAnalysis.CSharp.dll and Microsoft.CodeAnalysis.dll alone total about 12.12 MiB. McpPlugin.dll is currently enabled for Any platform, not restricted to Editor. An existing local test build contains MCP runtime libraries and Microsoft.CodeAnalysis.dll, demonstrating that development-related assemblies entered that older build. This does not prove their exact inclusion in a new release build. Prefer supported package configuration, editor-only import settings where compatible, or a release build exclusion strategy. Do not delete individual DLL dependencies at random.

Keep Input System, URP, UI/TMP, 2D physics, tilemaps, animation and particle systems used by gameplay. Do not mass-remove built-in modules based only on their names.

## 5. Local files that can be cleared or archived

| Folder | Approximate size | Effect and condition |
| --- | --- | --- |
| World Of Spirits/ProfilerCaptures | 265.67 MiB | Old captures; delete/archive if no longer needed. Disk only. |
| World Of Spirits/BulidTestPeformance | 244.95 MiB | Previously exported game build; delete if no longer needed. Rebuild to test later. |
| Repository tmp | 18.91 MiB at inventory time | Old scene/prefab backups, screenshots, helper scripts, rendered documents and audit outputs. Keep desired source prompts/backups before clearing. |
| World Of Spirits/tmp | 3.91 MiB | Generated art working files; archive any originals needed. |
| World Of Spirits/.vs | 10.94 MiB | Generated IDE state. Close the IDE before clearing. |
| World Of Spirits/Logs | 2.09 MiB | Old logs. Keep diagnostics needed for open bugs. |
| World Of Spirits/Temp | 36.84 MiB | Generated working data. Only clear with Unity closed. |
| World Of Spirits/Library | 3707.62 MiB | Rebuildable Unity cache. Only clear with Unity closed. Next launch reimports assets; this is not a routine FPS optimization. |

Old builds and profiler captures alone account for about 510.62 MiB. Cache clearing can free several GiB locally but does not shrink an already exported game or improve its frame rate.

Do not delete .git, Packages or ProjectSettings. Keep AI/agent configuration if those tools are still used. Existing unrelated working-tree modifications were left alone.

## Recommended order

1. Exclude upgrade/unlock test UI from release gameplay and disable verbose logs.
2. Remove unused TMP examples and archive recovery/legacy content.
3. Remove the four unused script candidates and old prefabs in small groups, checking compilation and gameplay each time.
4. Audit development DLL/package inclusion with a fresh release build and build report.
5. Clear old builds, profiler captures and temporary outputs as desired; leave Library unless disk space or cache issues justify rebuilding it.

Supporting files: cleanup-unreferenced-assets.txt and project-cleanup-sizes.json in this same tmp folder.

Unity reference: https://docs.unity.com/en-us/engine/6000.0/manual/get-started/project-configuration/special-folders
