---
type: technical
status: active
tags:
  - development
  - ui
  - cards
  - art
---

# Card art setup

## Asset location

Imported card UI assets live here:

`World Of Spirits/Assets/_WorldOfSpirits/Art/UI/Upgrade cards`

The current pack contains `pixelCardAssest_V01.png`, imported as a multiple-sprite UI texture.

## Upgrade cards

Upgrade card data lives in:

`World Of Spirits/Assets/_WorldOfSpirits/Data/Upgrades`

Each `UpgradeCardDefinition` now supports:

- **Icon:** the small symbol for the upgrade.
- **Card Artwork:** an optional larger illustration for that specific card.

Use **Icon** for simple symbols and **Card Artwork** when a card should have its own larger image.

## Starter spirit cards

Spirit data lives in:

`World Of Spirits/Assets/_WorldOfSpirits/Data/Spirits`

Each `SpiritDefinition` now supports:

- **Card Portrait:** an optional portrait used by starter selection cards.

If **Card Portrait** is empty, the starter card falls back to the spirit prefab's gameplay sprite.

## UI prefab setup

Upgrade card UI prefab:

`World Of Spirits/Assets/_WorldOfSpirits/Prefabs/UI/Upgrades/Upgrade Card.prefab`

Starter spirit card UI prefab:

`World Of Spirits/Assets/_WorldOfSpirits/Prefabs/UI/SpiritSelection/Spirit Card.prefab`

For upgrade cards, add or assign an Image object to the new **Artwork** field on `UpgradeCardView` if you want the larger **Card Artwork** sprite to appear in the card layout.

Recommended card layers:

- Background or frame from the card asset pack.
- Optional Card Artwork illustration.
- Icon.
- Title, description, rarity, and level text.

## Demo priority

For the demo, make card art first for:

- Fire Spirit, Wind Spirit, Water Spirit, and Earth Spirit portraits.
- Contract cards for those four spirits.
- Weapon Mastery cards for those four spirits.
- Ascension cards for those four spirits.
- The three ability cards for each demo spirit.

