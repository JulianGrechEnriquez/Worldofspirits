---
type: milestone
status: active
milestone: vertical-slice
reviewed: 2026-10-05
tags: [planning, demo, vertical-slice]
---

# Demo scope and completion plan

## Demo promise

The first public demo is a polished Burning Plains vertical slice that demonstrates the game's distinctive movement states, spirit party building, upgrades, enemy pressure, and Fire Phoenix guardian fight.

The demo is a production milestone for the larger six-stage Story Mode. It does not need to contain the complete campaign or Infinity Mode.

## Player flow

1. Open the main menu.
2. Choose Spirit Tamer, Spirit Warrior or Spirit Shepherd and start Story Mode.
3. Enter Burning Plains with Fire as the required starting spirit.
4. Survive enemy waves and collect experience.
5. Contract additional spirits within the selected character's party limit; the Shepherd's exact larger capacity remains to be decided.
6. Use the selected character's combat rules: Tamer form switching, Warrior weapon combat or Shepherd companion abilities.
7. Survive ten minutes.
8. Defeat the Fire Phoenix, including its Rebirth.
9. View rewards and run statistics.
10. Return to the menu with completion saved.

## Included

- Three playable characters: Spirit Tamer, Spirit Warrior and Spirit Shepherd.
- Shepherd-only combined ability moves: first candidate is Whirlpool + Tornado. Finalize its upgrade placement, maximum-level prerequisites and casting rules; spirit fusion is excluded.

- Burning Plains with Fire Runner, Fire Flier, and Fire Tank.
- Fire Phoenix with phases, telegraphs, recovery windows, elemental rules, and Rebirth.
- Fire, Earth, Water, and Wind as the initial production-ready contract pool.
- The four demo spirits' base weapons and three abilities with five-level progression, plus character-specific compatibility and Warrior paired-weapon work.
- Tamer three-spirit party acquisition and rotation; Warrior party/weapon rules and a larger Shepherd party require final tuning.
- Tamer one-second rotation cooldown and four elemental rotation buffs; define appropriate main-slot or lead-companion behavior for Warrior and Shepherd.
- Tamer moving support state, stationary weapon state, Focused and Empowered charge; Warrior movement-focused weapon-only combat; Shepherd companion-only combat.
- Character-exclusive pools of 5 Common, 4 Uncommon, 3 Epic and 2 Legendary upgrades each, alongside compatible existing general and spirit cards. Warrior/Shepherd drafts exist; the complete pools still require implementation and balancing.
- Experience, upgrades, health, victory, defeat, pause, retry, and results.
- Versioned save data for settings and Burning Plains completion.
- Keyboard and controller support, essential settings, accessibility, audio, onboarding, and performance work.

## Not required for this demo

- Spirit Beastmaster: keep independent spirit creatures, creature health and disappearance/recovery for later development.
- Merging spirits into a new spirit. Shepherd combined moves keep the participating spirits separate.

- Frozen Wastes through Celestial Temple.
- Infinity Mode.
- Completed Ice, Lightning, Poison, Necrotic, or Holy kits.
- Earth Golem, Water Leviathan, or Wind Roc challenge encounters.
- Online features, daily runs, achievements, or a large permanent-upgrade tree.

## Spirit definition of done

Shared spirit content must be validated under each demo character's allowed forms. Tamer stance/slot requirements below apply to the Tamer; Warrior and Shepherd require their own checks.

- [ ] Stable ID, element, role, weapon, and three abilities.
- [ ] Five levels for the weapon and every ability.
- [ ] Works as main, support one, and support two.
- [ ] Rotation buff and one-second cooldown behave correctly.
- [ ] Transformation, Focused, Empowered, and support-state feedback are readable.
- [ ] Applicable global upgrades work correctly.
- [ ] Final or demo-quality art, animation, VFX, icons, descriptions, and audio.
- [ ] Extreme combinations do not create runaway objects or errors.

## Character definition of done

- [ ] Select any of the three demo characters before a run.
- [ ] Tamer: companion/weapon switching and stationary charging work correctly.
- [ ] Warrior: all owned spirits stay weapons; movement combat, main-slot pairing and chosen armour mechanics work correctly.
- [ ] Shepherd: companions cannot become weapons, cast while stationary and obey the approved larger party limit.
- [ ] Each character has its approved 14-card exclusive pool, with shared/spirit cards filtered for compatibility.
- [ ] HUD, tooltips and controls communicate the selected character's rules.
- [ ] Each character can complete Burning Plains and retry without leaked state or pooled effects.
- [ ] Profile simultaneous weapon attacks and the Shepherd's maximum companion party.

## Fire Phoenix definition of done

- [ ] Phase gates at 66% and 33% produce readable escalation.
- [ ] Fire Dash displays its locked line warning.
- [ ] Feather Barrage gains a fan warning, wing wind-up, and audio cue.
- [ ] Flame Tornado displays its spawn circle and persists predictably.
- [ ] Meteor Rain displays five impact markers and falling meteors.
- [ ] Rebirth pauses attacks, communicates invulnerability, and restores 50% health once.
- [ ] Water deals bonus damage and Fire is resisted.
- [ ] Freeze, stun, pull, and pin convert correctly against the boss.
- [ ] Boss UI, death, victory, reward, retry, and return-to-menu work reliably.

## Technical acceptance

- [ ] Complete menu-to-results flow without developer tools.
- [ ] Run-state, pools, UI, spirits, timers, and boss reset correctly on retry.
- [ ] Meet the selected frame target with 250 active enemies on minimum hardware.
- [ ] Test high projectile, pickup, VFX, and damage-number counts.
- [ ] No missing references, release-path console errors, or development-only DLLs in the build.
- [ ] Test keyboard and controller through the complete flow.
- [ ] Complete at least one external playtest pass.

## Production order

As of 4 October, the menu entry, basic settings and several spirit effects are implemented. The complete menu-to-results loop, every five-level upgrade behavior, performance target and controller acceptance remain to be validated. See [[Notes/Development/Spirit Ability Implementation Status|the current ability checklist]] and [[Notes/Development/2026-10-04 Gameplay and UI Update|the latest changelog]].

1. Finish the short menu-to-Phoenix loop.
2. Complete movement-state, charging, rotation, and buff feedback.
3. Standardize and validate the four initial spirit kits.
4. Finish Burning Plains pacing and Fire Phoenix presentation.
5. Add saving, settings, accessibility, audio, and controller polish.
6. Profile, playtest, fix blockers, and capture portfolio media.

## Related

- [[Notes/Game Design/Game Overview|Game overview]]
- [[Notes/Game Design/Combat Rules and Elements|Combat rules]]
- [[Notes/Game Design/Spirits|Spirits]]
- [[Notes/Game Design/Areas and Bosses|Areas and bosses]]
- [[Notes/Planning/Roadmap|Roadmap]]
- [[Showcase/World of Spirits|Public showcase draft]]

