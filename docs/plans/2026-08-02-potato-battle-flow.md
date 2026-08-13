# 土豆 Boss Battle Flow Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build and verify the keyboard-only Boss projectile, pink projectile energy collection, three-slot energy meter, and full-energy super attack flow.

**Architecture:** Load all new gameplay constants from a JSON config service before spawning actors. Keep Boss attack scheduling, pooled projectile behavior, player energy, HUD, and VFX in separate modules connected by narrow public methods.

**Tech Stack:** Unity 6, C#, Unity Input System with legacy fallback, `JsonUtility`, `ParticleSystem`, `UnityEngine.UI`, existing `FrameAnimator`, `ComponentObjectPool<T>`.

---

### Task 1: Add global configuration and damage contracts

**Files:**
- Create: `Assets/Resources/Config/GameConfig.json`
- Create: `Assets/Scripts/Game/Config/GameConfig.cs`
- Create: `Assets/Scripts/Game/Combat/IDamageable.cs`
- Modify: `Assets/Scripts/Game/Core/GameManager.cs`

Steps: create the JSON defaults, load/normalize them before actor creation, expose serializable config classes, and add a minimal damage interface for player bullets and the super attack.

Verification: compile the project and run a PowerShell JSON validation command; expected zero errors and `maxEnergy=3`.

### Task 2: Implement pooled Boss projectiles and attacks

**Files:**
- Create: `Assets/Scripts/Game/Projectiles/BossProjectile.cs`
- Modify: `Assets/Scripts/Game/Bosses/Potato/PotatoBossController.cs`
- Modify: `Assets/Scripts/Game/Bosses/Potato/PotatoBossStateDriver.cs`
- Modify: `Assets/Scripts/Game/Bosses/Potato/PotatoBossSpawner.cs`

Steps: add normal/pink projectile types, generated placeholder visuals, trigger damage/collection entry points, pooled lifetime handling, Boss attack timing and random selection, and attack/dead animation states.

Verification: compile and inspect the generated object hierarchy; expected one Boss with attack state and pooled projectile root.

### Task 3: Add player energy, hit handling, and super attack input

**Files:**
- Create: `Assets/Scripts/Game/Player/PlayerEnergyController.cs`
- Create: `Assets/Scripts/Game/VFX/SuperAttackEffect.cs`
- Create: `Assets/Scripts/Game/VFX/EnergyPickupEffect.cs`
- Modify: `Assets/Scripts/Game/Player/PlayerController2D.cs`
- Modify: `Assets/Scripts/Game/Player/PlayerSpawner.cs`
- Modify: `Assets/Scripts/Game/Player/PlayerStateDriver.cs`
- Modify: `Assets/Scripts/Game/Projectiles/PlayerProjectile.cs`

Steps: add three-slot energy state, keyboard `K` activation, invulnerable super window, boss damage callback, pink projectile collection, and VFX bursts.

Verification: compile and trace the public state transitions; expected energy is clamped to three and super cannot activate below full energy.

### Task 4: Add the energy meter and bind the runtime flow

**Files:**
- Create: `Assets/Scripts/Game/UI/PlayerEnergyMeter.cs`
- Modify: `Assets/Scripts/Game/Core/GameManager.cs`
- Modify: `Assets/Scripts/Game/Core/PrototypeAssetLoader.cs`

Steps: create a screen-space runtime HUD with three fixed-size slots, bind it to the spawned player, pass the player target to the Boss, and correct the current Boss pivot fallback to the normalized bottom-center convention.

Verification: compile and inspect UI object creation; expected the meter is created once and updates on energy events.

### Task 5: Regression verification and Chinese logs

**Files:**
- Modify: `Plan/daily_dev_log.md`
- Modify: `Plan/test_log.md`
- Modify: `task_plan.md`
- Modify: `progress.md`
- Modify: `findings.md`

Steps: run config/resource validation, build all available project assemblies, run Unity batch validation if the editor executable is available, and record actual results and remaining manual Play Mode checks in Chinese.

Verification: no new compiler errors; logs explicitly distinguish automated verification from Play Mode verification.
