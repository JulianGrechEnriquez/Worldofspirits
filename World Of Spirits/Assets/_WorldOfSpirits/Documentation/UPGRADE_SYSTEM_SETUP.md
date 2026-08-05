# World of Spirits Upgrade System

## Runtime flow

1. An XP pickup or enemy reward calls `PlayerLevelSystem.AddExperience(amount)`.
2. Each gained level is queued by `UpgradeSelectionManager`.
3. The manager filters the catalog by player level, ownership, maximum level, prerequisites, spirit slots, and authored weapon/ability limits.
4. Three unique cards are selected with weighted random selection. The game pauses.
5. `UpgradeScreen` binds its three pre-created `UpgradeCardView` objects.
6. The chosen card is applied by `UpgradeRuntimeStats`; queued level-ups are shown one after another.

The candidate lists are allocated once and reused. Active numeric stats use fixed arrays. A dictionary maps stable card IDs to obtained levels so prerequisites are constant-time lookups.

## Unity setup

1. Let Unity compile, then run **World of Spirits > Upgrades > Generate Starter Catalog**.
2. Add `PlayerLevelSystem`, `UpgradeRuntimeStats`, and `UpgradeSelectionManager` to the Player.
3. Assign **Main Upgrade Catalog** to the selection manager. References on the same Player auto-cache.
4. Create a screen-space Canvas with a full-screen panel and `CanvasGroup`.
5. Add `UpgradeScreen` to the panel and assign the Player's selection manager.
6. Create three child card buttons with `UpgradeCardView`; assign their TMP text, icon, border, and button fields. Put all three views in the screen list.
7. Connect enemy XP rewards or XP pickups to `PlayerLevelSystem.AddExperience`.
8. Use **Debug Open Upgrade Choice** on `UpgradeSelectionManager` to test without enemies.

Do not disable the upgrade panel GameObject. `UpgradeScreen` hides it through `CanvasGroup`, allowing it to continue receiving selection events.

## Progression rules

- Ability 1 starts at level 1.
- Later abilities start locked at level 0.
- A later ability becomes eligible after the previous ability reaches the configured investment threshold (default 2).
- Started weapon and ability card paths receive a 1.75x selection weight, encouraging completion without forcing it.
- Contracts begin at player level 4 and require an empty spirit slot.
- Generated evolutions begin at level 25. Add exact prerequisite card IDs in their asset and unique transformation behaviour on the relevant spirit prefab.
- Rare and Epic pity weights rise after configurable dry streaks. This increases odds; it does not force a Legendary.
- Duplicate cards level the same card until its maximum. Three offers are always unique within one choice.

## Default rarity presentation

| Rarity | Approximate relative weight | Border | Suggested presentation |
|---|---:|---|---|
| Common | 100% | Silver | Simple reveal and soft click |
| Uncommon | 35% | Green | Short glow and brighter chime |
| Rare | 12% | Blue | Light sweep and layered chime |
| Epic | 3.5% | Purple | Pulse, particles, deep impact |
| Legendary | 0.6% | Gold | Screen flash, animated border, unique sting |

Final appearance probability depends on which cards are eligible, Luck, and pity. This is intentional: a raw fixed percentage would produce poor choices when many cards are unavailable.

## Adding content

- **Player or Legendary:** Create an Upgrade Card asset, select modifiers, rarity, maximum level, and requirements.
- **Weapon:** Set category to Weapon and assign its Spirit Definition. Applying it calls the existing `SpiritProgression.TryLevelWeapon`.
- **Ability:** Set category to Spirit Ability, assign its Spirit Definition and zero-based ability index.
- **Contract:** Assign the Spirit Definition and matching spirit prefab. Duplicate ownership and full formations are rejected.
- **Evolution:** Add prerequisite card IDs and required levels. Keep its dramatic visual/behaviour code on a dedicated component on the relevant spirit or weapon.

Balance percentage modifiers around 5-12% per common level, 12-25% for rares, and reserve mechanical changes for Epic/Legendary cards. Spawn only effects that the weapon implementation supports; unsupported modifier values remain safely stored for future weapon classes.
