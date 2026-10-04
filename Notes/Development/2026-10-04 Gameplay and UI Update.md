---
type: development-log
status: recorded
updated: 2026-10-04
tags: [development, gameplay, ui]
---

# Gameplay and UI update — 4 October 2026

Source update: [GitHub commit 27a5491](https://github.com/JulianGrechEnriquez/Worldofspirits/commit/27a54919ad2b179479b0b1f57b9c73f4ebf58680). See [[Notes/Development/Spirit Ability Implementation Status|the ability checklist]] and [[Notes/Development/Cleanup Review|cleanup review]] for follow-up work.

## Earth spirit

- Draw Quicksand Domain below characters and above the floor with a translucent surface.
- Apply a 40% movement slow to enemies inside the domain, including crowd movement. Remove the area slow on exit or pool release; overlapping areas use the strongest slow.
- Fix Boulder collision settings so projectiles can damage enemies, and add a pixel art boulder sprite and ability icon.
- Add Stone Spikes artwork, a pooled effect prefab, enemy targeting, growth/retraction, burst damage and bleed at higher levels. Connect all five upgrade levels.

## Water spirit

- Add a cloud sprite and Rain Cloud prefab with rain particles, enemy following, periodic damage and safe retargeting when enemies are pooled.
- Add a Whirlpool prefab with three inward rotating particle rings, ground-level sorting, radial pull, swirl and higher-level damage.
- Connect Rain Clouds and Whirlpool upgrade data to the new prefabs. Share radius scaling, lifetime and pooled reset behavior through WaterAreaEffect.

## Fire and movement activation

- Replace the Fiery Talons trail sprite with pooled flame particles and a dedicated material.
- Reevaluate movement activation before ability cooldowns. Stop movement-dependent effects immediately when their activation condition fails, while respecting support and mastery exceptions.
- Track pool spawn versions when releasing spawned ability effects to avoid releasing a reused object.
- Remove stale damage-zone occupants and prevent hit-flash coroutines from starting on inactive enemies.

## Ice, ability tooling and compilation

- Add an Avalanche projectile and prefab that grow, capture eligible enemies, carry them and release them on expiry or pool return.
- Preserve authored upgrade tuning when connecting existing data-driven abilities; simplify the spirit prefab connector.
- Remove obsolete ability implementations and legacy cleanup/lava builder scripts. Remove the deleted lava builder's leftover component from Game.
- Keep weapon size adjustment available in player builds while leaving debug visualization editor-only, fixing the ThrustMeleeWeaponBase compilation issue.
- Connect Gale Barrier in the Wind spirit's runtime ability list.

## Menus, HUD and project setup

- Build out the MainMenu scene and controller with Play, help, saved volume/fullscreen settings and Quit.
- Update the health HUD fill and tint, timer/boss warning visibility, starter spirit selection and spirit card layout.
- Organize the Game scene hierarchy and hide gameplay HUD elements while choosing a starter spirit.
- Add MainMenu ahead of Game in build settings, restore the default time scale and preload the input action asset.
- Update the Unity MCP package and its bundled dependencies together.

## Verification

- The latest available Windows standalone build report succeeded with zero errors. It predates the final removal of the missing lava builder component.
- Both enabled build scenes were inspected after that removal and contain zero missing scripts.
- Player compilation of the weapon size adjustment was checked with editor-only code excluded.
- This is not a complete playthrough of every upgrade combination.

## Remaining ability work

- Fire: feather fire patches, advanced Talons spreading/explosions and Phoenix fire zones still need implementation; revisit the revive limitation.
- Earth: Boulder splitting/stun and final-impact explosion behavior, advanced Quicksand slow/elite immobilization and sequential level-five Stone Spikes need follow-up. Current level-five spikes spawn together.
- Wind: moving Tornado behavior and Gale Barrier twin-blast behavior remain unfinished.
- Ice: Orbital Snowball freeze chance still needs to be applied.

Exported builds, local generated tool instructions and temporary backups/previews are excluded from this source update. Cleanup candidates from the separate audit have not been removed merely because they were listed.
