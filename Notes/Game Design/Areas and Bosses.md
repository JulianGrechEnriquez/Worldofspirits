---
type: design
status: active
updated: 2026-08-17
tags: [game-design, levels, bosses]
---

# Areas and bosses

## Story Mode sequence

Each plane lasts ten minutes and ends with its guardian. Completing the encounter unlocks the next plane.

| # | Plane | Starting spirit | Guardian | Weakness |
|---:|---|---|---|---|
| 1 | Burning Plains | Fire | Fire Phoenix | Water |
| 2 | Frozen Wastes | Ice | Ice Yeti | Fire |
| 3 | Thunder Peaks | Lightning | Storm Dragon | Earth |
| 4 | Poison Marsh | Poison | Giant Scorpion | Wind |
| 5 | Shadow Realm | Necrotic | Necrotic Bat King | Holy |
| 6 | Celestial Temple | Holy | Fallen Angel | Necrotic |

## Infinity Mode

Infinity Mode unlocks after all Story Mode guardians are defeated. Enemies and hazards from every plane share one continuous run, all six guardians appear sequentially, and endless scaling continues afterward.

## Burning Plains vertical slice

> Menu → Burning Plains → ten-minute survival → Fire Phoenix → reward → stage completion → menu

- **Fire Runner:** fast pressure enemy.
- **Fire Flier:** ranged or evasive threat.
- **Fire Tank:** slow, durable space blocker.

## Fire Phoenix

The runtime uses phase thresholds at 66% and 33% health. Its attack controller avoids immediately repeating the previous attack.

### Phase plan

- **Phase 1 — The Hunt (100–66%):** Fire Dash and Feather Barrage teach line and cone avoidance.
- **Phase 2 — Burning Sky (66–33%):** Flame Tornado reduces safe space.
- **Phase 3 — Phoenix Storm (33–0%):** Meteor Rain joins the full attack pool.
- **Rebirth:** on its first death, the Phoenix becomes invulnerable for two seconds and returns at 50% health in its most dangerous phase.

### Telegraphs and counterplay

- **Fire Dash:** 10-unit by 1.5-unit warning line for 0.8 seconds; dodge perpendicular to it.
- **Feather Barrage:** 0.6-second wind-up before seven feathers across a 70-degree fan; add a visible fan, wing animation, and audio cue.
- **Flame Tornado:** circle warning at the recorded player position for 0.7 seconds; tornado lasts four seconds.
- **Meteor Rain:** five marked circles for one second while meteors visibly fall.
- **Rebirth:** show a flame core and harmless shockwave; the pause is a repositioning window.

The Phoenix takes 25% additional Water damage and 25% less Fire damage.

## Planned guardian move sets

- **Ice Yeti:** Snowball, Ice Shards, Blizzard, Frozen Slam.
- **Storm Dragon:** Lightning Strike, Thunder Roar, Lightning Dash, Thunder Dash.
- **Giant Scorpion:** Venom Tail, Toxic Glob, Poison Stinger, Acid Spray.
- **Necrotic Bat King:** not designed.
- **Fallen Angel:** not designed.

## Optional challenge guardians

Earth Golem, Water Leviathan, and Wind Roc sit outside the six-stage Story Mode sequence. They may become summoning challenges, optional guardians, or post-story encounters.

- **Earth Golem:** Boulder Leap, Ground Spike, Quicksand, Rockfall, Boulder Bounce.
- **Water Leviathan:** Wave Dash, Wave Crash, Whirlpool, Rain Clouds, Bubble Beam.
- **Wind Roc:** Sky Dive, Razor Gust, Tornado Trail, Wing Cyclone.

## Shared area requirements

Every plane requires final environment art, at least three normal enemy roles, elite variation, an elemental hazard, tuned spawning, music and ambience, readable boss telegraphs, victory rewards, and reliable retry/return flow.

