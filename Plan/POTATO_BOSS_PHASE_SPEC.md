# Potato Boss Phase Specification

Date: 2026-08-18

Scope: V0.1 Boss battle. This spec defines three phases, two attack grammars per phase, phase transition safety, and battle statistics. It does not add audio, controller, shop, or long levels.

## Phase Health

| Phase | Health range | Theme | New pressure |
| --- | --- | --- | --- |
| 1 | 100 to 71 | Sprout spit | Horizontal shots plus lobbed seed. |
| 2 | 70 to 36 | Root disturbance | Faster shotgun pattern plus ground root hazard. |
| 3 | 35 to 1 | Insect burst | Shotgun pattern plus summoned insects. |
| 0 | 0 | Dead | Death, effect, victory, restart. |

Phase transition threshold is evaluated after damage. A phase transition disables new attacks for 0.6 seconds, clears active boss projectiles, logs the transition, and increments battle statistics.

## Attack Grammar Tables

### Phase 1 Grammar 1: Lane Shotgun

- Behavior: spawn top, middle, bottom horizontal projectiles.
- Tell: attack animation clip starts 0.15 seconds before first projectile.
- Dodge: move between lanes or jump over low lane.
- Reward: pink projectiles can be collected.
- Config: shot count 3, interval 0.18, pink chance 0.35.

### Phase 1 Grammar 2: Lobbed Seed

- Behavior: one seed lobs toward the player with vertical arc.
- Tell: boss leans backward before launching.
- Dodge: move horizontally and keep altitude.
- Reward: creates a vertical obstacle that can be jumped over.
- Config: appears on alternating attacks.

### Phase 2 Grammar 1: Faster Shotgun

- Behavior: four shots, shorter interval, higher pink chance.
- Tell: faster attack animation and short cooldown.
- Dodge: requires dash or diagonal movement.
- Reward: energy collection becomes easier.
- Config: shot count 4, interval 0.15, pink chance 0.45.

### Phase 2 Grammar 2: Root Belch

- Behavior: one or two root hazards pop from the ground near the player.
- Tell: soil bump marker appears for 0.35 seconds.
- Dodge: jump before the root reaches the player.
- Reward: roots limit ground camping and reward airborne play.
- Config: root speed moderate, lifetime 2.2 seconds.

### Phase 3 Grammar 1: Insect Swarm

- Behavior: two to four slow insects fly toward the player.
- Tell: small insect spawn animation beside the boss.
- Dodge: shoot insects before they close distance.
- Reward: insects can be destroyed by player bullets.
- Config: speed moderate, one-hit kill.

### Phase 3 Grammar 2: Final Shotgun

- Behavior: shotgun plus root hazard alternation in short windows.
- Tell: attack animation alternates lanes.
- Dodge: combines dash, jump, and super decisions.
- Reward: pink projectiles remain available.
- Config: shot count 5, interval 0.14, pink chance 0.5.

## Player Answers

| Hazard | Player answer | Fail state |
| --- | --- | --- |
| Horizontal projectile | Move between lanes / dash | Damage 1, invincibility 0.75s |
| Lobbed seed | Horizontal move / jump | Landing on seed damages player |
| Root hazard | Jump before arrival | Contact damages player |
| Insect | Shoot before contact | Contact damages player |
| Super | Consumes 3 energy, high burst | Miss if boss transitions during effect |

## Statistics

Recorded at runtime:

- Battle duration in seconds.
- Player hits taken.
- Pink projectiles collected.
- Super uses.
- Phase transitions reached.
- Victory or defeat.

Stats reset when a battle starts and are shown inside the result panel after victory or defeat.

## Lifecycle

- GameManager resets all pooled runtime objects at battle start.
- BossProjectile, BossRootHazard, and BossInsect own their pools and despawn methods.
- PlayerProjectile can destroy insects and collect pink projectiles.
- BossProjectile active set is cleared on phase transition.
- BattleStats resets on battle start and persists until the result panel is shown.

## Acceptance

EditMode/PlayMode checks:

1. Boss starts in phase 1.
2. Boss reaches phase 2 after taking 30 total damage.
3. Boss reaches phase 3 after taking 65 total damage.
4. Phase transition prevents attacks for at least 0.5 seconds.
