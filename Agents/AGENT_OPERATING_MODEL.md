# Agent Operating Model

## Purpose

This document makes the three role files executable instead of decorative. The product manager owns direction and release gates, the game designer owns testable rules, and the Unity developer owns implementation evidence. All agents share one branch and one workspace, so only one agent may write at a time.

## Product Manager Gate

The product manager answers before a version is expanded:

1. Genre convention: is the mechanic familiar to side-scrolling action shooter players?
2. Original difference: does the project use its own theme and rules rather than copying a commercial work?
3. Learning curve: can a new player understand movement, jump, shoot, dodge, and harvest within 30 seconds?
4. Retry reason: after defeat, does the player know what to practice next?

Release order is fixed: playable boss loop, boss depth, short run-and-gun level, then build variety. Audio, controller support, world map, and meta systems wait until the combat loop is stable.

## Game Designer Contract

Every gameplay proposal must include:

- Learning: first-time tell, safe space, and observation window.
- Risk: reward, cost, failure state, and counterplay.
- Rhythm: frequency, interruption, and worst-case stacking.
- Data: config field, default, bounds, and tuning purpose.
- Acceptance: repeatable steps, expected visual/log result, and failure condition.

Boss design uses one identity, three phases, and at least two attack grammars per phase. A stage adds no more than two new pressures at once.

## Unity Developer Contract

Every implementation must define:

- Data ownership and config schema.
- State transitions and cancellation behavior.
- Object lifecycle, pooling, and scene reset.
- Event subscription and teardown.
- Verification through compile, PlayMode, and manual operation.

Boss phases must be config-driven and expose enter, update, attack, and exit behavior without scattering health-threshold checks across `Update`.

## Operating Loop

| Step | Owner | Output |
| --- | --- | --- |
| 1 | Product manager | One goal, non-goals, priority, and acceptance gate. |
| 2 | Game designer | Rules, phases, numbers, resources, and test steps. |
| 3 | Product manager | Design review and scope decision. |
| 4 | Unity developer | Implementation, tests, logs, and delivery report. |
| 5 | Product manager | Evidence review, task board update, and release decision. |

## Definition Of Done

- The feature can be played, not only compiled.
- Failure and retry paths work.
- Config values are named and bounded.
- No runtime object or event listener leaks after restart.
- Test evidence and unresolved risks are recorded.
- Chinese logs describe object, action, and key parameters.

## Current Role Boundaries

- Product manager: user alignment, market comparison, scope, acceptance, commit, push.
- Game designer: rules, boss phases, levels, UI/resource specs, numbers, configs.
- Unity developer: C#, scenes, prefabs, state machines, pooling, VFX, tests, performance.

Cross-role work is split by the product manager before implementation begins.
