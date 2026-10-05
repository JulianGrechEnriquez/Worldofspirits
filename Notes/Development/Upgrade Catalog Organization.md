---
type: technical
status: active
updated: 2026-10-05
tags: [development, upgrades, characters, catalog]
---

# Upgrade catalog organization

The existing 100-card catalog is organized by ownership separately from effect category and rarity. This structure is implemented; the new Warrior/Shepherd combat kits and their proposed upgrades are still design work.

## Upgrade groups

| Group | Current count | Contents |
|---|---:|---|
| General | 51 | Shared stat upgrades and shared legendary upgrades. Names such as Tamer's Might do not make a card exclusive. |
| Character | 1 | Spirit Master, restricted to Spirit Tamer. Future character abilities belong here. |
| Spirit | 48 | Eight spirits, each with a contract, weapon mastery, three ability cards and an ascension. |

Spirit Master retains its effect: the main spirit may cast unlocked abilities while channeling its weapon. It is now marked CharacterAbility and Tamer-only, rather than treating Legendary as its only classification.

## Three separate properties

- **Group:** General, Character or Spirit. Derived from the card's spirit target and character restriction, avoiding contradictory duplicated group data.
- **Category:** existing Player/stat, Weapon, SpiritAbility, SpiritContract, Evolution and Legendary categories, plus CharacterAbility. Existing serialized numeric values are preserved.
- **Rarity:** Common, Uncommon, Rare, Epic or Legendary. A legendary can belong to any group.

A spirit-targeted card stays in Spirit even if it later receives a character restriction. Character eligibility is checked both when offering upgrades and when applying them directly. The player upgrade component defaults to Spirit Tamer; its character setting is eligibility scaffolding, not a complete character-selection or combat implementation.

## Asset folders

Within `World Of Spirits/Assets/_WorldOfSpirits/Data/Upgrades`:

- `General/<Rarity>`: all shared cards, including shared legendaries.
- `Characters/SpiritTamer/Legendary`: Spirit Master. Future cards use their character and rarity folders.
- `Spirits/<Spirit>/Contracts`, `Weapons`, `Abilities`, `Ascensions`: spirit cards by purpose.
- `Main Upgrade Catalog.asset`: the single runtime catalog holding all 100 cards.

The organization moves existing assets through Unity, preserving their GUIDs and card IDs. Existing levels, rarity, weights and effects remain intact. Empty old Player and Legendary folders are removed only when they contain no files.

## Authoring and browsing

- Select Main Upgrade Catalog in Unity for General/Character/Spirit tabs, search and links to individual cards. Expand Edit catalog references to add/remove entries.
- The F4 upgrade tester has group filters alongside effect categories, displays character restrictions and disables incompatible cards.
- Content Creator exposes the character restriction and creates cards in the matching folders.
- `World of Spirits > Upgrades > Organize Existing Catalog` reorganizes existing content without generating new cards or replacing effects.
- The starter generator reuses moved cards and preserves custom catalog entries, including future character cards. Generating starter content still reapplies starter tuning, so use Organize for layout-only changes.

## Character-exclusive upgrade target

Each character is planned to have **5 Common, 4 Uncommon, 3 Epic and 2 Legendary upgrades**, totaling 14. Existing shared and spirit cards remain available where usable. Character-exclusive cards deliberately have no Rare tier in this proposed structure; existing Rare cards are retained.

Only Spirit Master currently occupies the runtime exclusive pool. [[Notes/Game Design/Character Exclusive Upgrades|A first draft of 14 Warrior and 14 Shepherd upgrades]] is recorded in the vault; these are not Unity cards or implemented effects. Warrior weapon-only and Shepherd companion-only compatibility beyond character restrictions must be implemented with their combat rules.

## Verification

- Unity imported and compiled the changed scripts.
- All 100 catalog entries retained their IDs, GUIDs, rarities, maximum levels and weights after reorganization; no missing entries were found.
- Spirit Master eligibility and direct application passed for Tamer and were rejected for Warrior and Shepherd without changing their upgrade level or effect stats.
- Shared cards remained eligible for all three character identities.
- Running the organizer again preserved the same catalog contents.

## Related

- [[Notes/Game Design/Playable Characters|Playable character concepts]]
- [[Notes/Game Design/Spirit Warrior|Warrior weapon and armour concepts]]
- [[Notes/Development/Spirit Ability Implementation Status|Spirit ability implementation status]]
