# World of Spirits Project Structure

All project-owned content lives under `Assets/_WorldOfSpirits`. Imported tools,
TextMesh Pro, and recovery files stay outside this folder so they are easy to
distinguish from game content.

## Main Folders

- `Animations` - animation clips and animator controllers.
- `Art` - sprites, tiles, interface art, and visual effects.
- `Audio` - music, ambience, and sound effects.
- `Data` - ScriptableObject definitions and catalogs.
- `Documentation` - setup guides and project notes.
- `Prefabs` - reusable gameplay and interface objects.
- `Scenes` - the four authored game scenes.
- `Scripts` - runtime code and editor-only tools.
- `Settings` - render-pipeline, input, and scene-template assets.

## Runtime Scripts

- `Scripts/Core` - game state and shared cross-feature services.
- `Scripts/Combat` - damage, targeting, projectiles, and weapons.
- `Scripts/Crowd` - large enemy-group simulation.
- `Scripts/Enemies` - enemy and boss behaviour.
- `Scripts/Player` - player identity and movement.
- `Scripts/Progression` - experience, upgrades, unlocks, and pickups.
- `Scripts/Spawning` - enemy spawning, separation, and pooling.
- `Scripts/Spirits` - spirit definitions, abilities, weapons, and progression.
- `Scripts/UI/Core` - shared UI actions and screen coordination.
- `Scripts/UI/Screens` - pause, loss, and other screen controllers.
- `Scripts/UI/Upgrades` - upgrade card and upgrade-screen presentation.
- `Scripts/UI/Progression` - progression HUD components.
- `Scripts/UI/SpiritSelection` - starter-spirit selection views.
- `Scripts/UI/Debug` - development-only controls and diagnostics.

## Data and Prefabs

- `Data/Abilities/<Element>` - ability definitions grouped by element.
- `Data/Enemies/Definitions` - enemy spawn and identity data.
- `Data/Enemies/Movement Profiles` - reusable crowd movement settings.
- `Data/Spirits` - spirit definitions.
- `Data/Upgrades` - upgrade catalog and upgrade entries.
- `Data/Weapons` - weapon definitions.
- `Prefabs/Projectiles/<Element>` - projectile prefabs grouped by element.
- `Prefabs/Spirits` - primary spirit prefabs.
- `Prefabs/Spirits/Variants` - experimental or alternate spirit prefabs.
- `Prefabs/UI` - reusable interface prefabs.

## Rules

- Put new files in the most specific existing folder.
- Keep runtime scripts out of `Scripts/Editor`.
- Keep generated gameplay data inside `Data`, not beside scripts.
- Use `Variants` for alternatives; do not mix them with the primary prefab.
- Move assets inside Unity so their `.meta` files and references are preserved.
- Create a new folder only when at least two related assets need it.
