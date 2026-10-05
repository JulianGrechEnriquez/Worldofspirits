---
type: roadmap
status: active
updated: 2026-10-05
tags: [planning]
---

# Roadmap

## Product direction

- **Story Mode:** six sequential ten-minute planes with required starting spirits and guardian bosses.
- **Infinity Mode:** unlocks after Story Mode and combines all six guardians in a continuous run.
- **Current release milestone:** a polished Burning Plains vertical slice rather than the entire campaign.
- **Demo characters:** Spirit Tamer, Spirit Warrior and Spirit Shepherd. Spirit Beastmaster follows later.
- Spirit fusion is excluded. Only the Spirit Shepherd can unlock combined ability moves; see [[Notes/Game Design/Spirit Shepherd|Shepherd design]].

## Milestone 1 — Burning Plains vertical slice

- [x] Add MainMenu Play, help, saved master volume/fullscreen settings and Quit.
- [x] Add health HUD fill/tint and update starter selection presentation.
- [ ] Finish and validate results, retry and return-to-menu flow.

- [ ] Connect menu → Burning Plains → Fire Phoenix → reward → menu.
- [ ] Use a two-to-three-minute development timer, then validate the ten-minute version.
- [ ] Complete Fire Runner, Fire Flier, and Fire Tank roles.
- [ ] Complete Fire Spirit's five-level weapon and three five-level abilities.
- [ ] Implement the one-second spirit-rotation cooldown and HUD feedback.
- [ ] Implement Fire, Earth, Water, and Wind rotation buffs.
- [ ] Implement stationary transformation, Focused, and Empowered feedback.
- [ ] Complete Fire Phoenix phase gates, warnings, recovery windows, and Rebirth presentation.
- [ ] Apply Water weakness, Fire resistance, and boss crowd-control conversion.
- [ ] Connect victory, loss, retry, pause, and return-to-menu flows.
- [ ] Profile 50, 100, 200, and 250 enemies in a Development Build.

## Milestone 2 — Four production-ready spirits

### Demo character work

- [ ] Add character selection for Tamer, Warrior and Shepherd.
- [ ] Prototype Warrior weapon-only movement combat, main-slot weapon pairs and armour.
- [ ] Prototype Shepherd companion-only casting and its larger party capacity.
- [ ] Finalize Shepherd-only combined move prerequisites and prototype Whirlpool + Tornado without merging spirits.
- [ ] Finalize and implement each character's 14 exclusive upgrades; retain compatible shared and spirit cards.
- [ ] Validate all three characters through Burning Plains, retry and return-to-menu.
- [ ] Profile the Warrior arsenal and the Shepherd's maximum party.

### Shared spirit work

Progress on 4 October: Earth art/core effects, Water Rain Clouds/Whirlpool and Fire trail particles are connected. Higher-level behaviors remain unfinished, so the spirit completion boxes stay open. See [[Notes/Development/Spirit Ability Implementation Status|the implementation checklist]].

- [ ] Complete Fire: Flame Bow, Fiery Feathers, Fiery Talons, Phoenix Dive.
- [ ] Complete Earth: Stone Hammer, Quicksand Domain, Boulder Throw, Stone Spikes.
- [ ] Complete Water: Water Trident, Tidal Wave, Whirlpool, Rain Clouds.
- [ ] Complete Wind: Chakrams, Razor Wind, Tornado, Gale Barrier.
- [ ] Finish all five-level data, icons, animations, VFX, audio, and descriptions.
- [ ] Validate every spirit as main, support one, and support two.
- [ ] Validate extreme upgrade combinations and pool budgets.

## Milestone 3 — Story Mode foundation

Spirit Beastmaster remains later character work. Its independent creature AI, health and recovery system are outside the three-character demo.

- [ ] Build stage selection and saved sequential unlocks.
- [ ] Enforce the plane's required starting spirit.
- [ ] Complete Frozen Wastes and standardize the Ice Spirit.
- [ ] Complete Thunder Peaks and standardize the Lightning Spirit.
- [ ] Complete Poison Marsh and standardize the Poison Spirit.
- [ ] Design Necrotic abilities and complete Shadow Realm.
- [ ] Design Holy abilities and complete Celestial Temple.
- [ ] Finish all six guardian fights and story completion presentation.

## Milestone 4 — Infinity Mode

- [ ] Unlock only after Story Mode completion.
- [ ] Combine enemy, hazard, and biome rules from all planes.
- [ ] Schedule all six guardians sequentially in one run.
- [ ] Continue with endless scaling after the sixth guardian.
- [ ] Add Infinity-specific statistics, leaderboards only if appropriate, and performance validation.

## Release and portfolio pass

- [ ] Add versioned saves, settings, accessibility, controller support, audio, credits, and licenses.
- [ ] Run external playtests and resolve progression blockers.
- [ ] Capture screenshots and short clips listed in [[Showcase/World of Spirits#Portfolio asset checklist|the showcase checklist]].
- [ ] Produce a playable build and concise development retrospective.

## Open design work

- [ ] Define rotation buffs for Ice, Lightning, Poison, Necrotic, and Holy.
- [ ] Set exact status stack limits and durations.
- [ ] Tune elemental weakness and resistance percentages.
- [ ] Design Necrotic Spirit abilities.
- [ ] Finalize Holy Spirit abilities.
- [ ] Decide the optional challenge roles for Earth Golem, Water Leviathan, and Wind Roc.

## Milestone template

Use [[Notes/Templates/Feature Note|Feature Note]] for work that needs design, implementation, and acceptance criteria.

