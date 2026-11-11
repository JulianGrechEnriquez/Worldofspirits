# World of Spirits

A Unity 2D roguelike survival game. Open `World Of Spirits` in Unity Hub; use the editor version recorded in `World Of Spirits/ProjectSettings/ProjectVersion.txt`.

## Where things belong

| Location | Purpose |
| --- | --- |
| `World Of Spirits/Assets/_WorldOfSpirits` | First-party game assets and code |
| `World Of Spirits/Assets/_WorldOfSpirits/Scripts/Editor` | Editor tooling, including custom inspectors |
| `World Of Spirits/Assets/Plugins`, other imported asset folders | Third-party tools and dependencies |
| `World Of Spirits/Packages`, `ProjectSettings` | Dependency and project configuration |
| `Notes/Game Design` | Design intent |
| `Notes/Development` | Implementation notes, reviews and verification |
| `Notes/Planning` | Scope, decisions and backlog |
| `Showcase` | Public presentation material |
| `tmp` | Local working outputs and historical audit evidence |

Start with [Home](Home.md) for the design vault, [project structure](World%20Of%20Spirits/Assets/_WorldOfSpirits/Documentation/PROJECT_STRUCTURE.md) for asset placement, and the [11 November review](Notes/Development/2026-11-11%20Project%20Review.md) for current findings.

## Development and verification

The enabled build scenes are MainMenu and Game. Lobby and GameTestGround are authoring/test scenes. Start the game from MainMenu.

Use **World of Spirits → Validate Spirit System** to inspect authored ability and spirit data. Use **Create Required Project Folders** to restore the standard folder structure. Move Unity assets through AssetDatabase/the Project window so GUIDs are preserved.

Upgrade testing (F4), combat diagnostics (F3), and unlock/reset test buttons are available only in the Editor or development builds. Validate a non-development player before shipping. Keep license files with imported assets.

Generated Library, Temp, logs, builds, and profiler captures are local outputs. The `.gitignore` excludes their standard project locations. Existing tracked files remain tracked until deliberately reviewed.
