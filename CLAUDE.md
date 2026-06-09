# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a 2D Tower Defense game ("Quantall") built in Unity (C#). The planning document `KeHoach_LapTrinh_Quantall_Unity.md` defines the full intended OOP architecture; the current codebase implements the early gameplay phases.

## Unity Workflow

There is no CLI build command — this is a Unity project. All compilation and testing is done inside the Unity Editor. To validate C# changes:
- Open the project in Unity 2022 LTS or later
- Unity auto-compiles on file save; check the Console for errors
- Play Mode tests are run from the Editor (Window → General → Test Runner)
- The main game scene is `Assets/GameScenes/SampleScene.unity`

## Code Architecture

All game scripts live in `Assets/Code/GameScripts/`. There are three singleton managers that most other scripts reference:

| Class | File | Role |
|---|---|---|
| `Level_Manager` | `Level_Manager.cs` | Global state: player currency, enemy path waypoints (`startPoint` + `path[]`) |
| `EnemySpawner` | `EnemySpawner.cs` | Wave management: spawns enemies over time with difficulty scaling; fires `onEnemyDestroy` UnityEvent |
| `BuildManager` | `BuildManager.cs` | Tower shop: holds array of `Tower` data objects, tracks which tower is selected |

### Gameplay Loop

**Enemy flow:** `EnemySpawner` instantiates enemies at `Level_Manager.main.startPoint`. `EnemyMovement` advances enemies along `Level_Manager.main.path[]` (array of `Transform` waypoints). When an enemy reaches the end, it invokes `EnemySpawner.onEnemyDestroy` and destroys itself.

**Tower flow:** `Plot` (on each map tile) listens for clicks. On click it reads `BuildManager.main.GetSelectedTower()`, checks `Level_Manager.main.currency`, calls `Level_Manager.main.SpendCurrency()`, then `Instantiate`s the turret prefab. `TurretScript` detects enemies via `Physics2D.CircleCastAll` using an `enemyMask` layer, rotates toward the target, and fires `Bullet` prefabs. Bullets home in on a specific `Transform target`; on collision they call `Health.TakeDamage()`.

**Economy:** `Health.TakeDamage()` calls `Level_Manager.main.IncreaseCurrency(currencyWorth)` when an enemy dies. `Menu.cs` updates the currency display every `OnGUI` frame.

**Wave scaling:** `EnemySpawner.EnemiesPerWave()` = `baseEnemies * currentWave ^ difficultyScalingFactor`.

### Key Design Constraints

- Bullets are **target-tracking homing projectiles** — they hold a `Transform target` reference and home in each `FixedUpdate`. If the target is destroyed, the bullet self-destructs. Collision only counts if the hit collider belongs to the tracked target (`IsTargetCollider` checks `.transform`, `.attachedRigidbody.transform`, and `.transform.root`).
- `TurretScript` imports `UnityEditor` for the `OnDrawGizmosSelected` range visualization — this is intentional but must be wrapped in `#if UNITY_EDITOR`.
- `Plot.OnMouseEnter/Exit/Down` — hover highlighting uses the Unity Input System's legacy mouse events. If the project switches to the new Input System, these must be replaced.
- `Tower` in `Assets/Code/GameScripts/Tower.cs` is a plain `[Serializable]` C# class (not a `MonoBehaviour`), used as a data container inside `BuildManager`'s inspector array.

### Planned Architecture (not yet implemented)

The design document specifies the full target architecture: `GameSession` (singleton coordinator), `Base` (player base HP), `Level`/`Wave` (ScriptableObjects), `Player` (input handler), `PowerUp` (ScriptableObject), `Path`/`Waypoint`, and 7 UI View classes. Scene flow should be Boot → MainMenu → Gameplay. See `KeHoach_LapTrinh_Quantall_Unity.md` for class signatures and phased implementation plan.

One naming caveat from the design doc: avoid naming fields `name` on `MonoBehaviour` subclasses — it shadows `UnityEngine.Object.name`. Use `towerName`, `enemyName`, etc. instead.
