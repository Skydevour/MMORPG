# Genre Benchmark And Expansion Plan

Date: 2026-08-17. This document extracts public genre patterns only. It does not copy characters, art, animation, audio, level layouts, or exact values from any commercial game.

## Public Research Summary

Cuphead is publicly described as a Unity run-and-gun game built around side-scrolling combat, boss fights, limited ability loadouts, and multiplayer. Its important lesson is learnable pressure: readable tells, phase changes, parry resources, super meter, and post-fight evaluation.

Contra: Operation Galuga reviews highlight a modern approach to a classic hard run-and-gun: default double jump, dash, optional life bar, weapon upgrades, perks, and shop choices. The lesson is not lower difficulty, but controllable difficulty.

Metal Slug, Huntdown, Mighty Goose, Blazing Chrome, and similar titles show the other major route: continuous stages, enemy waves, weapon pickups, vehicles or power-ups, cooperative set pieces, and strong route spectacle.

## Genre Patterns

| Dimension | Common pattern | Project decision |
| --- | --- | --- |
| Movement | Immediate response, double jump, dash, air choices | Keep keyboard-only control until the core loop is stable. |
| Difficulty | One-hit classics versus modern life bars and options | Keep 3 HP now; add easy/standard/expert later. |
| Weapons | Pickup, upgrade, replacement, and loss risk | Add a second weapon only after the boss loop is complete. |
| Boss | Phases, tells, and attack grammar | Expand Potato Boss from one phase to three phases. |
| Levels | Advance, encounter, escape, checkpoint, boss | Add a short stage after boss depth, not before. |
| Retry | Fast restart and visible failure cause | Add battle statistics and grade after victory/defeat. |
| Co-op | Common value but high debugging cost | Evaluate local two-player after V1 scope. |

## Cuphead Route

Use these design principles without copying content:

1. Every attack has a readable pre-action.
2. Pink projectiles convert danger into energy.
3. The super attack is powerful but does not replace normal shooting.
4. Phase changes alter position, pressure, or interaction.
5. Result stats tell the player what to improve.

Current implementation already has movement, double jump, dash, shooting, dodge, pink projectile pickup, energy, super, health, hit feedback, pause, defeat, and restart. The main gap is depth: Potato Boss still remains in phase 1 and mostly fires horizontal shots from three lanes.

## Contra / Metal Slug Route

Use these principles later:

1. A stage is a sequence of movement, encounter, escape, and checkpoint beats.
2. Weapon pickups create temporary power and risk.
3. Terrain and enemy placement change player stance instead of only increasing bullet count.
4. Short repeatable encounters are more valuable than one long scripted level.

The project should not build a long map yet. First make a three-phase boss vertical slice, then add a three-to-five minute run-and-gun level.

## Original Theme

Use an original garden-soil fantasy:

- The hero defends the edge of a living garden.
- Potato Boss evolves through seed spit, roots, soil pressure, and insect summons.
- Player skills use harvest, seed, fertilizer, and burst-growth language.
- Art direction is original hand-drawn garden theatre, not a copy of vintage cartoon or another commercial style.

## V0.1: Three-Phase Potato Boss

### Phase 1: Sprout Spit, 100 to 70 HP

- Keep current top/middle/bottom horizontal seed shots.
- Add one lobbed seed that creates a short ground marker.
- Transition at 70 HP: boss dips, rises, clears projectiles, and telegraphs the next phase.

### Phase 2: Root Disturbance, 70 to 35 HP

- Add one or two root hazards with a 0.35 second soil bump tell.
- Increase shot pressure, but also increase pink projectile opportunity.
- Transition at 35 HP: shell cracking animation and brief safe window.

### Phase 3: Insect Burst, 35 to 0 HP

- Summon two to four slow insects that can be shot down.
- Alternate seed shots and root hazards in short windows.
- Death triggers a soil burst and victory statistics.

### Numeric Rules

- Player HP: 3.
- Normal projectile damage: 1.
- Damage invincibility: 0.75 seconds.
- Dash and super remain invincible.
- Phase transition safe window: 0.5 to 0.8 seconds.
- Pink projectiles must pass through reachable player space.
- Super damage should not skip an entire phase.

## V0.2: Feedback And Difficulty

- Projectile tell: 0.25 to 0.35 seconds.
- Ground hazard tell: 0.35 to 0.45 seconds.
- Add hit direction, short hit stop, and defeat summary data.
- Add easy/standard/expert presets that change HP, projectile speed, cooldown, and forgiveness without changing attack logic.
- Track clear time, remaining HP, pink pickups, hits taken, and super uses.
- Grade no-damage and speed first, pickup efficiency second.

## V0.3: Short Run-and-Gun Level

Build five beats:

1. Teaching: flat ground, simple enemy, basic shooting.
2. Vertical: platforms and high enemy.
3. Pressure: narrow route, ground hazard, ranged enemy.
4. Harvest: pink targets and weapon pickup.
5. Boss: separate arena.

Use Tilemap for interactive collision only. Use full images for atmosphere and background. Start with crawler, low flyer, and fixed turret enemies.

## V0.4: Build Variety

- Scatter seed gun: short range, high output, close-risk.
- Tracking vine: low damage, automatic targeting, lower burst.
- Charms: extra dodge, energy retention after damage, bonus pink energy.
- Equip limit: two items.
- Unlock by grade, no-damage, or harvest goals, not material grinding.

## Next Agent Tasks

Product manager:

- Keep the V0.1 gate closed until three phases, statistics, victory, defeat, and restart are all testable.
- Do not add audio, controller, shop, or world map in V0.1.

Game designer:

- Produce a phase table, attack grammar table, tell timing, hazard area, resource probability, and acceptance steps.
- Define each enemy role before requesting art.

Unity developer:

- Refactor boss timing into phase states and data-driven attack patterns.
- Add battle statistics and no-leak restart validation.
- Keep pooling, frame animation, state machine, and config boundaries clean.

## Stop-Loss Rules

- If three phases take more than two development days, ship phases 1 and 3 first.
- If a stage slows the core loop, reuse the boss arena for enemy waves.
- If build variety harms default weapon feel, keep it behind an experimental flag.
- If art blocks progress, use current frames and runtime placeholder visuals until gameplay is accepted.
