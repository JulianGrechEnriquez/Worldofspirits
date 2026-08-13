---
type: milestone
status: active
milestone: demo
reviewed: 2026-08-06
tags:
  - planning
  - demo
  - spirits
  - areas
---

# Demo scope and completion plan

## Demo promise

The demo gives players a polished sample of the full game through:

- **Four complete spirits:** Fire, Wind, Water, and Earth.
- **Two complete areas:** Burning Plains and Earth Plains (working title).
- **Two final bosses:** Fire Phoenix and Earth Golem.
- A complete flow from menu and spirit selection to gameplay, victory, rewards, and return to menu.

The demo is complete when these features feel finished and reliable. Spirits, areas, modes, and meta-progression outside this scope are post-demo work.

## Player flow

1. Open the main menu.
2. Select New Run or Continue Demo Progress.
3. Choose Fire, Wind, Water, or Earth as the starter spirit.
4. Play Burning Plains.
5. Level up, choose upgrades, and contract up to two additional demo spirits.
6. Defeat Fire Phoenix and unlock Earth Plains.
7. Play Earth Plains with the unlocked starter options and progression.
8. Defeat Earth Golem.
9. View the demo-complete screen, run statistics, and call to action.
10. Return to the menu and retain intended demo unlocks and settings.

## Scope boundaries

### Included

- Four playable spirits and their complete combat kits.
- Two areas with unique visuals, enemies, encounters, and bosses.
- Three-spirit party acquisition and rotation.
- Experience, level-ups, upgrade choices, rerolls, victory, defeat, pause, and retry.
- A small save system for settings, area unlocks, and demo progression.
- Keyboard and controller support.
- Essential settings, accessibility options, audio, onboarding, and performance work.

### Not required for the demo

- Ice, Lightning, Poison, Necrotic, or Holy as playable spirits.
- Frozen Wastes, Thunder Peaks, Poison Marsh, Shadow Realm, or Celestial Temple.
- The complete campaign or every planned boss.
- Endless Mode, daily runs, achievements, or online features.
- A large permanent-upgrade tree.
- A complete fusion catalog. At most one polished fusion may be included as a teaser after the core demo is complete.

## Four-spirit completion checklist

Every demo spirit must satisfy the shared definition of done below.

### Shared gameplay requirements

- [ ] Has a stable ID, display name, description, element, role, and rarity rules.
- [ ] Can be selected as the starter spirit.
- [ ] Can be offered as an in-run contract when not selected first.
- [ ] Works correctly in main, support one, and support two slots.
- [ ] Rotates correctly between slots.
- [ ] Has one complete elemental weapon with a clear attack identity.
- [ ] Has three complete abilities with distinct purposes.
- [ ] Has complete weapon and ability level paths.
- [ ] Has contract, weapon mastery, ability, and ascension upgrade cards.
- [ ] Has balanced cooldown, damage, range, duration, targeting, and status values.
- [ ] Has readable descriptions and upgrade comparison text.
- [ ] Works with projectile count, size, speed, duration, critical, cooldown, and other applicable global upgrades.
- [ ] Handles extreme upgrade combinations without errors or runaway object counts.
- [ ] Has automated definition and prefab validation.
- [ ] Passes a full run as starter, main, and support spirit.

### Shared art and presentation requirements

- [ ] Final spirit design and colour palette.
- [ ] Selection-card portrait.
- [ ] In-game spirit sprite or animation set.
- [ ] Idle, movement/follow, attack, transform, merge, and re-emerge presentation as required by the design.
- [ ] Final weapon art.
- [ ] Final icon for the spirit, weapon, each ability, contract, mastery, and ascension.
- [ ] Final projectile, area, impact, status, and persistent-effect visuals.
- [ ] Consistent sorting layers, pivots, scale, and pixels-per-unit.
- [ ] Readable visuals when many enemies and abilities overlap.
- [ ] No temporary placeholder art in the normal demo path.
- [ ] Distinct weapon, ability, impact, contract, and rotation sounds.

### Fire Spirit — Phoenix

- [ ] Finish and polish the Flame Bow.
- [ ] Finish Fiery Feathers.
- [ ] Finish Fiery Talons.
- [ ] Finish Phoenix Dive.
- [ ] Validate burning, fire patches, homing, explosions, and revive behaviour.
- [ ] Complete all Fire icons, animations, VFX, SFX, descriptions, and upgrade levels.

### Wind Spirit — Roc

- [ ] Finish and polish Chakrams.
- [ ] Finish Razor Wind.
- [ ] Finish Tornado.
- [ ] Finish Gale Barrier as the third Wind ability.
- [ ] Validate piercing, pull, orbit/barrier, projectile deflection or damage rules, and multi-tornado behaviour.
- [ ] Complete all Wind icons, animations, VFX, SFX, descriptions, and upgrade levels.

### Water Spirit — Leviathan

- [ ] Finish and polish the Water Trident.
- [ ] Finish Tidal Wave.
- [ ] Finish Whirlpool.
- [ ] Finish Rain Clouds.
- [ ] Validate directional waves, pull behaviour, following clouds, multiple clouds, and target switching.
- [ ] Complete all Water icons, animations, VFX, SFX, descriptions, and upgrade levels.

### Earth Spirit — Golem

- [ ] Finish and polish the Stone Hammer.
- [ ] Finish Quicksand Domain.
- [ ] Finish Boulder Throw.
- [ ] Finish Stone Spikes.
- [ ] Remove RockFall from the demo kit or redefine it without duplicating Stone Spikes.
- [ ] Validate slow, pull, stun, bounce, split, explosion, bleed, and spike-chain behaviour.
- [ ] Complete all Earth icons, animations, VFX, SFX, descriptions, and upgrade levels.

## Two-area completion checklist

### Shared area requirements

- [ ] Has a stable area ID, display name, description, preview image, and unlock rule.
- [ ] Has final environment art, tiles, props, boundaries, collision, and spawnable ground.
- [ ] Has a distinct colour palette and strong visual identity.
- [ ] Has at least three normal enemy roles with final art and animation.
- [ ] Has at least one elite variant or elite modifier.
- [ ] Has a tuned spawn roster, budget curve, and difficulty phases.
- [ ] Has at least one area-specific hazard or encounter event.
- [ ] Has clear start, midpoint, final-minute, and boss pacing.
- [ ] Has a final boss with multiple attacks and readable telegraphs.
- [ ] Has area music, ambience, enemy SFX, boss SFX, and a victory cue.
- [ ] Has an area-introduction title and boss introduction.
- [ ] Has victory, reward, retry, and return-to-menu behaviour.
- [ ] Has a shortened development mode for rapid testing.
- [ ] Passes a complete performance and playtest checklist.

### Area 1 — Burning Plains

- [ ] Complete final Burning Plains environment art and layout.
- [ ] Finish Fire Runner as a fast pressure enemy.
- [ ] Finish Fire Flier as a ranged or evasive enemy.
- [ ] Finish Fire Tank as a slow, durable blocker.
- [ ] Add at least one fire-themed elite variation.
- [ ] Add a readable fire hazard that does not hide enemy attacks.
- [ ] Tune early-run onboarding and difficulty growth.
- [ ] Complete Fire Phoenix art, animations, VFX, SFX, and boss UI.
- [ ] Implement and tune Fire Dash, Feather Barrage, Flame Tornado, Meteor Rain, and Rebirth.
- [ ] Reward victory by unlocking Earth Plains.

### Area 2 — Earth Plains (working title)

- [ ] Confirm the final area name. Alternatives include Stonewilds, Shattered Highlands, and Earthen Expanse.
- [ ] Create the area definition, scene, environment art, tiles, props, and collision.
- [ ] Design at least three Earth-area enemy archetypes with different combat roles.
- [ ] Create final enemy sprites, animations, attacks, stats, rewards, and spawn definitions.
- [ ] Add at least one Earth elite variation.
- [ ] Add an Earth hazard such as falling rocks, fault lines, stone walls, or quicksand.
- [ ] Create and tune the Earth biome spawn roster and difficulty curve.
- [ ] Complete Earth Golem art, animations, VFX, SFX, and boss UI.
- [ ] Implement and tune Boulder Leap, Ground Spike, Quicksand, Rock Fall, and Boulder Bounce.
- [ ] Add a final demo-complete reward or presentation after defeating Earth Golem.

## Systems required for demo completion

### Front end and scene flow

- [ ] Main menu with Play, Continue, Settings, Credits, and Quit.
- [ ] Starter-spirit selection showing the four demo spirits.
- [ ] Area selection showing unlock status for both demo areas.
- [ ] Loading transition and error-safe scene loading.
- [ ] Pause, loss, victory, retry, next-area, and return-to-menu flow.
- [ ] Demo-complete screen after Earth Golem.

### Run progression

- [ ] Experience and level-up flow.
- [ ] Reliable three-card upgrade choices.
- [ ] Contract offers for unowned demo spirits.
- [ ] Maximum of three owned spirits.
- [ ] Spirit rotation with keyboard and controller input.
- [ ] Reroll cost and availability.
- [ ] Upgrade prerequisites and maximum-level handling.
- [ ] Post-run statistics.

### Save data

- [ ] Save version.
- [ ] Save Earth Plains unlock state.
- [ ] Save intended spirit unlock state.
- [ ] Save settings and control preferences.
- [ ] Save best run or completion status if shown in the UI.
- [ ] Handle missing or corrupted saves safely.
- [ ] Add Reset Demo Progress with confirmation.

### UI and onboarding

- [ ] Explain moving versus stationary attacks.
- [ ] Explain main and support spirit roles.
- [ ] Explain contracts, leveling, upgrades, and rotation.
- [ ] Show health, experience, level, timer, spirit slots, cooldowns, and boss health clearly.
- [ ] Show upgrade details without relying only on icons.
- [ ] Support keyboard, mouse, and controller navigation.
- [ ] Verify legibility at the demo's supported resolutions.

### Audio, settings, and accessibility

- [ ] Master, music, and SFX volume.
- [ ] Resolution, display mode, and quality options.
- [ ] Screen-shake intensity.
- [ ] Damage-number density.
- [ ] Reduced flashing.
- [ ] UI scale.
- [ ] Remappable controls or a clearly documented supported layout.
- [ ] Elemental icons and shapes so information is not communicated by colour alone.

### Performance and release readiness

- [ ] Meet the chosen frame target with 250 active enemies on minimum hardware.
- [ ] Profile all four spirits and both areas in Development Builds.
- [ ] Test high projectile, pickup, VFX, and damage-number counts.
- [ ] Verify pools reset correctly between areas and retries.
- [ ] Exclude MCP, Roslyn, debug tools, and other development-only DLLs from player builds.
- [ ] Remove missing references, placeholder art, and release-path console errors.
- [ ] Test a clean install and first launch.
- [ ] Test keyboard and controller from menu through demo completion.
- [ ] Test 30-minute repeated runs for memory growth.
- [ ] Produce and inspect a final build report.
- [ ] Add version number, credits, licenses, and feedback/contact information.
- [ ] Capture screenshots, a short trailer, store description, and control instructions.

## Demo definition of done

The demo is ready when:

- [ ] All four spirits pass their gameplay and presentation checklists.
- [ ] Both areas can be completed without developer tools.
- [ ] Both bosses are polished, fair, and stable.
- [ ] A new player can understand the core mechanics without external instructions.
- [ ] Progress, settings, victory, defeat, retry, and scene transitions work reliably.
- [ ] There are no placeholder assets in the normal player path.
- [ ] There are no known progression blockers, save-loss bugs, or frequent console errors.
- [ ] Performance meets the agreed target on minimum hardware.
- [ ] The build has been tested by people who did not develop it.
- [ ] Feedback from at least one external playtest pass has been reviewed and prioritized.

## Recommended production order

1. Complete the short Burning Plains loop with Fire Spirit.
2. Make menu, victory, defeat, retry, and saving reliable.
3. Finish and validate all four spirit kits without requiring final polish art.
4. Finish Burning Plains enemies and Fire Phoenix.
5. Build Earth Plains enemies, environment, and Earth Golem.
6. Complete final art, animation, VFX, icons, audio, and UI for all demo content.
7. Run balance, accessibility, performance, controller, and save-data passes.
8. Conduct external playtests and fix blockers.
9. Produce the release build and demo presentation material.

## Related notes

- [[Notes/Planning/Roadmap|Roadmap]]
- [[Notes/Planning/Project Audit and Backlog|Project audit and backlog]]
- [[Notes/Game Design/Spirits|Spirits]]
- [[Notes/Game Design/Areas and Bosses|Areas and bosses]]
- [[Notes/Planning/Decision Log|Decision log]]
