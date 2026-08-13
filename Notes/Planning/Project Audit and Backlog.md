---
type: backlog
status: active
reviewed: 2026-08-06
tags:
  - planning
  - performance
  - architecture
  - vertical-slice
---

# Project audit and backlog

## Demo scope

The current release target is a demo with **four complete spirits** (Fire, Wind, Water, and Earth) and **two complete areas** (Burning Plains and Earth Plains). See [[Notes/Planning/Demo Scope and Completion Plan|Demo scope and completion plan]] for the authoritative scope and definition of done.

## Recommendation

The next milestone is a complete **Burning Plains vertical slice**:

> Main menu → select Fire Spirit → survive ten minutes → fight Fire Phoenix → receive a reward → return to the menu.

Use Burning Plains as the first production milestone. Do not expand beyond the four demo spirits and two demo areas until this loop is playable. The project already has broad content and strong technical foundations, but a complete run is needed to validate whether combat, progression, performance, and architecture work together.

For faster development, first make the run two to three minutes long. Expand it to ten minutes after the full loop works and has been profiled.

## Current audit snapshot

Reviewed on 2026-08-06 from the Unity project and planning vault.

- Unity version: 6000.3.11f1.
- Runtime source: 93 C# files and approximately 12,683 lines.
- Data includes eight spirit definitions, 24 ability definitions, five weapon definitions, a Burning Plains biome, three Fire enemy definitions, and a large upgrade catalog.
- Only `Game.unity` is currently enabled in Build Settings.
- `MainMenu.unity` and `Lobby.unity` contain no attached scripts yet.
- There are no dedicated automated test files or assembly definitions.
- Runtime pooling, combat spatial indexing, crowd simulation, animator culling, and data-driven content are already present.
- `Assets/Plugins/NuGet` contains approximately 17.2 MB of development DLLs. Their import settings currently allow Any Platform, so player-build exclusion needs to be verified.
- Runtime audio folders are currently empty.

## Immediate execution plan

- [ ] Make a two-to-three-minute development version of Burning Plains.
- [ ] Connect Main Menu, starter-spirit selection, gameplay, victory, loss, retry, and return-to-menu flow.
- [ ] Complete the Fire Phoenix boss trigger, encounter, death, and reward.
- [ ] Verify that all run state resets correctly when retrying or returning to the menu.
- [ ] Profile the complete short run with 50, 100, 200, and 250 enemies.
- [ ] Fix the largest measured gameplay and performance problems.
- [ ] Expand the validated run to ten minutes.
- [ ] Complete the other three demo spirits, then build Earth Plains after the vertical slice is stable.

## P0 — Complete the playable loop

- [ ] Build a functional main menu.
- [ ] Add Play, Settings, Credits, and Quit actions.
- [ ] Connect the menu to starter-spirit selection.
- [ ] Let the player select the Fire Spirit.
- [ ] Load Burning Plains after selection.
- [ ] Add an area title or short introduction.
- [ ] Validate movement responsiveness.
- [ ] Clearly communicate stationary weapon mode.
- [ ] Clearly communicate moving support-ability mode.
- [ ] Add difficulty phases or enemy waves.
- [ ] Keep the run timer clearly visible.
- [ ] Trigger Fire Phoenix at the end of the run.
- [ ] Pause normal enemy spawning during the boss encounter.
- [ ] Add a boss health bar and readable attack warnings.
- [ ] Add boss death feedback.
- [ ] Add victory, loss, reward, retry, and return-to-menu screens and actions.
- [ ] Reset timers, pools, progression, spirits, and UI correctly on retry.
- [ ] Test pausing during gameplay, level-up selection, boss combat, victory, and defeat.
- [ ] Add a short, skippable tutorial for movement, attack states, upgrades, and spirit rotation.

## P0 — Performance baseline

- [ ] Choose the primary target platform and minimum hardware.
- [ ] Choose a frame-time target: 16.6 ms for 60 FPS or 33.3 ms for 30 FPS.
- [ ] Create a dedicated performance test scene.
- [ ] Test 50, 100, 200, 250, and 400 enemies.
- [ ] Record average FPS and worst frame time.
- [ ] Record CPU, rendering, physics, memory, and garbage-collection costs.
- [ ] Record active and peak enemies, projectiles, pickups, VFX, and damage numbers.
- [ ] Profile a normal build and an intentionally extreme upgrade build.
- [ ] Add profiler markers around spawning, targeting, abilities, pickups, and pooling.
- [ ] Profile a Development Build as well as the Unity Editor.
- [ ] Create a repeatable performance test checklist.
- [ ] Save baseline results before optimizing.

## P0 — Reduce build and resource usage

- [ ] Exclude Unity MCP, Roslyn, and other development DLLs from player builds.
- [ ] Mark development-only plugins as Editor-only.
- [ ] Generate a build report and inspect included assemblies and assets.
- [ ] Review whether AI inference, multiplayer, Visual Scripting, Timeline, terrain, cloth, XR, and vehicle support are required.
- [ ] Remove unused packages only after confirming that no project asset depends on them.
- [ ] Disable HDR in the URP asset if no visual effect requires it.
- [ ] Create a lightweight 2D URP configuration.
- [ ] Disable unnecessary real-time shadows and lights in lower quality settings.
- [ ] Create sprite atlases for assets drawn together.
- [ ] Set texture maximum sizes based on actual source resolution and screen use.
- [ ] Disable mipmaps on UI and pixel-art sprites where appropriate.
- [ ] Set suitable texture compression for each target platform.
- [ ] Keep source GIF, Aseprite, reference, and documentation files out of player builds where appropriate.
- [ ] Remove unused recovery scenes from production asset folders after confirming they are no longer needed.
- [ ] Add an asset and build-size audit to the release checklist.

## P1 — Runtime performance

- [ ] Use profiler evidence to order optimization work.
- [ ] Move experience-orb updates into a centralized pickup simulation if orb counts become expensive.
- [ ] Merge nearby experience drops into higher-value orbs and set a maximum active count.
- [ ] Collect or merge old and distant orbs safely.
- [ ] Consider centralizing damage-number timing instead of updating one emitter per enemy.
- [ ] Allow damage numbers to be reduced or disabled in Settings.
- [ ] Remove or centralize `Physics2D.SyncTransforms()` from individual melee attacks.
- [ ] Avoid recreating renderer arrays when melee visuals change.
- [ ] Prewarm pools using measured peak counts.
- [ ] Add a maximum retained count to each pool.
- [ ] Track created, active, inactive, and peak object counts per pool.
- [ ] Verify that pooled objects reset colliders, particles, trails, audio, events, and runtime state.
- [ ] Spread unusually expensive ability casts across frames when safe.
- [ ] Cap chain, bounce, and area target queries for extreme builds.
- [ ] Remove release-build debug logs and development HUDs.
- [ ] Configure required components on prefabs instead of adding them at runtime.
- [ ] Reduce active Animator components when simple sprite animation is enough.
- [ ] Cull off-screen VFX and particles.
- [ ] Use shared materials and avoid accidental material instances.
- [ ] Add reasonable projectile, pickup, VFX, and damage-number budgets.

## P1 — OOP and architecture

### Preserve these strengths

- [ ] Continue using ScriptableObject definitions for abilities, weapons, enemies, biomes, and upgrades.
- [ ] Preserve useful interfaces such as damage, status, reward, and classification contracts.
- [ ] Preserve centralized combat and crowd simulations.
- [ ] Preserve spatial indexing, non-allocating queries, pooling, and data-driven spawning.

### Improve responsibility boundaries

- [ ] Split `ThrustMeleeWeaponBase` into targeting, punch motion, hit resolution, and visual management.
- [ ] Split `SpawnDirector` into run timing, difficulty, enemy selection, spawn positioning, and boss-event coordination.
- [ ] Split `DataDrivenAbility` into small execution strategies.
- [ ] Keep `SpiritManager` focused on party membership and rotation.
- [ ] Move spirit input handling and formation animation into separate components.
- [ ] Replace the growing ability execution switch with executor or ScriptableObject strategy implementations.
- [ ] Use the same strategy approach for weapon families if more execution types are added.
- [ ] Prefer composition for special weapon behaviour instead of deep inheritance.
- [ ] Rename the two `EnemyPool` types so their responsibilities are unambiguous.
- [ ] Standardize enemy, projectile, pickup, and VFX pooling behind one clear API.
- [ ] Remove one of the duplicate `GameManager` state-change events unless they acquire different meanings.
- [ ] Reduce reliance on static singletons and `FindFirstObjectByType`.
- [ ] Use serialized scene dependencies and a small scene bootstrap or installer.
- [ ] Validate missing prefab components in Editor tools instead of repairing normal prefabs at runtime.
- [ ] Use stable IDs instead of cleaned GameObject names for spirits, upgrades, abilities, and saves.
- [ ] Keep run-state data separate from scene objects.
- [ ] Separate visual presentation from combat calculations.
- [ ] Separate targeting rules from damage application.
- [ ] Add interfaces only at useful boundaries.
- [ ] Standardize event subscription and unsubscription ownership.
- [ ] Document which system owns each runtime object's lifecycle.
- [ ] Add Runtime, Editor, and Tests assembly definitions.
- [ ] Move debug UI into a development-only assembly.

## P1 — Automated tests and validation

- [ ] Add Edit Mode tests for damage calculation.
- [ ] Test status duration, stacking, replacement, and expiry.
- [ ] Test upgrade prerequisites, rarity, and weighting.
- [ ] Test experience thresholds and multiple level-ups from one reward.
- [ ] Test spirit capacity, duplicate contracts, and rotation order.
- [ ] Test ability and weapon level lookup.
- [ ] Test spawn budget, difficulty milestones, and weighted enemy selection.
- [ ] Test pool reuse and state reset.
- [ ] Test death-event cleanup and unsubscribe behaviour.
- [ ] Add Play Mode tests for starting, winning, losing, retrying, and leaving a run.
- [ ] Add prefab and ScriptableObject validation tests.
- [ ] Add missing-reference validation for every build scene.
- [ ] Add a test that loads every build scene.
- [ ] Add a shortened automated boss-run smoke test.
- [ ] Add an automated player-build smoke test.

## P1 — Combat and game feel

- [ ] Add subtle hit-stop for powerful attacks.
- [ ] Add camera shake with user-configurable intensity.
- [ ] Keep enemy hit flash compatible with batching.
- [ ] Improve elemental damage colours and icons.
- [ ] Add distinct critical-hit feedback.
- [ ] Add knockback resistance for heavy enemies and bosses.
- [ ] Define boss resistance to freeze, stun, pull, and other control effects.
- [ ] Add target caps or falloff to extreme area attacks where balance requires it.
- [ ] Improve projectile impact feedback.
- [ ] Add anticipation frames and telegraphs to dangerous attacks.
- [ ] Add clear player invulnerability feedback after taking damage.
- [ ] Add directional damage indicators.
- [ ] Add distinct sounds for weapons, abilities, critical hits, level-ups, bosses, victory, and defeat.
- [ ] Add varied enemy death and experience-collection feedback.
- [ ] Add controller rumble where supported.
- [ ] Show ability cooldowns and label moving, stationary, and always-active attacks.
- [ ] Add feedback and a short lockout for spirit rotation.
- [ ] Consider optional manual aiming while retaining automatic targeting.
- [ ] Restrict enemy health bars to elites and bosses.
- [ ] Give elites distinct silhouettes and effects.

## P1 — Burning Plains enemies and Fire Phoenix

- [ ] Make Fire Runner a fast pressure enemy.
- [ ] Make Fire Flier a ranged or evasive threat.
- [ ] Make Fire Tank a slow, durable space blocker.
- [ ] Add at least one elite modifier to each archetype.
- [ ] Add readable spawn formations and one or two mid-run events.
- [ ] Add a Burning Plains environmental hazard.
- [ ] Add a mid-run mini-boss only if the base pacing needs it.
- [ ] Give Fire Phoenix multiple distinct phases.
- [ ] Telegraph every high-damage boss attack.
- [ ] Provide recovery windows between attack patterns.
- [ ] Prevent invalid boss spawn positions.
- [ ] Add an arena boundary or safe boss-repositioning rule.
- [ ] Scale difficulty through enemy combinations and behaviour, not only health and count.
- [ ] Add an enemy simulation-LOD debugging overlay.

## P2 — Progression and build variety

- [ ] Finish acquiring a second and third spirit during a run.
- [ ] Improve spirit-contract upgrade presentation.
- [ ] Show current spirit slots during upgrade selection.
- [ ] Preview how rotation changes weapons and abilities.
- [ ] Add upgrade comparison text and tags such as Weapon, Projectile, Area, Status, and Support.
- [ ] Add upgrade filtering and search to development tools.
- [ ] Clearly explain reroll cost and consider Skip and Banish mechanics.
- [ ] Add evolution and fusion previews.
- [ ] Add post-run statistics by spirit, weapon, and ability.
- [ ] Add collection screens for unlocked and discovered content.
- [ ] Design the Spirit Dust economy and permanent unlock pacing.
- [ ] Define exact fusion eligibility and offer rules.
- [ ] Prototype only two fusion recipes first.
- [ ] Add duplicate-upgrade and bad-luck protection.
- [ ] Add deterministic run seeds for testing.

## P2 — Save system

- [ ] Define a versioned save-data format.
- [ ] Save settings separately from progression.
- [ ] Save unlocked spirits, currency, permanent upgrades, and discovered content.
- [ ] Add save migration and backup files.
- [ ] Recover gracefully from corrupted saves.
- [ ] Add a confirmed Reset Progression action.
- [ ] Never save direct Unity object references.
- [ ] Use stable IDs for every saved definition.
- [ ] Add development tools to inspect and modify save data.

## P2 — UI, accessibility, and usability

- [ ] Add master, music, and SFX volume controls.
- [ ] Add resolution, display mode, and quality controls.
- [ ] Add damage-number density and screen-shake settings.
- [ ] Add a reduced-flashing option.
- [ ] Use colour-blind-friendly elemental icons as well as colour.
- [ ] Add UI scaling.
- [ ] Add remappable keyboard and controller controls.
- [ ] Add complete controller navigation and aim-assist options.
- [ ] Show current upgrades in the pause menu.
- [ ] Add consistent tooltips and improve upgrade-card readability.
- [ ] Move player-facing text into localization-ready data.
- [ ] Avoid embedding player-facing strings directly in scripts.
- [ ] Add a compendium for spirits, enemies, bosses, upgrades, and fusions.

## P3 — Content after the vertical slice

- [ ] Complete the remaining spirit weapons.
- [ ] Give every spirit a distinct mechanical identity.
- [ ] Balance the existing upgrade catalog before producing many more cards.
- [ ] Build the second area using reusable area and encounter data.
- [ ] Add area-specific hazards and enemy rosters.
- [ ] Decide whether Earth Golem and Wind Roc are bosses, mini-bosses, or challenge encounters.
- [ ] Build reusable boss-phase, wave, and event data.
- [ ] Add challenge modes, achievements, seeded runs, and difficulty modifiers after the campaign loop works.
- [ ] Add Endless Mode only after the normal campaign is stable.
- [ ] Add fusion-specific VFX and UI.
- [ ] Add narrative area introductions and boss conclusions.
- [ ] Add music, ambience, and a complete audio mix.

## Recommended order

1. Complete scene flow and the short vertical slice.
2. Establish a performance baseline in a Development Build.
3. Exclude development DLLs and unused content from player builds.
4. Complete Fire Phoenix, victory, and reward flow.
5. Refactor only the classes that are actively slowing development or profiling poorly.
6. Add automated tests for progression, pooling, spawning, and scene flow.
7. Improve combat feedback, settings, and accessibility.
8. Add versioned saves and meta-progression.
9. Prototype two fusion recipes.
10. Start the second area.

## Related notes

- [[Notes/Planning/Demo Scope and Completion Plan|Demo scope and completion plan]]
- [[Notes/Planning/Roadmap|Roadmap]]
- [[Notes/Development/Technical Overview|Technical overview]]
- [[Notes/Game Design/Game Overview|Game overview]]
- [[Notes/Game Design/Areas and Bosses|Areas and bosses]]
- [[Notes/Planning/Decision Log|Decision log]]
