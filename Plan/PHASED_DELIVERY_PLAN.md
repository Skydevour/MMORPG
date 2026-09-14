# Phased Delivery Plan

Date: 2026-08-17

This plan turns the genre roadmap into an execution pipeline. Each stage has one goal, a fixed scope, three role checks, and a playability gate. A stage cannot be marked complete when only its code exists.

## Stage Rules

1. One stage is active at a time.
2. The product manager opens the stage and confirms scope.
3. The game designer produces testable rules before implementation.
4. The Unity developer implements only the confirmed scope.
5. All three roles run their checks.
6. The stage closes only when the playability gate passes.
7. Any new idea outside the stage goes to the backlog, not into the current branch.

## Three-Agent Check Gate

| Check | Product manager | Game designer | Unity developer |
| --- | --- | --- | --- |
| Scope | Is the stage goal clear and small enough? | Does the proposal solve a player problem? | Is the implementation scope bounded? |
| Design | Is it original rather than a copy? | Are rules, tells, numbers, and counterplay explicit? | Does the architecture preserve the rule? |
| Playability | Can a new player understand it quickly? | Does it create a meaningful choice? | Does it feel responsive at 60 FPS target? |
| Data | Is the metric observable? | Are config bounds and tuning purpose clear? | Are values data-driven and logged? |
| Lifecycle | Is retry/restart covered? | Are failure and success states defined? | Are pools, events, and scene objects reset? |
| Evidence | Is the acceptance path repeatable? | Are manual test steps defined? | Are compile and PlayMode results recorded? |

A failed check creates a rework task. It does not create a parallel workaround.

## Stage 0: Agent Pipeline Stabilization

Status: current setup is mostly complete; this document closes the remaining gap.

Goal: ensure every later stage follows one measurable workflow.

Deliverables:

- Role files and shared protocol.
- Task board with dependencies and status.
- Phased delivery plan.
- Design, implementation, test, and log conventions.

Playability gate:

- No gameplay claim is accepted without task, design, implementation, and evidence records.

Stop-loss:

- If a task exceeds its allowed file scope, stop and split it instead of expanding it silently.

## Stage 1: V0.1 Boss Battle Complete Loop

Goal: make one three-phase Potato Boss battle feel complete and replayable.

Deliverables:

- Three boss phases with distinct pressure.
- At least two attack grammars per phase.
- Phase transition tells and short safe windows.
- Victory, defeat, restart, and battle statistics.
- Stable keyboard movement, jump, double jump, dash, dodge, shoot, pink pickup, and super.

Role checks:

- Product manager: one battle lasts 3-8 minutes and the retry reason is visible.
- Game designer: every attack has tell, dodge answer, risk, reward, and acceptance step.
- Unity developer: phase changes, damage edges, pooling, and restart have test evidence.

Playability gate:

- A tester can win and lose deliberately, restart, and identify what to improve.
- No projectile, enemy, effect, listener, or statistic survives incorrectly after restart.

Stop-loss:

- If the full three-phase schedule stalls, ship phases 1 and 3 first and move phase 2 to the next stage.

## Stage 2: V0.2 Hand Feel And Feedback

Goal: make the existing loop feel responsive and readable.

Deliverables:

- Standardized projectile and hazard tells.
- Hit direction, hit stop, damage flash, and clearer phase banners.
- Easy, standard, and expert presets.
- Result grade based on time, damage, pickups, and super use.

Playability gate:

- A tester can explain why they were hit and what to change next attempt.

Stop-loss:

- If effects obscure bullets, reduce VFX before adding options.

## Stage 3: V0.3 Short Run-And-Gun Level

Goal: validate the second major mode without building a long map first.

Deliverables:

- One 3-5 minute linear garden stage.
- Five beats: teaching, vertical, pressure, harvest, boss entrance.
- Three reusable enemy roles: crawler, low flyer, turret.
- Checkpoint and restart flow.
- Tilemap for interaction only; full-image atmosphere backgrounds.

Playability gate:

- Death sends the player to a fair checkpoint, not to a long repeated walk.

Stop-loss:

- If the level expands beyond five beats, cut it and reuse the boss arena for wave validation.

## Stage 4: V0.4 Weapons And Build Choices

Goal: add strategy without weakening the default weapon.

Deliverables:

- Scatter seed gun.
- Tracking vine.
- Two equip slots.
- Charms for extra dodge, energy retention, or pink energy bonus.
- Unlock goals based on grade or performance.

Playability gate:

- Each loadout has a visible advantage and cost against the same boss.

Stop-loss:

- If a build trivializes the boss, put it behind an experimental flag until rebalanced.

## Stage 5: V0.5 Content Expansion

Goal: grow from a slice to a small game.

Deliverables:

- Three original bosses.
- Two short run-and-gun stages.
- Boss select and simple world progression.
- Save best grades and unlocks.

Playability gate:
